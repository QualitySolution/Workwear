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
		
		public static string GetPersonalNumber(IMatchEmployeesSettings settings, ISheetRow row, int columnIndex) {
			var original = settings.ConvertPersonnelNumber ? 
				ConvertPersonnelNumber(row.CellStringValue(columnIndex)) : row.CellStringValue(columnIndex);
			return original?.Trim();
		}
	}
}
