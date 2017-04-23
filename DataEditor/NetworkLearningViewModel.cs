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
    public class NetworkLearningViewModel
    {
        public string Log { get; set; } = string.Empty;

        public ICommand StartLearningCommand { get; }

        public ICommand CancelLearningCommand { get; }

        public float LearningRate { get; set; } = 0.7f;

        public float DesiredError { get; set; } = 0.01f;

        public uint MaxIterations { get; set; } = 1000;

        public uint IterationsBetweenReports { get; set; } = 1;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly Dispatcher _dispatcher;
        private readonly NeuralNet _network;
        private readonly PatternCollection _patterns;

        public NetworkLearningViewModel(NeuralNet network, PatternCollection patterns, Dispatcher currentDispatcher)
        {
            _network = network;
            _patterns = patterns;
            _dispatcher = currentDispatcher;

            StartLearningCommand = new AsyncRelayCommand(x => StartLearning());
            CancelLearningCommand = new RelayCommand(x => CancelLearning());
        }

        public ObservableCollection<EpochInfo> Epochs { get; } = new ObservableCollection<EpochInfo>();

        public class EpochInfo
        {
            public uint Number { get; set; }

            public float Error { get; set; }
        }

        private async Task StartLearning()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() =>
            {
                string filePath = $@"{DateTime.Now:yyyyMMddhhmmss}.txt";
                _patterns.SaveToFann(filePath);
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
                        return 0;
                    }, null);

                    _network.TrainOnData(data, MaxIterations, IterationsBetweenReports, DesiredError);

                    int i = 0;
                    foreach (var pattern in _patterns)
                    {
                        var input = pattern.ToVector();
                        var desiredOutput = new double[_patterns.Count()];
                        desiredOutput[i] = 1.0;
                        
                        var calculatedOutput = _network.Run(input);
                        var difference = Enumerable.Zip(calculatedOutput, desiredOutput, (xc, xd) => xc - xd);

                        AddLine($"{pattern.Name} -> ({FormatArray(calculatedOutput)}), should be ({FormatArray(desiredOutput)}), differences = ({FormatArray(difference)})");

                        i++;
                    }
                }

            }, _cancellationTokenSource.Token);
        }

        private void AddLine(string line)
        {
            Log += line + "\n";
        }

        private static string Format(double x)
        {
            return FannAbs(x) < float.Epsilon
                ? 0.ToString()
                : x.ToString("0.0000");
        }

        private static string FormatArray(IEnumerable<double> arr)
        {
            return string.Join(", ", arr.Select(Format));
        }

        static double FannAbs(double value)
        {
            return value > 0 ? value : -value;
        }

        private void CancelLearning()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
