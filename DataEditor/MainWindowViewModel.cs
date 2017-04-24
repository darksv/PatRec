using System.IO;
using System.Linq;
using System.Windows.Threading;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class MainWindowViewModel
    {
        private const string DefaultDataFile = "Resources/letters.xml";

        public PatternEditorViewModel PatternEditor { get; }
        public NetworkTrainingViewModel NetworkTraining { get; }
        public NetworkTestingViewModel NetworkTesting { get; }

        public MainWindowViewModel()
        {
            PatternEditor = new PatternEditorViewModel(_patternContainer);
            NetworkTraining = new NetworkTrainingViewModel(_network, _patternContainer, Dispatcher.CurrentDispatcher);
            NetworkTesting = new NetworkTestingViewModel(_network, _patternContainer);

            if (File.Exists(DefaultDataFile))
            {
                _patternContainer.LoadFromXml(DefaultDataFile);
                PatternEditor.CurrentLetter = _patternContainer.Patterns.FirstOrDefault();
            }
        }

        private readonly NeuralNetwork _network = new NeuralNetwork();
        private readonly PatternContainer _patternContainer = new PatternContainer();
    }
}
