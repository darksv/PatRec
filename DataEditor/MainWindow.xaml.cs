using System.Windows;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();
        }
    }
}
