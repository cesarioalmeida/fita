using System;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;

namespace ddeploy.ui
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SplashScreenManager.CreateFluent(new DXSplashScreenViewModel
            {
                Copyright = string.Empty,
                IsIndeterminate = true,
                Status = "starting ...",
                Title = "ddeploy",
                Subtitle = "powered by twentySix.Framework 3.0", 
                Logo = new Uri(@"pack://application:,,,/ddeploy.ui;component/ddeploy.ico")
            }).ShowOnStartup();

            var bootstrapper = new Bootstrapper(this);
            bootstrapper.Run();
        }
    }
}