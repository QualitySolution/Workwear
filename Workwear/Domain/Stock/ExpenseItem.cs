using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Statements;
using workwear.Repository.Operations;
using workwear.Repository.Stock;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки выдачи",
		Nominative = "строка выдачи")]
	public class ExpenseItem : PropertyChangedBase, IDomainObject
	{
		#region Сохраняемые свойства

		public virtual int Id { get; set; }

		Expense expenseDoc;

		[Display (Name = "Документ")]
		public virtual Expense ExpenseDoc {
			get { return expenseDoc; }
			set { SetField (ref expenseDoc, value, () => ExpenseDoc); }
		}

		ProtectionTools protectionTools;

		[Display(Name = "Номенклатура ТОН")]
		public virtual ProtectionTools ProtectionTools {
			get { return protectionTools; }
			set { SetField(ref protectionTools, value, () => ProtectionTools); }
		}

		Nomenclature nomenclature;

		[Display (Name = "Номеклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		private IssuanceSheetItem issuanceSheetItem;
		[Display(Name = "Строка ведомости")]
		public virtual IssuanceSheetItem IssuanceSheetItem {
			get => issuanceSheetItem;
			set => SetField(ref issuanceSheetItem, value);
		}

		int amount;

		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		SubdivisionPlace subdivisionPlace;

		[Display (Name = "Размещение в подразделении")]
		public virtual SubdivisionPlace SubdivisionPlace {
			get { return subdivisionPlace; }
			set { SetField (ref subdivisionPlace, value, () => SubdivisionPlace); }
		}

		private EmployeeIssueOperation employeeIssueOperation;

		[Display(Name = "Операция выдачи сотруднику")]
		public virtual EmployeeIssueOperation EmployeeIssueOperation
		{
			get { return employeeIssueOperation; }
			set { SetField(ref employeeIssueOperation, value); }
		}

		private SubdivisionIssueOperation subdivisionIssueOperation;

		[Display(Name = "Операция выдачи на подразделение")]
		public virtual SubdivisionIssueOperation SubdivisionIssueOperation {
			get { return subdivisionIssueOperation; }
			set { SetField(ref subdivisionIssueOperation, value); }
		}

		private WarehouseOperation warehouseOperation = new WarehouseOperation();
		[Display(Name = "Операция на складе")]
		public virtual WarehouseOperation WarehouseOperation {
			get { return warehouseOperation; }
			set { SetField(ref warehouseOperation, value);}
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

		string aktNumber;
		[Display(Name = "Номер акта")]
		public virtual string AktNumber {
			get {
				return aktNumber;
			}
			set {
				if(IsWriteOff)
					SetField(ref aktNumber, value);
				else SetField(ref aktNumber, "");
			}
		}

		private EmployeeIssueOperation oldEmployeeOperationIssue;

		[Display(Name = "Старая операция выдачи")]
		public virtual EmployeeIssueOperation OldEmployeeOperationIssue {
			get { return oldEmployeeOperationIssue; }
			set { SetField(ref oldEmployeeOperationIssue, value); }
		}

		private bool isWriteOff;
		[Display(Name = "Выдача по списанию")]
		public virtual bool IsWriteOff {
			get {
				if(EmployeeIssueOperation?.EmployeeOperationIssueOnWriteOff != null) return true;
				return isWriteOff;
			}
			set {
				if(value && EmployeeIssueOperation?.EmployeeOperationIssueOnWriteOff == null) {
					if(firstActualOperIssue != null) {
						this.OldEmployeeOperationIssue = firstActualOperIssue;
					}
				}
				else if(EmployeeIssueOperation?.EmployeeOperationIssueOnWriteOff != null) {
					EmployeeIssueOperation.EmployeeOperationIssueOnWriteOff = null;
					AktNumber = "";
				}

				if(this.OldEmployeeOperationIssue == null)
					isWriteOff = false;
				else
					isWriteOff = value;
			}
		}
		#endregion

		#region Не сохраняемые в базу свойства

		private string buhDocument;

		[Display(Name = "Документ бухгалтерского учета")]
		//В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		public virtual string BuhDocument
		{
			get { return buhDocument ?? EmployeeIssueOperation?.BuhDocument; }
			set { SetField(ref buhDocument, value); }
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
			get {
				return new StockPosition(Nomenclature, Size, WearGrowth, WearPercent);
			}
			set {
				Nomenclature = value.Nomenclature;
				Size = value.Size;
				WearGrowth = value.Growth;
				WearPercent = value.WearPercent;
			}
		}

		StockBalanceDTO stockBalanceSetter;
		public virtual StockBalanceDTO StockBalanceSetter {
			get {
				return stockBalanceSetter ?? new StockBalanceDTO {Nomenclature = Nomenclature, Growth = WearGrowth, Size = Size, WearPercent = WearPercent } ;
			}
			set {
				stockBalanceSetter = value;
				Nomenclature = value.Nomenclature;
				Size = value.Size;
				WearGrowth = value.Growth;
				WearPercent = value.WearPercent;
			}
		}

		[Display(Name = "Опреация выдачи с которой можно списать")]
		private EmployeeIssueOperation firstActualOperIssue;
		#endregion

		#region Расчетные свойства
		public virtual string Title {
			get { return String.Format ("Выдача со склада {0} в количестве {1} {2}",
				Nomenclature.Name,
				Amount,
				Nomenclature.Type.Units.Name
			);}
		}

		[Display(Name = "Наличие действующей операции выдачи по данной номенклатуре")]
		public virtual bool IsEnableWriteOff {
			get {
				if(EmployeeIssueOperation?.EmployeeOperationIssueOnWriteOff != null) return true;
				firstActualOperIssue = GetActualEmployeeOperationBalance();
				if(firstActualOperIssue != null)
					return true;
				else return false;
			}
		}
		#endregion

		public ExpenseItem ()
		{
		}

		#region Функции

		private EmployeeIssueOperation GetActualEmployeeOperationBalance()
		{
			var oper = EmployeeIssueRepository.GetActualEmployeeOperation(ExpenseDoc.Employee.UoW, ExpenseDoc.Employee, this.ExpenseDoc.Date);
			if(this.OldEmployeeOperationIssue != null)
				return this.OldEmployeeOperationIssue;
			else if(oper != null) 
				return oper.FirstOrDefault(x => x.Nomenclature == this.Nomenclature); //Операция выдачи, с которой можно списать
			return null;
		}

		public virtual void UpdateOperations(IUnitOfWork uow, IInteractiveQuestion askUser)
		{
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);

			//Выдача сотруднику
			if(expenseDoc.Operation == ExpenseOperations.Employee)
			{
				if (EmployeeIssueOperation == null)
					EmployeeIssueOperation = new EmployeeIssueOperation();

				EmployeeIssueOperation.Update(uow, askUser, this);

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
					SubdivisionIssueOperation = new SubdivisionIssueOperation();

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

			if(IsWriteOff) {
				var currentWriteoffItem = this.ExpenseDoc.WriteOffDoc.Items.FirstOrDefault(x => x.EmployeeWriteoffOperation.Nomenclature == Nomenclature);

				if(currentWriteoffItem == null) {
					this.ExpenseDoc.WriteOffDoc.AddItem(OldEmployeeOperationIssue, Amount);

				}
					var currenOperationtWriteOff = this.ExpenseDoc.WriteOffDoc.Items.First(x => x.EmployeeWriteoffOperation.Nomenclature == Nomenclature).EmployeeWriteoffOperation;
					EmployeeIssueOperation.EmployeeOperationIssueOnWriteOff = currenOperationtWriteOff;
					currentWriteoffItem = this.ExpenseDoc.WriteOffDoc.Items.FirstOrDefault(x => x.EmployeeWriteoffOperation.Nomenclature == Nomenclature);
					currentWriteoffItem.UpdateOperations(uow);
					currentWriteoffItem.Amount = this.Amount;
					currentWriteoffItem.AktNumber = this.AktNumber ?? "";

			}
			else {
				var currentWriteoffItem = this.ExpenseDoc.WriteOffDoc.Items.FirstOrDefault(x => x.EmployeeWriteoffOperation.Nomenclature == Nomenclature);
				if(currentWriteoffItem != null) {
					var currenOperationtWriteOff = currentWriteoffItem.EmployeeWriteoffOperation;
					currentWriteoffItem.EmployeeWriteoffOperation = null;
					ExpenseDoc.WriteOffDoc.Items.Remove(currentWriteoffItem);
					uow.Delete(currenOperationtWriteOff);
				}
			}
		}

		#endregion

	}
}

