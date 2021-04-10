using System;
using System.Globalization;
using System.Windows.Data;

namespace OrionLab.StressStrainCurveApplication.Converters
{
    internal class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = 0.0;
            if (value != null && (double.TryParse(value.ToString(), out result)))
                return  result;
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}