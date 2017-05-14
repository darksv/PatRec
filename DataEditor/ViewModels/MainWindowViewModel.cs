using System.Windows.Threading;
using DataEditor.Network;
using PropertyChanged;

namespace DataEditor.ViewModels
{
    [ImplementPropertyChanged]
    public class MainWindowViewModel
    {
        public PatternEditorViewModel PatternEditor { get; }
        public NetworkEditorViewModel NetworkEditor { get; }
        public NetworkTrainingViewModel NetworkTraining { get; }
        public NetworkTestingViewModel NetworkTesting { get; }

        public MainWindowViewModel()
        {
            PatternEditor = new PatternEditorViewModel(_patternContainer);
            NetworkEditor = new NetworkEditorViewModel(_network);
            NetworkTraining = new NetworkTrainingViewModel(_network, _patternContainer, Dispatcher.CurrentDispatcher);
            NetworkTesting = new NetworkTestingViewModel(_network, _patternContainer);
        }

        private readonly NeuralNetwork _network = new NeuralNetwork();
        private readonly PatternContainer _patternContainer = new PatternContainer();
    }
}
