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
            AddToTrainingSetCommand = new RelayCommand(x =>
            {
                var dialog = new SavePatternAsDialog
                {
                    ResponseText = Predictions?.FirstOrDefault(p => p.IsHighest)?.Name ?? string.Empty
                };
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                var pattern = new Pattern
                {
                    Name = dialog.ResponseText,
                    Rows = 15,
                    Columns = 10
                };

                // 0 - black, 1 - white
                pattern.FillUsing(Pixels.Cast<double>().Select(p => p < 0.5).ToArray());
                _patternContainer.Add(pattern);
            });
        }

        public NeuralNetwork Network => _network;

        public double[,] Pixels { get; set; }

        public Prediction[] Predictions { get; private set; }

        public ICommand PredictCommand { get; }

        public ICommand AddToTrainingSetCommand { get; }

        public void Predict()
        {
            if (Pixels == null)
            {
                return;
            }

            var output = _network.Run(Pixels.Cast<double>().Select(x => x >= 0.5 ? -1.0 : 1.0).ToArray());

            var maxIndex = output
                .Select((value, index) => new {Index = index, Value = value})
                .OrderByDescending(x => x.Value)
                .Select(x => x.Index)
                .First();

            Predictions = _patternContainer.Patterns
                .GroupBy(pattern => pattern.Name)
                .OrderBy(x => x.Key)
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
