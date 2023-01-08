using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using DryIoc;
using fita.ui.Views;
using twentySix.Framework.Core.Helpers;
using twentySix.Framework.Core.UI;
using twentySix.Framework.Core.UI.Interfaces;

namespace fita.ui
{
    public class Bootstrapper : DryIocBootstrapper
    {
        public Bootstrapper(Application app) : base(app, "fita")
        {
        }

        protected override void SetCulture()
        {
        }

        protected override IEnumerable<Assembly> GetRegistrationAssemblies()
        {
            yield return Assembly.LoadFrom(ApplicationHelper.GetFullPath("fita.data.dll"));
            yield return Assembly.LoadFrom(ApplicationHelper.GetFullPath("fita.services.dll"));
            yield return GetType().Assembly;
        }

        protected override IEnumerable<Assembly> GetAssembliesWithViews()
        {
            yield return GetType().Assembly;
        }

        protected override void ShowWindow()
        {
            foreach (var instance in Container.ResolveMany<ISeedData>())
            {
                instance.SeedData();
            }

            Container.Resolve<ShellView>().Show();
        }
    }
}