using System;
using System.Linq;
using fita.core.Common;
using fita.core.Models;
using NUnit.Framework;

namespace fita.core.tests.Models
{
    public class AccountTests
    {
        private Account _target;

        [SetUp]
        public void Setup()
        {
            _target = new Account();
        }

        [Test]
        public void NewObject_Default_Populated()
        {
            var result = _target;

            Assert.IsFalse(result.IsDeleted);
            Assert.AreEqual(DateTime.MinValue, result.LastUpdated);
            Assert.IsTrue(string.IsNullOrEmpty(result.Name));
            Assert.NotNull(result.Currency);
            Assert.IsNotNull(result.Transactions);
        }

        [Test]
        public void SyncFrom_Populated_Syncs()
        {
            var other = new Account {Name = "OCBC", Currency = {Name = "SGD"}};
            var transaction = new Transaction();
            other.Transactions.Add(DateTime.Now, transaction);

            _target.SyncFrom(other);

            Assert.AreEqual("OCBC", _target.Name);
            Assert.NotNull(_target.Currency);
            Assert.AreEqual("SGD", _target.Currency.Name);
            Assert.AreNotSame(_target.Currency, other.Currency);
            Assert.AreEqual(1, _target.Transactions.Count);
            Assert.AreSame(transaction, _target.Transactions.First().Value);
        }

        [Test]
        public void Transactions_SortedByDate_AscSorted()
        {
            var transaction1 = new Transaction {Date = DateTime.Today, Amount = 10m};
            var transaction2 = new Transaction {Date = DateTime.Today.AddDays(-2), Amount = 3m};

            _target.Transactions.Add(transaction1.Date, transaction1);
            _target.Transactions.Add(transaction2.Date, transaction2);

            Assert.AreEqual(2, _target.Transactions.Count);
            Assert.AreEqual(DateTime.Today.AddDays(-2), _target.Transactions.Values.ElementAt(0).Date);
            Assert.AreEqual(3m, _target.Transactions.Values.ElementAt(0).Amount);
            Assert.AreEqual(DateTime.Today, _target.Transactions.Values.ElementAt(1).Date);
        }

        [Test]
        public void CurrentBalance_SameCategory_Sums()
        {
            var category1 = new Category {Group = CategoryGroupEnum.PersonalIncome};
            var category2 = new Category {Group = CategoryGroupEnum.PersonalIncome};

            var transaction1 = new Transaction {Date = DateTime.Today.AddDays(-2), Amount = 3m, Category = category1};
            var transaction2 = new Transaction {Date = DateTime.Today, Amount = 10m, Category = category2};

            _target.Transactions.Add(transaction1.Date, transaction1);
            _target.Transactions.Add(transaction2.Date, transaction2);
            
            Assert.AreEqual(13m, _target.CurrentBalance);
        }
        
        [Test]
        public void CurrentBalance_DiffCategoryBothIncome_Sums()
        {
            var category1 = new Category {Group = CategoryGroupEnum.BusinessExpenses};
            var category2 = new Category {Group = CategoryGroupEnum.PersonalExpenses};

            var transaction1 = new Transaction {Date = DateTime.Today.AddDays(-2), Amount = 3m, Category = category1};
            var transaction2 = new Transaction {Date = DateTime.Today, Amount = 10m, Category = category2};

            _target.Transactions.Add(transaction1.Date, transaction1);
            _target.Transactions.Add(transaction2.Date, transaction2);
            
            Assert.AreEqual(-13m, _target.CurrentBalance);
        }
        
        [Test]
        public void CurrentBalance_DiffCategory_Subtracts()
        {
            var category1 = new Category {Group = CategoryGroupEnum.PersonalIncome};
            var category2 = new Category {Group = CategoryGroupEnum.PersonalExpenses};

            var transaction1 = new Transaction {Date = DateTime.Today.AddDays(-2), Amount = 10m, Category = category1};
            var transaction2 = new Transaction {Date = DateTime.Today, Amount = 7m, Category = category2};

            _target.Transactions.Add(transaction1.Date, transaction1);
            _target.Transactions.Add(transaction2.Date, transaction2);
            
            Assert.AreEqual(3m, _target.CurrentBalance);
        }
        
        [Test]
        public void CurrentBalance_DiffCategoryCreditCard_SubtractsAndInverts()
        {
            _target.IsCreditCard = true;
            
            var category1 = new Category {Group = CategoryGroupEnum.PersonalIncome};
            var category2 = new Category {Group = CategoryGroupEnum.PersonalExpenses};

            var transaction1 = new Transaction {Date = DateTime.Today.AddDays(-2), Amount = 10m, Category = category1};
            var transaction2 = new Transaction {Date = DateTime.Today, Amount = 7m, Category = category2};

            _target.Transactions.Add(transaction1.Date, transaction1);
            _target.Transactions.Add(transaction2.Date, transaction2);
            
            Assert.AreEqual(-3m, _target.CurrentBalance);
        }
    }
}