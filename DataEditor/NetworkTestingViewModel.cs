using System.Linq;
using System.Windows.Input;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class NetworkTestingViewModel
    {
        private readonly NeuralNetwork _network;
        private readonly PatternContainer _patternContainer;

        public NetworkTestingViewModel(NeuralNetwork network, PatternContainer patternContainer)
        {
            _network = network;
            _patternContainer = patternContainer;
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

            Predictions = _patternContainer.Patterns
                .GroupBy(pattern => pattern.Name)
                .Select((group, index) => new Prediction
                {
                    Name = group.Key,
                    Value = output[index],
                    IsHighest = index == maxIndex
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
