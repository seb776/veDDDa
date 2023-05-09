using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace veDDDa
{
    public enum ELogLevel
    {
        INFO = 0,
        WARNING = 1,
        ERROR = 2
    }

    public enum EEye
    {
        LEFT,
        RIGHT
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EEye _eye;

        public EEye Eye
        {
            get { return _eye; }
            private set { _eye = value; }
        }
        public event Action<ELogLevel, string> OnInfo;
        public MainWindowModel _model {  get { return this.DataContext as MainWindowModel; } }
        private GLControl _winformGLControl;
        private int _program;
        private Size size;
        private string _lastWorkingCode;
        public MainWindow(EEye eye)
        {
            this.DataContext = new MainWindowModel(eye);
            Eye = eye;
            this.Title = $"veDDDa {eye.ToString()} eye";
            size = new Size(800, 600);
            InitializeComponent();
            _winformGLControl = new GLControl();
            _winformGLControl.Dock = System.Windows.Forms.DockStyle.Fill;
            _winformGLControl.Margin = new System.Windows.Forms.Padding(0);
            _winformGLControl.Padding = new System.Windows.Forms.Padding(0);
            _winformGLControl.Paint += _winformGLControl_Paint;
            winFormsHost.Child = _winformGLControl;
            //winFormsHost.Width = this.ActualWidth;
            //winFormsHost.Height = this.ActualHeight;
            _winformGLControl.Size = new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight);
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            size = new Size(this.ActualWidth, this.ActualHeight);
            _winformGLControl.Size = new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight);
        }

        public void ForceUpdate()
        {
            _winformGLControl.Invalidate();
            _winformGLControl.Refresh();
        }

        private void _winformGLControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            _winformGLControl.MakeCurrent();
            GL.UseProgram(_program);
            //GL.ActiveTexture(TextureUnit.Texture0);
            //GL.BindTexture(TextureTarget.Texture2D, _textureName);
            //GL.Uniform1(_textureALocation, 0);
            GL.Viewport(0, 0, (int)size.Width, (int)size.Height);


            GL.Rect(-1, -1, 1, 1);
            GL.Finish();
            _winformGLControl.SwapBuffers();
        }

        public void UpdateShaderCode(string shader)
        {

            var boilerPlate = File.ReadAllText("./Boilerplate.glsl");
            var formattedCode = boilerPlate.Replace("__REPLACE__", shader);
            if (!Build(formattedCode))
                Build(_lastWorkingCode);
        }

        private static string GetShaderInfoLog(int shader)
        {
            const int MaxLength = 1024 * 10;

            //string infoLog = new StringBuilder(MaxLength).ToString();
            int length;
            var infoLog = GL.GetShaderInfoLog(shader);

            return (infoLog.ToString());
        }
        public bool Build(string code)
        {
            //_winformGLControl.MakeCurrent();
            int fragmentIndex = 0;
            fragmentIndex = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentIndex, code);
            GL.CompileShader(fragmentIndex);
            var err = GL.GetError();
            int compileStatus = 0;
            GL.GetShader(fragmentIndex, ShaderParameter.CompileStatus, out compileStatus);
            err = GL.GetError();
            //GL.

            if (compileStatus == 0)
            {
                string messageBoxText = "unable to compiler fragment shader: \n" + GetShaderInfoLog(fragmentIndex);
                string caption = "ShaderError";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result;

                if (OnInfo != null)
                    OnInfo(ELogLevel.ERROR, messageBoxText);
                
                //result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                //throw new InvalidOperationException("unable to compiler fragment shader: " + GetShaderInfoLog(fragmentIndex));
            }

            _program = GL.CreateProgram();

            GL.AttachShader(_program, fragmentIndex);

            //int[] arr = new int[42]; // TODO useless ?
            //GL.GetProgram(0, ProgramProperty.ActiveUniforms, arr);

            GL.LinkProgram(_program);
            int linkStatus = -1;
            GL.GetProgram(_program, (GetProgramParameterName)OpenTK.Graphics.OpenGL.All.LinkStatus, out linkStatus);
            if (compileStatus == 0)
            {
                string messageBoxText = "unable to link fragment shader";
                string caption = "ShaderError";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result;

                if (OnInfo != null)
                    OnInfo(ELogLevel.ERROR, messageBoxText);
                //result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                //throw new InvalidOperationException("unable to link program");
            }

            if (compileStatus != 0 && linkStatus != 0)
            {
                if (OnInfo != null)
                    OnInfo(ELogLevel.INFO, "Successfully compiled shader");
                _lastWorkingCode = code;
            }
            GL.UseProgram(_program);
            return !(compileStatus == 0 || linkStatus == 0);
            //_textureALocation = GL.GetUniformLocation(_program, "textureA");

        }
        public void UpdateUniforms(float time)
        {
            GL.UseProgram(_program);
            var timeLoc = GL.GetUniformLocation(_program, "time");
            GL.Uniform1(timeLoc, time);

            var resolutionLoc = GL.GetUniformLocation(_program, "resolution");
            GL.Uniform2(resolutionLoc, (float)size.Width, (float)size.Height);

            var eyeSepLoc = GL.GetUniformLocation(_program, "EyeDistance");
            float eyeSep = 0.1f;
            GL.Uniform1(eyeSepLoc, eyeSep);

            var eyePosLoc = GL.GetUniformLocation(_program, "EyePosition");
            GL.Uniform1(eyePosLoc, (Eye == EEye.LEFT ? -1.0f : 1.0f));

            var topLeftX = GL.GetUniformLocation(_program, "TopLeftX");
            GL.Uniform1(topLeftX, (float)_model.TopLeft.Left);
            var topLeftY = GL.GetUniformLocation(_program, "TopLeftY");
            GL.Uniform1(topLeftY, (float)_model.TopLeft.Top);

            var topRightX = GL.GetUniformLocation(_program, "TopRightX");
            GL.Uniform1(topRightX, (float)_model.TopRight.Left);
            var topRightY = GL.GetUniformLocation(_program, "TopRightY");
            GL.Uniform1(topRightY, (float)_model.TopRight.Top);

            var bottomLeftX = GL.GetUniformLocation(_program, "BottomLeftX");
            GL.Uniform1(bottomLeftX, (float)_model.BottomLeft.Left);
            var bottomLeftY = GL.GetUniformLocation(_program, "BottomLeftY");
            GL.Uniform1(bottomLeftY, (float)_model.BottomLeft.Top);

            var bottomRightX = GL.GetUniformLocation(_program, "BottomRightX");
            GL.Uniform1(bottomRightX, (float)_model.BottomRight.Left);
            var bottomRightY = GL.GetUniformLocation(_program, "BottomRightY");
            GL.Uniform1(bottomRightY, (float)_model.BottomRight.Top);


        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                if (this.WindowStyle != WindowStyle.None)
                {
                    // hide the window before changing window style
                    this.Visibility = Visibility.Collapsed;
                    //this.Topmost = true;
                    this.WindowStyle = WindowStyle.None;
                    this.ResizeMode = ResizeMode.NoResize;
                    // re-show the window after changing style
                    this.Visibility = Visibility.Visible;
                    Console.WriteLine("ON");
                }
                else
                {
                    this.Visibility = Visibility.Collapsed;
                    //this.Topmost = false;
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.ResizeMode = ResizeMode.CanResize;
                    this.WindowState = System.Windows.WindowState.Maximized;
                    this.Visibility = Visibility.Visible;
                    Console.WriteLine("OFF");
                }
            }
        }
    }
}
