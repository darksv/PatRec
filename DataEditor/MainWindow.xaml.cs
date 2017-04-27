using System;
using System.Threading.Tasks;
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

        private async void PixelDrawer_OnPatternChanged(object sender, EventArgs e)
        {
            await Task.Run(() => _viewModel.NetworkTesting.Predict());
        }

        private void ClearPattern_OnClick(object sender, RoutedEventArgs e)
        {
            PatternInput.Clear();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            InputPattern.Clear();
        }
    }
}
