﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using fita.data.Enums;
using fita.data.Models;
using fita.services;
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.Services;
using fita.ui.Views.Transactions;
using JetBrains.Annotations;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Transactions
{
    [POCOViewModel]
    public class TransactionsViewModel : ComposedViewModelBase
    {
        private List<Transaction> _transactions { get; set; }

        public virtual Account Account { get; set; }

        public LockableCollection<EntityModel> Data { get; set; } = new();

        public AccountRepoService AccountRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }

        public TradeRepoService TradeRepoService { get; set; }

        public IAccountService AccountService { get; set; }
        
        public IPortfolioService PortfolioService { get; set; }
        
        protected IDocumentManagerService ModalDocumentManagerService => GetService<IDocumentManagerService>("ModalWindowDocumentService", ServiceSearchMode.PreferParents);

        public async Task RefreshData()
        {
            IsBusy = true;

            try
            {
                if (Account is null)
                {
                    return;
                }
                
                Account = await AccountRepoService.DetailsEnrichedAsync(Account.AccountId);
                if (Account is null)
                {
                    return;
                }

                _transactions = (await TransactionRepoService.AllEnrichedForAccountAsync(Account?.AccountId)).ToList();

                Data.BeginUpdate();
                Data.Clear();

                Data.Add(EntityModel.GetInitialBalance(Account));

                var balance = Account.InitialBalance;

                foreach (var transaction in _transactions)
                {
                    balance = await AccountService.CalculateBalance(transaction, balance);
                    Data.Add(new EntityModel(Account, transaction, balance));
                }
                
                Data.EndUpdate();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        public async Task Edit(EntityModel model)
        {
            var detailsSaved = model.Entity.Category.Group is CategoryGroupEnum.TransfersIn or CategoryGroupEnum.TransfersOut
                ? EditTransfer(model)
                : model.Entity.Category.Group is CategoryGroupEnum.TradeBuy or CategoryGroupEnum.TradeSell
                    ? await EditTrade(model)
                    : EditTransaction(model);

            if (detailsSaved)
            {
                await RefreshData();
            }
        }

        [UsedImplicitly]
        public bool CanEdit(EntityModel model)
        {
            return model?.Entity.Category is not null && model.Entity.TradeId is null;
        }

        [UsedImplicitly]
        public async Task NewTransaction()
        {
            var detailsSaved = EditTransaction(null);

            if (detailsSaved)
            {
                await RefreshData();
            }
        }

        [UsedImplicitly]
        public async Task NewTransfer()
        {
            var detailsSaved = EditTransfer(null);

            if (detailsSaved)
            {
                await RefreshData();
            }
        }

        [UsedImplicitly]
        public async Task Delete(EntityModel model)
        {
            if (model is null)
            {
                return;
            }

            if (WinUIMessageBox.Show(
                $"Are you sure you want to delete the transaction {model.Entity}?",
                "Delete transaction",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            IsBusy = true;

            try
            {
                if (model.Entity.TransferTransactionId is not null)
                {
                    await TransactionRepoService.DeleteAsync(model.Entity.TransferTransactionId);
                }

                if (model.Entity.TradeId is not null)
                {
                    await PortfolioService.DeleteTrade(model.Entity.TradeId);
                }

                Messenger.Default.Send(
                    await TransactionRepoService.DeleteAsync(model.Entity.TransactionId) == Result.Fail
                        ? new NotificationMessage("Failed to delete transaction.", NotificationStatusEnum.Error)
                        : new NotificationMessage($"Transaction {model.Entity} deleted.",
                            NotificationStatusEnum.Success));

                await RefreshData();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        public bool CanDelete(EntityModel model) => model?.Entity.Category is not null;

        [UsedImplicitly]
        public async Task Trade(TradeActionEnum action)
        {
            var viewModel = ViewModelSource.Create<TradeDetailsViewModel>();
            viewModel.Trade = new Trade
            {
                Action = action,
                AccountId = Account.AccountId
            };
            viewModel.Account = Account;

            var document = ModalDocumentManagerService.CreateDocument(nameof(TradeDetailsView), viewModel, null, this);
            document.DestroyOnClose = true;
            document.Show();

            if (viewModel.Saved)
            {
                await RefreshData();
            }
        }
        
        [UsedImplicitly]
        public void NavigateTo() => NavigationService?.Navigate("PortfolioView", Account, this);

        protected override async void OnNavigatedTo()
        {
            Account = Parameter as Account;

            await RefreshData();
        }

        private async Task<bool> EditTrade(EntityModel model)
        {
            var viewModel = ViewModelSource.Create<TradeDetailsViewModel>();
            viewModel.Trade = await TradeRepoService.DetailsEnrichedAsync(model.Entity.TradeId);
            viewModel.Account = Account;
            viewModel.Transaction = model.Entity;

            var document = ModalDocumentManagerService.CreateDocument(nameof(TradeDetailsView), viewModel, null, this);

            document.DestroyOnClose = true;
            document.Show();

            return viewModel.Saved;
        }

        private bool EditTransfer(EntityModel model)
        {
            var viewModel = ViewModelSource.Create<TransferDetailsViewModel>();
            viewModel.Transaction = model?.Entity ?? new Transaction {AccountId = Account.AccountId};
            viewModel.Account = Account;

            var document = ModalDocumentManagerService.CreateDocument(nameof(TransferDetailsView), viewModel, null, this);

            document.DestroyOnClose = true;
            document.Show();

            return viewModel.Saved;
        }

        private bool EditTransaction(EntityModel model)
        {
            var viewModel = ViewModelSource.Create<TransactionDetailsViewModel>();
            viewModel.Entity = model?.Entity ?? new Transaction {AccountId = Account.AccountId};
            viewModel.Account = Account;

            var document = ModalDocumentManagerService.CreateDocument(nameof(TransactionDetailsView), viewModel, null, this);

            document.DestroyOnClose = true;
            document.Show();

            return viewModel.Saved;
        }

        [UsedImplicitly]
        public record EntityModel(Account Account, Transaction Entity, decimal? Balance)
        {
            public static EntityModel GetInitialBalance(Account account)
            {
                return new EntityModel(account, new Transaction
                    {
                        AccountId = account.AccountId, Description = "Initial balance", Deposit = account.InitialBalance
                    },
                    account.InitialBalance);
            }
        }
    }
}