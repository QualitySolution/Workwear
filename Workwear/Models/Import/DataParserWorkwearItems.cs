﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Stock;
using workwear.Measurements;
using workwear.Repository.Company;
using workwear.Repository.Regulations;
using workwear.Repository.Stock;
using workwear.ViewModels.Import;
using Workwear.Domain.Regulations;
using Workwear.Measurements;

namespace workwear.Models.Import
{
	public class DataParserWorkwearItems : DataParserBase<DataTypeWorkwearItems>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly NomenclatureRepository nomenclatureRepository;
		private readonly PostRepository postRepository;
		private readonly NormRepository normRepository;
		private readonly SizeService sizeService;

		public DataParserWorkwearItems(
			NomenclatureRepository nomenclatureRepository,
			PostRepository postRepository,
			NormRepository normRepository,
			SizeService sizeService)
		{
			AddColumnName(DataTypeWorkwearItems.PersonnelNumber,
				"Табельный номер"
				);
			AddColumnName(DataTypeWorkwearItems.ProtectionTools,
				"Номенклатура нормы"
				);
			AddColumnName(DataTypeWorkwearItems.Nomenclature,
				"Номенклатура"
				);
			AddColumnName(DataTypeWorkwearItems.Subdivision,
				"Подразделение"
				);
			AddColumnName(DataTypeWorkwearItems.Post,
				"Должность"
				);
			AddColumnName(DataTypeWorkwearItems.Size,
				"Размер",
				"Значение антропометрии" //ОСМиБТ
				);
			AddColumnName(DataTypeWorkwearItems.Growth,
				"Рост"
				);
			AddColumnName(DataTypeWorkwearItems.SizeAndGrowth,
				"Характеристика"
				);
			AddColumnName(DataTypeWorkwearItems.IssueDate,
				"Дата выдачи"
				);
			AddColumnName(DataTypeWorkwearItems.Count,
				"Количество",
				"Кол-во"
				);
				
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
			this.normRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
		}

		private void AddColumnName(DataTypeWorkwearItems type, params string[] names)
		{
			foreach(var name in names)
				ColumnNames.Add(name.ToLower(), type);
		}

		#region Сопоставление данных

