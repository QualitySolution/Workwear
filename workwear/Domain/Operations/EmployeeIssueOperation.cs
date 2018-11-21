using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Tools;

namespace workwear.Domain.Operations
{
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
			set { SetField(ref useAutoWriteoff, value); }
		}

		private DateTime expenseByNorm;

		[Display(Name = "Износ по норме")]
		public virtual DateTime ExpenseByNorm
		{
			get { return expenseByNorm; }
			set { SetField(ref expenseByNorm, value); }
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

		#region Методы обновленя операций

		public virtual void Update(IUnitOfWork uow, ExpenseItem item)
		{
			//Внимание здесь сравниваются даты без времени.
			if (item.ExpenseDoc.Date.Date != OperationTime.Date)
				OperationTime = item.ExpenseDoc.Date;

			Employee = item.ExpenseDoc.EmployeeCard;
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
			//RecalculateDatesOfIssueOperation(graph);
		}

		public void RecalculateDatesOfIssueOperation(IssueGraph graph, Func<string, bool> askUser)
		{
			if (NormItem == null)
			{
				logger.Error($"Для операциия id:{Id} выдачи {Nomenclature.Name} от {OperationTime} не установлена норма поэтому прерасчет даты выдачи и использования не возможен.");
				return;
			}
			DateTime startUsing = operationTime;

			var amountAtBegin = graph.AmountAtBeginOfDay(OperationTime.Date, this);
			var amountByNorm = NormItem.Amount;
			if (amountAtBegin >= amountByNorm)
			{
				//Ищем первый интервал где числящееся меньше нормы.
				DateTime moveTo;
				var firstLessNorm = graph.Intervals
					.Where(x => x.StartDate.Date >= OperationTime.Date)
					.OrderBy(x => x.StartDate)
					.FirstOrDefault(x => graph.AmountAtEndOfDay(x.StartDate, this) < NormItem.Amount);
				if (firstLessNorm == null)
				{
					var lastInterval = graph.Intervals
											.OrderBy(x => x.StartDate)
											.LastOrDefault();
					moveTo = lastInterval.ActiveItems.Max(x => x.IssueOperation.ExpenseByNorm);
				}
				else
					moveTo = firstLessNorm.StartDate;

				if (askUser($"На {operationTime:d} за сотрудником уже числится {amountAtBegin} x {Nomenclature.TypeName}, при этом по нормам положено {amountByNorm}. Передвинуть начало экспуатации вновь выданных {Issued} на {moveTo}?")){
					startUsing = moveTo;
				}
			}

			ExpenseByNorm = NormItem.CalculateExpireDate(startUsing);

			if (Issued > amountByNorm)
			{
				if(askUser($"За раз выдается {Issued} x {Nomenclature.Type.Name} это больше чем положено по норме {amountByNorm}. Увеличить период эксплуатации выданного пропорционально количеству?"))
				{
					ExpenseByNorm = NormItem.CalculateExpireDate(startUsing, Issued);
				}
			}

			if (UseAutoWriteoff)
				AutoWriteoffDate = ExpenseByNorm;
			else
				AutoWriteoffDate = null;
		}

		#endregion

		#region Методы гупповой обработки операций


		#endregion
	}
}
