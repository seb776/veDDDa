using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace veDDDa
{
    public class LogToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var logLvl = (ELogLevel)value;
            switch (logLvl)
            {
                case ELogLevel.INFO:
                    return new SolidColorBrush(Colors.White);
                case ELogLevel.WARNING:
                    return new SolidColorBrush(Colors.Yellow);
                case ELogLevel.ERROR:
                    return new SolidColorBrush(Color.FromArgb(0xFF, 0xe0, 0x63, 0x48));
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
