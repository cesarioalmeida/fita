using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using fita.services.Core;
using FluentAssertions;
using NUnit.Framework;

namespace fita.services.tests.Core
{
    [TestFixture]
    public class AccountServiceTests : ContainerFixture
    {
        [Import]
        private IAccountService _accountService;

        [Test]
        public async Task CalculateBalance_AccountNull_Returns0()
        {
            var result = await _accountService.CalculateBalance(null, null);
            
            result.Should().Be(0m);
        }

        [Test]
        public async Task CalculateBalance_AccountNoTransactions_ReturnsInitialBalance()
        {
            var account = new Account {InitialBalance = 10m};

            Assert.AreEqual(10m, await _accountService.CalculateBalance(account, null));
        }

        [Test]
        public async Task CalculateBalance_OnlyDepositTransactions_ReturnsBalance()
        {
            var account = new Account {InitialBalance = 10m};
            var transactions = new[]
            {
                new Transaction {Deposit = 2m},
                new Transaction {Deposit = 3m},
                new Transaction {Deposit = 10m},
            }.ToList();

            Assert.AreEqual(25m, await _accountService.CalculateBalance(account, transactions));
        }

        [Test]
        public async Task CalculateBalance_OnlyPaymentTransactions_ReturnsBalance()
        {
            var account = new Account {InitialBalance = 10m};
            var transactions = new[]
            {
                new Transaction {Payment = 2m},
                new Transaction {Payment = 3m},
                new Transaction {Payment = 3m},
            }.ToList();

            Assert.AreEqual(2m, await _accountService.CalculateBalance(account, transactions));
        }

        [Test]
        public async Task CalculateBalance_MixTransactions_ReturnsBalance()
        {
            var account = new Account {InitialBalance = 10m};
            var transactions = new[]
            {
                new Transaction {Deposit = 2m},
                new Transaction {Deposit = 3m},
                new Transaction {Payment = 10m},
            }.ToList();

            Assert.AreEqual(5m, await _accountService.CalculateBalance(account, transactions));
        }

        [Test]
        public async Task CalculateBalance_OutOfOrderTransactions_ReturnsBalance()
        {
            var account = new Account {InitialBalance = 10m};
            var transactions = new[]
            {
                new Transaction {Date = DateTime.Today.AddDays(-9), Deposit = 2m},
                new Transaction {Date = DateTime.Today.AddDays(-1), Deposit = 3m},
                new Transaction {Date = DateTime.Today.AddDays(-10), Payment = 10m},
            }.ToList();

            Assert.AreEqual(5m, await _accountService.CalculateBalance(account, transactions));
        }

        [Test]
        public async Task CalculateBalance_TransactionNull_ReturnsBalance()
        {
            Assert.AreEqual(10m, await _accountService.CalculateBalance(null, 10m));
        }

        [Test]
        public async Task CalculateBalance_DepositTransaction_ReturnsBalance()
        {
            var transaction = new Transaction {Deposit = 2m};
            Assert.AreEqual(12m, await _accountService.CalculateBalance(transaction, 10m));
        }
        
        [Test]
        public async Task CalculateBalance_PaymentTransaction_ReturnsBalance()
        {
            var transaction = new Transaction {Payment = 2m};
            Assert.AreEqual(8m, await _accountService.CalculateBalance(transaction, 10m));
        }
    }
}