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
        private MainWindowModel _model {  get { return this.DataContext as MainWindowModel; } }
        private GLControl _winformGLControl;
        private int _program;
        private Size size;
        public MainWindow(EEye eye)
        {
            this.DataContext = new MainWindowModel();
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
            Build(formattedCode);
        }

        private static string GetShaderInfoLog(int shader)
        {
            const int MaxLength = 1024 * 10;

            //string infoLog = new StringBuilder(MaxLength).ToString();
            int length;
            var infoLog = GL.GetShaderInfoLog(shader);

            return (infoLog.ToString());
        }
        public void Build(string code)
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

                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                //throw new InvalidOperationException("unable to compiler fragment shader: " + GetShaderInfoLog(fragmentIndex));
            }

            _program = GL.CreateProgram();

            GL.AttachShader(_program, fragmentIndex);

            //int[] arr = new int[42]; // TODO useless ?
            //GL.GetProgram(0, ProgramProperty.ActiveUniforms, arr);

            GL.LinkProgram(_program);

            GL.GetProgram(_program, (GetProgramParameterName)OpenTK.Graphics.OpenGL.All.LinkStatus, out compileStatus);
            if (compileStatus == 0)
            {
                string messageBoxText = "unable to link fragment shader";
                string caption = "ShaderError";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result;

                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                //throw new InvalidOperationException("unable to link program");
            }

            GL.UseProgram(_program);

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
            float eyeSep = 0.2f;
            GL.Uniform1(eyeSepLoc, eyeSep);

            var eyePosLoc = GL.GetUniformLocation(_program, "EyePosition");
            GL.Uniform1(eyePosLoc, (Eye == EEye.LEFT ? -1.0f : 1.0f));

            var topLeftX = GL.GetUniformLocation(_program, "TopLeftX");
            GL.Uniform1(topLeftX, _model.TopLeftX);
            var topLeftY = GL.GetUniformLocation(_program, "TopLeftY");
            GL.Uniform1(topLeftY, _model.TopLeftY);

            var topRightX = GL.GetUniformLocation(_program, "TopRightX");
            GL.Uniform1(topRightX, _model.TopRightX);
            var topRightY = GL.GetUniformLocation(_program, "TopRightY");
            GL.Uniform1(topRightY, _model.TopRightY);

            var bottomLeftX = GL.GetUniformLocation(_program, "BottomLeftX");
            GL.Uniform1(bottomLeftX, _model.BottomLeftX);
            var bottomLeftY = GL.GetUniformLocation(_program, "BottomLeftY");
            GL.Uniform1(bottomLeftY, _model.BottomLeftY);

            var bottomRightX = GL.GetUniformLocation(_program, "BottomRightX");
            GL.Uniform1(bottomRightX, _model.BottomRightX);
            var bottomRightY = GL.GetUniformLocation(_program, "BottomRightY");
            GL.Uniform1(bottomRightY, _model.BottomRightY);

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
