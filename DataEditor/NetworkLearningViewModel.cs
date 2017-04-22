﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FANNCSharp;
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

        private CancellationTokenSource _cancellationTokenSource;

        public NetworkLearningViewModel()
        {
            StartLearningCommand = new AsyncRelayCommand(x => StartLearning());
            CancelLearningCommand = new RelayCommand(x => CancelLearning());
        }

        private async Task StartLearning()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() =>
            {
                const float learningRate = 0.01f;
                const uint numLayers = 3;
                const uint numInput = 35;
                const uint numHidden = 100;
                const uint numOutput = 10;
                const float desiredError = 0.00001f;
                const uint maxIterations = 300000;
                const uint iterationsBetweenReports = 1000;

                using (var net = new NeuralNet(NetworkType.LAYER, numLayers, numInput, numHidden, numOutput))
                {
                    net.LearningRate = learningRate;

                    net.ActivationSteepnessHidden = 0.25f;
                    net.ActivationSteepnessOutput = 0.15f;

                    net.ActivationFunctionHidden = ActivationFunction.SIGMOID_SYMMETRIC;
                    net.ActivationFunctionOutput = ActivationFunction.SIGMOID;

                    using (TrainingData data = new TrainingData(@"C:\Users\Host\Documents\visual studio 2017\Projects\Recogniser\Trainer\digits.txt"))
                    {
                        net.InitWeights(data);

                        AddLine($"Max Epochs {maxIterations,8:D}. Desired Error: {desiredError,-8:F}");
                        net.SetCallback((nett, train, maxEpochs, epochsBetweenReports, _, epochs, userData) =>
                        {
                            AddLine($"Epochs     {epochs,8:D}. Current Error: {nett.MSE,-8:F}");
                            return 0;
                        }, null);

                        net.TrainOnData(data, maxIterations, iterationsBetweenReports, desiredError);

                        AddLine("\nTesting network.");
                        for (uint i = 0; i < data.TrainDataLength; i++)
                        {
                            var input = data.InputAccessor[(int)i];
                            var desiredOutput = data.OutputAccessor[(int)i];
                            var calculatedOutput = net.Run(input);
                            var difference = Enumerable.Zip(calculatedOutput, desiredOutput.Array, (xc, xd) => xc - xd);

                            AddLine($"({FormatArray(input.Array)}) -> ({FormatArray(calculatedOutput)}), should be ({FormatArray(desiredOutput.Array)}), differences = ({FormatArray(difference)})");
                        }
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
