using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TBChestTracker
{
    public class MidpointValueConverter : IMultiValueConverter
    {
        #region Convert/ConvertBack Methods
        public object Convert(object[] values, Type targetType,
          object parameter, CultureInfo culture)
        {
            double extra = 0;

            if (values == null || values.Length < 2)
            {
                throw new ArgumentException("The MidpointValueConverter  class requires 2 double values to be passed to it.\r\n " +
                    "First pass the Total Overall Width, then the\r\n " +
                    "Control Width to Center.", "values");
            }

            double totalMeasure = (double)values[0];
            double controlMeasure = (double)values[1];

            if (parameter != null)
                extra = System.Convert.ToDouble(parameter);

            return (object)(((totalMeasure - controlMeasure) / 2) + extra);
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
          object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
