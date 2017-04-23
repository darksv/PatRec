using System.IO;
using System.Windows.Threading;
using FANNCSharp;
using FANNCSharp.Double;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class MainWindowViewModel
    {
        const string DefaultDataFile = "Resources/letters.xml";

        public PatternEditorViewModel PatternEditor { get; }
        public NetworkLearningViewModel NetworkLearning { get; }
        public NetworkTestingViewModel NetworkTesting { get; }

        public MainWindowViewModel()
        {
            _network = new NeuralNet(NetworkType.LAYER, (uint) Layers.Length, Layers)
            {
                ActivationSteepnessHidden = 0.75f,
                ActivationSteepnessOutput = 1.0f,

                ActivationFunctionHidden = ActivationFunction.SIGMOID_SYMMETRIC,
                ActivationFunctionOutput = ActivationFunction.SIGMOID,

                TrainingAlgorithm = TrainingAlgorithm.TRAIN_INCREMENTAL
            };

            if (File.Exists(DefaultDataFile))
            {
                _patterns.LoadFromXml(DefaultDataFile);
            }

            PatternEditor = new PatternEditorViewModel(_patterns);
            NetworkLearning = new NetworkLearningViewModel(_network, _patterns, Dispatcher.CurrentDispatcher);
            NetworkTesting = new NetworkTestingViewModel(_network, _patterns);
        }
        
        private readonly uint[] Layers = { 55, 25, 35 };
        private readonly NeuralNet _network;
        private readonly PatternCollection _patterns = new PatternCollection();
    }
}
