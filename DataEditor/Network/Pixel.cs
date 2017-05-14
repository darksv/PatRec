using PropertyChanged;

namespace DataEditor.Network
{
    [ImplementPropertyChanged]
    public class Pixel
    {
        public bool IsSelected { get; set; }
    }
}
