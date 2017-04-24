using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using FANNCSharp.Double;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class NetworkTrainingViewModel
    {
        public string Log { get; set; } = string.Empty;

        public ICommand StartTrainingCommand { get; }

        public ICommand CancelTrainingCommand { get; }

        public bool CanStartTraining { get; private set; } = true;

        public bool CanCancelTraining { get; private set; } = false;

        public float LearningRate { get; set; } = 0.7f;

        public float DesiredError { get; set; } = 0.005f;

        public uint MaxIterations { get; set; } = 1000;

        public uint IterationsBetweenReports { get; set; } = 10;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly Dispatcher _dispatcher;
        private readonly NeuralNet _network;
        private readonly PatternContainer _patternContainer;

        public NetworkTrainingViewModel(NeuralNet network, PatternContainer patternContainer, Dispatcher currentDispatcher)
        {
            _network = network;
            _patternContainer = patternContainer;
            _dispatcher = currentDispatcher;

            StartTrainingCommand = new AsyncRelayCommand(x => StartTraining());
            CancelTrainingCommand = new RelayCommand(x => CancelTraining());
        }

        public ObservableCollection<EpochInfo> Epochs { get; } = new ObservableCollection<EpochInfo>();

        public class EpochInfo
        {
            public uint Number { get; set; }

            public float Error { get; set; }
        }

        private async Task StartTraining()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            CanStartTraining = false;
            CanCancelTraining = true;

            var token = _cancellationTokenSource.Token;

            try
            {
                await Task.Run(() => ExecuteTraining(token), token);
            }
            catch (Exception e)
            {
                
            }
           
            CanStartTraining = true;
            CanCancelTraining = false;
        }

        private static string Format(double x)
        {
            return Math.Abs(x) < float.Epsilon
                ? 0.ToString()
                : x.ToString("0.0000");
        }

        private static string FormatArray(IEnumerable<double> arr)
        {
            return string.Join(", ", arr.Select(Format));
        }

        private void ExecuteTraining(CancellationToken token)
        {
            string filePath = $@"{DateTime.Now:yyyyMMddHHmmss}.txt";
            _patternContainer.SaveToFann(filePath);
            _network.LearningRate = LearningRate;

            using (TrainingData data = new TrainingData(filePath))
            {
                data.ShuffleTrainData();

                _dispatcher.Invoke(() => Epochs.Clear());

                _network.InitWeights(data);
                _network.SetCallback((net, train, maxEpochs, epochsBetweenReports, _, epochs, userData) =>
                {
                    _dispatcher.Invoke(() => Epochs.Add(new EpochInfo
                    {
                        Number = epochs,
                        Error = net.MSE,
                    }));

                    token.ThrowIfCancellationRequested();

                    return 0;
                }, null);

                _network.TrainOnData(data, MaxIterations, IterationsBetweenReports, DesiredError);

                var patterns = _patternContainer.Patterns.ToArray();
                for (int i = 0; i < patterns.Length; ++i)
                {
                    var input = patterns[i].ToVector();
                    var desiredOutput = Enumerable.Repeat(0.0, patterns.Length).ToArray();
                    desiredOutput[i] = 1.0;

                    var calculatedOutput = _network.Run(input);
                    var difference = calculatedOutput.Zip(desiredOutput, (calculated, desired) => calculated - desired);

                    Log += $"{patterns[i].Name} -> ({FormatArray(calculatedOutput)}), should be ({FormatArray(desiredOutput)}), differences = ({FormatArray(difference)})\n";
                }
            }
        }

        private void CancelTraining()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
