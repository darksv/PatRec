using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DataEditor
{
    public partial class SavePatternAsDialog : Window
    {
        public SavePatternAsDialog()
        {
            InitializeComponent();

            LayoutRoot.DataContext = this;

            ResponseTextBox.Focus();
        }

        public static readonly DependencyProperty ResponseTextProperty = DependencyProperty.Register(
            "ResponseText",
            typeof(string),
            typeof(SavePatternAsDialog),
            new PropertyMetadata());

        public string ResponseText
        {
            get => (string) GetValue(ResponseTextProperty);
            set => SetValue(ResponseTextProperty, value);
        }
       
        public ICommand OkCommand => new RelayCommand(() =>
        {
            DialogResult = true;
        });

        public ICommand CancelCommand => new RelayCommand(() =>
        {
            DialogResult = false;
        });

        private void ResponseTextBox_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }
    }
}
