using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Dates;
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

		private Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get { return nomenclature; }
			set { SetField(ref nomenclature, value); }
		}

		private decimal wearPercent;

		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent
		{
			get { return wearPercent; }
			set { SetField(ref wearPercent, value); }
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

		private IncomeItem incomeOnStock;

		[Display(Name = "Строка поступление на склад")]
		public virtual IncomeItem IncomeOnStock
		{
			get { return incomeOnStock; }
			set { SetField(ref incomeOnStock, value); }
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

		public EmployeeIssueOperation()
		{
			useAutoWriteoff = BaseParameters.DefaultAutoWriteoff;
		}

		#region Расчетные

		public virtual string Title => Issued > Returned
			? $"Выдача {Employee.ShortName} <= {Issued} х {Nomenclature.Name}"
			: $"Списание {Employee.ShortName} => {Returned} х {Nomenclature.Name}";

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
			if(double.IsNaN(addPercent))
				return beginWearPercent;

			return beginWearPercent + (decimal)addPercent;
		}

		#endregion

		#region Методы обновленя операций

		public virtual void Update(IUnitOfWork uow, IInteractiveQuestion askUser, ExpenseItem item)
		{
			//Внимание здесь сравниваются даты без времени.
			if (item.ExpenseDoc.Date.Date != OperationTime.Date)
				OperationTime = item.ExpenseDoc.Date;

			Employee = item.ExpenseDoc.Employee;
			Nomenclature = item.Nomenclature;
			WearPercent = 1 - item.IncomeOn.LifePercent;
			Issued = item.Amount;
			Returned = 0;
			IssuedOperation = null;
			IncomeOnStock = item.IncomeOn;
			BuhDocument = item.BuhDocument;

			if (NormItem == null)
				NormItem = Employee.WorkwearItems.FirstOrDefault(x => x.Item == Nomenclature.Type)?.ActiveNormItem;

			var graph = IssueGraph.MakeIssueGraph(uow, Employee, Nomenclature.Type);
			RecalculateDatesOfIssueOperation(graph, askUser);
		}

		public virtual void RecalculateDatesOfIssueOperation(IssueGraph graph, IInteractiveQuestion askUser)
		{
			if(NormItem == null) {
				//Пробуем найти норму сами.
				var cardItem = Employee.WorkwearItems.FirstOrDefault(x => Nomenclature.Type.IsSame(x.Item));
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
				DateTime moveTo;
				var firstLessNorm = graph.Intervals
					.Where(x => x.StartDate.Date >= OperationTime.Date)
					.OrderBy(x => x.StartDate)
					.FirstOrDefault(x => graph.UsedAmountAtEndOfDay(x.StartDate, this) < NormItem.Amount);
				if (firstLessNorm != null)
				{
					moveTo = firstLessNorm.StartDate;
					if(askUser.Question($"На {operationTime:d} за сотрудником уже числится {amountAtEndDay} x {Nomenclature.TypeName}, при этом по нормам положено {NormItem.Amount} на {normItem.LifeText}. Передвинуть начало экспуатации вновь выданных {Issued} на {moveTo:d}?")) {
						startOfUse = moveTo;
					}
				}
			}

			ExpiryByNorm = NormItem.CalculateExpireDate(StartOfUse.Value);

			if(Issued > amountByNorm)
			{
				if(askUser.Question($"За раз выдается {Issued} x {Nomenclature.Type.Name} это больше чем положено по норме {amountByNorm}. Увеличить период эксплуатации выданного пропорционально количеству?"))
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

		public virtual void Update(IUnitOfWork uow, Func<string, bool> askUser, IncomeItem item)
		{
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			Employee = item.Document.EmployeeCard;
			Nomenclature = item.Nomenclature;
			WearPercent = 1 - item.LifePercent;
			Issued = 0;
			Returned = item.Amount;
			IssuedOperation = item.IssuedOn.EmployeeIssueOperation;
			IncomeOnStock = null;
			BuhDocument = item.BuhDocument;
			NormItem = null;
			ExpiryByNorm = null;
			AutoWriteoffDate = null;
		}

		public virtual void Update(IUnitOfWork uow, Func<string, bool> askUser, WriteoffItem item)
		{
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			IssuedOperation = item.IssuedOn.EmployeeIssueOperation;
			Employee = item.IssuedOn.ExpenseDoc.Employee;
			Nomenclature = item.Nomenclature;
			WearPercent = IssuedOperation.CalculatePercentWear(OperationTime);
			Issued = 0;
			Returned = item.Amount;
			IncomeOnStock = null;
			BuhDocument = item.BuhDocument;
			NormItem = null;
			ExpiryByNorm = null;
			AutoWriteoffDate = null;
		}

		#endregion
	}
}
