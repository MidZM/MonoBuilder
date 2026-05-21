using MonoBuilder.Models.generics.enums;
using MonoBuilder.Views.ViewUtils;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using MonoBuilder.ViewModels.MainModel;

namespace MonoBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

			MainViewModel mainViewModel = new() { Owner = this };
			DataContext = mainViewModel;

			// Initialize the watcher after the fact so that the Owner has a chance to initialize properly.
			mainViewModel.InitializeWatcher();
		}
	}
}