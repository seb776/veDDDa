﻿using System;
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
        public const string SHADER_PATH = @".\Shader.frag";
        public List<MainWindow> _windows;
        public ControlWindow _controlWindow;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
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
                    win.UpdateUniforms(accTime);
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
                    foreach (var win in _windows)
                    {
                        win.UpdateShaderCode(shaderCode);
                    }
                    loaded = true;
                }
                catch (Exception e) { }
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
