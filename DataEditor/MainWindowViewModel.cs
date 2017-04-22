﻿using System.Collections.ObjectModel;
using System.Windows.Threading;
using FANNCSharp;
using FANNCSharp.Double;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class MainWindowViewModel
    {
        public PatternEditorViewModel PatternEditor { get; }
        public NetworkLearningViewModel NetworkLearning { get; }
        public NetworkTestingViewModel NetworkTesting { get; }

        public MainWindowViewModel()
        {
            _network = new NeuralNet(NetworkType.LAYER, NumLayers, NumInput, NumHidden, NumOutput)
            {
                ActivationSteepnessHidden = 1.0f,
                ActivationSteepnessOutput = 1.0f,

                ActivationFunctionHidden = ActivationFunction.SIGMOID_SYMMETRIC,
                ActivationFunctionOutput = ActivationFunction.SIGMOID,
            };

            PatternEditor = new PatternEditorViewModel(_patterns);
            NetworkLearning = new NetworkLearningViewModel(_network, _patterns, Dispatcher.CurrentDispatcher);
            NetworkTesting = new NetworkTestingViewModel(_network);
        }

        private const uint NumLayers = 3;
        private const uint NumInput = 55;
        private const uint NumHidden = 40;
        private const uint NumOutput = 35;

        private readonly NeuralNet _network;
        private readonly ObservableCollection<Pattern> _patterns = new ObservableCollection<Pattern>();
    }
}
