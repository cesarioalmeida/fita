﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Repositories;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Core
{
    public class PortfolioService : IPortfolioService
    {
        private readonly List<Category> _categories = new();

        public ILoggingService LoggingService { get; set; }

        public TradeRepoService TradeRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }

        public CategoryRepoService CategoryRepoService { get; set; }

        public SecurityPositionRepoService SecurityPositionRepoService { get; set; }

        public ClosedPositionRepoService ClosedPositionRepoService { get; set; }

        public Task<bool> IsTradePossible(Trade trade)
        {
            return Task.Run(
                async () =>
                {
                    if (trade.Action == TradeActionEnum.Buy)
                    {
                        return true;
                    }

                    var securityPosition =
                        await SecurityPositionRepoService.DetailsForSecurityEnrichedAsync(trade.Security.SecurityId);

                    return trade.Quantity <= securityPosition.Quantity;
                });
        }

        public Task<bool> ProcessTrade(Trade trade, Transaction transaction)
        {
            return Task.Run(
                async () =>
                {
                    try
                    {
                        transaction = await PrepareTransaction(trade, transaction);
                        var positionSuccess = await PrepareSecurityPosition(trade);

                        return positionSuccess && await TradeRepoService.SaveAsync(trade) == Result.Success &&
                               await TransactionRepoService.SaveAsync(transaction) == Result.Success;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(ProcessTrade)}: {ex}");
                        return false;
                    }
                });
        }

        public Task<bool> DeleteTrade(ObjectId tradeId)
        {
            return Task.Run(
                async () =>
                {
                    try
                    {
                        var trade = await TradeRepoService.DetailsEnrichedAsync(tradeId);
                        
                        return await DeleteSecurityPosition(trade) &&
                               await TradeRepoService.DeleteAsync(trade.TradeId) == Result.Success;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DeleteTrade)}: {ex}");
                        return false;
                    }
                });
        }

        private async Task RefreshData()
        {
            if (!_categories.Any())
            {
                _categories.AddRange(await CategoryRepoService.AllAsync());
            }
        }

        private async Task<Transaction> PrepareTransaction(Trade trade, Transaction transaction)
        {
            if (!_categories.Any())
            {
                _categories.AddRange(await CategoryRepoService.AllAsync());
            }

            transaction ??= new Transaction();
            transaction.AccountId = trade.AccountId;
            transaction.TradeId = trade.TradeId;
            transaction.Date = trade.Date;
            transaction.Description = $"Trade {trade.Security.Name}: " +
                                      (trade.Action == TradeActionEnum.Buy ? "Bought" : "Sold") +
                                      $" {trade.Quantity:F0} @ {trade.Price:f4}";
            transaction.Category = _categories.Single(x =>
                x.Group == (trade.Action == TradeActionEnum.Buy
                    ? CategoryGroupEnum.TradeBuy
                    : CategoryGroupEnum.TradeSell));
            transaction.Notes = trade.Security.Name;
            transaction.Payment = trade.Action == TradeActionEnum.Buy ? trade.Value : null;
            transaction.Deposit = trade.Action == TradeActionEnum.Sell ? trade.Value : null;
            return transaction;
        }

        private async Task<bool> PrepareSecurityPosition(Trade trade)
        {
            var securityPosition =
                await SecurityPositionRepoService.DetailsForSecurityEnrichedAsync(trade.Security.SecurityId);

            if (trade.Action == TradeActionEnum.Buy)
            {
                if (securityPosition == null || securityPosition.Quantity == 0)
                {
                    securityPosition ??= new SecurityPosition {AccountId = trade.AccountId, Security = trade.Security};
                    securityPosition.Quantity = trade.Quantity;
                    securityPosition.Value = trade.Value;
                }
                else if (securityPosition.Quantity > 0)
                {
                    securityPosition.Quantity += trade.Quantity;
                    securityPosition.Value += trade.Value;
                }

                return await SecurityPositionRepoService.SaveAsync(securityPosition) == Result.Success;
            }

            if (securityPosition == null || securityPosition.Quantity == 0 ||
                securityPosition.Quantity < trade.Quantity)
            {
                return false;
            }

            var closedPosition = new ClosedPosition
            {
                AccountId = trade.AccountId,
                Security = trade.Security,
                Quantity = trade.Quantity,
                SellDate = trade.Date,
                BuyPrice = securityPosition.BreakEvenPrice,
                SellPrice = trade.BreakEvenPrice,
                ProfitLoss = trade.Value - trade.Quantity * securityPosition.BreakEvenPrice
            };

            securityPosition.Quantity -= trade.Quantity;
            securityPosition.Value -= trade.Value;

            return await ClosedPositionRepoService.SaveAsync(closedPosition) == Result.Success
                   && await SecurityPositionRepoService.SaveAsync(securityPosition) == Result.Success;
        }

        private async Task<bool> DeleteSecurityPosition(Trade trade)
        {
            var securityPosition =
                await SecurityPositionRepoService.DetailsForSecurityEnrichedAsync(trade.Security.SecurityId);

            if (trade.Action == TradeActionEnum.Buy)
            {
                securityPosition.Quantity -= trade.Quantity;
                securityPosition.Value -= trade.Value;

                return await SecurityPositionRepoService.SaveAsync(securityPosition) == Result.Success;
            }

            if (securityPosition == null || securityPosition.Quantity == 0)
            {
                securityPosition ??= new SecurityPosition {AccountId = trade.AccountId, Security = trade.Security};
                securityPosition.Quantity = trade.Quantity;
                securityPosition.Value = trade.Value;
            }
            else if (securityPosition.Quantity > 0)
            {
                securityPosition.Quantity += trade.Quantity;
                securityPosition.Value += trade.Value;
            }

            var closedPositionToDelete =
                (await ClosedPositionRepoService.AllEnrichedForSecurityAsync(trade.Security.SecurityId))
                .SingleOrDefault(x => x.Quantity == trade.Quantity && x.SellDate == trade.Date);

            if (closedPositionToDelete != null)
            {
                await ClosedPositionRepoService.DeleteAsync(closedPositionToDelete.ClosedPositionId);
            }

            return await SecurityPositionRepoService.SaveAsync(securityPosition) == Result.Success;
        }
    }
}