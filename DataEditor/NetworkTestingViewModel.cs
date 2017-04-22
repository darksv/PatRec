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

        public NetworkTestingViewModel(NeuralNet network)
        {
            _network = network;
            PredictCommand = new RelayCommand(x => Predict());
        }

        public Pattern Pattern { get; } = new Pattern();

        public double[] Prediction { get; private set; }

        public ICommand PredictCommand { get; }

        public void Predict()
        {
            Prediction = _network.Run(Pattern.Pixels.Select(x => x.IsSelected ? 1.0 : 0.0).ToArray());
        }
    }
}
