using System;
using System.Windows;
using System.Windows.Threading;
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                new Action(InitializeAsyncSafe));
        }

        private async void InitializeAsyncSafe()
        {
            try
            {
                await _viewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось инициализировать окно", ex);
            }
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
