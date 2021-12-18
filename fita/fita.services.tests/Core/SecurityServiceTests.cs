using System.Threading.Tasks;
using fita.data.Models;
using fita.services.Core;
using JetBrains.Annotations;
using NUnit.Framework;

namespace fita.services.tests.Core
{
    [TestFixture]
    public class SecurityServiceTests : ContainerFixture
    {
        [UsedImplicitly]
        private ISecurityService _securityService;

        [Test]
        public async Task UpdateAsync_SecurityHistoryNull_ReturnsFail()
        {
            Assert.AreEqual(Result.Fail, await _securityService.Update(null));
        }

        [Test]
        public async Task UpdateAsync_SecuritySymbolNull_ReturnsFail()
        {
            var securityHistory = new SecurityHistory { Security = new() };

            Assert.AreEqual(Result.Fail, await _securityService.Update(securityHistory));
        }

        [Test]
        public async Task UpdateAsync_SecuritySymbolNotValid_ReturnsFail()
        {
            var securityHistory = new SecurityHistory
            {
                Security = new Security { Symbol = "ttttt" }
            };

            Assert.AreEqual(Result.Fail, await _securityService.Update(securityHistory));
        }

        [Test]
        public async Task UpdateAsync_SecuritySymbolValid_ReturnsData()
        {
            var securityHistory = new SecurityHistory
            {
                Security = new Security { Symbol = "AAPL" }
            };

            var result = await _securityService.Update(securityHistory);

            Assert.AreEqual(Result.Success, result);
            Assert.NotNull(securityHistory.Price.DataPoints);
            Assert.IsTrue(securityHistory.Price.DataPoints.Count > 0);
            Assert.NotNull(securityHistory.Price.LatestDate);
            Assert.NotNull(securityHistory.Price.LatestValue);
        }
    }
}