using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Media.TextFormatting;
using System.Diagnostics;
using static OpenTK.Graphics.OpenGL.GL;
using System.Windows.Media.Media3D;

namespace veDDDa
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const double FRAMERATE = 60.0;
        public const string SHADER_PATH = @"C:\Users\sebas\OneDrive\Parties\Enscehede_17_11_2023\Shader.frag";
        public const string VEDARC_PATH = @"C:\Users\sebas\OneDrive\Parties\Enscehede_17_11_2023\.vedarc";
        public const string HTML_COLORCODE_BOILERPLATE_PATH = @"./BoilerPlate_colorcode.html";
        public const string HTML_COLORCODE_PATH = @"./index.html";

        private HandleFFT _handleFFT;
        Dictionary<string, int> _loadedTextures;
        public List<MainWindow> _windows;
        public ControlWindow _controlWindow;
        private byte[] _bufferCodeHightlight;
        private int _textureCodeHightlight = 0; // Pass it through GL Texture unit 3

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Process.Start("http-server");
            _loadedTextures = new Dictionary<string, int>();
            _windows = new List<MainWindow>();



            MainWindow leftEyeWin = new MainWindow(EEye.LEFT);
            leftEyeWin.OnInfo += LeftEyeWin_OnInfo;
            leftEyeWin.Closed += EyeWin_Closed;
            _windows.Add(leftEyeWin);
            leftEyeWin.Show();
            MainWindow rightEyeWin = new MainWindow(EEye.RIGHT);
            rightEyeWin.Closed += EyeWin_Closed;
            _windows.Add(rightEyeWin);
            rightEyeWin.Show();
            DispatcherTimer timerSave = new DispatcherTimer(DispatcherPriority.Send);
            timerSave.Interval = TimeSpan.FromMilliseconds(1000.0 / 3.0f);
            timerSave.Tick += (s, _) =>
            {
                foreach (var win in _windows)
                {
                    win._model.SaveData();
                }
            };
            timerSave.Start();
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);
            timer.Interval = TimeSpan.FromMilliseconds(1000.0 / FRAMERATE);
            float accTime = 0.0f;
            var lastTime = DateTime.Now;
            timer.Tick += (s, _) =>
            {
                var curTime = DateTime.Now;
                float diffTime = (float)((curTime - lastTime).TotalSeconds);
                accTime += diffTime;
                lastTime = curTime;
                foreach (var win in _windows)
                {
                    win.ForceUpdate();
                    _handleFFT.PlotSignal();
                    win.UpdateUniforms(accTime, _loadedTextures);
                }
                //_winformGLControl.Invalidate();
                //_winformGLControl_Paint(null, null);
            };
            timer.Start();


            var controlWin = new ControlWindow(this);
            _controlWindow = controlWin;
            controlWin.OnClickNewWindow += ControlWin_OnClickNewWindow;
            controlWin.SetWatchingFile(SHADER_PATH);
            controlWin.Show();
            //DispatcherTimer timerCodeHighlight = new DispatcherTimer(DispatcherPriority.Send);
            //timerCodeHighlight.Interval = TimeSpan.FromMilliseconds(1000.0 / 2.0);
            //timerCodeHighlight.Tick += (s, _) =>
            //{

            //    int codetexWidth_ = 0;
            //    int codetexHeight_ = 0;
            //    controlWin.CaptureCodeHighlight(_bufferCodeHightlight, ref codetexWidth_, ref codetexHeight_);
            //    foreach (var win in _windows)
            //    {
            //        win.UpdateCodeHighlightTexture(_textureCodeHightlight, _bufferCodeHightlight, codetexWidth_, codetexHeight_);
            //    }
            //};
            //timerCodeHighlight.Start();
            try
            {

                var watcher = new FileSystemWatcher(Path.GetDirectoryName(SHADER_PATH));
                watcher.NotifyFilter = NotifyFilters.Attributes
                                     | NotifyFilters.CreationTime
                                     | NotifyFilters.DirectoryName
                                     | NotifyFilters.FileName
                                     | NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.Security
                                     | NotifyFilters.Size;

                watcher.Changed += Watcher_Changed;

                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            _handleFFT = new HandleFFT();
            _handleFFT.StartListeningToMicrophone(0);
            try
            {

                var vedarc = File.ReadAllText(VEDARC_PATH);
                var parsedVedarc = JObject.Parse(vedarc);

                int i = 4; // 0 is reserved for fft 1 & 2 are reserved for eyes backbuffer, 3 is reserved for code highlight
                foreach (var import in parsedVedarc["IMPORTED"])
                {

                    var property = import as JProperty;
                    var textureName = property.Name;
                    var texturePath = (import.First().First() as JProperty).Value.ToString();
                    try
                    {

                        Image<Rgba32> image = Image.Load<Rgba32>(texturePath);

                        //ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
                        //This will correct that, making the texture display properly.
                        image.Mutate(x => x.Flip(FlipMode.Vertical));

                        //Use the CopyPixelDataTo function from ImageSharp to copy all of the bytes from the image into an array that we can give to OpenGL.
                        var pixels = new byte[4 * image.Width * image.Height];
                        image.CopyPixelDataTo(pixels);

                        // creating a texture
                        int texture = 0;
                        GL.GenTextures(1, out texture);
                        GL.ActiveTexture(TextureUnit.Texture0 + i);
                        GL.BindTexture(TextureTarget.Texture2D, texture);
                        GL.Enable(EnableCap.Texture2D);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                        _loadedTextures.Add(textureName, i); // i is texture unit
                        LeftEyeWin_OnInfo(ELogLevel.INFO, $"Loaded texture \"{property.Name}\" from path \"{texturePath}\"");
                    }
                    catch (Exception exTex)
                    {
                        LeftEyeWin_OnInfo(ELogLevel.WARNING, $"Failed to load texture \"{property.Name}\" from path \"{texturePath}\"\n{exTex.ToString()}");
                    }
                }
                ++i;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            rightEyeWin.GenerateBackBufferTexture();
            leftEyeWin.GenerateBackBufferTexture();
            _loadedTextures.Add("spectrum", 0);
            int codetexWidth = 0;
            int codetexHeight = 0;
            controlWin.CaptureCodeHighlight(_bufferCodeHightlight, ref codetexWidth, ref codetexHeight);
            _bufferCodeHightlight = new byte[codetexWidth * codetexHeight * 4];
            GL.GenTextures(1, out _textureCodeHightlight);
            GL.ActiveTexture(TextureUnit.Texture0 + 3);
            GL.BindTexture(TextureTarget.Texture2D, _textureCodeHightlight);
            GL.Enable(EnableCap.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            _loadedTextures.Add("codeHighlight", 3);
            _updateShader();
        }

        private void LeftEyeWin_OnInfo(ELogLevel arg1, string arg2)
        {
            var logInfo = new LogInfo();
            logInfo.LogLevel = arg1;
            logInfo.Message = $"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}] {arg2}";
            if (_controlWindow.Logs.Count > 100)
                _controlWindow.Logs.RemoveAt(0);
            _controlWindow.Logs.Add(logInfo);
            _controlWindow.LogsScroll.ScrollToBottom();
        }

        private void ControlWin_OnClickNewWindow(EEye obj)
        {
            if (_windows.Find((el) => { return el.Eye == obj; }) == null)
            {
                var win = new MainWindow(obj);
                win.Closed += EyeWin_Closed;
                win.Show();
                _windows.Add(win);
                _updateShader();
            }
        }

        private void EyeWin_Closed(object sender, EventArgs e)
        {
            _windows.Remove((MainWindow)sender);
        }

        private void _updateShader()
        {
            bool loaded = false;

            while (!loaded)
            {
                try
                {
                    var shaderCode = File.ReadAllText(SHADER_PATH);
                    var htmlBoilerPlate = File.ReadAllText(HTML_COLORCODE_BOILERPLATE_PATH);
                    var generatedHTML = htmlBoilerPlate.Replace("{INJECT_GLSL_CODE}", shaderCode);
                    File.WriteAllText(HTML_COLORCODE_PATH, generatedHTML);

                    // Run command  --headless --disable-gpu --screenshot="C:\screenshot.png" http://127.0.0.1:8080/

                    string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                    var arguments = $"--headless --disable-gpu --hide-scrollbars --window-size=1920,10000 --start-maximized --screenshot=\"C:\\screenshot.png\" http://127.0.0.1:8080/";

                    var procStartInfo = new ProcessStartInfo();
                    procStartInfo.RedirectStandardInput = true;
                    procStartInfo.FileName = chromePath;
                    procStartInfo.Arguments = arguments;
                    procStartInfo.UseShellExecute = false;
                    var proc = new Process { StartInfo = procStartInfo };
                    proc.Start();
                    proc.StandardInput.Write("\n\n\n");
                    //proc.WaitForExit();

                    int codetexWidth_ = 0;
                    int codetexHeight_ = 0;
                    _controlWindow.CaptureCodeHighlight(_bufferCodeHightlight, ref codetexWidth_, ref codetexHeight_);
                    foreach (var win in _windows)
                    {
                        win.UpdateCodeHighlightTexture(_textureCodeHightlight, _bufferCodeHightlight, codetexWidth_, codetexHeight_);
                    }

                    foreach (var win in _windows)
                    {
                        win.UpdateShaderCode(shaderCode);
                    }
                    loaded = true;
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == SHADER_PATH)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    _updateShader();
                }));
            }
        }
    }
}
