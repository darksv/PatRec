using System;
using FANNCSharp;
using FANNCSharp.Double;

namespace DataEditor
{
    public class NeuralNetwork
    {
        public NeuralNetwork()
        {
            _network = new NeuralNet(NetworkType.LAYER, (uint)_layers.Length, _layers)
            {
                ActivationSteepnessHidden = 0.75f,
                ActivationSteepnessOutput = 1.0f,

                ActivationFunctionHidden = ActivationFunction.SIGMOID_SYMMETRIC,
                ActivationFunctionOutput = ActivationFunction.SIGMOID,

                TrainingAlgorithm = TrainingAlgorithm.TRAIN_INCREMENTAL,

                LearningRate = 0.7f
            };
        }

        public float LearningRate
        {
            get { return _network.LearningRate; }
            set { _network.LearningRate = value; }
        }

        public double[] Run(double[] input)
        {
            return _network.Run(input);
        }

        public void Train(string filePath, uint maxIterations, uint iterationsBetweenReports, float desiredError, Action<uint, float> costCallback)
        {
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

        private readonly uint[] _layers = { 55, 25, 35 };
        private readonly NeuralNet _network;
    }
}
