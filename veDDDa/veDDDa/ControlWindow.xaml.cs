using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;

namespace veDDDa
{
    public class LogInfo
    {
        public ELogLevel LogLevel { get; set; }
        public string Message { get; set; }
    }
    /// <summary>
    /// Interaction logic for ControlWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
        private App _app;
        public ObservableCollection<LogInfo> Logs { get; set; }

        public ControlWindow(App app)
        {

            Logs = new ObservableCollection<LogInfo>();
            _isDragging = false;
            _draggedEllipse = null;
            _app = app;
            InitializeComponent();
            this.Closed += ControlWindow_Closed;
        }

        private void ControlWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        public MainWindow LeftEyeWin { get { return _app._windows.Find((el) => { return el.Eye == EEye.LEFT; }); } }
        public MainWindow RightEyeWin { get { return _app._windows.Find((el) => { return el.Eye == EEye.RIGHT; }); } }
        public void CaptureCodeHighlight(byte[] buffer, ref int width, ref int height)
        {
            // Picture is generated on code changes (APP.updateShader) => we should do image loading there too

            // Load image
            try
            {

                Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>("C:\\screenshot.png");
                width = image.Width;
                height = image.Height;
                //ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
                //This will correct that, making the texture display properly.
                image.Mutate(x => x.Flip(FlipMode.Vertical));

                image.CopyPixelDataTo(buffer);
            }
            catch (Exception ex) {
                return;
            }

            // =============== This approach does not work as the webbrowser content is not part of the WPF rendering
            // Stride = (width) x (bytes per pixel)
            //int stride = (int)1920 * (32 / 8);

            //RenderTargetBitmap renderTargetBitmap =
            // new RenderTargetBitmap(1920, 1080, 96, 96, PixelFormats.Pbgra32);
            //renderTargetBitmap.Render(this.webBrowser);
            //renderTargetBitmap.CopyPixels(buffer, stride, 0);
        }
        public void SetWatchingFile(string file)
        {
            this.textRunFilepath.Text = file;
        }

        public event Action<EEye> OnClickNewWindow;
        private void ResetLeft(object sender, RoutedEventArgs e)
        {
            if (LeftEyeWin != null)
                LeftEyeWin._model._resetCorners();
        }
        private void ResetRight(object sender, RoutedEventArgs e)
        {
            if (RightEyeWin != null)
                RightEyeWin._model._resetCorners();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (OnClickNewWindow != null)
                OnClickNewWindow(EEye.LEFT);
        }

        private void TextBlock_MouseLeftButtonDown_1(object sender, RoutedEventArgs e)
        {
            // Right
            if (OnClickNewWindow != null)
                OnClickNewWindow(EEye.RIGHT);
        }

        private void TextBlock_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private bool _isDragging;
        private Ellipse _draggedEllipse;


        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _draggedEllipse = (Ellipse)sender;
        }

        private void parentLeft_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _draggedEllipse != null)
            {
                var position = e.GetPosition((Canvas)sender);

                var thickness = _draggedEllipse.Margin;

                thickness.Left = position.X;// - (_draggedEllipse.ActualWidth / 2);
                thickness.Top = position.Y;// - (_draggedEllipse.ActualHeight / 2);

                _draggedEllipse.Margin = thickness;
            }
        }

        private void parentLeft_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _draggedEllipse = null;
        }
    }
}
