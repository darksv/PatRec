using System;
using System.Linq;
using System.Windows.Input;
using DataEditor.Network;
using DataEditor.Utils;
using FANNCSharp;
using PropertyChanged;

namespace DataEditor.ViewModels
{
	[ImplementPropertyChanged]
    public class NetworkEditorViewModel
	{
	    public ActivationFunction[] ActivationFunctions { get; }
			= Enum.GetValues(typeof(ActivationFunction)).Cast<ActivationFunction>().ToArray();

        public TrainingAlgorithm[] TrainingAlgorithms { get; }
            = Enum.GetValues(typeof(TrainingAlgorithm)).Cast<TrainingAlgorithm>().ToArray();

        public NeuralNetwork Network { get; private set; }

        public ICommand AddLayerCommand => new RelayCommand(() =>
        {
            Network.HiddenLayers.Add(new NetworkLayer(Network.NumberOfInputs));
        });

	    public ICommand RemoveLayerCommand => new RelayCommand<NetworkLayer>(layer =>
	    {
	        Network.HiddenLayers.Remove(layer);
	    });

        public NetworkEditorViewModel(NeuralNetwork network)
	    {
	        Network = network;
	    }
	}
}
