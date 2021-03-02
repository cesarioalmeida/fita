using System;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;

namespace fita.ui
{
    public partial class App
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SplashScreenManager.CreateFluent(new DXSplashScreenViewModel
            {
                Copyright = "Cesario Almeida",
                IsIndeterminate = true,
                Status = "starting ...",
                Title = "fita",
                Subtitle = "financial manager", 
                Logo = new Uri(@"pack://application:,,,/fita.ui;component/fita.ico")
            }).ShowOnStartup();

            var bootstrapper = new Bootstrapper(this);
            bootstrapper.Run();
        }
    }
}