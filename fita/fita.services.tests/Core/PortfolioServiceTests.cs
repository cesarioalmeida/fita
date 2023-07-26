using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using FluentAssertions;
using NUnit.Framework;
using twentySix.Framework.Core.Services;

namespace fita.services.tests.Core;

[TestFixture]
public class PortfolioServiceTests : ContainerFixture
{
    private Account _account;
    private Security _security;

    [Import] private IPortfolioService _portfolioService;
    [Import] private AccountRepoService _accountRepoService;
    [Import] private ISecurityService _securityService;
    [Import] private CategoryRepoService _categoryRepoService;
    [Import] private SecurityRepoService _securityRepoService;
    [Import] private SecurityPositionRepoService _securityPositionRepoService;
    [Import] private TradeRepoService _tradeRepoService;
    [Import] private ClosedPositionRepoService _closedPositionRepoService;
    [Import] private DBHelperServiceFactory _dbHelperServiceFactory;
    [Import] private TransactionRepoService _transactionRepoService;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _categoryRepoService?.SeedData();

        _account = new Account {InitialBalance = 1000m};
        _security = new Security {Symbol = "AAPL"};
        var securityHistory = new SecurityHistory {Security = _security};
        await _accountRepoService.Save(_account);
        await _securityRepoService.Save(_security);
        await _securityService.Update(securityHistory);
    }
    
    [OneTimeTearDown]
    public void TearDown()
    {
        _dbHelperServiceFactory?.GetInstance().Dispose();
    }

    [SetUp]
    public async Task TestSetup()
    {
        var trades = await _tradeRepoService.GetAll();
        foreach (var trade in trades)
        {
            await _tradeRepoService.Delete(trade.TradeId);
        }
        
        var securityPositions = await _securityPositionRepoService.GetAll();
        foreach (var securityPosition in securityPositions)
        {
            await _securityPositionRepoService.Delete(securityPosition.SecurityPositionId);
        }
        
        var closedPositions = await _closedPositionRepoService.GetAll();
        foreach (var closedPosition in closedPositions)
        {
            await _closedPositionRepoService.Delete(closedPosition.ClosedPositionId);
        }
        
        var transactions = await _transactionRepoService.GetAll();
        foreach (var transaction in transactions)
        {
            await _transactionRepoService.Delete(transaction.TransactionId);
        }
    }

    [Test]
    public async Task IsTradePossible_TradeNull_ReturnsFalse()
    {
        var result = await _portfolioService.IsTradePossible(null);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsTradePossible_TradeSecurityNull_ReturnsFalse()
    {
        var trade = new Trade {AccountId = _account.AccountId};

        var result = await _portfolioService.IsTradePossible(trade);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsTradePossible_TradeAccountIdEmpty_ReturnsFalse()
    {
        var trade = new Trade {Security = _security};

        var result = await _portfolioService.IsTradePossible(trade);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsTradePossible_TradeQuantityZero_ReturnsFalse()
    {
        var trade = new Trade {AccountId = _account.AccountId, Security = _security, Quantity = 0};

        var result = await _portfolioService.IsTradePossible(trade);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsTradePossible_TradeActionBuy_ReturnsTrue()
    {
        var trade = new Trade
            {AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Buy, Quantity = 1m};

        var result = await _portfolioService.IsTradePossible(trade);

        result.Should().BeTrue();
    }

    [Test]
    public async Task IsTradePossible_TradeActionSellNoPosition_ReturnsFalse()
    {
        var trade = new Trade {AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Sell};

        var result = await _portfolioService.IsTradePossible(trade);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsTradePossible_TradeActionSellPositionQuantityBelowPortfolioPosition_ReturnsTrue()
    {
        var buyTrade = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Buy, Quantity = 2m,
            Value = 100m, Price = 0.5m
        };
        var transaction = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};
        await _portfolioService.ProcessTrade(buyTrade, transaction);

        var trade = new Trade
            {AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Sell, Quantity = 1m};

        var result = await _portfolioService.IsTradePossible(trade);

        result.Should().BeTrue();
    }
    
    [Test]
    public async Task IsTradePossible_TradeActionSellPositionQuantityHigherPortfolioPosition_ReturnsFalse()
    {
        var buyTrade = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Buy, Quantity = 2m,
            Value = 100m, Price = 0.5m
        };
        var transaction = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};
        await _portfolioService.ProcessTrade(buyTrade, transaction);

        var trade = new Trade
            {AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Sell, Quantity = 3m};

        var result = await _portfolioService.IsTradePossible(trade);

        result.Should().BeFalse();
    }
    
    [Test]
    public async Task IsTradePossible_TradeActionSellPositionQuantityEqualToPortfolioPosition_ReturnsTrue()
    {
        var buyTrade = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Buy, Quantity = 2m,
            Value = 100m, Price = 0.5m
        };
        var transaction = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};
        await _portfolioService.ProcessTrade(buyTrade, transaction);

        var trade = new Trade
            {AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Sell, Quantity = 2m};

        var result = await _portfolioService.IsTradePossible(trade);

        result.Should().BeTrue();
    }
    
    [Test]
    public async Task ProcessTrade_TradeNull_ReturnsFalse()
    {
        var result = await _portfolioService.ProcessTrade(null, null);

        result.Should().BeFalse();
    }

    [Test]
    public async Task OneBuyTrade_CalculateCorrectPosition()
    {
        var buyTrade = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Buy, Quantity = 4m,
            Value = 8m, Price = 2m
        };
        var transaction = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};
        
        var result = await _portfolioService.ProcessTrade(buyTrade, transaction);
        var positions = (await _securityPositionRepoService.GetAllForAccount(_account.AccountId)).ToList();
        var closedPositions = (await _closedPositionRepoService.GetAllForSecurity(_security.SecurityId)).ToList();
        
        result.Should().BeTrue();
        positions.Should().HaveCount(1);
        positions[0].Quantity.Should().Be(4m);
        positions[0].Value.Should().Be(8m);
        positions[0].BreakEvenPrice.Should().Be(2m);
        closedPositions.Should().HaveCount(0);
    }
    
    [Test]
    public async Task TwoBuyTrades_CalculateCorrectPosition()
    {
        var buyTrade1 = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Buy, Quantity = 3m,
            Value = 6m, Price = 2m
        };
        var buyTrade2 = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Buy, Quantity = 1m,
            Value = 3m, Price = 3m
        };
        var transaction1 = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};
        var transaction2 = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};
        
        var result1 = await _portfolioService.ProcessTrade(buyTrade1, transaction1);
        var result2 = await _portfolioService.ProcessTrade(buyTrade2, transaction2);
        
        var positions = (await _securityPositionRepoService.GetAllForAccount(_account.AccountId)).ToList();
        var closedPositions = (await _closedPositionRepoService.GetAllForSecurity(_security.SecurityId)).ToList();
        
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        positions.Should().HaveCount(1);
        positions[0].Quantity.Should().Be(4m);
        positions[0].Value.Should().Be(9m);
        positions[0].BreakEvenPrice.Should().Be(2.25m);
        closedPositions.Should().HaveCount(0);
    }
    
    [Test]
    public async Task SellAllOneTrade_CalculateCorrectPosition()
    {
        var buyTrade1 = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Buy, Quantity = 4m,
            Value = 8m, Price = 2m
        };
        
        var sellTrade1 = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Sell, Quantity = 4m,
            Value = 12m, Price = 3m
        };

        var transaction1 = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};
        var transaction2 = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};

        var result1 = await _portfolioService.ProcessTrade(buyTrade1, transaction1);
        var result2 = await _portfolioService.ProcessTrade(sellTrade1, transaction2);

        var positions = (await _securityPositionRepoService.GetAllForAccount(_account.AccountId)).ToList();
        var closedPositions = (await _closedPositionRepoService.GetAllForSecurity(_security.SecurityId)).ToList();
        
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        positions.Should().HaveCount(0);
        closedPositions.Should().HaveCount(1);
        closedPositions[0].Quantity.Should().Be(4m);
        closedPositions[0].ProfitLoss.Should().Be(4m);
    }
    
    [Test]
    public async Task SellPartialOneTrade_CalculateCorrectPositionAndPL()
    {
        var buyTrade1 = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Buy, Quantity = 4m,
            Value = 8m, Price = 2m
        };
        
        var sellTrade1 = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Sell, Quantity = 1m,
            Value = 3m, Price = 3m
        };

        var transaction1 = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};
        var transaction2 = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};

        var result1 = await _portfolioService.ProcessTrade(buyTrade1, transaction1);
        var result2 = await _portfolioService.ProcessTrade(sellTrade1, transaction2);

        var positions = (await _securityPositionRepoService.GetAllForAccount(_account.AccountId)).ToList();
        var closedPositions = (await _closedPositionRepoService.GetAllForSecurity(_security.SecurityId)).ToList();
        
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        positions.Should().HaveCount(1);
        positions[0].Quantity.Should().Be(3);
        positions[0].Value.Should().Be(6);
        positions[0].BreakEvenPrice.Should().Be(2);
        closedPositions.Should().HaveCount(1);
        closedPositions[0].Quantity.Should().Be(1);
        closedPositions[0].ProfitLoss.Should().Be(1);
    }
    
    [Test]
    public async Task SellTwoSalesTrade_CalculateCorrectPositionAndPL()
    {
        var buyTrade1 = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Buy, Quantity = 4m,
            Value = 8m, Price = 2m
        };
        
        var sellTrade1 = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Sell, Quantity = 1m,
            Value = 3m, Price = 3m
        };
        
        var sellTrade2 = new Trade
        {
            AccountId = _account.AccountId, Security = _security, Action = TradeActionEnum.Sell, Quantity = 3m,
            Value = 6m, Price = 2m
        };

        var transaction1 = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};
        var transaction2 = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};
        var transaction3 = new Transaction {AccountId = _account.AccountId, Date = DateTime.Now};

        var result1 = await _portfolioService.ProcessTrade(buyTrade1, transaction1);
        var result2 = await _portfolioService.ProcessTrade(sellTrade1, transaction2);
        var result3 = await _portfolioService.ProcessTrade(sellTrade2, transaction3);

        var positions = (await _securityPositionRepoService.GetAllForAccount(_account.AccountId)).ToList();
        var closedPositions = (await _closedPositionRepoService.GetAllForSecurity(_security.SecurityId)).ToList();
        
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
        positions.Should().HaveCount(0);
        closedPositions.Should().HaveCount(2);
        closedPositions[0].Quantity.Should().Be(1);
        closedPositions[0].ProfitLoss.Should().Be(1);
        closedPositions[1].Quantity.Should().Be(3);
        closedPositions[1].ProfitLoss.Should().Be(0);
    }
}