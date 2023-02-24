using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace veDDDa
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const double FRAMERATE = 60.0;
        public const string SHADER_PATH = @"C:\Users\z0rg\Documents\Projects\Perso\veDDDa\veDDDa\veDDDa\Shader.glsl";
        private List<MainWindow> _windows;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _windows = new List<MainWindow>();

            MainWindow leftEyeWin = new MainWindow(EEye.LEFT);
            _windows.Add(leftEyeWin);
            leftEyeWin.Show();
            MainWindow rightEyeWin = new MainWindow(EEye.RIGHT);
            _windows.Add(rightEyeWin);
            rightEyeWin.Show();

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
                    win.UpdateUniforms(accTime);
                }
                //_winformGLControl.Invalidate();
                //_winformGLControl_Paint(null, null);
            };
            timer.Start();
            var controlWin = new ControlWindow();
            controlWin.SetWatchingFile(SHADER_PATH);
            controlWin.Show();
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
            _updateShader();
        }

        private void _updateShader()
        {
            var shaderCode = File.ReadAllText(SHADER_PATH);
            foreach (var win in _windows)
            {
                win.UpdateShaderCode(shaderCode);
            }
        }
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == SHADER_PATH)
            {
                _updateShader();
            }
        }
    }
}
