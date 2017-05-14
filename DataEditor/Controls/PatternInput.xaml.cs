using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
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

        //public static readonly DependencyProperty PixelsProperty = PixelsPropertyKey.DependencyProperty;

        public double[,] Pixels
        {
            get { return (double[,]) GetValue(PixelsProperty); }
            set { SetValue(PixelsProperty, value); }
        }

        private static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            "Columns",
            typeof(int),
            typeof(PatternInput),
            new PropertyMetadata(1));

        public int Columns
        {
            get { return (int) GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        private static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
           "Rows",
           typeof(int),
           typeof(PatternInput),
           new PropertyMetadata(1));

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public PatternInput()
        {
            InitializeComponent();
        }

        private static BitmapSource CreateSaveBitmap(FrameworkElement canvas)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int) (canvas.ActualWidth),
                (int) (canvas.ActualHeight),
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

        private static System.Drawing.Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new System.Drawing.Bitmap(bitmap);
            }
        }

        private static Bitmap TrimBitmap(Bitmap source)
        {
            Rectangle srcRect = default(Rectangle);
            BitmapData data = null;
            try
            {
                data = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                byte[] buffer = new byte[data.Height * data.Stride];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                int xMin = int.MaxValue;
                int xMax = 0;
                int yMin = int.MaxValue;
                int yMax = 0;
                for (int y = 0; y < data.Height; y++)
                {
                    for (int x = 0; x < data.Width; x++)
                    {
                        byte a = buffer[y * data.Stride + 4 * x + 3];
                        byte r = buffer[y * data.Stride + 4 * x + 2];
                        byte g = buffer[y * data.Stride + 4 * x + 1];
                        byte b = buffer[y * data.Stride + 4 * x + 0];
                        if (a != 0 || (r==255&&b==255&&g==255))
                        {
                            if (x < xMin) xMin = x;
                            if (x > xMax) xMax = x;
                            if (y < yMin) yMin = y;
                            if (y > yMax) yMax = y;
                        }
                    }
                }
                if (xMax < xMin || yMax < yMin)
                {
                    // Image is empty...
                    return null;
                }
                srcRect = Rectangle.FromLTRB(xMin, yMin, xMax, yMax);
            }
            finally
            {
                if (data != null)
                    source.UnlockBits(data);
            }

            Bitmap dest = new Bitmap(srcRect.Width, srcRect.Height);
            Rectangle destRect = new Rectangle(0, 0, srcRect.Width, srcRect.Height);
            using (Graphics graphics = Graphics.FromImage(dest))
            {
                graphics.DrawImage(source, destRect, srcRect, GraphicsUnit.Pixel);
            }
            return dest;
        }

        public void Clear()
        {
            InkCanvas.Strokes.Clear();
        }

        public void Save(string file)
        {
            var renderTargetBitmap = CreateSaveBitmap(InkCanvas);
            var bitmap = BitmapImage2Bitmap(BitmapSourceToBitmapImage(renderTargetBitmap));
            bitmap.Save(file);
        }

        public event EventHandler PatternChanged; 
        
        private void InkCanvas_OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs args)
        {
            var pixels = new double[Rows, Columns];

            //            var renderTargetBitmap = CreateSaveBitmap(InkCanvas);
            //            var orgBitmap = TrimBitmap(BitmapImage2Bitmap(BitmapSourceToBitmapImage(renderTargetBitmap)));
            //
            //            var resized = new Bitmap(Columns, Rows);
            //            using (var g = Graphics.FromImage(resized))
            //            {
            //                g.InterpolationMode = InterpolationMode.NearestNeighbor;
            //                g.SmoothingMode = SmoothingMode.None;
            //                g.DrawImage(orgBitmap, new Rectangle(0, 0, Columns, Rows));
            //            }
            //            resized.Save(@"F:\aaa.bmp");

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
