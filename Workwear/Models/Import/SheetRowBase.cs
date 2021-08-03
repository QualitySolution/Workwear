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

		public string CellValue(int col)
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

		public string CellBackgroundColor(int col)
		{
			if(Skiped)
				return ExcelImportViewModel.ColorOfSkiped;

			var column = ChangedColumns.Keys.FirstOrDefault(x => x.Index == col);

			if(column != null) {
				switch(ChangedColumns[column]) {
					case ChangeType.NewEntity : return ExcelImportViewModel.ColorOfNew;
					case ChangeType.ChangeValue : return ExcelImportViewModel.ColorOfChanged;
					case ChangeType.ParseError: return ExcelImportViewModel.ColorOfError;
					case ChangeType.NotChanged: break;
					default:
						throw new NotImplementedException("Не известный тип изменения");
				}
			}
			return null;
		}

		public bool Skiped;

		public Dictionary<ImportedColumn<TDataTypeEnum>, ChangeType> ChangedColumns = new Dictionary<ImportedColumn<TDataTypeEnum>, ChangeType>();

		public bool HasChanges => !Skiped && ChangedColumns.Any(x => x.Value == ChangeType.ChangeValue || x.Value == ChangeType.NewEntity);
	}

	public interface ISheetRow
	{
		string CellValue(int col);
		string CellBackgroundColor(int col);
	}
}
