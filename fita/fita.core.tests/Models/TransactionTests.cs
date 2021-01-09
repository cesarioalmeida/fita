using System;
using System.Linq;
using fita.core.Common;
using fita.core.Models;
using NUnit.Framework;

namespace fita.core.tests.Models
{
    public class TransactionTests
    {
        private Transaction _targetDefault;
        private Transaction _targetPopulated;

        [SetUp]
        public void Setup()
        {
            _targetDefault = new Transaction();
            _targetPopulated = new Transaction
            {
                Date = DateTime.Today.AddDays(-1), Payee = "Pingo Doce", Tags = new[] {"supermarket", "shopping"},
                Category = new Category {Group = CategoryGroupEnum.PersonalExpenses, Name = "Groceries"}, Amount = 26.32m
            };
        }

        [Test]
        public void NewObject_Default_Populated()
        {
            var result = _targetDefault;

            Assert.IsFalse(result.IsDeleted);
            Assert.AreEqual(DateTime.MinValue, result.LastUpdated);
            Assert.AreEqual(DateTime.Today, result.Date);
            Assert.IsTrue(string.IsNullOrEmpty(result.Payee));
            Assert.IsTrue(string.IsNullOrEmpty(result.Memo));
            Assert.IsNull(result.Tags);
            Assert.AreEqual(default(CategoryGroupEnum), result.Category.Group);
        }

        [Test]
        public void GetDTO_Populated_ReturnsDTO()
        {
            var result = _targetPopulated.GetDTO();
            
            Assert.AreEqual(DateTime.Today.AddDays(-1), result.Date);
            Assert.AreEqual("Pingo Doce", result.Payee);
            Assert.IsTrue(result.Tags.Contains("shopping"));
            Assert.AreEqual(26.32m, result.Amount);
            Assert.AreEqual(_targetPopulated.Category.Id, result.CategoryId);
        }
        
        
        [Test]
        public void SyncFrom_Populated_Syncs()
        {
            var tags = new[] {"test"};
            var other = new Transaction
            {
                Date = DateTime.Today.AddDays(1), Payee = "Me",
                Category = new Category {Group = CategoryGroupEnum.TransfersIn}, Tags = tags, Amount = 0.3m
            };
        
            _targetPopulated.SyncFrom(other);
            
            Assert.AreEqual(DateTime.Today.AddDays(1), _targetPopulated.Date);
            Assert.AreEqual("Me", _targetPopulated.Payee);
            Assert.IsTrue(_targetPopulated.Tags.Contains("test"));
            Assert.AreNotSame(tags, _targetPopulated.Tags);
            Assert.AreEqual(CategoryGroupEnum.TransfersIn, _targetPopulated.Category.Group);
            Assert.AreEqual(0.3m, _targetPopulated.Amount);
        }
    }
}