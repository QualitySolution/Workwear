using System;
using System.Linq;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using Workwear.Domain.Sizes;
using Workwear.Measurements;

namespace workwear.Models.Import.Employees.DataTypes {
	public class DataTypeSizeAndHeight : DataTypeEmployeeBase {
		private readonly SizeService sizeService;
		private readonly SizeType sizeType;
		private readonly SizeType heightType;

		public DataTypeSizeAndHeight(SizeService sizeService, SizeType sizeType, SizeType heightType) : base(DataTypeEmployee.SizeAndHeight) {
			this.sizeService = sizeService;
			this.sizeType = sizeType;
			this.heightType = heightType;
		}

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var value = row.CellStringValue(target)?.Trim().ToLower();
			if(String.IsNullOrWhiteSpace(value)) {
				row.AddColumnChange(target, ChangeType.NotChanged);
				return;
			}
			row.AddColumnChange(target, CompareSize(row, uow, value));
		}
		
		#region Helpers
		private ChangeState CompareSize(SheetRowEmployee row, IUnitOfWork uow, string excelValue) {
			var parts = excelValue.Split( new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries);
			if(parts.Length != 2)
				return new ChangeState(ChangeType.ParseError, error: "Колонка обрабатывает значения в формате «Размер/Рост»");
					
			var height = GetHeight(uow, parts[1]);
			if(height == null)
				return new ChangeState(ChangeType.ParseError, error: "Неизвестный рост: " + parts[1]);
			//Здесь считаем что если человек маленький то скорей всего размер по госту, то есть на пересекающихся представлениях размеров
			//(типа 76 это большой обычный размер или маленький по госту).
			bool firstAlternative = int.Parse(parts[1].Substring(0, 3)) < 182; 
			var size = GetSize(uow, parts[0], firstAlternative);			
			if(size == null)
				return new ChangeState(ChangeType.ParseError, error: "Неизвестный размер: " + parts[0]);

			var employeeSize = row.EditingEmployee.Sizes.FirstOrDefault(x => x.SizeType == sizeType);
			var employeeHeight = row.EditingEmployee.Sizes.FirstOrDefault(x => x.SizeType == heightType);
			
			if(employeeSize?.Size == size && employeeHeight?.Size == height)
				return new ChangeState(ChangeType.NotChanged);
			string oldSize = employeeSize?.Size?.Name;
			string oldHeight = employeeHeight?.Size?.Name;
			string oldValue = oldSize == null && oldHeight == null ? null : $"{oldSize}/{oldHeight}";
			if (employeeSize is null) {
				employeeSize = new EmployeeSize
					{Size = size, SizeType = sizeType, Employee = row.EditingEmployee};
				row.AddSetValueAction(ValueSetOrder, () => row.EditingEmployee.Sizes.Add(employeeSize));
			}
			else
				row.AddSetValueAction(ValueSetOrder, () => employeeSize.Size = size);
			
			if (employeeHeight is null) {
				employeeHeight = new EmployeeSize
					{Size = height, SizeType = heightType, Employee = row.EditingEmployee};
				row.AddSetValueAction(ValueSetOrder, () => row.EditingEmployee.Sizes.Add(employeeSize));
			}
			else
				row.AddSetValueAction(ValueSetOrder, () => employeeHeight.Size = height);

			var state = employeeSize.Id == 0 || employeeHeight.Id == 0 ? ChangeType.NewEntity : ChangeType.ChangeValue;
			return new ChangeState(state, oldValue: oldValue);
		}

		public Size GetHeight(IUnitOfWork uow, string height) {
			return sizeService
				.GetSize(uow, heightType)
				.FirstOrDefault(x => x.Name.Trim().Equals(height));
		}
		
		/// <param name="firstAlternative">Если установлен пробуем сначала искать в альтернативных именах</param>
		public Size GetSize(IUnitOfWork uow, string size, bool firstAlternative) {
			Size result;
			if(firstAlternative)
				result = sizeService
					.GetSize(uow, sizeType)
					.FirstOrDefault(x => size.Equals(x.AlternativeName?.Trim().ToLower()));
			else 
				result = sizeService
					.GetSize(uow, sizeType)
					.FirstOrDefault(x => x.Name.Trim().ToLower().Equals(size));

			return result ?? sizeService
				.GetSize(uow, sizeType)
				.FirstOrDefault(x => x.Name.Trim().ToLower().Equals(size)
				                     || size.Equals(x.AlternativeName?.Trim().ToLower()));
		}
		#endregion
	}
}
