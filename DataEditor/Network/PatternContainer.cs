using System;
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

        public void SaveToFann(string trainFileName, string testFileName)
        {
            if (!_patterns.Any())
            {
                return;
            }

            var numberOfSamples = _patterns.Count;
            var numberOfTrainSamples = (int)(numberOfSamples * 0.7f);
            var numberOfTestSamples = numberOfSamples - numberOfTrainSamples;

            var patterns = _patterns
                .OrderBy(x => Guid.NewGuid())
                .ToArray();

            var trainSamples = patterns.Take(numberOfTrainSamples).ToArray();
            var testSamples = patterns.Skip(numberOfTrainSamples).Take(numberOfTestSamples).ToArray();

            var groups = _patterns
                .Select(pattern => pattern.Name)
                .Distinct()
                .OrderBy(name => name)
                .Select((name, index) => new {Name = name, Index = index})
                .ToDictionary(item => item.Name, item => item.Index);
            
            var inputs = _patterns[0].Pixels.Length;
            var outputs = groups.Count;

            using (var f = new StreamWriter(trainFileName))
            {
                f.WriteLine($"{numberOfTrainSamples} {inputs} {outputs}");
                
                foreach (var pattern in trainSamples)
                {
                    f.WriteLine(string.Join(" ", pattern.ToVector(-1.0, 1.0)));

                    var output = Enumerable.Repeat(-1.0, outputs).ToArray();
                    output[groups[pattern.Name]] = 1.0;

                    f.WriteLine(string.Join(" ", output));
                }
            }

            using (var f = new StreamWriter(testFileName))
            {
                f.WriteLine($"{numberOfTestSamples} {inputs} {outputs}");

                foreach (var pattern in testSamples)
                {
                    f.WriteLine(string.Join(" ", pattern.ToVector(-1.0, 1.0)));

                    var output = Enumerable.Repeat(-1.0, outputs).ToArray();
                    output[groups[pattern.Name]] = 1.0;

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
