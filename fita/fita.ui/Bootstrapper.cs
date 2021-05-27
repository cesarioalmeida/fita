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
        {
            ApplicationHelper.SetApplicationDetails("twentySix", ApplicationName);
        }

        protected override void ConfigureServiceContainer()
        {
            base.ConfigureServiceContainer();

            Container.RegisterAssembly("twentySix.Framework.*.dll");
            Container.RegisterAssembly("fita.common.dll");
            Container.RegisterAssembly("fita.data.dll");
            Container.RegisterAssembly("fita.services.dll");
            
            Container.RegisterFrom<Composition>();
        }

        protected override void ConfigureViewModelLocator()
        {
        }

        protected override void ShowWindow()
        {
            foreach (var instance in Container.GetAllInstances<ISeedData>())
            {
                instance.SeedData();
            }

            this.Container.GetInstance<ShellView>().Show();
        }

        protected override void OnExit()
        {
            base.OnExit();

            foreach (var obj in this.Container.GetAllInstances<IDependsOnClose>())
            {
                obj.OnClose();
            }
        }
    }
}