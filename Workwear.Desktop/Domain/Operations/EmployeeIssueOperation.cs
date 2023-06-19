using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Utilities.Dates;
using QS.Utilities.Numeric;
using Workwear.Domain.Company;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools;

namespace Workwear.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "операции выдачи сотруднику",
		Nominative = "операция выдачи сотруднику",
		Genitive ="операции выдачи сотруднику"
	)]
	[HistoryTrace]
	public class EmployeeIssueOperation : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual int Id { get; set; }

		DateTime operationTime = DateTime.Now;
		[Display(Name = "Время операции")]
		public virtual DateTime OperationTime {
			get => operationTime;
			set => SetField(ref operationTime, value);
		}

		private EmployeeCard employee;
		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee {
			get => employee;
			set => SetField(ref employee, value);
		}

		private ProtectionTools protectionTools;
		[Display(Name = "Номенклатура нормы")]
		public virtual ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}

		private Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}
		
		private Size wearSize;
		[Display(Name = "Размер")]
		public virtual Size WearSize {
			get => wearSize;
			set => SetField(ref wearSize, value);
		}
		
		private Size height;
		[Display(Name = "Рост одежды")]
		public virtual Size Height {
			get => height;
			set => SetField(ref height, value);
		}

		private decimal wearPercent;
		/// <summary>
		/// Процент износа не может быть меньше нуля.
		/// Новый СИЗ имеет 0%, далее нарастает при использовании.
		/// Процент хранится в виде коэффициента, то есть значение 1 = 100%
		/// И в базе ограничение на 3 хранимых символа поэтому максимальное значение 9.99
		/// </summary>
		/// <value>The wear percent.</value>
		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get => wearPercent;
			set => SetField(ref wearPercent, value.Clamp(0m, 9.99m));
		}

		private int issued;
		[Display(Name = "Выдано")]
		public virtual int Issued {
			get => issued;
			set => SetField(ref issued, value);
		}

		private int returned;
		[Display(Name = "Возвращено")]
		public virtual int Returned {
			get => returned;
			set => SetField(ref returned, value);
		}

		private bool useAutoWriteoff = true;
		[Display(Name = "Использовать автосписание")]
		public virtual bool UseAutoWriteoff {
			get => useAutoWriteoff;
			set {
				if (!SetField(ref useAutoWriteoff, value)) return;
				AutoWriteoffDate = value ? ExpiryByNorm : null;
			}
		}

		private DateTime? startOfUse;
		[Display(Name = "Начало использования")]
		public virtual DateTime? StartOfUse {
			get => startOfUse;
			set => SetField(ref startOfUse, value?.Date);
		}

		private DateTime? expiryByNorm;
		[Display(Name = "Износ по норме")]
		public virtual DateTime? ExpiryByNorm {
			get => expiryByNorm;
			set => SetField(ref expiryByNorm, value?.Date);
		}

		private DateTime? autoWriteoffDate;
		[Display(Name = "Дата автосписания")]
		public virtual DateTime? AutoWriteoffDate {
			get => autoWriteoffDate;
			set => SetField(ref autoWriteoffDate, value?.Date);
		}

		private EmployeeIssueOperation issuedOperation;
		[Display(Name = "Операция выдачи")]
		public virtual EmployeeIssueOperation IssuedOperation {
			get => issuedOperation;
			set => SetField(ref issuedOperation, value);
		}

		private WarehouseOperation warehouseOperation;
		[Display(Name = "Сопутствующая складская операция")]
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}

		private NormItem normItem;
		[Display(Name = "Строка нормы")]
		public virtual NormItem NormItem {
			get => normItem;
			set => SetField(ref normItem, value);
		}

		private string buhDocument;
		[Display(Name = "Документ бухгалтерского учета")]
		public virtual string BuhDocument {
			get => buhDocument;
			set => SetField(ref buhDocument, value);
		}

		private EmployeeIssueOperation employeeOperationIssueOnWriteOff;
		[Display(Name = "Операция списания при выдачи по списанию")]
		public virtual EmployeeIssueOperation EmployeeOperationIssueOnWriteOff {
			get => employeeOperationIssueOnWriteOff;
			set => SetField(ref employeeOperationIssueOnWriteOff, value);
		}

		private string signCardKey;
		[Display(Name = "UID карты доступа")]
		public virtual string SignCardKey {
			get => signCardKey;
			set => SetField(ref signCardKey, value);
		}

		private DateTime? signTimestamp;
		[Display(Name = "Отметка времени подписи")]
		public virtual DateTime? SignTimestamp {
			get => signTimestamp;
			set => SetField(ref signTimestamp, value);
		}

		private bool overrideBefore;
		[Display(Name = "Сбрасывает предыдущие движения")]
		public virtual bool OverrideBefore {
			get => overrideBefore;
			set => SetField(ref overrideBefore, value);
		}

		private bool manualOperation;

		[Display(Name = "Признак ручной операции")]
		public virtual bool ManualOperation {
			get => manualOperation;
			set => SetField(ref manualOperation, value);
		}
		
		private bool fixedOperation;
		[Display(Name = "Запрет пересчёта дат начала и окончания срока носки")]
		public virtual bool FixedOperation {
			get => fixedOperation;
			set => SetField(ref fixedOperation, value);
		}
		
		private IList<BarcodeOperation> barcodeOperations = new List<BarcodeOperation>();
		[Display(Name = "Операции")]
		public virtual IList<BarcodeOperation> BarcodeOperations {
			get => barcodeOperations;
			set => SetField(ref barcodeOperations, value);
		}

		/// <summary>
		/// Для создания операций выдачи надо использовать конструктор с BaseParameters
		/// </summary>
		public EmployeeIssueOperation() { }
		public EmployeeIssueOperation(BaseParameters baseParameters) {
			useAutoWriteoff = baseParameters.DefaultAutoWriteoff;
		}
		#region Расчетные
		public virtual string Title => Issued > Returned
			? $"Выдача {Employee.ShortName} <= {Issued} х {Nomenclature?.Name ?? ProtectionTools.Name}"
			: $"Списание {Employee.ShortName} => {Returned} х {Nomenclature?.Name ?? ProtectionTools.Name}";

		public virtual decimal? LifetimeMonth {
			get {
				if(StartOfUse == null || ExpiryByNorm == null)
					return null;

				var range = new DateRange(StartOfUse.Value, ExpiryByNorm.Value);
				return range.Months;
			}
		}
		#endregion
		#region Методы
		public virtual decimal CalculatePercentWear(DateTime atDate) => 
			CalculatePercentWear(atDate, StartOfUse, ExpiryByNorm, WearPercent);

		public virtual decimal CalculateDepreciationCost(DateTime atDate) => 
			CalculateDepreciationCost(atDate, StartOfUse, ExpiryByNorm, WarehouseOperation?.Cost ?? 0m);

		public virtual bool IsTouchDates(DateTime start, DateTime end) =>
			(OperationTime <= end || StartOfUse <= end)
			&& (ExpiryByNorm >= start || AutoWriteoffDate >= start || (ExpiryByNorm == null && AutoWriteoffDate == null));
		#endregion

		#region Статические методы
		public static decimal CalculatePercentWear(DateTime atDate, DateTime? startOfUse, DateTime? expiryByNorm, decimal beginWearPercent = 0) {
			if(startOfUse == null || expiryByNorm == null)
				return 0;
			if(beginWearPercent >= 1)
				return beginWearPercent;
			
			var addPercent = (atDate - startOfUse.Value).TotalDays / (expiryByNorm.Value - startOfUse.Value).TotalDays;
			if(double.IsNaN(addPercent) || double.IsInfinity(addPercent))
				return beginWearPercent;

			return Math.Round(beginWearPercent + (1 - beginWearPercent) * (decimal)addPercent, 2);
		}

		public static decimal CalculateDepreciationCost(DateTime atDate, DateTime? startOfUse, DateTime? expiryByNorm, decimal beginCost) {
			if(startOfUse == null || expiryByNorm == null)
				return 0;

			var removePercent = (atDate - startOfUse.Value).TotalDays / (expiryByNorm.Value - startOfUse.Value).TotalDays;
			if(double.IsNaN(removePercent) || double.IsInfinity(removePercent))
				return beginCost;

			return (beginCost - beginCost * (decimal)removePercent).Clamp(0, decimal.MaxValue);
		}
		#endregion
		#region Методы обновленя операций
		public virtual void Update(
			IUnitOfWork uow, 
			BaseParameters baseParameters, 
			IInteractiveQuestion askUser, 
			ExpenseItem item, 
			string signCardUid = null)
		{
			//Внимание здесь сравниваются даты без времени.
			if (item.ExpenseDoc.Date.Date != OperationTime.Date)
				OperationTime = item.ExpenseDoc.Date;

			Employee = item.ExpenseDoc.Employee;
			Nomenclature = item.Nomenclature;
			WearSize = item.WearSize;
			Height = item.Height;
			WearPercent = item.WarehouseOperation.WearPercent;
			Issued = item.Amount;
			Returned = 0;
			IssuedOperation = null;
			BuhDocument = item.BuhDocument;
			WarehouseOperation = item.WarehouseOperation;
			ProtectionTools = item.ProtectionTools;


			if(!String.IsNullOrEmpty(signCardUid)) {
				SignCardKey = signCardUid;
				SignTimestamp = DateTime.Now;
			}

			if (NormItem == null)
				NormItem = Employee.WorkwearItems
					.FirstOrDefault(x => x.ProtectionTools.MatchedNomenclatures
						.Contains(Nomenclature))?.ActiveNormItem;

			if(NormItem == null) {
				logger.Warn(
					$"В операции выдачи {Nomenclature.Name} " +
					$"не указана ссылка на норму, перерасчет сроков выдачи невозможен.");
				return;
			}

			if(ProtectionTools == null)
				ProtectionTools = NormItem.ProtectionTools;

			if(EmployeeOperationIssueOnWriteOff != null) {
				if(item.ExpenseDoc.Date.Date != OperationTime.Date)
					this.EmployeeOperationIssueOnWriteOff.OperationTime = item.ExpenseDoc.Date;
			}

			var anotherRows =
				item.ExpenseDoc.Items
					.Where(x => x.EmployeeIssueOperation != null && DomainHelper.EqualDomainObjects(x.ProtectionTools, ProtectionTools))
					.Select(x => x.EmployeeIssueOperation).ToArray();
			var graph = IssueGraph.MakeIssueGraph(uow, Employee, NormItem.ProtectionTools, anotherRows);
			RecalculateDatesOfIssueOperation(graph, baseParameters, askUser);
		}

		public virtual void Update(IUnitOfWork uow, BaseParameters baseParameters, IInteractiveQuestion askUser, CollectiveExpenseItem item)
		{
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			Employee = item.Employee;
			Nomenclature = item.Nomenclature;
			WearSize = item.WearSize;
			Height = item.Height;
			WearPercent = item.WarehouseOperation.WearPercent;
			Issued = item.Amount;
			Returned = 0;
			IssuedOperation = null;
			WarehouseOperation = item.WarehouseOperation;

			if(item.EmployeeCardItem?.ActiveNormItem != null)
				NormItem = item.EmployeeCardItem.ActiveNormItem;

			if(NormItem == null) {
				logger.Warn($"В операции выдачи {Nomenclature.Name} не указана ссылка на норму, " +
				            $"перерасчет сроков выдачи невозможен.");
				return;
			}

			if(item.EmployeeCardItem?.ProtectionTools != null)
				ProtectionTools = item.EmployeeCardItem.ProtectionTools;
				
			var graph = IssueGraph.MakeIssueGraph(uow, Employee, NormItem.ProtectionTools);
			RecalculateDatesOfIssueOperation(graph, baseParameters, askUser);
		}

		public virtual void Update(IUnitOfWork uow, IInteractiveQuestion askUser, IncomeItem item) {
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			Employee = item.Document.EmployeeCard;
			Nomenclature = item.Nomenclature;
			WearSize = item.WearSize;
			Height = item.Height;
			WearPercent = item.WearPercent;
			Issued = 0;
			Returned = item.Amount;
			WarehouseOperation = item.WarehouseOperation;
			IssuedOperation = item.IssuedEmployeeOnOperation;
			protectionTools = item.IssuedEmployeeOnOperation?.ProtectionTools;
			BuhDocument = item.BuhDocument;
			NormItem = null;
			ExpiryByNorm = null;
			AutoWriteoffDate = null;
		}

		public virtual void Update(IUnitOfWork uow, WriteoffItem item) {
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;
				
			Nomenclature = item.Nomenclature;
			Issued = 0;
			Returned = item.Amount;
			WarehouseOperation = item.WarehouseOperation;
			BuhDocument = item.BuhDocument;
			NormItem = null;
			ExpiryByNorm = null;
			AutoWriteoffDate = null;
			WearSize = item.WearSize;
			Height = item.Height;
		}
		
		public virtual void UpdateIssueOperation(EmployeeIssueOperation issueOperation, DateTime date) {
			Employee = issueOperation.Employee;
			Nomenclature = issueOperation.Nomenclature;
			operationTime = date;
			Issued = issueOperation.issued;
			Returned = issueOperation.returned;
			WarehouseOperation = null;
			BuhDocument = null;
			NormItem = issueOperation.normItem;;
			ExpiryByNorm = null;
			AutoWriteoffDate = null;
			protectionTools = issueOperation.protectionTools;
			WearSize = issueOperation.WearSize;
			Height = issueOperation.Height;
			ManualOperation = false;
			OverrideBefore = false;
		}
		#endregion
		#region Пересчет выдачи

		public virtual void RecalculateDatesOfIssueOperation(IssueGraph graph, BaseParameters baseParameters, IInteractiveQuestion askUser) {
			RecalculateStartOfUse(graph, baseParameters, askUser);
			RecalculateExpiryByNorm(baseParameters, askUser);
		}

		private bool CheckRecalculateCondition() {
			if(FixedOperation) {
				logger.Error(
					$"Операциия id:{Id} выдачи {Nomenclature?.Name} от {OperationTime} помечена как непересчитываемая.");
				return false;
			}
			
			if(ProtectionTools == null) {
				logger.Error(
					$"Для операциия id:{Id} выдачи {Nomenclature?.Name} от {OperationTime} не указана " +
					$"'Номеклатура нормы' поэтому прерасчет даты выдачи и использования не возможен.");
				return false;
			}

			if(NormItem == null) {
				//Пробуем найти норму сами.
				var cardItem = Employee.WorkwearItems.FirstOrDefault(x => ProtectionTools.IsSame(x.ProtectionTools));
				NormItem = cardItem?.ActiveNormItem;
			}

			if(NormItem == null) {
				logger.Error(
					$"Для операциия id:{Id} выдачи {Nomenclature?.Name ?? ProtectionTools?.Name} " +
					$"от {OperationTime} не установлена норма поэтому прерасчет даты выдачи и использования не возможен.");
				return false;
			}

			return true;
		}

		public virtual void RecalculateStartOfUse(IssueGraph graph, BaseParameters baseParameters, IInteractiveQuestion askUser) {
			if(!CheckRecalculateCondition())
				return;
			
			StartOfUse = operationTime;

			var amountAtEndDay = graph.UsedAmountAtEndOfDay(OperationTime.Date, this);
			if(amountAtEndDay >= NormItem.Amount) {
				//Ищем первый интервал где числящееся меньше нормы.
				var firstLessNorm = graph.Intervals
					.Where(x => x.StartDate.Date >= OperationTime.Date)
					.OrderBy(x => x.StartDate)
					.FirstOrDefault(x => graph.UsedAmountAtEndOfDay(x.StartDate, this) < NormItem.Amount);
				if(firstLessNorm != null && firstLessNorm.StartDate.AddDays(-baseParameters.ColDayAheadOfShedule) > OperationTime.Date) {
					switch(baseParameters.ShiftExpluatacion) {
						case AnswerOptions.Ask:
							if(askUser.Question(
								   $"На {operationTime:d} за {Employee.ShortName} уже числится {amountAtEndDay} " +
								   $"x {ProtectionTools.Name}, при этом по нормам положено {NormItem.Amount} на {normItem.LifeText}. " +
								   $"Передвинуть начало эксплуатации вновь выданных {Issued} на {firstLessNorm.StartDate:d}?"))
								StartOfUse = firstLessNorm.StartDate;
							break;
						case AnswerOptions.Yes:
							StartOfUse = firstLessNorm.StartDate;
							break;
						case AnswerOptions.No:
							break;
						default:
							throw new NotImplementedException();
					}
				}
			}
		}

		public virtual void RecalculateExpiryByNorm(BaseParameters baseParameters, IInteractiveQuestion askUser){
			if(!CheckRecalculateCondition())
				return;

			if(StartOfUse == null)
				StartOfUse = OperationTime;
			
			ExpiryByNorm = NormItem.CalculateExpireDate(StartOfUse.Value);
			
			if(Issued > NormItem.Amount && NormItem.Amount > 0)
			{
				switch(baseParameters.ExtendPeriod) {
					case AnswerOptions.Ask:
						if(askUser.Question(
							$"Сотруднику {Employee.ShortName} за раз выдается {Issued} x {ProtectionTools.Name} " +
							$"это больше чем положено по норме {NormItem.Amount}. " +
							$"Увеличить период эксплуатации выданного пропорционально количеству?")) {
							ExpiryByNorm = NormItem.CalculateExpireDate(StartOfUse.Value, Issued);
						}
						break;
					case AnswerOptions.Yes:
						ExpiryByNorm = NormItem.CalculateExpireDate(StartOfUse.Value, Issued);
						break;
					case AnswerOptions.No:
						break;
					default:
						throw new NotImplementedException();
				}
			}

			//Обрабатываем отпуска
			if(Employee.Vacations.Any(v => v.BeginDate <= ExpiryByNorm && v.EndDate >= StartOfUse && v.VacationType.ExcludeFromWearing)) {
				var ranges = Employee.Vacations
					.Where(v => v.VacationType.ExcludeFromWearing)
					.Select(v => new DateRange(v.BeginDate, v.EndDate));
				var wearTime = new DateRange(StartOfUse.Value, DateTime.MaxValue);
				wearTime.ExcludedRanges.AddRange(ranges);
				var endWearDate = wearTime.FillIntervals((ExpiryByNorm - StartOfUse.Value).Value.Days);
				ExpiryByNorm = endWearDate;
			}

			AutoWriteoffDate = UseAutoWriteoff ? ExpiryByNorm : null;
		}

		#endregion
	}
}
