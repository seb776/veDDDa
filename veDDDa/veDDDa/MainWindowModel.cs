using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace veDDDa
{
    public abstract class ANotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public class MainWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SaveData()
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(_fileName, json);
        }

        private string _fileName;
        public MainWindowModel()
        {
            // To avoid circular deserialization with newtonsoft
        }
        private void _resetCorners()
        {
            TopRight = new Thickness(0.5f, 0.5f, 0f, 0f);
            TopLeft = new Thickness(-0.5f, 0.5f, 0f, 0f);
            BottomLeft = new Thickness(-0.5f, -0.5f, 0f, 0f);
            BottomRight = new Thickness(0.5f, -0.5f, 0f, 0f);
        }
        public MainWindowModel(EEye eye)
        {
            _fileName = $"Data{eye.ToString()}Eye.json";
            _resetCorners();
            if (File.Exists(_fileName))
            {
                var data = File.ReadAllText(_fileName);
                var newData = JsonConvert.DeserializeObject<MainWindowModel>(data);
                this.TopLeft = newData.TopLeft;
                this.TopRight = newData.TopRight;
                this.BottomLeft = newData.BottomLeft;
                this.BottomRight = newData.BottomRight;

                //data
            }
        }
        private Thickness _topLeft;

        public Thickness TopLeft
        {
            get { return _topLeft; }
            set { _topLeft = value; RaisePropertyChanged(); }
        }

        private Thickness _topRight;

        public Thickness TopRight
        {
            get { return _topRight; }
            set { _topRight = value; RaisePropertyChanged(); }
        }

        private Thickness _bottomLeft;

        public Thickness BottomLeft
        {
            get { return _bottomLeft; }
            set { _bottomLeft = value; RaisePropertyChanged(); }
        }

        private Thickness _bottomRight;

        public Thickness BottomRight
        {
            get { return _bottomRight; }
            set { _bottomRight = value; RaisePropertyChanged(); }
        }


    }
}
