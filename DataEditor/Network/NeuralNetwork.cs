using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using DataEditor.Network;
using FANNCSharp;
using FANNCSharp.Double;
using PropertyChanged;

// ReSharper disable UnusedMember.Local

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class NeuralNetwork
    {
        public NeuralNetwork()
        {
            SubscribeLayers(HiddenLayers);

            HiddenLayers.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    SubscribeLayers(args.NewItems.Cast<NetworkLayer>());
                }

                DestroyNetwork();
            };
        }

        private void SubscribeLayers(IEnumerable<NetworkLayer> layers)
        {
            foreach (var layer in layers)
            {
                layer.PropertyChanged +=
                    (sender, args) => DestroyNetwork();
            }
        }

        private void RebuildNetwork()
        {
            var layers = MakeLayers().ToArray();

            _network = new NeuralNet(NetworkType.LAYER, (uint)layers.Length, layers)
            {
                ActivationSteepnessHidden = ActivationSteepnessHidden,
                ActivationSteepnessOutput = ActivationSteepnessOutput,

                ActivationFunctionHidden = ActivationFunctionHidden,
                ActivationFunctionOutput = ActivationFunctionOutput,

                TrainingAlgorithm = TrainingAlgorithm
            };
        }

        private IEnumerable<uint> MakeLayers()
        {
            yield return NumberOfInputs;
            foreach (var hiddenLayer in HiddenLayers)
            {
                yield return hiddenLayer.NumberOfNeurons;
            }
            yield return NumberOfOutputs;
        }

        public double[] Run(double[] input)
        {
            CreateNetworkIfNeccessary();

            return _network.Run(input);
        }

        public void Train(
            string filePath, 
            uint maxIterations, 
            uint iterationsBetweenReports, 
            float learningRate, 
            float desiredError, 
            Action<uint, float> costCallback)
        {
            CreateNetworkIfNeccessary();

            Status = NetworkStatus.OnTraining;

            using (TrainingData data = new TrainingData(filePath))
            {
                data.ShuffleTrainData();

                _network.InitWeights(data);
                _network.SetCallback((net, train, maxEpochs, epochsBetweenReports, _, epochs, userData) =>
                {
                    costCallback?.Invoke(epochs, net.MSE);

                    return 0;
                }, null);
                _network.LearningRate = learningRate;

                try
                {
                    _network.TrainOnData(data, maxIterations, iterationsBetweenReports, desiredError);
                }
                catch (Exception)
                {
                    Status = NetworkStatus.NotTrained;
                    throw;
                }
            }

            Status = NetworkStatus.Ready;
        }

        public float Test(string filePath)
        {
            CreateNetworkIfNeccessary();

            using (TrainingData data = new TrainingData(filePath))
            {
                return _network.TestData(data);
            }
        }

        private void CreateNetworkIfNeccessary()
        {
            if (_network == null)
            {
                RebuildNetwork();
            }
        }

        private void DestroyNetwork()
        {
            _network = null;
            Status = NetworkStatus.NotTrained;
        }

        public bool CanEdit => Status != NetworkStatus.OnTraining;

        public NetworkStatus Status { get; private set; }

        public uint NumberOfInputs { get; set; }
        public uint NumberOfOutputs { get; set; }
        public ObservableCollection<NetworkLayer> HiddenLayers { get; } = new ObservableCollection<NetworkLayer>
        {
            new NetworkLayer(600)
        };

        public double ActivationSteepnessHidden { get; set; } = 0.5;
        public double ActivationSteepnessOutput { get; set; } = 1.0;
        public ActivationFunction ActivationFunctionHidden { get; set; } = ActivationFunction.SIGMOID_SYMMETRIC;
        public ActivationFunction ActivationFunctionOutput { get; set; } = ActivationFunction.SIGMOID_SYMMETRIC;
        public TrainingAlgorithm TrainingAlgorithm { get; set; } = TrainingAlgorithm.TRAIN_RPROP;

        private void OnNumberOfInputsChanged()
        {
            DestroyNetwork();
        }

        private void OnNumberOfOutputsChanged()
        {
            DestroyNetwork();
        }

        private void OnActivationSteepnessHiddenChanged() => OnNetworkPropertyChanged();

        private void OnActivationSteepnessOutputChanged() => OnNetworkPropertyChanged();

        private void OnActivationFunctionOutputChanged() => OnNetworkPropertyChanged();

        private void OnActivationFunctionHiddenChanged() => OnNetworkPropertyChanged();

        private void OnTrainingAlgorithmChanged() => OnNetworkPropertyChanged();

        private void OnNetworkPropertyChanged()
        {
            Status = NetworkStatus.NotTrained;

            if (_network == null)
            {
                return;
            }

            _network.ActivationSteepnessHidden = ActivationSteepnessHidden;
            _network.ActivationSteepnessOutput = ActivationSteepnessOutput;
            _network.ActivationFunctionOutput = ActivationFunctionOutput;
            _network.ActivationFunctionHidden = ActivationFunctionHidden;
            _network.TrainingAlgorithm = TrainingAlgorithm;
        }

        private NeuralNet _network;
    }
}
