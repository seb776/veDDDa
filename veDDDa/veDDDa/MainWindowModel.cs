using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    class MainWindowModel : ANotifyPropertyChanged
    {
        public MainWindowModel()
        {
            TopLeftX = 0.2f; TopLeftY = 0.2f;
            TopRightX = 0.0f; TopRightY = 0.2f;
            BottomLeftX = 0.0f; BottomLeftY = 0.0f;
            BottomRightX = 0.2f; BottomRightY = 0.0f;
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
