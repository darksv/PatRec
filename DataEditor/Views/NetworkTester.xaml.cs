using System.Windows;
using System.Windows.Controls;

namespace DataEditor.Views
{
    public partial class NetworkTester : UserControl
    {
        public NetworkTester()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs args)
        {
            InputPattern.Clear();
        }
    }
}
