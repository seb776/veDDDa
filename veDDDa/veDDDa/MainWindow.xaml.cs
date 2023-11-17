using OpenTK;
using OpenTK.Graphics;
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
            //glEnable(GL_TEXTURE_2D);
            this.DataContext = new MainWindowModel(eye);
            Eye = eye;
            this.Title = $"veDDDa {eye.ToString()} eye";
            size = new Size(800, 600);
            InitializeComponent();
            _winformGLControl = new GLControl(new GraphicsMode(0, 0, 0, 0, 0, 2));
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
        public void GenerateBackBufferTexture()
        {
            GL.GenTextures(1, out _backbufferTexture);
            int textureUnit = (_eye == EEye.LEFT ? 1 : 2);
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit); // 0 is FFT, 1 is left eye, 2 is right eye 3 is code texture  > 3 is textures
            GL.BindTexture(TextureTarget.Texture2D, _backbufferTexture);
            GL.Enable(EnableCap.Texture2D);
            byte[] emptyArr = null;
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)this.ActualWidth, (int)this.ActualHeight, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, emptyArr);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }
        int _backbufferTexture = -1;
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_backbufferTexture != -1)
                GL.DeleteTexture(_backbufferTexture);
            GenerateBackBufferTexture();
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
            GL.Viewport(0, 0, (int)size.Width, (int)size.Height);
            GL.Rect(-1, -1, 1, 1);
            GL.Finish();
            int textureUnit = (_eye == EEye.LEFT ? 1 : 2);
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, (int)this.ActualWidth, (int)this.ActualHeight); // for backbuffer // example // https://stackoverflow.com/questions/33468528/copying-subdata-into-empty-texture-in-opengl-es-webgl
            _winformGLControl.SwapBuffers();
        }

        public void UpdateShaderCode(string shader)
        {
            StringBuilder sb = new StringBuilder();
            var boilerPlate = File.ReadAllText("./Boilerplate.glsl");

            sb.AppendLine(boilerPlate);
            sb.AppendLine(shader);

            var formattedCode = sb.ToString();
            if (!Build(formattedCode) && _lastWorkingCode != null)
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
        public void UpdateUniforms(float time, Dictionary<string, int> textures)
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

            GL.Enable(EnableCap.Texture2D);
            foreach (var kvp in textures)
            {
                var texLocation = GL.GetUniformLocation(_program, kvp.Key);
                GL.Uniform1(texLocation,  kvp.Value);
            }
            int textureUnit = (_eye == EEye.LEFT ? 1 : 2);
            var texLocation_ = GL.GetUniformLocation(_program, "backbuffer");
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, _backbufferTexture);
            GL.Uniform1(texLocation_, textureUnit);
        }

        public void UpdateCodeHighlightTexture(int textureTarget, byte[] buffer, int width, int height)
        {
            try
            {

            GL.Enable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0 + 3);
            GL.BindTexture(TextureTarget.Texture2D, textureTarget);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, buffer);
            }
            catch (Exception ex)
            {
            // Memory access exception on teximage2D

            }

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

 
        private void webBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            var browser = sender as WebBrowser;

            if (browser == null || browser.Document == null)
                return;

            dynamic document = browser.Document;

            if (document.readyState != "complete")
                return;

            dynamic script = document.createElement("script");
            script.type = @"text/javascript";
            script.text = @"window.onerror = function(msg,url,line){return true;}";
            document.head.appendChild(script);
        }
    }
}
