using System;
using System.Linq;
using System.Windows.Input;
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

        public ICommand AddLayerCommand => new RelayCommand(x =>
        {
            Network.HiddenLayers.Add(new NetworkLayer(Network.NumberOfInputs));
        });

	    public ICommand RemoveLayerCommand => new RelayCommand(x =>
	    {
	        Network.HiddenLayers.Remove((NetworkLayer) x);
	    });

        public NetworkEditorViewModel(NeuralNetwork network)
	    {
	        Network = network;
	    }
	}
}
