using System;
using System.ComponentModel.DataAnnotations;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Domain.Stock.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки прихода",
		Nominative = "строка прихода",
		Genitive = "строки прихода"
		)]
	[HistoryTrace]
	public class IncomeItem : PropertyChangedBase, IDomainObject , IDocItemSizeInfo
	{
		#region Свойства

		public virtual int Id { get; set; }

		private Income document;

		[Display(Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual Income Document {
			get => document;
			set => SetField(ref document, value);
		}

		Nomenclature nomenclature;
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		[Display(Name = "Тип Роста")]
		public virtual SizeType HeightType {
			get => nomenclature.Type.HeightType;
		}
		
		[Display(Name = "Тип размера одежды")]
		public virtual SizeType WearSizeType {
			get => nomenclature.Type.SizeType;
		}

		private int amount;
		[Display (Name = "Количество")]
		[PropertyChangedAlso("Total")]
		public virtual int Amount {
			get => amount;
			set { SetField (ref amount, value, () => Amount); }
		}

		private decimal cost;
		[Display (Name = "Цена")]
		[PropertyChangedAlso("Total")]
		public virtual decimal Cost {
			get => cost;
			set { SetField (ref cost, value, () => Cost); }
		}

		private string certificate;
		[Display(Name = "№ сертификата")]
		public virtual string Certificate {
			get => certificate;
			set { SetField(ref certificate, value, () => Certificate); }
		}

		private EmployeeIssueOperation returnFromEmployeeOperation;
		[Display(Name = "Операция возврата от сотрудника")]
		[IgnoreHistoryTrace]
		public virtual EmployeeIssueOperation ReturnFromEmployeeOperation {
			get => returnFromEmployeeOperation;
			set => SetField(ref returnFromEmployeeOperation, value);
		}

		private SubdivisionIssueOperation returnFromSubdivisionOperation;
		[Display(Name = "Операция возврата из подразделения")]
		[IgnoreHistoryTrace]
		public virtual SubdivisionIssueOperation ReturnFromSubdivisionOperation {
			get => returnFromSubdivisionOperation;
			set => SetField(ref returnFromSubdivisionOperation, value);
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
			set => wearSize = value;
		}
		private Size height;
		[Display(Name = "Рост одежды")]
		public virtual Size Height {
			get => height;
			set => height = value;
		}

		[Display(Name = "Собственник имущества")]
		public virtual Owner Owner {
			get => WarehouseOperation.Owner;
			set {
				if(WarehouseOperation.Owner != value) {
					WarehouseOperation.Owner = value;
					OnPropertyChanged();
				}
			}
		}

		#endregion
		#region Расчетные
		public virtual string Title =>
			$"Поступление на склад {Nomenclature?.Name} в количестве {Amount} {Nomenclature?.Type?.Units?.Name}";

		public virtual decimal Total => Cost * Amount;
		public virtual StockPosition StockPosition => 
			new StockPosition(Nomenclature, WarehouseOperation.WearPercent, WearSize, Height, Owner);
		#endregion
		#region Не сохраняемые в базу свойства
		private string buhDocument;
		[Display(Name = "Документ бухгалтерского учета")]
		//В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		public virtual string BuhDocument {
			get => buhDocument ?? ReturnFromEmployeeOperation?.BuhDocument;
			set => SetField(ref buhDocument, value);
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
		protected IncomeItem () { }
		public IncomeItem(Income income) {
			document = income;
		}

		#region Функции
		public virtual void UpdateOperations(IUnitOfWork uow, IInteractiveQuestion askUser) {
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

