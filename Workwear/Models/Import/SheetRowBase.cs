using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public abstract class SheetRowBase<TDataTypeEnum> : ISheetRow
		where TDataTypeEnum : System.Enum
	{
		private readonly IRow cells;

		public SheetRowBase(IRow cells)
		{
			this.cells = cells;
		}

		/// <summary>
		/// Получает значение ячейки видимое пользователю.
		/// </summary>
		public string CellValue(int col)
		{
			var cell = cells.GetCell(col);
			return cell?.ToString();
		}

		public string CellStringValue(int col)
		{
			var cell = cells.GetCell(col);

			if(cell != null) {
				// TODO: you can add more cell types capatibility, e. g. formula
				switch(cell.CellType) {
					case NPOI.SS.UserModel.CellType.Numeric:
						return cell.NumericCellValue.ToString();
					case NPOI.SS.UserModel.CellType.String:
						return cell.StringCellValue;
				}
			}
			return null;
		}

		public int? CellIntValue(int col)
		{
			var cell = cells.GetCell(col);

			if(cell != null) {
				switch(cell.CellType) {
					case NPOI.SS.UserModel.CellType.Numeric:
						return Convert.ToInt32(cell.NumericCellValue);
					case NPOI.SS.UserModel.CellType.String:
						if(int.TryParse(cell.StringCellValue, out int value))
							return value;
						else
							return null;
				}
			}
			return null;
		}

		public DateTime? CellDateTimeValue(int col)
		{
			var cell = cells.GetCell(col);

			if(cell != null) {
				switch(cell.CellType) {
					case NPOI.SS.UserModel.CellType.Numeric:
						return cell.DateCellValue;
					case NPOI.SS.UserModel.CellType.String:
						if(DateTime.TryParse(cell.StringCellValue, out DateTime value))
							return value;
						else
							return null;
				}
			}
			return null;
		}
		
		public string CellBackgroundColor(int col)
		{
			var column = ChangedColumns.Keys.FirstOrDefault(x => x.Index == col);

			if(column != null) {
				switch(ChangedColumns[column].ChangeType) {
					case ChangeType.NewEntity : return ExcelImportViewModel.ColorOfNew;
					case ChangeType.ChangeValue : return ExcelImportViewModel.ColorOfChanged;
					case ChangeType.NotFound: return ExcelImportViewModel.ColorOfNotFound;
					case ChangeType.ParseError: return ExcelImportViewModel.ColorOfError;
					case ChangeType.NotChanged: break;
					default:
						throw new NotImplementedException("Не известный тип изменения");
				}
			}
			return Skiped ? ExcelImportViewModel.ColorOfSkiped : null;
		}

		public bool Skiped;

		#region Работа с изменениями

		public Dictionary<ImportedColumn<TDataTypeEnum>, ChangeState> ChangedColumns = new Dictionary<ImportedColumn<TDataTypeEnum>, ChangeState>();

		public void AddColumnChange(ImportedColumn<TDataTypeEnum> column, ChangeType changeType, string oldValue = null)
		{
			ChangedColumns.Add(column, new ChangeState(changeType, oldValue));
		}

		public string CellTooltip(int col)
		{
			var column = ChangedColumns.Keys.FirstOrDefault(x => x.Index == col);
			if(column != null) {
				var change = ChangedColumns[column];
				if(change.OldValue != null)
					return $"Старое значение: {change.OldValue}";
			}
			return null;
		}

		public bool HasChanges => !Skiped && ChangedColumns.Any(x => x.Value.ChangeType == ChangeType.ChangeValue || x.Value.ChangeType == ChangeType.NewEntity);

		#endregion
	}

	public interface ISheetRow
	{
		string CellValue(int col);
		string CellTooltip(int col);
		string CellBackgroundColor(int col);
	}

	public class ChangeState
	{
		public ChangeType ChangeType;
		public string OldValue;

		public ChangeState(ChangeType changeType, string oldValue = null)
		{
			ChangeType = changeType;
			OldValue = oldValue;
		}
	}
}
