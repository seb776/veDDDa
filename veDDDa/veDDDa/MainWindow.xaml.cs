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
        private GLControl _winformGLControl;
        private int _program;
        private Size size;
        public MainWindow(EEye eye)
        {
            size = new Size(800, 600);
            InitializeComponent();
            _winformGLControl = new GLControl();
            _winformGLControl.Dock = System.Windows.Forms.DockStyle.Fill;
            _winformGLControl.Paint += _winformGLControl_Paint;
            winFormsHost.Child = _winformGLControl;
            winFormsHost.Width = 800;
            winFormsHost.Height = 600;
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
            const int MaxLength = 1024*10;

            //string infoLog = new StringBuilder(MaxLength).ToString();
            int length;
            var infoLog = GL.GetShaderInfoLog(shader);

            return (infoLog.ToString());
        }
        public void Build(string code)
        {
            int fragmentIndex = 0;
            fragmentIndex = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentIndex, code);
            GL.CompileShader(fragmentIndex);
            int compileStatus = 0;
            GL.GetShader(fragmentIndex, ShaderParameter.CompileStatus, out compileStatus);
            if (compileStatus == 0)
                throw new InvalidOperationException("unable to compiler fragment shader: " + GetShaderInfoLog(fragmentIndex));

            _program = GL.CreateProgram();

            GL.AttachShader(_program, fragmentIndex);

            //int[] arr = new int[42]; // TODO useless ?
            //GL.GetProgram(0, ProgramProperty.ActiveUniforms, arr);

            GL.LinkProgram(_program);

            GL.GetProgram(_program, (GetProgramParameterName)OpenTK.Graphics.OpenGL.All.LinkStatus, out compileStatus);
            if (compileStatus == 0)
                throw new InvalidOperationException("unable to link program");

            GL.UseProgram(_program);

            //_textureALocation = GL.GetUniformLocation(_program, "textureA");

        }
        public void UpdateUniforms(float time)
        {
            var timeLoc = GL.GetUniformLocation(_program, "time");
            GL.Uniform1(timeLoc, time);

            var resolutionLoc = GL.GetUniformLocation(_program, "resolution");
            GL.Uniform2(resolutionLoc, (float)size.Width, (float)size.Height);
        }
    }
}
