using System.ComponentModel;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class NetworkLayer : INotifyPropertyChanged
    {
        public uint NumberOfNeurons { get; set; }

        public NetworkLayer(uint numberOfNeurons)
        {
            NumberOfNeurons = numberOfNeurons;
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
