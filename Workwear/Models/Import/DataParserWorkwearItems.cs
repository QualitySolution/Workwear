using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using workwear.Domain.Stock;
using workwear.Repository.Company;
using workwear.Repository.Regulations;
using workwear.Repository.Stock;
using workwear.ViewModels.Import;
using Workwear.Measurements;

namespace workwear.Models.Import
{
	public class DataParserWorkwearItems : DataParserBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
			AddColumnName(DataTypeWorkwearItems.Fio,
				"ФИО",
				"Ф.И.О.",
				"Фамилия Имя Отчество",
				"Сотрудник"
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
		
		#region Сопоставление данных
		public void MatchChanges(
			IProgressBarDisplayable progress, 
			SettingsWorkwearItemsViewModel settings, 
			CountersViewModel counters, 
			IUnitOfWork uow, 
			IEnumerable<SheetRowWorkwearItems> list, 
			List<ImportedColumn<DataTypeWorkwearItems>> columns)
		{
			var personnelNumberColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.PersonnelNumber);
			var fioColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.Fio);
			var protectionToolsColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.ProtectionTools);
			var nomenclatureColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.Nomenclature);
			var issueDateColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.IssueDate);
			var countColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.Count);
			var postColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.Post);
			var subdivisionColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.Subdivision);
			var sizeAndGrowthColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.SizeAndGrowth);
			var sizeColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.Size);
			var growthColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.Growth);

			progress.Start(list.Count() * 2 + 3, text: "Загрузка сотрудников");
			IQueryOver<EmployeeCard, EmployeeCard> employeesQuery;
			var employeeRepository = new EmployeeRepository(uow);
			if(personnelNumberColumn != null) {
				var personnelNumbers = list.Select(x => EmployeeParse.GetPersonalNumber(settings, x, personnelNumberColumn.Index))
					.Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToArray();
				employeesQuery = employeeRepository.GetEmployeesByPersonalNumbers(personnelNumbers);
			}
			else {
				var fios = list.Select(x => GetFIO(x, fioColumn.Index));
				employeesQuery = employeeRepository.GetEmployeesByFIOs(fios);
			}

			IList<EmployeeCard> employees = employeesQuery.Fetch(SelectMode.Fetch, x => x.WorkwearItems).List();
			
			foreach(var row in list) {
				progress.Add(text: "Сопоставление");
				row.Date = ParseDateOrNull(row.CellStringValue(issueDateColumn.Index));
				if(row.Date == null) {
					row.ProgramSkipped = true;
					row.AddColumnChange(issueDateColumn, ChangeType.ParseError);
					continue;
				}

				if(row.CellIntValue(countColumn.Index) == null) {
					row.ProgramSkipped = true;
					row.AddColumnChange(countColumn, ChangeType.ParseError);
					continue;
				}

				if(personnelNumberColumn != null)
					row.Employee = employees.FirstOrDefault(x => x.PersonnelNumber == EmployeeParse.GetPersonalNumber(settings, row, personnelNumberColumn.Index));
				else
					row.Employee = employees.FirstOrDefault(x => EmployeeParse.CompareFio(x, GetFIO(row, fioColumn.Index)));

				if(row.Employee == null) {
					if(personnelNumberColumn != null)
						logger.Warn(
						$"Не найден сотрудник с табельным номером [{EmployeeParse.GetPersonalNumber(settings, row, personnelNumberColumn.Index)}]. Пропускаем.");
					else 
						logger.Warn(
							$"Не найден сотрудник с ФИО [{GetFIO(row, fioColumn.Index)}]. Пропускаем.");
					
					row.ProgramSkipped = true;
					row.AddColumnChange(personnelNumberColumn ?? fioColumn, ChangeType.NotFound);
					counters.AddCount(CountersWorkwearItems.EmployeeNotFound);
					continue;
				}

				var protectionToolName = row.CellStringValue(protectionToolsColumn.Index);
				row.WorkwearItem = row.Employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools.Name == protectionToolName);
				if(row.WorkwearItem == null && postColumn != null && subdivisionColumn != null) {
					if(!TryAddNorm(uow, row.CellStringValue(postColumn.Index), 
						row.CellStringValue(subdivisionColumn.Index), row.Employee)) {
						row.AddColumnChange(postColumn, ChangeType.NotFound);
						row.AddColumnChange(subdivisionColumn, ChangeType.NotFound);
					}
					else
						counters.AddCount(CountersWorkwearItems.EmployeesAddNorm);
				}

				if (row.WorkwearItem != null) continue;
				row.ProgramSkipped = true;
				row.AddColumnChange(protectionToolsColumn, ChangeType.NotFound);
			}

			progress.Add(text: "Загрузка номенклатуры");
			var nomenclatureTypes = new NomenclatureTypes(uow, sizeService, true);
			var nomenclatureNames = 
				list.Select(x => x.CellStringValue(nomenclatureColumn.Index)).Where(x => x != null).Distinct().ToArray();
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

				var nomenclature = 
					UsedNomenclatures.FirstOrDefault(x => String.Equals(x.Name, nomenclatureName, StringComparison.CurrentCultureIgnoreCase));
				if(nomenclature == null) {
					nomenclature = 
						nomenclatures.FirstOrDefault(x => String.Equals(x.Name, nomenclatureName, StringComparison.CurrentCultureIgnoreCase));
					if(nomenclature == null) {
						nomenclature = new Nomenclature {
							Name = nomenclatureName,
							Type = row.WorkwearItem.ProtectionTools.Type,
							Comment = "Создана при импорте выдачи из Excel",
						};
						nomenclature.Sex = nomenclatureTypes.ParseSex(nomenclature.Name) ?? ClothesSex.Universal;
						row.WorkwearItem.ProtectionTools.AddNomeclature(nomenclature);
					}
					UsedNomenclatures.Add(nomenclature);
				}

				row.Operation = operations.FirstOrDefault(group => group.Key == row.Employee)
					?.FirstOrDefault(x => x.OperationTime.Date == row.Date && x.Nomenclature == nomenclature);

				if(row.Operation != null) {
					//TODO Обновление операций не реализовано
					continue;
				}

				if (row.Operation != null) continue; 
				{
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
					TryParseSizeAndHeight(uow, row, columns);

					//Проставляем размер в сотрудника.
					if(row.Operation.Height != null) {
						var employeeSize = row.Employee.Sizes.FirstOrDefault(x => x.SizeType == row.Operation.Height.SizeType);
						if (employeeSize is null) {
							employeeSize = new EmployeeSize
								{Size = row.Operation.Height, SizeType = row.Operation.Height.SizeType, Employee = row.Employee};
							row.Employee.Sizes.Add(employeeSize);
						}
						ChangedEmployees.Add(row.Employee);
						counters.AddCount(CountersWorkwearItems.EmployeesSetSize);
					}
					if(row.Operation.WearSize != null) {
						var employeeSize = row.Employee.Sizes.FirstOrDefault(x => x.SizeType == row.Operation.WearSize.SizeType);
						if (employeeSize is null) {
							employeeSize = new EmployeeSize
								{Size = row.Operation.WearSize, SizeType = row.Operation.WearSize.SizeType, Employee = row.Employee};
							row.Employee.Sizes.Add(employeeSize);
						}
						ChangedEmployees.Add(row.Employee);
						counters.AddCount(CountersWorkwearItems.EmployeesSetSize);
					}
					var toSetChangeColumns = columns.Where(
						x => x.DataTypeEnum != DataTypeWorkwearItems.Unknown 
						     && x.DataTypeEnum != DataTypeWorkwearItems.SizeAndGrowth
						     && x.DataTypeEnum != DataTypeWorkwearItems.Size
						     && x.DataTypeEnum != DataTypeWorkwearItems.Growth
					);
					foreach(var column in toSetChangeColumns) {
						if(!row.ChangedColumns.ContainsKey(column))
							row.ChangedColumns.Add(column, new ChangeState(ChangeType.NewEntity));
					}
				}
			}
			progress.Close();
		}
		private bool TryAddNorm(IUnitOfWork uow, string postName, string subdivisionName, EmployeeCard employee) {
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
		public readonly List<Nomenclature> UsedNomenclatures = new List<Nomenclature>();
		public readonly HashSet<EmployeeCard> ChangedEmployees = new HashSet<EmployeeCard>();
		#region Helpers
		private DateTime? ParseDateOrNull(string value) {
			if(DateTime.TryParse(value, out var date))
				return date;
			return null;
		}
		private void TryParseSizeAndHeight(
			IUnitOfWork uow,
			SheetRowWorkwearItems row, 
			List<ImportedColumn<DataTypeWorkwearItems>> columns
		) 
		{
			var sizeAndGrowthColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.SizeAndGrowth);
			var sizeColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.Size);
			var heightColumn = columns.FirstOrDefault(x => x.DataTypeEnum == DataTypeWorkwearItems.Growth);
			if(sizeAndGrowthColumn != null) {
				var sizeAndGrowthValue = row.CellStringValue(sizeAndGrowthColumn.Index);
				if(String.IsNullOrWhiteSpace(sizeAndGrowthValue))
					row.ChangedColumns.Add(sizeAndGrowthColumn, new ChangeState(ChangeType.NotChanged));
				else {
					var sizeAndHeight = SizeParser.ParseSizeAndGrowth(sizeAndGrowthValue, uow, sizeService);
					bool sizeOk = true, heightOk = true;
					string error = null;
					
					if(!String.IsNullOrEmpty(sizeAndHeight.Height)) {
						if(row.Operation.ProtectionTools.Type.HeightType == null)
							row.ChangedColumns.Add(sizeAndGrowthColumn, new ChangeState(ChangeType.ParseError, error: "У типа номенклатуры не указан тип роста!"));
						else {
							row.Operation.Height = SizeParser.ParseSize(uow, sizeAndHeight.Height, sizeService, row.Operation.ProtectionTools.Type.HeightType);
							heightOk = row.Operation.Height != null;
							if(!heightOk)
								error = "Не удалось сопоставить рост.";
						}
					}
					
					if(!String.IsNullOrEmpty(sizeAndHeight.Size)) {
						if(row.Operation.ProtectionTools.Type.SizeType == null)
							row.ChangedColumns.Add(sizeAndGrowthColumn, new ChangeState(ChangeType.ParseError, error: "У типа номенклатуры не указан тип размера!"));
						else {
							row.Operation.WearSize = SizeParser.ParseSize(uow, sizeAndHeight.Size, sizeService, row.Operation.ProtectionTools.Type.SizeType);
							sizeOk = row.Operation.WearSize != null;
							if(!sizeOk)
								error = "Не удалось сопоставить размер.";
						}
					}

					row.ChangedColumns.Add(sizeAndGrowthColumn, 
						new ChangeState(sizeOk && heightOk ? ChangeType.NewEntity : ChangeType.ParseError, error: error));
				}
			}
			
			if(sizeColumn != null && row.Operation.WearSize == null) {
				var sizeValue = row.CellStringValue(sizeColumn.Index);
				if(String.IsNullOrWhiteSpace(sizeValue))
					row.ChangedColumns.Add(sizeColumn, new ChangeState(ChangeType.NotChanged));
				else if(row.Operation.ProtectionTools.Type.SizeType == null)
					row.ChangedColumns.Add(sizeColumn, new ChangeState(ChangeType.ParseError, error: "У типа номенклатуры не указан тип размера!"));
				else {
					row.Operation.WearSize = SizeParser.ParseSize(uow, sizeValue, sizeService, row.Operation.ProtectionTools.Type.SizeType);
					row.ChangedColumns.Add(sizeColumn, 
						new ChangeState(row.Operation.WearSize != null ? ChangeType.NewEntity : ChangeType.ParseError, 
							interpretedValue: sizeValue != row.Operation.WearSize?.Name ? row.Operation.WearSize?.Name : null));
				}
			}

			if(heightColumn != null && row.Operation.Height == null && row.Operation.ProtectionTools.Type.HeightType != null) {
				var heightValue = row.CellStringValue(heightColumn.Index);
				if(String.IsNullOrWhiteSpace(heightValue))
					row.ChangedColumns.Add(heightColumn, new ChangeState(ChangeType.NotChanged));
				else if(row.Operation.ProtectionTools.Type.HeightType == null)
					row.ChangedColumns.Add(heightColumn, new ChangeState(ChangeType.ParseError, error: "У типа номенклатуры не указан тип роста!"));
				else {
					row.Operation.Height =
						SizeParser.ParseSize(uow, heightValue, sizeService, row.Operation.ProtectionTools.Type.HeightType);
					row.ChangedColumns.Add(heightColumn,
						new ChangeState(row.Operation.Height != null ? ChangeType.NewEntity : ChangeType.ParseError,
							interpretedValue: heightValue != row.Operation.Height?.Name ? row.Operation.Height?.Name : null));
				}
			}
		}
	
		public FIO GetFIO(SheetRowWorkwearItems row, int fioColumn) {
			var fio = new FIO();
			row.CellStringValue(fioColumn)?.SplitFullName(out fio.LastName, out fio.FirstName, out fio.Patronymic);
			return fio;
		}
		#endregion
	}
}
