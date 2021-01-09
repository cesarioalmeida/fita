﻿using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using DevExpress.Mvvm.UI;
using fita.ui.Views;
using LightInject;
using twentySix.Framework.Core.Helpers;
using twentySix.Framework.Core.UI;
using twentySix.Framework.Core.UI.Interfaces;

namespace fita.ui
{
    public class Bootstrapper : LightInjectBootstrapper
    {
        public Bootstrapper(Application app) : base(app)
        {
        }

        protected override void SetCulture()
        {
        }

        protected override void ConfigureApplication()
        {
            ApplicationHelper.SetApplicationDetails("twentySix", "fita");
        }

        protected override void ConfigureServiceContainer()
        {
            base.ConfigureServiceContainer();

            Container.RegisterAssembly("fita.core.dll");
            this.Container.RegisterFrom<Composition>();
        }

        protected override void ConfigureViewModelLocator()
        {
            //// devexpress
            //var callingAssemblyLocation = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            //var appFiles = Directory.EnumerateFiles(callingAssemblyLocation, "fita.core.dll")
            //    .Concat(Directory.EnumerateFiles(callingAssemblyLocation, "fita.ui.exe"));
            //var viewLocator = new ViewLocator(appFiles.Select(Assembly.LoadFile));
            //ViewLocator.Default = viewLocator;
        }

        protected override void ShowWindow()
        {
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