using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using FANNCSharp;
using FANNCSharp.Double;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class NeuralNetwork
    {
        public NeuralNetwork()
        {
            HiddenLayers.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (NetworkLayer layer in args.NewItems)
                    {
                        layer.PropertyChanged +=
                            (a, b) => _network = null;
                    }
                }

                _network = null;
            };
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

                TrainingAlgorithm = TrainingAlgorithm,

                LearningRate = LearningRate
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

        public void Train(string filePath, uint maxIterations, uint iterationsBetweenReports, float desiredError,
            Action<uint, float> costCallback)
        {
            CreateNetworkIfNeccessary();

            using (TrainingData data = new TrainingData(filePath))
            {
                data.ShuffleTrainData();

                _network.InitWeights(data);
                _network.SetCallback((net, train, maxEpochs, epochsBetweenReports, _, epochs, userData) =>
                {
                    costCallback?.Invoke(epochs, net.MSE);

                    return 0;
                }, null);

                _network.TrainOnData(data, maxIterations, iterationsBetweenReports, desiredError);
            }
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
        
        private uint _numberOfInputs = 55;
        private uint _numberOfOutputs = 35;
        private float _learningRate = 0.35f;
        private double _activationSteepnessHidden = 0.5;
        private double _activationSteepnessOutput = 1.0;
        private ActivationFunction _activationFunctionHidden = ActivationFunction.SIGMOID_SYMMETRIC;
        private ActivationFunction _activationFunctionOutput = ActivationFunction.SIGMOID_SYMMETRIC;
        private TrainingAlgorithm _trainingAlgorithm = TrainingAlgorithm.TRAIN_RPROP;
        
        public uint NumberOfInputs
        {
            get { return _numberOfInputs; }
            set
            {
                if (_numberOfOutputs == value)
                {
                    return;
                }

                _numberOfInputs = value;
                _network = null;
            }
        }

        public ObservableCollection<NetworkLayer> HiddenLayers { get; } = new ObservableCollection<NetworkLayer>
        {
            new NetworkLayer(400)
        };

        public uint NumberOfOutputs
        {
            get { return _numberOfOutputs; }
            set
            {
                if (_numberOfOutputs == value)
                {
                    return;
                }
                _numberOfOutputs = value;
                _network = null;
            }
        }

        public float LearningRate
        {
            get { return _learningRate; }
            set
            {
                _learningRate = value;

                if (_network != null)
                {
                    _network.LearningRate = value;
                }
            }
        }

        public double ActivationSteepnessHidden
        {
            get { return _activationSteepnessHidden; }
            set
            {
                _activationSteepnessHidden = value;

                if (_network != null)
                {
                    _network.ActivationSteepnessHidden = value;
                }
            }
        }

        public double ActivationSteepnessOutput
        {
            get { return _activationSteepnessOutput; }
            set
            {
                _activationSteepnessOutput = value;

                if (_network != null)
                {
                    _network.ActivationSteepnessOutput = value;
                }
            }
        }

        public ActivationFunction ActivationFunctionHidden
        {
            get { return _activationFunctionHidden; }
            set
            {
                _activationFunctionHidden = value;

                if (_network != null)
                {
                    _network.ActivationFunctionHidden = value;
                }
            }
        }

        public ActivationFunction ActivationFunctionOutput
        {
            get { return _activationFunctionOutput; }
            set
            {
                _activationFunctionOutput = value;

                if (_network != null)
                {
                    _network.ActivationFunctionOutput = value;
                }
            }
        }

        public TrainingAlgorithm TrainingAlgorithm
        {
            get { return _trainingAlgorithm; }
            set
            {
                _trainingAlgorithm = value;

                if (_network != null)
                {
                    _network.TrainingAlgorithm = value;
                }
            }
        }

        private NeuralNet _network;
    }
}
