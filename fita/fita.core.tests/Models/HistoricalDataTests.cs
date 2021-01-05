using System;
using System.Collections.Generic;
using System.Linq;
using fita.core.Models;
using LiteDB;
using NUnit.Framework;

namespace fita.core.tests.Models
{
    public class HistoricalDataTests
    {
        private HistoricalData _defaultObj;
        private HistoricalData _populatedObj;
        
        [SetUp]
        public void Setup()
        {
            _defaultObj = new HistoricalData();
            _populatedObj = new HistoricalData();
            _populatedObj.AddOrUpdate(DateTime.Today, 0.1m);
            _populatedObj.AddOrUpdate(DateTime.Today.AddDays(-1), 0.2m);
            _populatedObj.AddOrUpdate(DateTime.Today.AddDays(-3), 0.4m);
        }

        [Test]
        public void GetDTO_Default_ReturnsDTO()
        {
            var result = _defaultObj.GetDTO();
            
            Assert.AreNotEqual(ObjectId.Empty, result.Id);
            Assert.IsFalse(result.IsDeleted);
            Assert.AreEqual(DateTime.MinValue, result.LastUpdated);
            CollectionAssert.IsEmpty(result.Data);
        }
        
        [Test]
        public void GetDTO_Populated_ReturnsDTO()
        {
            var result = _populatedObj.GetDTO();
            
            CollectionAssert.IsNotEmpty(result.Data);
            Assert.AreNotSame(_populatedObj.Data, result.Data);
            Assert.AreEqual(3, result.Data.Count);
            Assert.AreEqual(DateTime.Today, result.Data.First().Key);
            Assert.AreEqual(0.1m, result.Data.First().Value);
        }
        
        [Test]
        public void AddOrUpdate_AddsSingleItem_AddsAndSorts()
        {
            _populatedObj.AddOrUpdate(DateTime.Today.AddDays(-2), 0.3m);
            
            Assert.AreEqual(4, _populatedObj.Data.Count);
            Assert.AreEqual(DateTime.Today, _populatedObj.Data.First().Key);
            Assert.AreEqual(DateTime.Today.AddDays(-2), _populatedObj.Data.ElementAt(2).Key);
        }
        
        [Test]
        public void AddOrUpdate_UpdateSingleItem_AddsAndSorts()
        {
            _populatedObj.AddOrUpdate(DateTime.Today, 0.9m);
            
            Assert.AreEqual(3, _populatedObj.Data.Count);
            Assert.AreEqual(DateTime.Today, _populatedObj.Data.First().Key);
            Assert.AreEqual(0.9m, _populatedObj.Data.First().Value);
        }
        
        [Test]
        public void AddOrUpdate_AddsDictionary_AddsAndSorts()
        {
            var other = new Dictionary<DateTime, decimal>
            {
                {DateTime.Today, 1.1m},
                {DateTime.Today.AddDays(-2), 0.3m},
                {DateTime.Today.AddDays(1), 100m},
            };
            
            _populatedObj.AddOrUpdate(other);
            
            Assert.AreEqual(5, _populatedObj.Data.Count);
            Assert.AreEqual(DateTime.Today.AddDays(1), _populatedObj.Data.First().Key);
            Assert.AreEqual(DateTime.Today.AddDays(-1), _populatedObj.Data.ElementAt(2).Key);
            Assert.AreEqual(1.1m, _populatedObj.Data.ElementAt(1).Value);
        }
    }
}