using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
            TopRightX = 0.5f; TopRightY = 0.5f;
            TopLeftX = -0.5f; TopLeftY = 0.5f;
            BottomLeftX = -0.5f; BottomLeftY = -0.5f;
            BottomRightX = 0.5f; BottomRightY = -0.5f;
        }
        public MainWindowModel(EEye eye)
        {
            _fileName = $"Data{eye.ToString()}Eye.json";
            _resetCorners();
            if (File.Exists(_fileName))
            {
                var data = File.ReadAllText(_fileName);
                var newData = JsonConvert.DeserializeObject<MainWindowModel>(data);
                this.TopLeftX = newData.TopLeftX;
                this.TopLeftY = newData.TopLeftY;

                this.TopRightX = newData.TopRightX;
                this.TopRightY = newData.TopRightY;

                this.BottomLeftX = newData.BottomLeftX;
                this.BottomLeftY = newData.BottomLeftY;

                this.BottomRightX = newData.BottomRightX;
                this.BottomRightY = newData.BottomRightY;
                //data
            }
        }
        private float _topLeftX;
        private float _topLeftY;

        public float TopLeftX
        {
            get { return _topLeftX; }
            set { _topLeftX = value; RaisePropertyChanged(); }
        }

        public float TopLeftY
        {
            get { return _topLeftY; }
            set { _topLeftY = value; RaisePropertyChanged(); }
        }

        private float _topRightX;
        private float _topRightY;

        public float TopRightX
        {
            get { return _topRightX; }
            set { _topRightX = value; RaisePropertyChanged(); }
        }

        public float TopRightY
        {
            get { return _topRightY; }
            set { _topRightY = value; RaisePropertyChanged(); }
        }

        private float _bottomLeftX;
        private float _bottomLeftY;

        public float BottomLeftX
        {
            get { return _bottomLeftX; }
            set { _bottomLeftX = value; RaisePropertyChanged(); }
        }

        public float BottomLeftY
        {
            get { return _bottomLeftY; }
            set { _bottomLeftY = value; RaisePropertyChanged(); }
        }

        private float _bottomRightX;
        private float _bottomRightY;

        public float BottomRightX
        {
            get { return _bottomRightX; }
            set { _bottomRightX = value; RaisePropertyChanged(); }
        }

        public float BottomRightY
        {
            get { return _bottomRightY; }
            set { _bottomRightY = value; RaisePropertyChanged(); }
        }


    }
}
