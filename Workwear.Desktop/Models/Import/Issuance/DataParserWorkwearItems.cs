using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Measurements;
using Workwear.Models.Import;
using Workwear.Repository.Company;
using Workwear.Repository.Regulations;
using Workwear.Repository.Stock;
using Workwear.ViewModels.Import;

namespace Workwear.Models.Import.Issuance
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
			AddColumnName(DataTypeWorkwearItems.NameWithInitials);
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
				"Дата выдачи",
				"дата получения"
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
			IImportModel importModel,
			IProgressBarDisplayable progress, 
			SettingsWorkwearItemsViewModel settings, 
			CountersViewModel counters, 
			IUnitOfWork uow, 
			IEnumerable<SheetRowWorkwearItems> list)
		{
			var personnelNumberColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.PersonnelNumber);
			var fioColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.Fio);
			var nameWithInitialsColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.NameWithInitials);
			var protectionToolsColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.ProtectionTools);
			var nomenclatureColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.Nomenclature);
			var issueDateColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.IssueDate);
			var countColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.Count);
			var postColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.Post);
			var subdivisionColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.Subdivision);

			progress.Start(list.Count() * 2 + 3, text: "Загрузка сотрудников");
			IQueryOver<EmployeeCard, EmployeeCard> employeesQuery;
			var employeeRepository = new EmployeeRepository(uow);
			if(personnelNumberColumn != null) {
				var personnelNumbers = list.Select(x => EmployeeParse.GetPersonalNumber(settings, x, personnelNumberColumn))
					.Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToArray();
				employeesQuery = employeeRepository.GetEmployeesByPersonalNumbers(personnelNumbers);
			}
			else if(fioColumn != null) {
				var fios = list.Select(x => GetFIO(x, fioColumn))
					.Where(x => !x.IsEmpty).Distinct();
				employeesQuery = employeeRepository.GetEmployeesByFIOs(fios);
			}
			else {
				employeesQuery = employeeRepository.ActiveEmployeesQuery();
			}

			IList<EmployeeCard> employees = employeesQuery
				.Fetch(SelectMode.Fetch, x => x.WorkwearItems)
				.List();
			
			foreach(var row in list) {
				progress.Add(text: "Сопоставление");
				row.Date = row.CellDateTimeValue(issueDateColumn);
				if(row.Date == null) {
					row.ProgramSkipped = true;
					row.AddColumnChange(issueDateColumn, ChangeType.ParseError);
					continue;
				}

				if(countColumn != null && row.CellIntValue(countColumn) == null) {
					row.ProgramSkipped = true;
					row.AddColumnChange(countColumn, ChangeType.ParseError);
					continue;
				}

				if(personnelNumberColumn != null)
					row.Employee = employees.FirstOrDefault(x => x.PersonnelNumber == EmployeeParse.GetPersonalNumber(settings, row, personnelNumberColumn));
				else if(fioColumn != null)
					row.Employee = employees.FirstOrDefault(x => EmployeeParse.CompareFio(x, GetFIO(row, fioColumn)));
				else 
					row.Employee = employees.FirstOrDefault(x => EmployeeParse.CompareNameWithInitials(x, row.CellStringValue(nameWithInitialsColumn)));

				if(row.Employee == null) {
					if(personnelNumberColumn != null)
						logger.Warn(
						$"Не найден сотрудник с табельным номером [{EmployeeParse.GetPersonalNumber(settings, row, personnelNumberColumn)}]. Пропускаем.");
					else if (fioColumn != null)
						logger.Warn($"Не найден сотрудник с ФИО [{GetFIO(row, fioColumn).FullName}]. Пропускаем.");
					else 
						logger.Warn($"Не найден сотрудник [{row.CellStringValue(nameWithInitialsColumn)}]. Пропускаем.");
					
					row.ProgramSkipped = true;
					row.AddColumnChange(personnelNumberColumn ?? fioColumn ?? nameWithInitialsColumn, ChangeType.NotFound);
					counters.AddCount(CountersWorkwearItems.EmployeeNotFound);
					continue;
				}

				var protectionToolName = TextParser.PrepareForCompare(row.CellStringValue(protectionToolsColumn));
				row.WorkwearItem = row.Employee.WorkwearItems.FirstOrDefault(x => TextParser.PrepareForCompare(x.ProtectionTools.Name) == protectionToolName);
				if(row.WorkwearItem == null && postColumn != null && subdivisionColumn != null) {
					if(!TryAddNorm(uow, row.CellStringValue(postColumn), 
						row.CellStringValue(subdivisionColumn), row.Employee)) {
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
			IList<Nomenclature> nomenclatures = new List<Nomenclature>();
			if(nomenclatureColumn != null) {
				var nomenclatureNames = 
					list.Select(x => x.CellStringValue(nomenclatureColumn)).Where(x => x != null).Distinct().ToArray();
				nomenclatures = nomenclatureRepository.GetNomenclatureByName(uow, nomenclatureNames);
			}

			progress.Add(text: "Загрузка операций выдачи");
			var dates = list
				.Select(x => x.Date)
				.Where(x => x != null)
				.Select(x => x.Value);
			IList<EmployeeIssueOperation> loadedOperations = new List<EmployeeIssueOperation>();
			if(dates.Any()) {
				var startDate = dates.Min();
				var endDate = dates.Max();
				var employeeIds = list.Where(x => x.Employee != null)
					.Select(x => x.Employee.Id).Distinct().ToArray();
				loadedOperations = uow.Session.QueryOver<EmployeeIssueOperation>()
					.Where(x => x.Employee.Id.IsIn(employeeIds))
					.Where(x => x.OperationTime >= startDate)
					.Where(x => x.OperationTime < endDate.AddDays(1))
					.Where(x => x.ManualOperation)
					.List();
			}
			var operations = loadedOperations.GroupBy(x => x.Employee);
			foreach(var row in list) {
				progress.Add(text: "Обработка операций выдачи");
				if(row.Skipped)
					continue;

				Nomenclature nomenclature = null;
				if(nomenclatureColumn != null) {
					var nomenclatureName = row.CellStringValue(nomenclatureColumn);
					if(String.IsNullOrWhiteSpace(nomenclatureName)) {
						row.AddColumnChange(nomenclatureColumn, ChangeType.NotFound);
						continue;
					}

					nomenclature =
						UsedNomenclatures.FirstOrDefault(x =>
							String.Equals(x.Name, nomenclatureName, StringComparison.CurrentCultureIgnoreCase));
					if(nomenclature == null) {
						nomenclature =
							nomenclatures.FirstOrDefault(x =>
								String.Equals(x.Name, nomenclatureName, StringComparison.CurrentCultureIgnoreCase));
						if(nomenclature == null) {
							nomenclature = new Nomenclature {
								Name = nomenclatureName,
								Type = row.WorkwearItem.ProtectionTools.Type,
								Comment = "Создана при импорте выдачи из Excel",
							};
							nomenclature.Sex = nomenclatureTypes.ParseSex(nomenclature.Name);
							row.WorkwearItem.ProtectionTools.AddNomeclature(nomenclature);
						}

						UsedNomenclatures.Add(nomenclature);
					}
					
					row.Operation = operations.FirstOrDefault(group => group.Key == row.Employee)
						?.FirstOrDefault(x => x.OperationTime.Date == row.Date && x.Nomenclature == nomenclature);
				}
				else {
					row.Operation = operations.FirstOrDefault(group => group.Key == row.Employee)
						?.FirstOrDefault(x => x.OperationTime.Date == row.Date && x.ProtectionTools == row.WorkwearItem.ProtectionTools);
				}

				if(row.Operation != null) {
					//TODO Обновление операций не реализовано
					row.ProgramSkipped = true;
					row.ProgramSkippedReason = "Обновление операций не реализовано";
					logger.Info("Обновление операций не реализовано, пропускаем...");
					continue;
				}
				
				var count = countColumn != null ? row.CellIntValue(countColumn).Value : row.WorkwearItem.ActiveNormItem.Amount;
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
					ManualOperation = true, //Загруженные из Excel операции будут выглядеть как ручные.
				};
				//Обрабатываем размер.
				TryParseSizeAndHeight(uow, row, importModel);

				//Проставляем размер в сотрудника.
				if(row.Operation.Height != null) {
					var employeeSize = row.Employee.Sizes.FirstOrDefault(x => x.SizeType == row.Operation.Height.SizeType);
					if (employeeSize is null) {
						employeeSize = new EmployeeSize
							{Size = row.Operation.Height, SizeType = row.Operation.Height.SizeType, Employee = row.Employee};
						AddSetEmployeeSize(row, employeeSize, counters);
					}
				}
				if(row.Operation.WearSize != null) {
					var employeeSize = row.Employee.Sizes.FirstOrDefault(x => x.SizeType == row.Operation.WearSize.SizeType);
					if (employeeSize is null) {
						employeeSize = new EmployeeSize
							{Size = row.Operation.WearSize, SizeType = row.Operation.WearSize.SizeType, Employee = row.Employee};
						AddSetEmployeeSize(row, employeeSize, counters);
					}
				}
				//FIXME Пока не знаю что делать с этим кодом, возможно нужно все сильно перефигачить чтобы этого кода здесь не было.
				// var toSetChangeColumns = columns.Where(
				// 	x => x.DataTypeEnum != DataTypeWorkwearItems.Unknown 
				// 	     && x.DataTypeEnum != DataTypeWorkwearItems.SizeAndGrowth
				// 	     && x.DataTypeEnum != DataTypeWorkwearItems.Size
				// 	     && x.DataTypeEnum != DataTypeWorkwearItems.Growth
				// );
				// foreach(var column in toSetChangeColumns) {
				// 	if(!row.ChangedColumns.ContainsKey(column))
				// 		row.ChangedColumns.Add(column, new ChangeState(ChangeType.NewEntity));
				// }
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

		private void AddSetEmployeeSize(SheetRowWorkwearItems row, EmployeeSize employeeSize, CountersViewModel counters) {
			row.Employee.Sizes.Add(employeeSize);
			ChangedEmployees.Add(row.Employee);
			counters.AddCount(CountersWorkwearItems.EmployeesSetSize);

			var stateSizeAndHeight = row.ChangedColumns.FirstOrDefault(x => DataTypeWorkwearItems.SizeAndGrowth.Equals(x.Key.DataTypeByLevels[row.RowLevel].DataType?.Data));
			if(stateSizeAndHeight.Value != null && (new [] {ChangeType.NewEntity, ChangeType.ChangeValue}.Contains(stateSizeAndHeight.Value.ChangeType))){
				stateSizeAndHeight.Value.AddCreatedValues(employeeSize.Title);
			}

			if(employeeSize.SizeType.CategorySizeType == CategorySizeType.Height) {
				var stateHeight = row.ChangedColumns.FirstOrDefault(x => DataTypeWorkwearItems.Growth.Equals(x.Key.DataTypeByLevels[row.RowLevel].DataType?.Data));
				if(stateHeight.Value != null && (new [] {ChangeType.NewEntity, ChangeType.ChangeValue}.Contains(stateHeight.Value.ChangeType))){
					stateHeight.Value.AddCreatedValues(employeeSize.Title);
				}	
			}
			
			if(employeeSize.SizeType.CategorySizeType == CategorySizeType.Size) {
				var stateSize = row.ChangedColumns.FirstOrDefault(x => DataTypeWorkwearItems.Size.Equals(x.Key.DataTypeByLevels[row.RowLevel].DataType?.Data));
				if(stateSize.Value != null && (new [] {ChangeType.NewEntity, ChangeType.ChangeValue}.Contains(stateSize.Value.ChangeType))){
					stateSize.Value.AddCreatedValues(employeeSize.Title);
				}	
			}
		}
		#endregion
		public readonly List<Nomenclature> UsedNomenclatures = new List<Nomenclature>();
		public readonly HashSet<EmployeeCard> ChangedEmployees = new HashSet<EmployeeCard>();
		#region Helpers
		private void TryParseSizeAndHeight(
			IUnitOfWork uow,
			SheetRowWorkwearItems row, 
			IImportModel importModel
		) 
		{
			var sizeAndGrowthColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.SizeAndGrowth);
			var sizeColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.Size);
			var heightColumn = importModel.GetColumnForDataType(DataTypeWorkwearItems.Growth);
			if(sizeAndGrowthColumn != null) {
				var sizeAndGrowthValue = row.CellStringValue(sizeAndGrowthColumn);
				var state = GetChangeStateSizeAndHeight(sizeAndGrowthValue, uow, row);
				row.AddColumnChange(sizeAndGrowthColumn, state);
			}
			
			if(sizeColumn != null && row.Operation.WearSize == null) {
				var sizeValue = row.CellStringValue(sizeColumn);
				row.AddColumnChange(sizeColumn, GetChangeStateSizeAndHeight(sizeValue, uow, row));
			}

			if(heightColumn != null && row.Operation.Height == null) {
				var heightValue = row.CellStringValue(heightColumn);
				row.AddColumnChange(heightColumn, GetChangeStateSizeAndHeight(heightValue, uow, row));
			}
		}

		#region MakeChanges
		private ChangeState GetChangeStateSizeAndHeight(string sizeAndGrowthValue, IUnitOfWork uow, SheetRowWorkwearItems row) {
			if(String.IsNullOrWhiteSpace(sizeAndGrowthValue))
				return new ChangeState(ChangeType.NotChanged);
			else {
				var sizeAndHeight = SizeParser.ParseSizeAndGrowth(sizeAndGrowthValue, uow, sizeService);
				bool sizeOk = true, heightOk = true;
				string error = null;
					
				if(!String.IsNullOrEmpty(sizeAndHeight.Height)) {
					if(row.Operation.ProtectionTools.Type.HeightType == null)
						return new ChangeState(ChangeType.ParseError, error: "У типа номенклатуры не указан тип роста!");
					else {
						row.Operation.Height = SizeParser.ParseSize(uow, sizeAndHeight.Height, sizeService, row.Operation.ProtectionTools.Type.HeightType);
						heightOk = row.Operation.Height != null;
						if(!heightOk)
							error = "Не удалось сопоставить рост.";
					}
				}
					
				if(!String.IsNullOrEmpty(sizeAndHeight.Size)) {
					if(row.Operation.ProtectionTools.Type.SizeType == null)
						return new ChangeState(ChangeType.ParseError, error: "У типа номенклатуры не указан тип размера!");
					else {
						row.Operation.WearSize = SizeParser.ParseSize(uow, sizeAndHeight.Size, sizeService, row.Operation.ProtectionTools.Type.SizeType);
						sizeOk = row.Operation.WearSize != null;
						if(!sizeOk)
							error = "Не удалось сопоставить размер.";
					}
				}
				
				return new ChangeState(sizeOk && heightOk ? ChangeType.NewEntity : ChangeType.ParseError, error: error);
			}
		}

		private ChangeState GetChangeStateSize(string sizeValue, IUnitOfWork uow, SheetRowWorkwearItems row) {
			if(String.IsNullOrWhiteSpace(sizeValue))
				return new ChangeState(ChangeType.NotChanged);
			else if(row.Operation.ProtectionTools.Type.SizeType == null)
				return new ChangeState(ChangeType.ParseError, error: "У типа номенклатуры не указан тип размера!");
			else {
				row.Operation.WearSize = SizeParser.ParseSize(uow, sizeValue, sizeService, row.Operation.ProtectionTools.Type.SizeType);
				return new ChangeState(row.Operation.WearSize != null ? ChangeType.NewEntity : ChangeType.ParseError, 
						interpretedValue: sizeValue != row.Operation.WearSize?.Name ? row.Operation.WearSize?.Name : null);
			}
		}

		private ChangeState GetChangeStateHeight(string heightValue, IUnitOfWork uow, SheetRowWorkwearItems row) {
			if(String.IsNullOrWhiteSpace(heightValue))
				return new ChangeState(ChangeType.NotChanged);
			else if(row.Operation.ProtectionTools.Type.HeightType == null)
				return new ChangeState(ChangeType.ParseError, error: "У типа номенклатуры не указан тип роста!");
			else {
				row.Operation.Height =
					SizeParser.ParseSize(uow, heightValue, sizeService, row.Operation.ProtectionTools.Type.HeightType);
				return new ChangeState(row.Operation.Height != null ? ChangeType.NewEntity : ChangeType.ParseError,
						interpretedValue: heightValue != row.Operation.Height?.Name ? row.Operation.Height?.Name : null);
			}
		}
		#endregion
		
		public FIO GetFIO(SheetRowWorkwearItems row, ExcelValueTarget fioColumn) {
			var fio = new FIO();
			row.CellStringValue(fioColumn)?.SplitFullName(out fio.LastName, out fio.FirstName, out fio.Patronymic);
			return fio;
		}
		#endregion
	}
}
