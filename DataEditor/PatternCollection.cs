using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DataEditor
{
    public class PatternCollection : IEnumerable<Pattern>
    {
        private readonly ObservableCollection<Pattern> _patterns = new ObservableCollection<Pattern>();

        public void SaveToFann(string fileName)
        {
            using (var f = new StreamWriter(fileName))
            {
                var groups = _patterns
                    .GroupBy(pattern => pattern.Name)
                    .ToArray();

                f.WriteLine($"{_patterns.Count} {_patterns[0].Pixels.Length} {groups.Length}");
                
                for (int i = 0; i < groups.Length; ++i)
                {
                    foreach (var pattern in groups[i])
                    {
                        f.WriteLine(string.Join(" ", pattern.ToVector()));

                        var output = Enumerable.Repeat(0.0, groups.Length).ToArray();
                        output[i] = 1.0;

                        f.WriteLine(string.Join(" ", output));
                    }
                }
            }
        }

        public void SaveToXml(string fileName)
        {
            var root = new XElement("Project");
            foreach (var pattern in _patterns)
            {
                var item = new XElement("Matrix");
                item.SetAttributeValue("Name", pattern.Name);
                item.SetAttributeValue("Rows", pattern.Rows);
                item.SetAttributeValue("Columns", pattern.Columns);
                item.Value = string.Join(",", pattern.Pixels.Select(x => x.IsSelected ? "1" : "0"));
                root.Add(item);
            }

            var doc = new XDocument(root);
            doc.Save(fileName);
        }

        public void LoadFromXml(string fileName)
        {
            _patterns.Clear();

            var doc = XDocument.Load(fileName);
            foreach (var element in doc.Descendants("Matrix"))
            {
                var name = (string)element.Attribute("Name");
                var rows = (int)element.Attribute("Rows");
                var columns = (int)element.Attribute("Columns");
                var pixels = element.Value.Split(',').Select(x => x == "1").ToArray();

                var pattern = new Pattern
                {
                    Columns = columns,
                    Rows = rows,
                    Name = name
                };
                pattern.FillUsing(pixels);
                _patterns.Add(pattern);
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Pattern> GetEnumerator()
        {
            return _patterns.GetEnumerator();
        }

        public void Add(Pattern pattern)
        {
            _patterns.Add(pattern);
        }
    }
}
