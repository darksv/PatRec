using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class Pixel
    {
        public bool IsSelected { get; set; }
    }
}
