using DNDinventory.View;
using DNDinventory.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DNDinventory
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Random Random;
        private SplashScreenWindow SplashScreen;
        private MainWindow mainWindow;
        private SplashScreenViewModel viewModel;
        IProgress<double> progress;
        double curProgress;

        public App()
        {
            Random = new Random();
            progress = new Progress<double>(MainViewModel_LoadProgressChanged);
            curProgress = 0.0;
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            //initialize the splash screen and set it as the application main 
            viewModel = new SplashScreenViewModel();
            SplashScreen = new SplashScreenWindow(viewModel);
            this.MainWindow = SplashScreen;
            SplashScreen.Show();

            //in order to ensure the UI stays responsive, we need to
            //do the work on a different thread
            SplashScreen.Dispatcher.Invoke(() => viewModel.Progress = 0);
            var buildMain = Task.Run(() =>
            {
                var mainViewModel = new MainViewModel();
                this.Dispatcher.Invoke(() =>
                  {
                      mainWindow = new MainWindow(mainViewModel);
                      this.MainWindow = mainWindow;
                      mainViewModel.SetupInv(progress);
                  });
            });

            var progressMonitor = Task.Run(() =>
            {
                while (curProgress < 100.0) { Thread.Sleep(50); };
            });

            await Task.WhenAll(buildMain, progressMonitor);

            this.Dispatcher.Invoke(() =>
            {
                //initialize the main window, set it as the application main window
                //and close the splash screen
                MainWindow.Show();
                SplashScreen.Close();
            });
        }

        private void MainViewModel_LoadProgressChanged(double value)
        {
            curProgress = value;
            SplashScreen.Dispatcher.Invoke(() => viewModel.Progress = value);
        }
    }
}
