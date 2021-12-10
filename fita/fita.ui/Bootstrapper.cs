using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using fita.services.Repositories;
using fita.ui.Views;
using LightInject;
using twentySix.Framework.Core.Helpers;
using twentySix.Framework.Core.UI;
using twentySix.Framework.Core.UI.Interfaces;

namespace fita.ui
{
    public class Bootstrapper : LightInjectBootstrapper
    {
        public string ApplicationName { get; set; } = "fita";

        public Bootstrapper(Application app) : base(app)
        {
        }

        protected override void SetCulture()
        {
        }

        protected override void ConfigureApplication() 
            => ApplicationHelper.SetApplicationDetails("twentySix", ApplicationName);

        protected override IEnumerable<Assembly> GetRegistrationAssemblies()
        {
            yield return Assembly.LoadFile(ApplicationHelper.GetFullPath("fita.common.dll"));
            yield return Assembly.LoadFile(ApplicationHelper.GetFullPath("fita.data.dll"));
            yield return Assembly.LoadFile(ApplicationHelper.GetFullPath("fita.services.dll"));
            yield return GetType().Assembly;
        }

        protected override void ShowWindow()
        {
            foreach (var instance in Container.GetAllInstances<ISeedData>())
            {
                instance.SeedData();
            }

            Container.GetInstance<ShellView>().Show();
        }

        protected override void OnExit()
        {
            base.OnExit();

            foreach (var obj in Container.GetAllInstances<IDependsOnClose>())
            {
                obj.OnClose();
            }
        }
    }
}