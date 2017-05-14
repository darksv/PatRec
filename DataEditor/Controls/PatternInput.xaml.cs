using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;
using Size = System.Windows.Size;


namespace DataEditor.Controls
{
    public partial class PatternInput : UserControl
    {
        private static readonly DependencyProperty PixelsProperty = DependencyProperty.Register(
            "Pixels",
            typeof(double[,]),
            typeof(PatternInput),
            new FrameworkPropertyMetadata(null));

        public double[,] Pixels
        {
            get => (double[,]) GetValue(PixelsProperty);
            set => SetValue(PixelsProperty, value);
        }

        private static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            "Columns",
            typeof(int),
            typeof(PatternInput),
            new PropertyMetadata(1));

        public int Columns
        {
            get => (int) GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        private static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
           "Rows",
           typeof(int),
           typeof(PatternInput),
           new PropertyMetadata(1));

        public int Rows
        {
            get => (int)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        public PatternInput()
        {
            InitializeComponent();
        }

        private static BitmapSource CreateSaveBitmap(FrameworkElement canvas)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int) canvas.ActualWidth,
                (int) canvas.ActualHeight,
                96d,
                96d,
                PixelFormats.Pbgra32);

            // needed otherwise the image output is black
            canvas.Measure(new Size((int) canvas.Width, (int) canvas.Height));
            canvas.Arrange(new Rect(new Size((int) canvas.Width, (int) canvas.Height)));

            renderBitmap.Render(canvas);

            return renderBitmap;
        }

        private static BitmapImage BitmapSourceToBitmapImage(BitmapSource bitmapSource)
        {
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new BmpBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        private static BitmapImage RenderControl(FrameworkElement control, int width, int height)
        {
            var renderTargetBitmap = CreateSaveBitmap(control);
            return BitmapSourceToBitmapImage(Resize(renderTargetBitmap, width, height, BitmapScalingMode.Linear));
        }

        private static BitmapSource Resize(BitmapSource photo, int width, int height,
            BitmapScalingMode scalingMode)
        {
            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, scalingMode);
            group.Children.Add(new ImageDrawing(photo, new Rect(0, 0, width, height)));
            var targetVisual = new DrawingVisual();
            var targetContext = targetVisual.RenderOpen();
            targetContext.DrawDrawing(group);
            var target = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            targetContext.Close();
            target.Render(targetVisual);
            return target;
        }

        private static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public void Clear()
        {
            InkCanvas.Strokes.Clear();
        }
       
        public event EventHandler PatternChanged; 
        
        private void InkCanvas_OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs args)
        {
            var pixels = new double[Rows, Columns];

            var resized = BitmapImage2Bitmap(RenderControl(InkCanvas, Columns, Rows));

            for (int i = 0; i < Rows; ++i)
            {
                for (int j = 0; j < Columns; ++j)
                {
                    var pixel = resized.GetPixel(j, i);
                    pixels[i, j] = (pixel.R + pixel.G + pixel.B) / (3 * 255.0d);
                }
            }

            Pixels = pixels;

            PatternChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
