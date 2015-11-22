using System;
using Gamma.Binding;
using System.Globalization;

namespace workwear.Domain
{
	public class SizeStandardCodeConverter : IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Measurements.GetSizeStdEnum ((string)value);
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Measurements.GetSizeStdCode (value);
		}
	}
}

