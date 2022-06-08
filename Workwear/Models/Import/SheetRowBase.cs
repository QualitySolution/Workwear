﻿using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
using QS.DomainModel.Entity;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public abstract class SheetRowBase<TDataTypeEnum> : PropertyChangedBase, ISheetRow
		where TDataTypeEnum : System.Enum
	{
		private readonly IRow cells;

		public SheetRowBase(IRow cells)
		{
			this.cells = cells;
		}

		#region Получение значений ячейки
		readonly Dictionary<int, string> changedValues = new Dictionary<int, string>();
		/// <summary>
		/// Получает значение ячейки видимое пользователю.
		/// </summary>
		public string CellValue(int col)
		{
			//Используем кешированные значения, так как графика для отрисовки очень часто дергает этот метод.
			if(changedValues.ContainsKey(col))
				return changedValues[col];

			string value;
			var cell = GetCell(col);
			if(cell?.CellType == CellType.Blank)
				value = null;
			if(cell?.CellType == CellType.Error && cell.ErrorCellValue == 0)
				value = null;
			value = cell?.ToString();
			changedValues[col] = value;
			return value;
		}

		public string CellStringValue(int col)
		{
			var cell = GetCell(col);

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
			var cell = GetCell(col);

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
			var cell = GetCell(col);

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
		
		private ICell GetCell(int col)
		{
			var cell = cells.GetCell(col);
			if (cell != null && cell.IsMergedCell)
				cell = GetMergedCell(cell);
			return cell;
		}
		private ICell GetMergedCell(ICell cell)
		{
			var start = DateTime.Now;
			// Get current sheet.
			var currentSheet = cell.Sheet;

			// Loop through all merge regions in this sheet.
			for (int i = 0; i < currentSheet.NumMergedRegions; i++)
			{
				var mergeRegion = currentSheet.GetMergedRegion(i);

				// If this merged region contains this cell.
				if (mergeRegion.FirstRow <= cell.RowIndex && cell.RowIndex <= mergeRegion.LastRow &&
				    mergeRegion.FirstColumn <= cell.ColumnIndex && cell.ColumnIndex <= mergeRegion.LastColumn)
				{
					// Find the top-most and left-most cell in this region.
					var firstRegionCell = currentSheet.GetRow(mergeRegion.FirstRow)
						.GetCell(mergeRegion.FirstColumn);

					Console.WriteLine($"GetMergedCell: {(DateTime.Now - start).TotalMilliseconds}");
					// And return its value.
					return firstRegionCell;
				}
			}
			// This should never happen.
			throw new Exception("Cannot find this cell in any merged region");
		}
		#endregion
		
		public string CellBackgroundColor(int col)
		{
			if (UserSkipped)
				return ExcelImportViewModel.ColorOfSkipped;
			
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
			return ProgramSkipped ? ExcelImportViewModel.ColorOfSkipped : null;
		}

		public bool Skipped => ProgramSkipped || UserSkipped;

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

		public bool HasChanges => !Skipped && ChangedColumns.Any(x => x.Value.ChangeType == ChangeType.ChangeValue || x.Value.ChangeType == ChangeType.NewEntity);

		public bool ProgramSkipped { get; set; }

		private bool userSkipped;

		public virtual bool UserSkipped {
			get => userSkipped;
			set => SetField(ref userSkipped, value);
		}
		#endregion
	}

	public interface ISheetRow
	{
		string CellValue(int col);
		string CellTooltip(int col);
		string CellBackgroundColor(int col);

		bool UserSkipped { get; set; }
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
