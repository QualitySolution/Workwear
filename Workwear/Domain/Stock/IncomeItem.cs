using System;
using System.ComponentModel.DataAnnotations;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using workwear.Domain.Operations;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки прихода",
		Nominative = "строка прихода",
		Genitive = "строки прихода"
		)]
	[HistoryTrace]
	public class IncomeItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private Income document;

		[Display(Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual Income Document {
			get { return document; }
			set { SetField(ref document, value); }
		}

		Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		int amount;

		[Display (Name = "Количество")]
		[PropertyChangedAlso("Total")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		decimal cost;

		[Display (Name = "Цена")]
		[PropertyChangedAlso("Total")]
		public virtual decimal Cost {
			get { return cost; }
			set { SetField (ref cost, value, () => Cost); }
		}

		private string certificate;

		[Display(Name = "№ сертификата")]
		public virtual string Certificate
		{
			get { return certificate; }
			set { SetField(ref certificate, value, () => Certificate); }
		}

		private EmployeeIssueOperation returnFromEmployeeOperation;

		[Display(Name = "Операция возврата от сотрудника")]
		[IgnoreHistoryTrace]
		public virtual EmployeeIssueOperation ReturnFromEmployeeOperation
		{
			get { return returnFromEmployeeOperation; }
			set { SetField(ref returnFromEmployeeOperation, value); }
		}

		private SubdivisionIssueOperation returnFromSubdivisionOperation;

		[Display(Name = "Операция возврата из подразделения")]
		[IgnoreHistoryTrace]
		public virtual SubdivisionIssueOperation ReturnFromSubdivisionOperation {
			get { return returnFromSubdivisionOperation; }
			set { SetField(ref returnFromSubdivisionOperation, value); }
		}

		private WarehouseOperation warehouseOperation = new WarehouseOperation();
		[Display(Name = "Операция на складе")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation WarehouseOperation {
			get { return warehouseOperation; }
			set { SetField(ref warehouseOperation, value); }
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

		#endregion

		#region Расчетные

		public virtual string Title {
			get { return String.Format ("Поступление на склад {0} в количестве {1} {2}",
				Nomenclature?.Name,
				Amount,
				Nomenclature?.Type?.Units?.Name
			);}
		}

		public virtual decimal Total => Cost * Amount;

		public virtual StockPosition StockPosition => new StockPosition(Nomenclature, Size, WearGrowth, WarehouseOperation.WearPercent);

		#endregion

		#region Не сохраняемые в базу свойства

		private string buhDocument;

		[Display(Name = "Документ бухгалтерского учета")]
		//В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		public virtual string BuhDocument
		{
			get { return buhDocument ?? ReturnFromEmployeeOperation?.BuhDocument; }
			set { SetField(ref buhDocument, value); }
		}

		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get => WarehouseOperation.WearPercent;
			set => WarehouseOperation.WearPercent = value;
		}

		private EmployeeIssueOperation issuedEmployeeOnOperation;

		/// <summary>
		/// Это ссылка на операцию выдачи по которой был выдан сотруднику поступивший от него СИЗ
		/// В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		/// </summary>
		[Display(Name = "Операция выдачи сотруднику")]
		public virtual EmployeeIssueOperation IssuedEmployeeOnOperation {
			get => issuedEmployeeOnOperation ?? ReturnFromEmployeeOperation?.IssuedOperation;
			set => SetField(ref issuedEmployeeOnOperation, value);
		}

		private SubdivisionIssueOperation issuedSubdivisionOnOperation;

		/// <summary>
		/// Это ссылка на операцию выдачи по которой был выдан на подразделение поступивший от него СИЗ
		/// В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		/// </summary>
		[Display(Name = "Операция выдачи на подразделение")]
		public virtual SubdivisionIssueOperation IssuedSubdivisionOnOperation {
			get => issuedSubdivisionOnOperation ?? returnFromSubdivisionOperation?.IssuedOperation;
			set => SetField(ref issuedSubdivisionOnOperation, value);
		}

		#endregion

		protected IncomeItem ()
		{
		}

		public IncomeItem(Income income)
		{
			document = income;
		}

		#region Функции

		public virtual void UpdateOperations(IUnitOfWork uow, IInteractiveQuestion askUser)
		{
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);

			if(Document.Operation == IncomeOperations.Return) {
				if(ReturnFromEmployeeOperation == null)
					ReturnFromEmployeeOperation = new EmployeeIssueOperation();
				ReturnFromEmployeeOperation.Update(uow, askUser, this);
				uow.Save(ReturnFromEmployeeOperation);
			}
			else if(ReturnFromEmployeeOperation != null) {
				uow.Delete(ReturnFromEmployeeOperation);
				ReturnFromEmployeeOperation = null;
			}

			if(Document.Operation == IncomeOperations.Object) {
				if(ReturnFromSubdivisionOperation == null)
					ReturnFromSubdivisionOperation = new SubdivisionIssueOperation();
				ReturnFromSubdivisionOperation.Update(uow, askUser, this);
				uow.Save(ReturnFromSubdivisionOperation);
			}
			else if(ReturnFromSubdivisionOperation != null) {
				uow.Delete(ReturnFromSubdivisionOperation);
				ReturnFromSubdivisionOperation = null;
			}
		}

		#endregion

	}
}

