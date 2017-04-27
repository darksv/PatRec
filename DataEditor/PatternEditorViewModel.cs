using System.Collections.Specialized;
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
        
        public PatternGroup[] PatternGroups { get; private set; }

        public class PatternGroup
        {
            public string Name { get; set; }

            public int NumberOfSamples { get; set; }
        }

        public string SaveAsName { get; set; }

        public double[,] Pixels { get; set; }

        public ICommand AddToTrainingSetCommand { get; }

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

            ((INotifyCollectionChanged) _patternContainer.Patterns).CollectionChanged += (sender, args) =>
            {
                PatternGroups = _patternContainer.Patterns
                    .GroupBy(x => x.Name)
                    .OrderBy(x => x.Key)
                    .Select(x => new PatternGroup {Name = x.Key, NumberOfSamples = x.Count()})
                    .ToArray();
            };
            
            AddToTrainingSetCommand = new RelayCommand(x => AddToTrainingSet());
            NewPatternCommand = new RelayCommand(x => NewPattern());
            SaveToFannCommand = new RelayCommand(x => {});
            SaveToXmlCommand = new RelayCommand(x => SaveToXml());
            LoadFromXmlCommand = new RelayCommand(x => LoadFromXml());
        }

        private void AddToTrainingSet()
        {
            if (string.IsNullOrEmpty(SaveAsName))
            {
                return;
            }

            var pattern = new Pattern
            {
                Name = SaveAsName,
                Rows = 15,
                Columns = 10
            };

            // 0 - black, 1 - white
            pattern.FillUsing(Pixels.Cast<double>().Select(x => x < 0.5).ToArray());
            _patternContainer.Add(pattern);
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
        }
    }
}
