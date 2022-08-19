using System;
using System.Linq;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using Workwear.Domain.Sizes;
using Workwear.Measurements;

namespace workwear.Models.Import.Employees.DataTypes {
	public class DataTypeEmployeeSize : DataTypeEmployeeBase {
		protected readonly SizeService sizeService;
		protected readonly SizeType sizeType;

		public DataTypeEmployeeSize(SizeService sizeService, SizeType sizeType) : base(sizeType) {
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.sizeType = sizeType ?? throw new ArgumentNullException(nameof(sizeType));
			
			if (sizeType.Id == 2)
				ColumnNameRegExp = "(одежда|одежды)";
			if (sizeType.Id == 3) {
				ColumnNameRegExp = "зим.+(одежда|одежды)";
				ColumnNameDetectPriority = 5; //Повышенный приоритет для того чтобы сначала срабатывало привило с зимним вариантом
			}
			if (sizeType.Id == 4)
				ColumnNameRegExp = "(обувь|обуви)";
			if (sizeType.Id == 5) {
				ColumnNameRegExp = "зим.+(обувь|обуви)";
				ColumnNameDetectPriority = 5;
			}
			if (sizeType.Id == 6)
				ColumnNameRegExp = "(головного|головной)";
			if (sizeType.Id == 9) {
				ColumnNameRegExp = "зим.+(головного|головной)";
				ColumnNameDetectPriority = 5; //Повышенный приоритет для того чтобы сначала срабатывало привило с зимним вариантом
			}
			if (sizeType.Id == 7)
				ColumnNameRegExp = "(перчаток|перчатки)";
			if (sizeType.Id == 8)
				ColumnNameRegExp = "(рукавиц|рукавицы)";
			if (sizeType.Id == 10)
				ColumnNameRegExp = "противогаза?";
			if (sizeType.Id == 11)
				ColumnNameRegExp = "респиратора?";
			if (sizeType.Id == 12)
				ColumnNameRegExp = "носк(и|ов)";
			
			ColumnNameKeywords.Add(sizeType.Name.ToLower());
		}

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var value = row.CellStringValue(target)?.Trim().ToLower();
			if(String.IsNullOrWhiteSpace(value)) {
				row.AddColumnChange(target, ChangeType.NotChanged);
				return;
			}
			var size = ParseValue(uow, value);
			row.AddColumnChange(target, CompareSize(row, size, value));
		}
		
		#region Helpers

		internal virtual Size ParseValue(IUnitOfWork uow, string value) {
			var size = sizeService
				.GetSize(uow, sizeType)
				.FirstOrDefault(x => 
					x.Name.Trim().ToLower().Equals(value)
					|| value.Equals(x.AlternativeName?.Trim().ToLower()));

			if(size == null && sizeType.Id == 1) {
				//Дополнительно конвертируем рост.
				var interpreted = SizeParser.HeightToGOST(value);
				size = sizeService
					.GetSize(uow, sizeType)
					.FirstOrDefault(x => x.Name.Trim().ToLower().Equals(interpreted));
			}

			return size;
		}
		
		private ChangeState CompareSize(SheetRowEmployee row, Size newValue, string excelValue) {
			var employeeSize = row.EditingEmployee.Sizes.FirstOrDefault(x => x.SizeType == sizeType);
			var rowChange = row.EditingEmployee.Id == 0 ? ChangeType.NewEntity : ChangeType.ChangeValue;

			if (newValue == null && !String.IsNullOrWhiteSpace(excelValue))
				return new ChangeState(ChangeType.ParseError, employeeSize?.Size?.Name);
			if(employeeSize?.Size == newValue)
				return new ChangeState(ChangeType.NotChanged);
			string oldValue = employeeSize?.Size?.Name;
			if (employeeSize is null) {
				employeeSize = new EmployeeSize
					{Size = newValue, SizeType = sizeType, Employee = row.EditingEmployee};
				row.AddSetValueAction(ValueSetOrder, () => row.EditingEmployee.Sizes.Add(employeeSize));
			}
			else
				row.AddSetValueAction(ValueSetOrder, () => employeeSize.Size = newValue);
			
			return new ChangeState(rowChange, oldValue: oldValue, newValue?.Name != excelValue ? newValue?.Name : null);
		}
		#endregion
	}
}
