using System;
using System.Linq;
using PropertyChanged;

namespace DataEditor
{
    [ImplementPropertyChanged]
    public class Letter
    {
        public Letter()
        {
            Clear();
        }

        public string Name { get; set; } = "Bez nazwy";

        private int _columns = 5;
        private int _rows = 7;
        private Pixel[,] _pixels;

        public int Columns
        {
            get { return _columns; }
            set
            {
                _columns = value;
                UpdateSize();
            }
        }

        public int Rows
        {
            get { return _rows; }
            set
            {
                _rows = value;
                UpdateSize();
            }
        }

        [DependsOn(nameof(Columns))]
        public int Width => Columns * 25;

        [DependsOn(nameof(Rows))]
        public int Height => Rows * 25;
        
        public Pixel[] Pixels { get; private set; }
        
        public Pixel this[int row, int column] => _pixels[row, column];
        
        private void UpdateSize()
        {
            var oldRows = _pixels.GetLength(0);
            var oldColumns = _pixels.GetLength(1);

            var newPixels = new Pixel[_rows, _columns];
            for (int i = 0; i < _rows; ++i)
            {
                for (int j = 0; j < _columns; ++j)
                {
                    if (i < oldRows && j < oldColumns)
                        newPixels[i, j] = _pixels[i, j];
                    else
                        newPixels[i, j] = new Pixel();
                }
            }

            _pixels = newPixels;
            Pixels = _pixels.Cast<Pixel>().ToArray();
        }

        private void Clear()
        {
            var newPixels = new Pixel[_rows, _columns];
            
            for (int i = 0; i < _rows; ++i)
            {
                for (int j = 0; j < _columns; ++j)
                {
                    newPixels[i, j] = new Pixel();
                }
            }

            _pixels = newPixels;
            Pixels = _pixels.Cast<Pixel>().ToArray();
        }
    }
}
