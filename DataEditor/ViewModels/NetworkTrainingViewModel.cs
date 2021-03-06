﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DataEditor.Network;
using DataEditor.Utils;
using PropertyChanged;

namespace DataEditor.ViewModels
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

        public float DesiredError { get; set; } = 0.000025f;

        public uint MaxIterations { get; set; } = 1000;

        public uint IterationsBetweenReports { get; set; } = 100;

        public double SetDivisionRatio { get; set; } = 0.7;

        public NeuralNetwork Network => _network;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly Dispatcher _dispatcher;
        private readonly NeuralNetwork _network;
        private readonly PatternContainer _patternContainer;

        public NetworkTrainingViewModel(NeuralNetwork network, PatternContainer patternContainer, Dispatcher currentDispatcher)
        {
            _network = network;
            _patternContainer = patternContainer;
            _dispatcher = currentDispatcher;

            StartTrainingCommand = new AsyncRelayCommand(StartTraining);
            CancelTrainingCommand = new RelayCommand(CancelTraining);
        }

        public ObservableCollection<EpochInfo> Epochs { get; } = new ObservableCollection<EpochInfo>();
        public double CurrentError { get; private set; }

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
            _patternContainer.SaveToFann(trainFile, testFile, SetDivisionRatio);

            _network.NumberOfInputs = (uint) _patternContainer.Patterns.First().Pixels.Length;
            _network.NumberOfOutputs = (uint) _patternContainer.Patterns.GroupBy(x => x.Name).Count();

            _network.Train(trainFile, MaxIterations, IterationsBetweenReports, LearningRate, DesiredError, (epochs, cost) =>
            {
                _dispatcher.Invoke(() =>
                {
                    Epochs.Add(new EpochInfo
                    {
                        Number = epochs,
                        Error = cost,
                    });

                    CurrentError = cost;
                });

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
