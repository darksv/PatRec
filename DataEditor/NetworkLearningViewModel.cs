using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using FANNCSharp.Double;
using PropertyChanged;
using Xceed.Wpf.Toolkit;

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

        public uint MaxIterations { get; set; } = 300000;

        public uint IterationsBetweenReports { get; set; } = 1000;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly Dispatcher _dispatcher;
        private readonly NeuralNet _network;
        private readonly ObservableCollection<Pattern> _patterns;

        public NetworkLearningViewModel(NeuralNet network, ObservableCollection<Pattern> patterns, Dispatcher currentDispatcher)
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
                const string filePath = @"letters.txt";

                if (!File.Exists(filePath))
                {
                    AddLine("Brak pliku " + filePath);
                    return;
                }

                _network.LearningRate = LearningRate;

                _dispatcher.Invoke(() => Epochs.Clear());
                using (TrainingData data = new TrainingData(filePath))
                {
                    _network.InitWeights(data);
                    _network.SetCallback((nett, train, maxEpochs, epochsBetweenReports, _, epochs, userData) =>
                    {
                        _dispatcher.Invoke(() => Epochs.Add(new EpochInfo
                        {
                            Number = epochs,
                            Error = nett.MSE,
                        }));
                        return 0;
                    }, null);

                    _network.TrainOnData(data, MaxIterations, IterationsBetweenReports, DesiredError);

                    AddLine("\nTesting network.");
                    for (uint i = 0; i < data.TrainDataLength; i++)
                    {
                        var input = data.InputAccessor[(int) i];
                        var desiredOutput = data.OutputAccessor[(int) i];
                        var calculatedOutput = _network.Run(input);
                        var difference = Enumerable.Zip(calculatedOutput, desiredOutput.Array, (xc, xd) => xc - xd);

                        AddLine(
                            $"({FormatArray(input.Array)}) -> ({FormatArray(calculatedOutput)}), should be ({FormatArray(desiredOutput.Array)}), differences = ({FormatArray(difference)})");
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
                : x.ToString("+0.#####;-0.#####");
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
