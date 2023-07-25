using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using FluentAssertions;
using NUnit.Framework;

namespace fita.services.tests.Core;

[TestFixture]
public class PortfolioServiceTests : ContainerFixture
{
    private Account _account;
    private Security _security;

    [Import] private IPortfolioService _portfolioService;
    [Import] private ISecurityService _securityService;
    [Import] private CategoryRepoService _categoryRepoService;
    [Import] private SecurityRepoService _securityRepoService;
    [Import] private SecurityPositionRepoService _securityPositionRepoService;
    [Import] private TradeRepoService _tradeRepoService;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _categoryRepoService?.SeedData();

        _account = new Account {InitialBalance = 1000m};
        _security = new Security {Symbol = "AAPL"};
        var securityHistory = new SecurityHistory {Security = _security};
        await _securityService.Update(securityHistory);
    }
    
    [SetUp]
    public async Task TestSetup()
    {
        await _securityRepoService.Delete(_security.SecurityId);
        
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
}