		public void MatchChanges(IProgressBarDisplayable progress, SettingsWorkwearItemsViewModel settings, CountersViewModel counters, IUnitOfWork uow, IEnumerable<SheetRowWorkwearItems> list, List<ImportedColumn<DataTypeWorkwearItems>> columns)
		{
			var personnelNumberColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.PersonnelNumber);
			var protectionToolsColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.ProtectionTools);
			var nomenclatureColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Nomenclature);
			var issuedateColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.IssueDate);
			var countColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Count);
			var postColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Post);
			var subdivisionColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Subdivision);
			var sizeAndGrowthColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.SizeAndGrowth);
			var sizeColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Size);
			var growthColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Growth);

			progress.Start(list.Count() * 2 + 3, text: "Загрузка сотрудников");
			var personnelNumbers = list.Select(x => GetPersonalNumber(settings, x, personnelNumberColumn.Index))
				.Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToArray();

			var employees = uow.Session.QueryOver<EmployeeCard>()
				.Where(x => x.PersonnelNumber.IsIn(personnelNumbers))
				.Fetch(SelectMode.Fetch, x => x.WorkwearItems)
				.List();

			foreach(var row in list) {
				progress.Add(text: "Сопоставление");
				row.Date = ParseDateOrNull(row.CellStringValue(issuedateColumn.Index));
				if(row.Date == null) {
					row.ProgramSkipped = true;
					row.AddColumnChange(issuedateColumn, ChangeType.ParseError);
					continue;
				}

				if(row.CellIntValue(countColumn.Index) == null) {
					row.ProgramSkipped = true;
					row.AddColumnChange(countColumn, ChangeType.ParseError);
					continue;
				}

				row.Employee = employees.FirstOrDefault(x => x.PersonnelNumber == GetPersonalNumber(settings, row, personnelNumberColumn.Index));
				if(row.Employee == null) {
					logger.Warn($"Не найден сотрудник в табельным номером [{GetPersonalNumber(settings, row, personnelNumberColumn.Index)}]. Пропускаем.");
					row.ProgramSkipped = true;
					row.AddColumnChange(personnelNumberColumn, ChangeType.NotFound);
					counters.AddCount(CountersWorkwearItems.EmployeeNotFound);
					continue;
				}

				var protectionToolName = row.CellStringValue(protectionToolsColumn.Index);
				row.WorkwearItem = row.Employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools.Name == protectionToolName);
				if(row.WorkwearItem == null && postColumn != null && subdivisionColumn != null) {
					if(!TryAddNorm(uow, row.CellStringValue(postColumn.Index), row.CellStringValue(subdivisionColumn.Index), row.Employee)) {
						row.AddColumnChange(postColumn, ChangeType.NotFound);
						row.AddColumnChange(subdivisionColumn, ChangeType.NotFound);
					}
					else
						counters.AddCount(CountersWorkwearItems.EmployeesAddNorm);
				}
				if(row.WorkwearItem == null) {
					row.ProgramSkipped = true;
					row.AddColumnChange(protectionToolsColumn, ChangeType.NotFound);
					continue;
				}
			}

			progress.Add(text: "Загрузка номенклатуры");
			var nomenclatureTypes = new NomenclatureTypes(uow, true);
			var nomenclatureNames = list.Select(x => x.CellStringValue(nomenclatureColumn.Index)).Where(x => x != null).Distinct().ToArray();
			var nomenclatures = nomenclatureRepository.GetNomenclatureByName(uow, nomenclatureNames);

			progress.Add(text: "Загрузка операций выдачи");
			var dates = list
				.Select(x => x.Date)
				.Where(x => x != null)
				.Select(x => x.Value);
			var startDate = dates.Min();
			var endDate = dates.Max();
			var employeeIds = list.Where(x => x.Employee != null)
				.Select(x => x.Employee.Id).Distinct().ToArray();
			var operations = uow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(x => x.Employee.Id.IsIn(employeeIds))
				.Where(x => x.OperationTime >= startDate)
				.Where(x => x.OperationTime < endDate.AddDays(1))
				.List()
				.GroupBy(x => x.Employee);

			foreach(var row in list) {
				progress.Add(text: "Обработка операций выдачи");
				if(row.Skipped)
					continue;
				var nomenclatureName = row.CellStringValue(nomenclatureColumn.Index);
				if(String.IsNullOrWhiteSpace(nomenclatureName)) {
					row.AddColumnChange(nomenclatureColumn, ChangeType.NotFound);
					continue;
				}

				var nomenclature = UsedNomeclature.FirstOrDefault(x => String.Equals(x.Name, nomenclatureName, StringComparison.CurrentCultureIgnoreCase));
				if(nomenclature == null) {
					nomenclature = nomenclatures.FirstOrDefault(x => String.Equals(x.Name, nomenclatureName, StringComparison.CurrentCultureIgnoreCase));
					if(nomenclature == null) {
						nomenclature = new Nomenclature {
							Name = nomenclatureName,
							Type = row.WorkwearItem.ProtectionTools.Type,
							Comment = "Создана при импорте выдачи из Excel",
						};
						if(nomenclature.Type.WearCategory != null && SizeHelper.HasClothesSex(nomenclature.Type.WearCategory.Value)) {
							nomenclature.Sex = nomenclatureTypes.ParseSex(nomenclature.Name) ?? ClothesSex.Universal;
						}
						row.WorkwearItem.ProtectionTools.AddNomeclature(nomenclature);
					}
					UsedNomeclature.Add(nomenclature);
				}

				row.Operation = operations.FirstOrDefault(group => group.Key == row.Employee)
					?.FirstOrDefault(x => x.OperationTime.Date == row.Date && x.Nomenclature == nomenclature);

				if(row.Operation != null) {
					//TODO Обновление операций не реализовано
					continue;
				}

				if(row.Operation == null) {
					var count = row.CellIntValue(countColumn.Index).Value;
					var expenseDate = row.WorkwearItem.ActiveNormItem.CalculateExpireDate(row.Date.Value, count);
					row.Operation = new EmployeeIssueOperation {
						OperationTime = row.Date.Value,
						Employee = row.Employee,
						Issued = count,
						Nomenclature = nomenclature,
						AutoWriteoffDate = expenseDate,
						ExpiryByNorm = expenseDate,
						NormItem = row.WorkwearItem.ActiveNormItem,
						ProtectionTools = row.WorkwearItem.ProtectionTools,
						Returned = 0,
						StartOfUse = row.Date,
						UseAutoWriteoff = expenseDate != null,
					};
					//Обрабатываем размер.
					if(TryParseSizeAndGrowth(row, columns, out SizeAndGrowth sizeAndGrowth)) {
						row.Operation.Size = sizeAndGrowth.Size;
						row.Operation.WearGrowth = sizeAndGrowth.Growth;
						//Если стандарт размера не заполнен для номенклатуры, проставляем его, по обнаруженному размеру.
						if(!String.IsNullOrEmpty(sizeAndGrowth.Size) 
							&& SizeHelper.HasСlothesSizeStd(nomenclature.Type.WearCategory.Value)
							&& String.IsNullOrEmpty(nomenclature.SizeStd)) {
							var standarts = SizeHelper.GetSizeStandartsEnum(nomenclature.Type.WearCategory.Value, nomenclature.Sex);
								nomenclature.SizeStd = sizeService.GetAllSizesForNomeclature(standarts).FirstOrDefault(x => x.Size == sizeAndGrowth.Size)?.StandardCode;
						}

						bool sizeOk = !SizeHelper.HasСlothesSizeStd(nomenclature.Type.WearCategory.Value)
									|| sizeService.GetSizesForNomeclature(nomenclature.SizeStd).Contains(sizeAndGrowth.Size);
						bool growthOk = !SizeHelper.HasGrowthStandart(nomenclature.Type.WearCategory.Value)
							|| (nomenclature.SizeStd?.EndsWith("Intl", StringComparison.InvariantCulture) ?? false)
							|| sizeService.GetGrowthForNomenclature().Contains(sizeAndGrowth.Growth);
						if(sizeAndGrowthColumn != null)
							row.ChangedColumns.Add(sizeAndGrowthColumn, new ChangeState( sizeOk && growthOk ? ChangeType.NewEntity : ChangeType.ParseError));
						if(sizeColumn != null)
							row.ChangedColumns.Add(sizeColumn, new ChangeState(sizeOk ? ChangeType.NewEntity : ChangeType.ParseError));
						if(growthColumn != null)
							row.ChangedColumns.Add(growthColumn, new ChangeState(growthOk ? ChangeType.NewEntity : ChangeType.ParseError));
					}
					//Проставляем размер в сотрудника.
					if(!String.IsNullOrEmpty(row.Operation.WearGrowth) && String.IsNullOrEmpty(row.Employee.WearGrowth)) {
						row.Employee.WearGrowth = row.Operation.WearGrowth;
						ChangedEmployees.Add(row.Employee);
						counters.AddCount(CountersWorkwearItems.EmployeesSetSize);
					}
					if(!String.IsNullOrEmpty(row.Operation.Size)) {
						var employeeSize = row.Employee.GetSize(nomenclature.Type.WearCategory.Value);
						if(employeeSize != null && String.IsNullOrEmpty(employeeSize.Size)) {
							var newSize = new SizePair(nomenclature.SizeStd, row.Operation.Size);
							row.Employee.SetSize(nomenclature.Type.WearCategory.Value, newSize);
							ChangedEmployees.Add(row.Employee);
							counters.AddCount(CountersWorkwearItems.EmployeesSetSize);
						}
					}
					var toSetChangeColumns = columns.Where(
						x => x.DataType != DataTypeWorkwearItems.Unknown 
						  && x.DataType != DataTypeWorkwearItems.SizeAndGrowth
						  && x.DataType != DataTypeWorkwearItems.Size
						  && x.DataType != DataTypeWorkwearItems.Growth
						);
					foreach(var column in toSetChangeColumns) {
						if(!row.ChangedColumns.ContainsKey(column))
							row.ChangedColumns.Add(column, new ChangeState(ChangeType.NewEntity));
					}
				}
			}
			progress.Close();
		}


		private bool TryAddNorm(IUnitOfWork uow, string postName, string subdivisionName, EmployeeCard employee)
		{
			var post = postRepository.GetPostByName(uow, postName, subdivisionName);
			if(post == null)
				return false;

			var norm = normRepository.GetNormsForPost(uow, post);
			if(!norm.Any())
				return false;

			//FIXME в идеале здесь перебирать все нормы и искать в них именно тот СИЗ который выдается.
			employee.AddUsedNorm(norm.First());
			ChangedEmployees.Add(employee);
			return true;
		}
		#endregion

		public readonly List<Nomenclature> UsedNomeclature = new List<Nomenclature>();
		public readonly HashSet<EmployeeCard> ChangedEmployees = new HashSet<EmployeeCard>();
		
		#region Helpers
		DateTime? ParseDateOrNull(string value)
		{
			if(DateTime.TryParse(value, out DateTime date))
				return date;
			else
				return null;
		}
		
		private bool TryParsePeriodAndCount(string value, out int amount, out int periods, out NormPeriodType periodType)
		{
			amount = 0;
			periods = 0;
			if(value.ToLower().Contains("до износа")) {
				amount = 1;
				periodType = NormPeriodType.Wearout;
				return true;
			}

			var regexp = new Regex(@"\((\d+) в (\d+) (месяц|месяца|месяцев)\)");
			periodType = NormPeriodType.Month;
			var matches = regexp.Matches(value);
			if(matches.Count == 0)
				return false;
			var parts = matches[0].Groups;
			if(parts.Count != 4)
				return false;
			amount = int.Parse(parts[1].Value);
			periods = int.Parse(parts[2].Value);
			return true;
		}

		private bool TryParseSizeAndGrowth(SheetRowWorkwearItems row, List<ImportedColumn<DataTypeWorkwearItems>> columns, out SizeAndGrowth sizeAndGrowth)
		{
			sizeAndGrowth = new SizeAndGrowth();
			var sizeAndGrowthColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.SizeAndGrowth);
			var sizeColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Size);
			var growthColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Growth);
			if(sizeAndGrowthColumn != null) {
				var sizeAndGrowthValue = row.CellStringValue(sizeAndGrowthColumn.Index);
				if(!String.IsNullOrEmpty(sizeAndGrowthValue))
					sizeAndGrowth = SizeParser.ParseSizeAndGrowth(sizeAndGrowthValue);
			};
			if(sizeColumn != null && String.IsNullOrEmpty(sizeAndGrowth.Size))
				sizeAndGrowth.Size = SizeParser.ParseSize(row.CellStringValue(sizeColumn.Index));
			if(growthColumn != null && String.IsNullOrEmpty(sizeAndGrowth.Growth))
				sizeAndGrowth.Growth = SizeParser.ParseSize(row.CellStringValue(sizeColumn.Index));
			return !String.IsNullOrEmpty(sizeAndGrowth.Size) || !String.IsNullOrEmpty(sizeAndGrowth.Growth);
		}

		public string GetPersonalNumber(SettingsWorkwearItemsViewModel settings, SheetRowWorkwearItems row, int columnIndex)
		{
			var original = settings.ConvertPersonnelNumber ? EmployeeParse.ConvertPersonnelNumber(row.CellStringValue(columnIndex)) : row.CellStringValue(columnIndex);
			return original?.Trim();
		}

		#endregion
	}
}
