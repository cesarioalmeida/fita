using System;
using fita.core.Common;
using fita.core.Models;
using LiteDB;
using NUnit.Framework;

namespace fita.core.tests.Models
{
    public class CategoryTests
    {
        private Category _targetDefault;
        private Category _targetPopulated;

        [SetUp]
        public void Setup()
        {
            _targetDefault = new Category();
            _targetPopulated = new Category {Group = CategoryGroupEnum.BusinessIncome, Name = "Salary"};
        }

        [Test]
        public void NewObject_Default_Populated()
        {
            var result = _targetDefault;

            Assert.IsFalse(result.IsDeleted);
            Assert.AreEqual(DateTime.MinValue, result.LastUpdated);
            Assert.IsTrue(string.IsNullOrEmpty(result.Name));
            Assert.IsTrue(string.IsNullOrEmpty(result.FullName));
            Assert.AreEqual(default(CategoryGroupEnum), result.Group);
            Assert.IsNull(result.Parent);
        }

        [Test]
        public void FullName_ParentAndChild_Populates()
        {
            var parent = new Category {Name = "Auto", Group = CategoryGroupEnum.PersonalExpenses};
            var child = new Category {Name = "Fuel", Group = CategoryGroupEnum.PersonalExpenses, Parent = parent};

            Assert.AreEqual("Auto", parent.FullName);
            Assert.AreEqual("Fuel:Auto", child.FullName);
        }

        [Test]
        public void GetDTO_Populated_ReturnsDTO()
        {
            var result = _targetPopulated.GetDTO();
            
            Assert.AreEqual(CategoryGroupEnum.BusinessIncome, result.Group);
            Assert.AreEqual("Salary", result.Name);
            Assert.IsNull(result.ParentId);
        }

        [Test]
        public void SyncFrom_ChildPopulated_Syncs()
        {
            var parent = new Category {Name = "Auto", Group = CategoryGroupEnum.PersonalExpenses};
            var child = new Category {Name = "Fuel", Group = CategoryGroupEnum.PersonalExpenses, Parent = parent};

            _targetDefault.SyncFrom(child);

            Assert.AreEqual("Fuel", _targetDefault.Name);
            Assert.AreEqual("Fuel:Auto", _targetDefault.FullName);
            Assert.AreEqual(child.Group, _targetDefault.Group);
            Assert.AreSame(child.Parent, _targetDefault.Parent);
        }
    }
}