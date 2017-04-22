using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Microsoft.Win32;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class PatternEditorViewModel
    {
        public ObservableCollection<Pattern> Letters { get; } = new ObservableCollection<Pattern>();

        public Pattern CurrentLetter { get; set; }

        public ICommand NewMatrixCommand { get; }

        public ICommand SaveToFannCommand { get; }

        public ICommand SaveToXmlCommand { get; }

        public PatternEditorViewModel()
        {
            NewLetter();
            NewMatrixCommand = new RelayCommand(x => NewLetter());
            SaveToFannCommand = new RelayCommand(x => SaveToFann());
            SaveToXmlCommand = new RelayCommand(x => SaveToXml());
        }

        private void NewLetter()
        {
            Pattern letter;
            if (Letters.Any())
            {
                letter = new Pattern
                {
                    Rows = Letters.Last().Rows,
                    Columns = Letters.Last().Columns
                };
            }
            else
            {
                letter = new Pattern();
            }

            letter.Name = $"#{Letters.Count}";

            Letters.Add(letter);
            CurrentLetter = letter;
        }

        private void SaveToFann()
        {
            var previous = Letters.First();
            foreach (var letter in Letters.Skip(1))
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

            using (var f = new StreamWriter(dialog.FileName))
            {
                f.WriteLine($"{Letters.Count} {Letters[0].Pixels.Length} {Letters.Count}");

                for (int i = 0; i < Letters.Count; ++i)
                {

                    f.WriteLine(string.Join(" ", Letters[i].Pixels.Select(x => x.IsSelected ? "1" : "0")));

                    var output = new double[Letters.Count];
                    output[i] = 1.0;


                    f.WriteLine(string.Join(" ", output));
                }
            }
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

            var root = new XElement("Project");
            foreach (var letter in Letters)
            {
                var item = new XElement("Matrix");
                item.SetAttributeValue("Name", letter.Name);
                item.SetAttributeValue("Rows", letter.Rows);
                item.SetAttributeValue("Columns", letter.Columns);
                item.Value = string.Join(",", letter.Pixels.Select(x => x.IsSelected ? "1" : "0"));
                root.Add(item);
            }

            var doc = new XDocument(root);
            doc.Save(dialog.FileName);
        }
    }
}
