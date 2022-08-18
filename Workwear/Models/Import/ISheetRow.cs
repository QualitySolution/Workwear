namespace workwear.Models.Import
{
	public interface ISheetRow
	{
		string CellValue(int col);
		string CellStringValue(ExcelValueTarget valueTarget);
		string CellTooltip(int col);
		string CellBackgroundColor(int col);
		string CellForegroundColor(int col);

		bool ProgramSkipped { get; set; }
		bool UserSkipped { get; set; }
	}
}
