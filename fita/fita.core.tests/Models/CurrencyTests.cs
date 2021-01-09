using System;
using fita.core.Models;
using NUnit.Framework;

namespace fita.core.tests.Models
{
    public class CurrencyTests
    {
        private Currency _target;
        
        [SetUp]
        public void Setup()
        {
            _target = new Currency();
        }

        [Test]
        public void NewObject_Default_Populated()
        {
            var result = _target;
            
            Assert.IsFalse(result.IsDeleted);
            Assert.AreEqual(DateTime.MinValue, result.LastUpdated);
            Assert.AreEqual("Euro", result.Name);
            Assert.AreEqual("€", result.Symbol);
            Assert.NotNull(result.ExchangeData);
            CollectionAssert.IsEmpty(result.ExchangeData.Data);
        }

        [Test]
        public void SyncFrom_Populated_Syncs()
        {
            var other = new Currency {Name = "SGD", Symbol = "S$"};
            other.ExchangeData.AddOrUpdate(DateTime.Today, 0.1m);
            
            _target.SyncFrom(other);
            
            Assert.AreEqual("SGD", _target.Name);
            Assert.AreEqual("S$", _target.Symbol);
            Assert.NotNull(_target.ExchangeData);
            CollectionAssert.IsNotEmpty(_target.ExchangeData.Data);
            Assert.AreNotSame(_target.ExchangeData.Data, other.ExchangeData.Data);
        }

        [Test]
        public void CurrentExchangeRate_NoHistorialData_Returns1()
        {
            Assert.AreEqual(1m, _target.CurrentExchangeRate);
        }
        
        [Test]
        public void CurrentExchangeRate_WithHistorialData_ReturnsLastest()
        {
            _target.ExchangeData.AddOrUpdate(DateTime.Today.AddDays(-2), 0.3m);
            _target.ExchangeData.AddOrUpdate(DateTime.Today, 0.1m);
            _target.ExchangeData.AddOrUpdate(DateTime.Today.AddDays(-1), 0.5m);

            Assert.AreEqual(0.1m, _target.CurrentExchangeRate);
        }
    }
}