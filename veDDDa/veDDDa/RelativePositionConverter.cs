using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace veDDDa
{
    public class RelativePositionConverter : IValueConverter
    {
        private const float SCALING = 1.0f;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parentGrid = (Canvas)(((Grid)parameter).DataContext);
            var cur = (Thickness)value;
            double center = 0.5;
            double left = cur.Left + center;
            double top = cur.Top + center;
            double right = cur.Right + center;
            double bottom = cur.Bottom + center;
            return new Thickness(left* parentGrid.ActualWidth*SCALING, top * parentGrid.ActualHeight * SCALING, 0,0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parentGrid = (Canvas)(((Grid)parameter).DataContext);
            var cur = (Thickness)value;
            double center = 0.5;
            double left = cur.Left;
            double top = cur.Top;
            double right = cur.Right;
            double bottom = cur.Bottom;
            return new Thickness(left / parentGrid.ActualWidth -center, top / parentGrid.ActualHeight - center, 0,0);

        }
    }
}
