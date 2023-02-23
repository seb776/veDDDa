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
        public const string SHADER_PATH = @"C:\path\to\folder";
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

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000.0 / FRAMERATE)
            };
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
                    win.UpdateUniforms(accTime);
                }
                //_winformGLControl.Invalidate();
                //_winformGLControl_Paint(null, null);
            };

            var watcher = new FileSystemWatcher(SHADER_PATH);

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

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var shaderCode = File.ReadAllText(e.FullPath);
            foreach (var win in _windows)
            {
                win.UpdateShaderCode(shaderCode);
            }
        }
    }
}
