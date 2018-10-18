using System;
using System.Diagnostics;
using System.Windows;

namespace TextToTranslationList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window 
    {
        public MainWindow(ViewModel.MainWindowViewModel viewModel)
        {
            try
            {
                InitializeComponent();
                DataContext = viewModel;
            }
            catch (Exception ex) {
                Debugger.Break();
            }
            
        }

    }


}
