using System.Windows;
using RevitNumbering.Revit.ViewModels;

namespace RevitNumbering.Revit.Views
{
    public partial class NumberingWindow : Window
    {
        private readonly NumberingViewModel _viewModel;

        public NumberingWindow()
        {
            InitializeComponent();

            _viewModel = new NumberingViewModel();
            DataContext = _viewModel;

            Loaded += OnLoaded;
            Closing += OnClosing;
            Closed += OnClosed;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitializeAsync();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.SaveOnClose();
        }

        private void OnClosed(object sender, System.EventArgs e)
        {
            _viewModel.Dispose();
        }
    }
}
