using System;
using System.Windows;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _viewModel;
        }

        private void PixelDrawer_OnPatternChanged(object sender, EventArgs e)
        {
            _viewModel.NetworkTesting.Predict();
        }
    }
}
