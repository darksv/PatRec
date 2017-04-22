using FANNCSharp;
using FANNCSharp.Double;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class MainWindowViewModel
    {
        public PatternEditorViewModel PatternEditor { get; }
        public NetworkLearningViewModel NetworkLearning { get; }
        public NetworkTestingViewModel NetworkTesting { get; }

        public MainWindowViewModel()
        {
            _network = new NeuralNet(NetworkType.LAYER, NumLayers, NumInput, NumHidden, NumOutput)
            {
                LearningRate = 0.7f,
                ActivationSteepnessHidden = 1.0f,
                ActivationSteepnessOutput = 1.0f,

                ActivationFunctionHidden = ActivationFunction.SIGMOID_SYMMETRIC,
                ActivationFunctionOutput = ActivationFunction.SIGMOID,
            };

            PatternEditor = new PatternEditorViewModel();
            NetworkLearning = new NetworkLearningViewModel(_network);
            NetworkTesting = new NetworkTestingViewModel(_network);
        }

        private const uint NumLayers = 3;
        private const uint NumInput = 55;
        private const uint NumHidden = 40;
        private const uint NumOutput = 35;

        private readonly NeuralNet _network;
    }
}
