using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Revalis.Converters
{
    public class IconPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fileName && !string.IsNullOrEmpty(fileName))
            {
                try
                {
                    var uri = new Uri($"pack://application:,,,/Revalis;component/Assets/Icons/{fileName}", UriKind.Absolute);
                    return new BitmapImage(uri);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
