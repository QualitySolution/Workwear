using System;
namespace workwear.Models.Import
{
	public static class EmployeeParse
	{
		public static string ConvertPersonnelNumber(string cellValue)
		{
			if(int.TryParse(cellValue, out int number))
				return number.ToString();
			else
				return cellValue;
		}
	}
}
