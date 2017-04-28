using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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

        public float LearningRate { get; set; } = 0.35f;

        public float DesiredError { get; set; } = 0.0025f;

        public uint MaxIterations { get; set; } = 1000;

        public uint IterationsBetweenReports { get; set; } = 100;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly Dispatcher _dispatcher;
        private readonly NeuralNetwork _network;
        private readonly PatternContainer _patternContainer;

        public NetworkTrainingViewModel(NeuralNetwork network, PatternContainer patternContainer, Dispatcher currentDispatcher)
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
            _dispatcher.Invoke(() => Epochs.Clear());

            if (!_patternContainer.Patterns.Any())
            {
                MessageBox.Show("Brak danych do uczenia!");
                return;
            }

            string trainFile = $@"{DateTime.Now:yyyyMMddHHmmss}.train";
            string testFile = $@"{DateTime.Now:yyyyMMddHHmmss}.test";
            _patternContainer.SaveToFann(trainFile, testFile);

            _network.NumberOfInputs = (uint) _patternContainer.Patterns.First().Pixels.Length;
            _network.NumberOfOutputs = (uint) _patternContainer.Patterns.GroupBy(x => x.Name).Count();

            _network.LearningRate = LearningRate;
            _network.Train(trainFile, MaxIterations, IterationsBetweenReports, DesiredError, (epochs, cost) =>
            {
                _dispatcher.Invoke(() => Epochs.Add(new EpochInfo
                {
                    Number = epochs,
                    Error = cost,
                }));

                token.ThrowIfCancellationRequested();
            });

            Log += $"[{DateTime.Now}] Błąd zbioru testowego: {_network.Test(testFile)}\n";
        }

        private void CancelTraining()
        {
            CanCancelTraining = false;
            _cancellationTokenSource?.Cancel();   
        }
    }
}
