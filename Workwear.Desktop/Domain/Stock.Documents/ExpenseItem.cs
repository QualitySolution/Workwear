using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Statements;
using Workwear.Repository.Stock;
using Workwear.Tools;

namespace Workwear.Domain.Stock.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки выдачи",
		Nominative = "строка выдачи",
		Genitive = "строки выдачи"
		)]
	[HistoryTrace]
	public class ExpenseItem : PropertyChangedBase, IDomainObject
	{
		#region Сохраняемые свойства

		public virtual int Id { get; set; }

		private Expense expenseDoc;
		[Display (Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual Expense ExpenseDoc {
			get => expenseDoc;
			set { SetField (ref expenseDoc, value, () => ExpenseDoc); }
		}

		private ProtectionTools protectionTools;
		[Display(Name = "Номенклатура нормы")]
		public virtual ProtectionTools ProtectionTools {
			get => protectionTools ?? EmployeeIssueOperation?.ProtectionTools;
			set { SetField(ref protectionTools, value, () => ProtectionTools); }
		}

		private Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		private IssuanceSheetItem issuanceSheetItem;
		[Display(Name = "Строка ведомости")]
		public virtual IssuanceSheetItem IssuanceSheetItem {
			get => issuanceSheetItem;
			set => SetField(ref issuanceSheetItem, value);
		}

		private int amount;
		[Display (Name = "Количество")]
		public virtual int Amount {
			get => amount;
			set { SetField (ref amount, value, () => Amount); }
		}

		private SubdivisionPlace subdivisionPlace;
		[Display (Name = "Размещение в подразделении")]
		public virtual SubdivisionPlace SubdivisionPlace {
			get => subdivisionPlace;
			set { SetField (ref subdivisionPlace, value, () => SubdivisionPlace); }
		}

		private EmployeeIssueOperation employeeIssueOperation;
		[Display(Name = "Операция выдачи сотруднику")]
		[IgnoreHistoryTrace]
		public virtual EmployeeIssueOperation EmployeeIssueOperation
		{
			get => employeeIssueOperation;
			set => SetField(ref employeeIssueOperation, value);
		}

		private SubdivisionIssueOperation subdivisionIssueOperation;
		[Display(Name = "Операция выдачи на подразделение")]
		[IgnoreHistoryTrace]
		public virtual SubdivisionIssueOperation SubdivisionIssueOperation {
			get => subdivisionIssueOperation;
			set => SetField(ref subdivisionIssueOperation, value);
		}

		private WarehouseOperation warehouseOperation = new WarehouseOperation();
		[Display(Name = "Операция на складе")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
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
		#endregion

		#region Не сохраняемые в базу свойства

		private bool isWriteOff;
		[Display(Name = "Выдача по списанию")]
		public virtual bool IsWriteOff {
			get => isWriteOff;
			set => isWriteOff = value;
		}

		private string aktNumber;
		[Display(Name = "Номер акта")]
		public virtual string AktNumber {
			get => IsWriteOff ? aktNumber : null;
			set => SetField(ref aktNumber, value);
		}

		public virtual bool IsEnableWriteOff { get; set; }

		private string buhDocument;

		[Display(Name = "Документ бухгалтерского учета")]
		//В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		public virtual string BuhDocument
		{
			get => buhDocument ?? EmployeeIssueOperation?.BuhDocument;
			set => SetField(ref buhDocument, value);
		}

		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get => WarehouseOperation.WearPercent;
			set => WarehouseOperation.WearPercent = value;
		}

		private EmployeeCardItem employeeCardItem;

		[Display(Name = "Процент износа")]
		public virtual EmployeeCardItem EmployeeCardItem {
			get => employeeCardItem;
			set => employeeCardItem = value;
		}

		public virtual StockPosition StockPosition {
			get => new StockPosition(Nomenclature, WearPercent, WearSize, Height);
			set {
				Nomenclature = value.Nomenclature;
				WearSize = value.WearSize;
				Height = value.Height;
				WearPercent = value.WearPercent;
			}
		}

		private StockBalanceDTO stockBalanceSetter;
		public virtual StockBalanceDTO StockBalanceSetter {
			get => stockBalanceSetter ?? 
			       new StockBalanceDTO {Nomenclature = Nomenclature, Height = Height, WearSize = WearSize, WearPercent = WearPercent };
			set {
				stockBalanceSetter = value;
				Nomenclature = value.Nomenclature;
				WearSize = value.WearSize;
				Height = value.Height;
				WearPercent = value.WearPercent;
			}
		}
		#endregion

		#region Расчетные свойства
		public virtual string Title =>
			String.Format ("Выдача со склада {0} в количестве {1} {2}",
				Nomenclature.Name,
				Amount,
				Nomenclature.Type.Units?.Name
			).TrimEnd();

		#endregion

		public ExpenseItem ()
		{
		}

		#region Функции

		public virtual void UpdateOperations(IUnitOfWork uow, BaseParameters baseParameters, IInteractiveQuestion askUser, string signCardUid = null)
		{
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);

			//Выдача сотруднику
			if(expenseDoc.Operation == ExpenseOperations.Employee)
			{
				if (EmployeeIssueOperation == null) {
					EmployeeIssueOperation = new EmployeeIssueOperation();
				}

				EmployeeIssueOperation.Update(uow, baseParameters, askUser, this, signCardUid);

				UpdateIssuedWriteOffOperation(uow);
									
				uow.Save(EmployeeIssueOperation);
			}
			else if(EmployeeIssueOperation != null)
			{
				uow.Delete(EmployeeIssueOperation);
				EmployeeIssueOperation = null;
			}

			//Выдача на подразделение
			if(expenseDoc.Operation == ExpenseOperations.Object) {
				if(SubdivisionIssueOperation == null)
					SubdivisionIssueOperation = new SubdivisionIssueOperation(baseParameters);

				SubdivisionIssueOperation.Update(uow, askUser, this);
				uow.Save(SubdivisionIssueOperation);
			}
			else if(SubdivisionIssueOperation != null) {
				uow.Delete(SubdivisionIssueOperation);
				SubdivisionIssueOperation = null;
			}
		}

		public virtual void UpdateIssuedWriteOffOperation(IUnitOfWork uow)
		{
			if(this.ExpenseDoc.WriteOffDoc == null)
				return;

			WriteoffItem relatedWriteoffItem = null;
			if(EmployeeIssueOperation.EmployeeOperationIssueOnWriteOff != null) 
				relatedWriteoffItem = this.ExpenseDoc.WriteOffDoc.Items
					.FirstOrDefault(x => EmployeeIssueOperation.EmployeeOperationIssueOnWriteOff.IsSame(x.EmployeeWriteoffOperation));

			if(IsWriteOff) {
				if(relatedWriteoffItem == null) {
					var graph = IssueGraph.MakeIssueGraph(uow, expenseDoc.Employee, ProtectionTools);
					var interval = graph.IntervalOfDate(ExpenseDoc.Date);
					var toWriteoff = interval.ActiveItems.First(x => x.IssueOperation != EmployeeIssueOperation);
					relatedWriteoffItem = ExpenseDoc.WriteOffDoc.AddItem(toWriteoff.IssueOperation, toWriteoff.AmountAtEndOfDay(ExpenseDoc.Date));
					EmployeeIssueOperation.EmployeeOperationIssueOnWriteOff = relatedWriteoffItem.EmployeeWriteoffOperation;
				}
				relatedWriteoffItem.AktNumber = this.AktNumber;
				relatedWriteoffItem.UpdateOperations(uow);

			}
			else if(EmployeeIssueOperation.EmployeeOperationIssueOnWriteOff != null && relatedWriteoffItem != null) {
					uow.Delete(EmployeeIssueOperation.EmployeeOperationIssueOnWriteOff);
					ExpenseDoc.WriteOffDoc.Items.Remove(relatedWriteoffItem);
					EmployeeIssueOperation.EmployeeOperationIssueOnWriteOff = null;
			}
		}

		#endregion

	}
}

