using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Dates;
using QS.Utilities.Numeric;
using workwear.Domain.Company;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Tools;

namespace workwear.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "операции выдачи сотруднику",
		Nominative = "операция выдачи сотруднику"
	)]
	public class EmployeeIssueOperation : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual int Id { get; set; }

		DateTime operationTime = DateTime.Now;

		public virtual DateTime OperationTime
		{
			get { return operationTime; }
			set { SetField(ref operationTime, value); }
		}

		private EmployeeCard employee;

		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee
		{
			get { return employee; }
			set { SetField(ref employee, value); }
		}

		private ProtectionTools protectionTools;
		[Display(Name = "Номенаклатуры нормы")]
		public virtual ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}

		private Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get { return nomenclature; }
			set { SetField(ref nomenclature, value); }
		}

		string size;

		[Display(Name = "Размер")]
		public virtual string Size {
			get { return size; }
			set { SetField(ref size, value, () => Size); }
		}

		string wearGrowth;

		[Display(Name = "Рост одежды")]
		public virtual string WearGrowth {
			get { return wearGrowth; }
			set { SetField(ref wearGrowth, value, () => WearGrowth); }
		}

		private decimal wearPercent;

		/// <summary>
		/// Процент износа не может быть меньше нуля.
		/// Новый СИЗ имеет 0%, далее нарастает при использовании.
		/// Процент хранится в виде коэфициента, то есть значение 1 = 100%
		/// И в базе ограничение на 3 хранимых символа поэтому максимальное значение 9.99
		/// </summary>
		/// <value>The wear percent.</value>
		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent
		{
			get { return wearPercent; }
			set { SetField(ref wearPercent, value.Clamp(0m, 9.99m)); }
		}

		private int issued;

		[Display(Name = "Выдано")]
		public virtual int Issued
		{
			get { return issued; }
			set { SetField(ref issued, value); }
		}

		private int returned;

		[Display(Name = "Возвращено")]
		public virtual int Returned
		{
			get { return returned; }
			set { SetField(ref returned, value); }
		}

		private bool useAutoWriteoff = true;

		[Display(Name = "Использовать автосписание")]
		public virtual bool UseAutoWriteoff
		{
			get { return useAutoWriteoff; }
			set { 
				if(SetField(ref useAutoWriteoff, value)) {
					if(value)
						AutoWriteoffDate = ExpiryByNorm;
					else
						AutoWriteoffDate = null;
				}
			}
		}

		private DateTime? startOfUse;

		[Display(Name = "Начало использования")]
		public virtual DateTime? StartOfUse {
			get { return startOfUse; }
			set { SetField(ref startOfUse, value); }
		}

		private DateTime? expiryByNorm;

		[Display(Name = "Износ по норме")]
		public virtual DateTime? ExpiryByNorm
		{
			get { return expiryByNorm; }
			set { SetField(ref expiryByNorm, value); }
		}

		private DateTime? autoWriteoffDate;

		[Display(Name = "Дата автосписания")]
		public virtual DateTime? AutoWriteoffDate
		{
			get { return autoWriteoffDate; }
			set { SetField(ref autoWriteoffDate, value); }
		}

		private EmployeeIssueOperation issuedOperation;

		[Display(Name = "Операция выдачи")]
		public virtual EmployeeIssueOperation IssuedOperation
		{
			get { return issuedOperation; }
			set { SetField(ref issuedOperation, value); }
		}

		private WarehouseOperation warehouseOperation;
		[Display(Name = "Сопутствующая складская операция")]
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}

		private NormItem normItem;

		[Display(Name = "Строка нормы")]
		public virtual NormItem NormItem
		{
			get { return normItem; }
			set { SetField(ref normItem, value); }
		}

		private string buhDocument;

		[Display(Name = "Документ бухгалтерского учета")]
		public virtual string BuhDocument
		{
			get { return buhDocument; }
			set { SetField(ref buhDocument, value); }
		}

		private EmployeeIssueOperation employeeOperationIssueOnWriteOff;

		[Display(Name = "Операция списания при выдачи по списанию")]
		public virtual EmployeeIssueOperation EmployeeOperationIssueOnWriteOff {
			get { return employeeOperationIssueOnWriteOff; }
			set { SetField(ref employeeOperationIssueOnWriteOff, value); }
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

		/// <summary>
		/// Для создания операций выдачи надо использвать конструктор с BaseParameters
		/// </summary>
		public EmployeeIssueOperation()
		{
		}

		public EmployeeIssueOperation(BaseParameters baseParameters)
		{
			useAutoWriteoff = baseParameters.DefaultAutoWriteoff;
		}

		#region Расчетные

		public virtual string Title => Issued > Returned
			? $"Выдача {Employee.ShortName} <= {Issued} х {Nomenclature.Name}"
			: $"Списание {Employee.ShortName} => {Returned} х {Nomenclature.Name}";

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

		public virtual decimal CalculatePercentWear(DateTime atDate)
		{
			return CalculatePercentWear(atDate, StartOfUse, ExpiryByNorm, WearPercent);
		}

		public static decimal CalculatePercentWear(DateTime atDate, DateTime? startOfUse, DateTime? expiryByNorm, decimal beginWearPercent)
		{
			if(startOfUse == null || expiryByNorm == null)
				return 0;

			var addPercent = (atDate - startOfUse.Value).TotalDays / (expiryByNorm.Value - startOfUse.Value).TotalDays;
			if(double.IsNaN(addPercent) || double.IsInfinity(addPercent))
				return beginWearPercent;

			return beginWearPercent + (decimal)addPercent;
		}

		public virtual decimal CalculateDepreciationCost(DateTime atDate)
		{
			return CalculateDepreciationCost(atDate, StartOfUse, ExpiryByNorm, WarehouseOperation.Cost);
		}

		public static decimal CalculateDepreciationCost(DateTime atDate, DateTime? startOfUse, DateTime? expiryByNorm, decimal beginCost)
		{
			if(startOfUse == null || expiryByNorm == null)
				return 0;

			var removePercent = (atDate - startOfUse.Value).TotalDays / (expiryByNorm.Value - startOfUse.Value).TotalDays;
			if(double.IsNaN(removePercent) || double.IsInfinity(removePercent))
				return beginCost;

			return (beginCost - beginCost * (decimal)removePercent).Clamp(0, decimal.MaxValue);
		}

		#endregion

		#region Методы обновленя операций

		public virtual void Update(IUnitOfWork uow, BaseParameters baseParameters, IInteractiveQuestion askUser, ExpenseItem item, string signCardUid = null)
		{
			//Внимание здесь сравниваются даты без времени.
			if (item.ExpenseDoc.Date.Date != OperationTime.Date)
				OperationTime = item.ExpenseDoc.Date;

			Employee = item.ExpenseDoc.Employee;
			Nomenclature = item.Nomenclature;
			Size = item.Size;
			WearGrowth = item.WearGrowth;
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
				NormItem = Employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools.MatchedNomenclatures.Contains(Nomenclature))?.ActiveNormItem;

			if(NormItem == null) {
				logger.Warn($"В операции выдачи {Nomenclature.Name} не указана ссылка на норму, перерасчет сроков выдачи невозможен.");
				return;
			}

			if(ProtectionTools == null)
				ProtectionTools = NormItem.ProtectionTools;

			if(EmployeeOperationIssueOnWriteOff != null) {
				if(item.ExpenseDoc.Date.Date != OperationTime.Date)
					this.EmployeeOperationIssueOnWriteOff.OperationTime = item.ExpenseDoc.Date;
			}

			var graph = IssueGraph.MakeIssueGraph(uow, Employee, NormItem.ProtectionTools);
			RecalculateDatesOfIssueOperation(graph, baseParameters, askUser);
		}

		public virtual void RecalculateDatesOfIssueOperation(IssueGraph graph, BaseParameters baseParameters, IInteractiveQuestion askUser)
		{
			if(ProtectionTools == null) {
				logger.Error($"Для операциия id:{Id} выдачи {Nomenclature.Name} от {OperationTime} не указана 'Номеклатура нормы' поэтому прерасчет даты выдачи и использования не возможен.");
				return;
			}

			if(NormItem == null) {
				//Пробуем найти норму сами.
				var cardItem = Employee.WorkwearItems.FirstOrDefault(x => ProtectionTools.IsSame(x.ProtectionTools));
				NormItem = cardItem?.ActiveNormItem;
			}

			if (NormItem == null){
				logger.Error($"Для операциия id:{Id} выдачи {Nomenclature.Name} от {OperationTime} не установлена норма поэтому прерасчет даты выдачи и использования не возможен.");
				return;
			}
			StartOfUse = operationTime;

			var amountAtEndDay = graph.UsedAmountAtEndOfDay(OperationTime.Date, this);
			var amountByNorm = NormItem.Amount;
			if (amountAtEndDay >= amountByNorm)
			{
				//Ищем первый интервал где числящееся меньше нормы.
				var firstLessNorm = graph.Intervals
					.Where(x => x.StartDate.Date >= OperationTime.Date)
					.OrderBy(x => x.StartDate)
					.FirstOrDefault(x => graph.UsedAmountAtEndOfDay(x.StartDate, this) < NormItem.Amount);
				if (firstLessNorm != null && firstLessNorm.StartDate.AddDays(-baseParameters.ColDayAheadOfShedule) > OperationTime.Date)
				{
					if(askUser.Question($"На {operationTime:d} за сотрудником уже числится {amountAtEndDay} x {ProtectionTools.Name}, при этом по нормам положено {NormItem.Amount} на {normItem.LifeText}. Передвинуть начало экспуатации вновь выданных {Issued} на {firstLessNorm.StartDate:d}?")) 
						startOfUse = firstLessNorm.StartDate;
				}
			}

			ExpiryByNorm = NormItem.CalculateExpireDate(StartOfUse.Value);

			if(Issued > amountByNorm)
			{
				if(askUser.Question($"За раз выдается {Issued} x {ProtectionTools.Name} это больше чем положено по норме {amountByNorm}. Увеличить период эксплуатации выданного пропорционально количеству?"))
				{
					ExpiryByNorm = NormItem.CalculateExpireDate(StartOfUse.Value, Issued);
				}
			}

			//Обрабатываем отпуска
			if(Employee.Vacations.Any(v => v.BeginDate <= ExpiryByNorm && v.EndDate >= StartOfUse && v.VacationType.ExcludeFromWearing)) {
				var ranges = Employee.Vacations.Where(v => v.VacationType.ExcludeFromWearing).Select(v => new DateRange(v.BeginDate, v.EndDate));
				var wearTime = new DateRange(StartOfUse.Value, DateTime.MaxValue);
				wearTime.ExcludedRanges.AddRange(ranges);
				var endWearDate = wearTime.FillIntervals((ExpiryByNorm - StartOfUse.Value).Value.Days);
				ExpiryByNorm = endWearDate;
			}

			if(UseAutoWriteoff)
				AutoWriteoffDate = ExpiryByNorm;
			else
				AutoWriteoffDate = null;
		}


		public virtual void Update(IUnitOfWork uow, IInteractiveQuestion askUser, IncomeItem item)
		{
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			Employee = item.Document.EmployeeCard;
			Nomenclature = item.Nomenclature;
			Size = item.Size;
			WearGrowth = item.WearGrowth;
			WearPercent = item.WearPercent;
			Issued = 0;
			Returned = item.Amount;
			WarehouseOperation = item.WarehouseOperation;
			IssuedOperation = item.IssuedEmployeeOnOperation;
			BuhDocument = item.BuhDocument;
			NormItem = null;
			ExpiryByNorm = null;
			AutoWriteoffDate = null;
		}

		public virtual void Update(IUnitOfWork uow, WriteoffItem item)
		{
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
		}

		#endregion
	}
}
