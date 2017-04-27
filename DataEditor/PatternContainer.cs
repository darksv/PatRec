using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DataEditor
{
    public class PatternContainer
    {
        private readonly ObservableCollection<Pattern> _patterns = new ObservableCollection<Pattern>();

        public ReadOnlyObservableCollection<Pattern> Patterns { get; }

        public PatternContainer()
        {
            Patterns = new ReadOnlyObservableCollection<Pattern>(_patterns);
        }

        public void SaveToFann(string trainFileName, string testFileName)
        {
            if (!_patterns.Any())
            {
                return;
            }

            var groups = _patterns
                .GroupBy(pattern => pattern.Name)
                .ToArray();

            var numberOfAllSamples = _patterns.Count;

            var minCount = groups.Min(x => x.Count());
            var numberOfTrainSamples = groups.Length * minCount;
            var numberOfTestSamples = numberOfAllSamples - numberOfTrainSamples;

            var inputs = _patterns[0].Pixels.Length;
            var outputs = groups.Length;

            using (var f = new StreamWriter(trainFileName))
            using (var g = new StreamWriter(testFileName))
            {
                f.WriteLine($"{numberOfTrainSamples} {inputs} {outputs}");
                g.WriteLine($"{numberOfTestSamples} {inputs} {outputs}");

                for (int i = 0; i < groups.Length; ++i)
                {
                    var trainSamples = groups[i].OrderBy(x => Guid.NewGuid()).Take(minCount);
                    var testSamples = groups[i].OrderBy(x => Guid.NewGuid()).Skip(minCount);

                    foreach (var pattern in trainSamples)
                    {
                        f.WriteLine(string.Join(" ", pattern.ToVector(-1.0, 1.0)));

                        var output = Enumerable.Repeat(-1.0, groups.Length).ToArray();
                        output[i] = 1.0;

                        f.WriteLine(string.Join(" ", output));
                    }

                    foreach (var pattern in testSamples)
                    {
                        g.WriteLine(string.Join(" ", pattern.ToVector(-1.0, 1.0)));

                        var output = Enumerable.Repeat(-1.0, groups.Length).ToArray();
                        output[i] = 1.0;

                        g.WriteLine(string.Join(" ", output));
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
                Add(pattern);
            }
        }
        
        public void Add(Pattern pattern)
        {
            _patterns.Add(pattern);
        }
    }
}
