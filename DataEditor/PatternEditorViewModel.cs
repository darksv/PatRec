using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class PatternEditorViewModel
    {
        public PatternCollection Patterns { get; }

        public Pattern CurrentLetter { get; set; }

        public ICommand NewPatternCommand { get; }

        public ICommand SaveToFannCommand { get; }

        public ICommand SaveToXmlCommand { get; }

        public ICommand LoadFromXmlCommand { get; }

        public PatternEditorViewModel(PatternCollection patterns)
        {
            Patterns = patterns;

            NewPattern();
            NewPatternCommand = new RelayCommand(x => NewPattern());
            SaveToFannCommand = new RelayCommand(x => SaveToFann());
            SaveToXmlCommand = new RelayCommand(x => SaveToXml());
            LoadFromXmlCommand = new RelayCommand(x => LoadFromXml());
        }

        private void NewPattern()
        {
            Pattern pattern;
            if (Patterns.Any())
            {
                var lastPattern = Patterns.Last();

                pattern = new Pattern
                {
                    Rows = lastPattern.Rows,
                    Columns = lastPattern.Columns
                };
            }
            else
            {
                pattern = new Pattern();
            }

            pattern.Name = $"#{Patterns.Count()}";

            Patterns.Add(pattern);
            CurrentLetter = pattern;
        }

        private void SaveToFann()
        {
            var previous = Patterns.First();
            foreach (var letter in Patterns.Skip(1))
            {
                if (previous.Rows != letter.Rows || previous.Columns != letter.Columns)
                {
                    MessageBox.Show(
                        $"Rozmiary matryc {letter.Name} i {previous.Name} nie są zgodne.", "Błąd",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Plik formatu FANN|*.*",
                DefaultExt = "txt",
                AddExtension = true
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            Patterns.SaveToFann(dialog.FileName);
        }

        private void SaveToXml()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Plik XML|*.xml",
                DefaultExt = "xml",
                AddExtension = true
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            Patterns.SaveToXml(dialog.FileName);
        }

        private void LoadFromXml()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Plik XML|*.xml",
                DefaultExt = "xml",
                AddExtension = true
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            Patterns.LoadFromXml(dialog.FileName);
            CurrentLetter = Patterns.FirstOrDefault();
        }
    }
}
