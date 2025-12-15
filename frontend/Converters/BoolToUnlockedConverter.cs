using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Navigation;

namespace Revalis.Converters
{
    public class BoolToUnlockedConverter : IValueConverter
    {
        // Convert the bool to string
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isUnlocked)
            {
                return isUnlocked ? "Unlocked" : "Locked";
            }
            return "Locked"; // default
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str.Equals("Unlocked", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
