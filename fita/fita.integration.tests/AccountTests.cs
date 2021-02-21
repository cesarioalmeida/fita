using fita.ui;
using NUnit.Framework;
using twentySix.Framework.Core.UI;

namespace fita.integration.tests
{
    [TestFixture]
    public class AccountTests
    {
        private LightInject.IServiceContainer _bootstrapper;

        [OneTimeSetUp]
        public void Initialize()
        {
            //_bootstrapper = new Bootstrapper(new Application()) {ApplicationName = "fita.tests"};
            //_bootstrapper.Run();
        }

        [OneTimeTearDown]
        public void ClassCleanUp()
        {
            _bootstrapper = null;
        }

        [Test]
        public void AddMethodTest()
        {
        }
    }
}