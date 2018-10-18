using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TextToTranslationList
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                ViewModel.MainWindowViewModel viewModels = new ViewModel.MainWindowViewModel();
                Window window = new MainWindow(viewModels);
                window.Show();
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }


        }
    }
}
