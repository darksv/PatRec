using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class MainWindowViewModel
    {
        public PatternEditorViewModel PatternEditor { get; }
        public NetworkLearningViewModel NetworkLearning { get; }

        public MainWindowViewModel()
        {
            PatternEditor = new PatternEditorViewModel();
            NetworkLearning = new NetworkLearningViewModel();
        }
    }
}
