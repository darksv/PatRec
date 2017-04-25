using System;
using System.Linq;
using FANNCSharp;
using PropertyChanged;

namespace DataEditor
{
	[ImplementPropertyChanged]
    public class NetworkEditorViewModel
	{
	    public ActivationFunction[] ActivationFunctions { get; }
			= Enum.GetValues(typeof(ActivationFunction)).Cast<ActivationFunction>().ToArray();

        public TrainingAlgorithm[] TrainingAlgorithms { get; }
            = Enum.GetValues(typeof(TrainingAlgorithm)).Cast<TrainingAlgorithm>().ToArray();

        public NeuralNetwork Network { get; private set; }

	    public NetworkEditorViewModel(NeuralNetwork network)
	    {
	        Network = network;
	    }
	}
}
