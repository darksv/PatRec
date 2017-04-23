using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using FANNCSharp.Double;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class NetworkTestingViewModel
    {
        private readonly NeuralNet _network;
        private readonly PatternCollection _patterns;

        public NetworkTestingViewModel(NeuralNet network, PatternCollection patterns)
        {
            _network = network;
            _patterns = patterns;
            PredictCommand = new RelayCommand(x => Predict());
        }

        public Pattern Pattern { get; } = new Pattern {Rows = 11, Columns = 5};

        public List<Prediction> Predictions { get; private set; }

        public ICommand PredictCommand { get; }

        public void Predict()
        {
            var output = _network.Run(Pattern.ToVector());

            var patterns = _patterns.ToArray();
            var maxIndex = output.Select((value, index) => new {Index = index, Value = value})
                .OrderByDescending(x => x.Value)
                .Select(x => x.Index)
                .First();

            Predictions = patterns.Select((t, i) => new Prediction
            {
                Name = t.Name,
                Value = output[i],
                IsHighest = i == maxIndex
            }).ToList();
        }

        public class Prediction
        {
            public string Name { get; set; }

            public double Value { get; set; }

            public bool IsHighest { get; set; }
        }
    }
}
