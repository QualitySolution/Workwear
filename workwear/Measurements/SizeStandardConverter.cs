using System;
using Gamma.Binding;
using System.Globalization;

namespace workwear.Measurements
{
	public class SizeStandardCodeConverter : IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return SizeHelper.GetSizeStdEnum ((string)value);
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return SizeHelper.GetSizeStdCode (value);
		}
	}
}

