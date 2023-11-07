using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TBChestTracker
{
    internal class BooleanToWidthConverter : IValueConverter
    {

        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            var isVisible = bool.Parse(value.ToString());
            var width = double.Parse(parameter as string);
            return isVisible ? width : 0.0;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
