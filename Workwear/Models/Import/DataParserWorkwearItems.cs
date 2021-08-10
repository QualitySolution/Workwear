using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Repository.Company;
using workwear.Repository.Regulations;
using workwear.Repository.Stock;
using Workwear.Domain.Regulations;

namespace workwear.Models.Import
{
	public class DataParserWorkwearItems : DataParserBase<DataTypeWorkwearItems>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly NormRepository normRepository;
		private readonly ProtectionToolsRepository protectionToolsRepository;
		private readonly NomenclatureRepository nomenclatureRepository;
		private readonly PostRepository postRepository;

		public DataParserWorkwearItems(
			NormRepository normRepository, 
			ProtectionToolsRepository protectionToolsRepository,
			NomenclatureRepository nomenclatureRepository, 
			PostRepository postRepository)
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
			//AddColumnName(DataTypeWorkwearItems.PeriodAndCount,
				//"Норма выдачи"
				//);
			AddColumnName(DataTypeWorkwearItems.SizeAndGrowth,
				"Характеристика номенклатуры"
				);
			AddColumnName(DataTypeWorkwearItems.IssueDate,
				"Дата выдачи"
				);
			AddColumnName(DataTypeWorkwearItems.Count,
				"Количество"
				);

			this.normRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));
			this.protectionToolsRepository = protectionToolsRepository ?? throw new ArgumentNullException(nameof(protectionToolsRepository));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.postRepository = postRepository;
		}

		private void AddColumnName(DataTypeWorkwearItems type, params string[] names)
		{
			foreach(var name in names)
				ColumnNames.Add(name.ToLower(), type);
		}

		#region Сопоставление данных

		public void MatchChanges(IUnitOfWork uow, IEnumerable<SheetRowWorkwearItems> list, List<ImportedColumn<DataTypeWorkwearItems>> columns)
		{
			var personnelNumberColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.PersonnelNumber);
			var protectionToolsColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.ProtectionTools);
			var nomenclatureColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Nomenclature);
			var issuedateColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.IssueDate);
			var countColumn = columns.FirstOrDefault(x => x.DataType == DataTypeWorkwearItems.Count);

			var personnelNumbers = list.Select(x => x.CellValue(personnelNumberColumn.Index))
				.Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToArray();

			var employees = uow.Session.QueryOver<EmployeeCard>()
				.Where(x => x.PersonnelNumber.IsIn(personnelNumbers))
				.Fetch(SelectMode.Fetch, x => x.WorkwearItems)
				.List();

			foreach(var row in list) {
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
					continue;
				}

				var protectionToolName = row.CellValue(protectionToolsColumn.Index);
				row.WorkwearItem = row.Employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools.Name == protectionToolName);
				if(row.WorkwearItem == null) {
					row.Skiped = true;
					row.ChangedColumns.Add(protectionToolsColumn, ChangeType.NotFound);
					continue;
				}
			}

			var nomenclatureTypes = new NomenclatureTypes(uow, true);
			var nomenclatureNames = list.Select(x => x.CellValue(nomenclatureColumn.Index)).Where(x => x != null).Distinct().ToArray();
			var nomenclatures = nomenclatureRepository.GetNomenclatureByName(uow, nomenclatureNames);

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

			foreach(var row in list.Where(x => !x.Skiped)) {

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
						row.WorkwearItem.ProtectionTools.AddNomeclature(nomenclature);
						//nomenclature.Type = nomenclatureTypes.ParseNomenclatureName(nomeclatureName);
						//if(nomenclature.Type == null) {
						//	nomenclature.Type = nomenclatureTypes.GetUnknownType();
						//	UndefinedNomeclatureNames.Add(nomeclatureName);
						//}
					}
					UsedNomeclature.Add(nomenclature);
				}

				row.Operation = operations.FirstOrDefault(group => group.Key == row.Employee)
					?.FirstOrDefault(x => x.OperationTime.Date == row.Date && x.Nomenclature == nomenclature);

				if(row.Operation != null) {
					//Обновление операций не реализовано
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
						//FIXME Размеры
						//Size =
						//WearGrowth =  
					};

					foreach(var column in columns.Where(x => x.DataType != DataTypeWorkwearItems.Unknown))
						row.ChangedColumns.Add(column, ChangeType.NewEntity);
				}
			}
		}

		#endregion


		public readonly List<Nomenclature> UsedNomeclature = new List<Nomenclature>();
		//public List<string> UndefinedNomeclatureNames = new List<string>();
		
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
