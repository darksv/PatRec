using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class PatternEditorViewModel
    {
        private readonly PatternContainer _patternContainer;

        public CollectionView Patterns { get; }

        public Pattern CurrentLetter { get; set; }

        public ICommand NewPatternCommand { get; }

        public ICommand SaveToFannCommand { get; }

        public ICommand SaveToXmlCommand { get; }

        public ICommand LoadFromXmlCommand { get; }

        public PatternEditorViewModel(PatternContainer patternContainer)
        {
            _patternContainer = patternContainer;

            var viewSource = new CollectionViewSource
            {
                Source = _patternContainer.Patterns,
                SortDescriptions =
                {
                    new SortDescription(nameof(Pattern.Name), ListSortDirection.Ascending)
                },
                IsLiveSortingRequested = true
            };

            Patterns = (CollectionView) viewSource.View;
            
            NewPatternCommand = new RelayCommand(x => NewPattern());
            SaveToFannCommand = new RelayCommand(x => SaveToFann());
            SaveToXmlCommand = new RelayCommand(x => SaveToXml());
            LoadFromXmlCommand = new RelayCommand(x => LoadFromXml());
        }

        private void NewPattern()
        {
            Pattern pattern;
            if (_patternContainer.Patterns.Any())
            {
                var lastPattern = _patternContainer.Patterns.Last();

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

            pattern.Name = $"#{_patternContainer.Patterns.Count()}";

            _patternContainer.Add(pattern);
            CurrentLetter = pattern;
        }

        private void SaveToFann()
        {
            var previous = _patternContainer.Patterns.First();
            foreach (var letter in _patternContainer.Patterns.Skip(1))
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

            _patternContainer.SaveToFann(dialog.FileName);
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

            _patternContainer.SaveToXml(dialog.FileName);
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

            _patternContainer.LoadFromXml(dialog.FileName);
            CurrentLetter = _patternContainer.Patterns.FirstOrDefault();
        }
    }
}
