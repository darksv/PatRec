using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DataEditor.Network
{
    public class PatternContainer
    {
        private readonly ObservableCollection<Pattern> _patterns = new ObservableCollection<Pattern>();

        public ReadOnlyObservableCollection<Pattern> Patterns { get; }

        public PatternContainer()
        {
            Patterns = new ReadOnlyObservableCollection<Pattern>(_patterns);
        }

        public void SaveToFann(string trainFileName, string testFileName, double divisionRatio)
        {
            if (!_patterns.Any())
            {
                return;
            }

            var samples = _patterns
                .OrderBy(pattern => Guid.NewGuid())
                .GroupBy(pattern => pattern.Name)
                .ToArray();

            var trainSamples = samples
                .SelectMany(group => group.Take((int)(group.Count() * divisionRatio)))
                .ToArray();

            var testSamples = samples
                .SelectMany(group => group.Skip((int)(group.Count() * divisionRatio)))
                .ToArray();

            var groups = _patterns
                .Select(pattern => pattern.Name)
                .Distinct()
                .OrderBy(name => name)
                .Select((name, index) => new {Name = name, Index = index})
                .ToDictionary(item => item.Name, item => item.Index);
            
            var inputs = _patterns[0].Pixels.Length;
            var outputs = groups.Count;
            
            SaveSamples(trainFileName, trainSamples, groups, inputs, outputs);
            SaveSamples(testFileName, testSamples, groups, inputs, outputs);
        }

        private void SaveSamples(string fileName, Pattern[] samples, Dictionary<string, int> groups, int inputs, int outputs)
        {
            using (var f = new StreamWriter(fileName))
            {
                f.WriteLine($"{samples.Length} {inputs} {outputs}");

                foreach (var sample in samples)
                {
                    f.WriteLine(string.Join(" ", sample.ToVector(-1.0, 1.0)));

                    var output = Enumerable.Repeat(-1.0, outputs).ToArray();
                    output[groups[sample.Name]] = 1.0;

                    f.WriteLine(string.Join(" ", output));
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
