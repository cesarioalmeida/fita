using System;
using fita.ui.Models;
using NUnit.Framework;

namespace fita.ui.tests.Models
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
        }

        [Test]
        public void SyncFrom_Populated_Syncs()
        {
            var other = new Account {Name = "OCBC", Currency = {Name = "SGD"}};

            _target.SyncFrom(other);
            
            Assert.AreEqual("OCBC", _target.Name);
            Assert.NotNull(_target.Currency);
            Assert.AreEqual("SGD", _target.Currency.Name);
            Assert.AreNotSame(_target.Currency, other.Currency);
        }
    }
}