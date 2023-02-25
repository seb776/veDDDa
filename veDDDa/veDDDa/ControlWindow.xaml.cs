using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Interaction logic for ControlWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
        private App _app;
        public ControlWindow(App app)
        {
            _app = app;
            InitializeComponent();
        }
        public MainWindow LeftEyeWin { get { return _app._windows.Find((el) => { return el.Eye == EEye.LEFT; }); } }
        public MainWindow RightEyeWin { get { return _app._windows.Find((el) => { return el.Eye == EEye.RIGHT; }); } }
        public void SetWatchingFile(string file)
        {
            this.textRunFilepath.Text = file;
        }

        public event Action<EEye> OnClickNewWindow;

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (OnClickNewWindow != null)
                OnClickNewWindow(EEye.LEFT);
        }

        private void TextBlock_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            // Right
            if (OnClickNewWindow != null)
                OnClickNewWindow(EEye.RIGHT);
        }
    }
}
