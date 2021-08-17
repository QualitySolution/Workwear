using System;
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
			AddColumnName(DataTypeWorkwearItems.SizeAndGrowth,
				"Характеристика номенклатуры"
				);
			AddColumnName(DataTypeWorkwearItems.IssueDate,
				"Дата выдачи"
				);
			AddColumnName(DataTypeWorkwearItems.Count,
				"Количество"
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

		public void MatchChanges(IProgressBarDisplayable progress, CountersViewModel counters, IUnitOfWork uow, IEnumerable<SheetRowWorkwearItems> list, List<ImportedColumn<DataTypeWorkwearItems>> columns)
		{
			var personnelNumberColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.PersonnelNumber);
			var protectionToolsColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.ProtectionTools);
			var nomenclatureColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Nomenclature);
			var issuedateColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.IssueDate);
			var countColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Count);
			var sizeAndGrowthColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.SizeAndGrowth);
			var postColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Post);
			var subdivisionColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Subdivision);

			progress.Start(list.Count() * 2 + 3, text: "Загрузка сотрудников");
			var personnelNumbers = list.Select(x => x.CellValue(personnelNumberColumn.Index))
				.Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToArray();

			var employees = uow.Session.QueryOver<EmployeeCard>()
				.Where(x => x.PersonnelNumber.IsIn(personnelNumbers))
				.Fetch(SelectMode.Fetch, x => x.WorkwearItems)
				.List();

			foreach(var row in list) {
				progress.Add(text: "Сопоставление");
				row.Date = ParseDateOrNull(row.CellValue(issuedateColumn.Index));
				if(row.Date == null) {
					row.Skiped = true;
					row.ChangedColumns.Add(issuedateColumn, ChangeType.ParseError);
					continue;
				}

				if(row.CellIntValue(countColumn.Index) == null) {
					row.Skiped = true;
					row.ChangedColumns.Add(countColumn, ChangeType.ParseError);
					continue;
				}

				row.Employee = employees.FirstOrDefault(x => x.PersonnelNumber == row.CellValue(personnelNumberColumn.Index));
				if(row.Employee == null) {
					row.Skiped = true;
					row.ChangedColumns.Add(personnelNumberColumn, ChangeType.NotFound);
					counters.AddCount(CountersWorkwearItems.EmployeeNotFound);
					continue;
				}

				var protectionToolName = row.CellValue(protectionToolsColumn.Index);
				row.WorkwearItem = row.Employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools.Name == protectionToolName);
				if(row.WorkwearItem == null) {
					if(!TryAddNorm(uow, row.CellValue(postColumn.Index), row.CellValue(subdivisionColumn.Index), row.Employee)) {
						row.ChangedColumns.Add(postColumn, ChangeType.NotFound);
						row.ChangedColumns.Add(subdivisionColumn, ChangeType.NotFound);
					}
					else
						counters.AddCount(CountersWorkwearItems.EmployeesAddNorm);
				}
				if(row.WorkwearItem == null) {
					row.Skiped = true;
					row.ChangedColumns.Add(protectionToolsColumn, ChangeType.NotFound);
					continue;
				}
			}

			progress.Add(text: "Загрузка номеклатуры");
			var nomenclatureTypes = new NomenclatureTypes(uow, true);
			var nomenclatureNames = list.Select(x => x.CellValue(nomenclatureColumn.Index)).Where(x => x != null).Distinct().ToArray();
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
				if(row.Skiped)
					continue;
				var nomeclatureName = row.CellValue(nomenclatureColumn.Index);
				if(String.IsNullOrWhiteSpace(nomeclatureName)) {
					row.ChangedColumns.Add(nomenclatureColumn, ChangeType.NotFound);
					continue;
				}

				var nomenclature = UsedNomeclature.FirstOrDefault(x => String.Equals(x.Name, nomeclatureName, StringComparison.CurrentCultureIgnoreCase));
				if(nomenclature == null) {
					nomenclature = nomenclatures.FirstOrDefault(x => String.Equals(x.Name, nomeclatureName, StringComparison.CurrentCultureIgnoreCase));
					if(nomenclature == null) {
						nomenclature = new Nomenclature {
							Name = nomeclatureName,
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
					var sizeAndGrowthValue = row.CellValue(sizeAndGrowthColumn.Index);
					if(!String.IsNullOrEmpty(sizeAndGrowthValue)) {
						var sizeAndGrowth = SizeParser.ParseSizeAndGrowth(sizeAndGrowthValue);
						row.Operation.Size = sizeAndGrowth.Size;
						row.Operation.WearGrowth = sizeAndGrowth.Growth;
						//Если стандарт размера не заполнен для номеклатуры, проставляем его, по обнаруженному размеру.
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
						row.ChangedColumns.Add(sizeAndGrowthColumn, sizeOk && growthOk ? ChangeType.NewEntity : ChangeType.ParseError);
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

					foreach(var column in columns.Where(x => x.DataType != DataTypeWorkwearItems.Unknown && x.DataType != DataTypeWorkwearItems.SizeAndGrowth)) {
						if(!row.ChangedColumns.ContainsKey(column))
							row.ChangedColumns.Add(column, ChangeType.NewEntity);
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

		#endregion
	}
}
