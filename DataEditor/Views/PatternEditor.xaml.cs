using System.Windows;
using System.Windows.Controls;

namespace DataEditor.Views
{
    public partial class PatternEditor : UserControl
    {
        public PatternEditor()
        {
            InitializeComponent();
        }

        private void ClearPattern(object sender, RoutedEventArgs args)
        {
            PatternInput.Clear();
        }
    }
}
