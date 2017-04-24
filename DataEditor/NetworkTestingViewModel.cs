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
        private readonly PatternContainer _patterns;

        public NetworkTestingViewModel(NeuralNet network, PatternContainer patterns)
        {
            _network = network;
            _patterns = patterns;
            PredictCommand = new RelayCommand(x => Predict());
        }

        public Pattern Pattern { get; } = new Pattern {Rows = 11, Columns = 5};

        public Prediction[] Predictions { get; private set; }

        public ICommand PredictCommand { get; }

        public void Predict()
        {
            var output = _network.Run(Pattern.ToVector());

            var maxIndex = output
                .Select((value, index) => new {Index = index, Value = value})
                .OrderByDescending(x => x.Value)
                .Select(x => x.Index)
                .First();

            Predictions = _patterns.Patterns
                .GroupBy(x => x.Name)
                .Select((t, i) => new Prediction
                {
                    Name = t.Key,
                    Value = output[i],
                    IsHighest = i == maxIndex
                }).ToArray();
        }

        public class Prediction
        {
            public string Name { get; set; }

            public double Value { get; set; }

            public bool IsHighest { get; set; }
        }
    }
}
