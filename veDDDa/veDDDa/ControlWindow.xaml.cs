using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

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
