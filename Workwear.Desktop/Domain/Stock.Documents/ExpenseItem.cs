using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Statements;
using Workwear.Models.Operations;
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
			get => protectionTools;
			set => SetField(ref protectionTools, value);
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

		private EmployeeIssueOperation employeeIssueOperation;
		[Display(Name = "Операция выдачи сотруднику")]
		[IgnoreHistoryTrace]
		public virtual EmployeeIssueOperation EmployeeIssueOperation
		{
			get => employeeIssueOperation;
			set => SetField(ref employeeIssueOperation, value);
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
		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get => WarehouseOperation.WearPercent;
			set => WarehouseOperation.WearPercent = value;
		}

		private EmployeeCardItem employeeCardItem;
		
		public virtual EmployeeCardItem EmployeeCardItem {
			get => employeeCardItem;
			set => employeeCardItem = value;
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

		public virtual StockPosition StockPosition {
			get => new StockPosition(Nomenclature, WearPercent, WearSize, Height, WarehouseOperation.Owner);
			set {
				Nomenclature = value.Nomenclature;
				WearSize = value.WearSize;
				Height = value.Height;
				WearPercent = value.WearPercent;
				Owner = value.Owner;
			}
		}

		private StockBalance stockBalanceSetter;
		public virtual StockBalance StockBalanceSetter {
			get => stockBalanceSetter ?? (Nomenclature != null ? new StockBalance(StockPosition, 0) : null);
			set {
				stockBalanceSetter = value;
				StockPosition = value.Position;
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

	
		public virtual string BarcodeTextFunc(BarcodeTypes type) {
			if((!Nomenclature?.UseBarcode ?? true) || Amount <= 0)
				return null;
			var actualOperations = EmployeeIssueOperation?.BarcodeOperations?
				.Where(x => x.Barcode?.Type == type).ToList() ?? new List<BarcodeOperation>();
			if (!actualOperations.Any())
				return "необходимо создать";
			else { //Рассчитываем максимум на 3 строки, если штрих кода 3, отображаем их все. Если больше 3-х третью строку занимаем под надпись...
				var willTake = actualOperations.Count() > 3 ? 2 : 3;
				var text = String.Join("\n", actualOperations.Take(willTake).Select(x => x.Barcode.Title));
				if(actualOperations.Count > 3) {
					text += $"\nещё {actualOperations.Count - 2}";
				}
				return text;
			}
		}

		public virtual string BarcodesTextColor {
			get {
				if(Nomenclature == null || !Nomenclature.UseBarcode || EmployeeIssueOperation == null)
					return null;

				if(Amount == EmployeeIssueOperation.BarcodeOperations.Count)
					return null;

				if(Amount < EmployeeIssueOperation.BarcodeOperations.Count)
					return "blue";
				if(Amount > EmployeeIssueOperation.BarcodeOperations.Count)
					return "red";
				return null;
			}
		}
		#endregion

		public ExpenseItem ()
		{
		}

		#region Функции

		public virtual void UpdateWarehouseOperations(IUnitOfWork uow) {
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);
		}
		
		public virtual void UpdateOperations(IUnitOfWork uow, BaseParameters baseParameters, IInteractiveQuestion askUser, string signCardUid = null) 
		{
			UpdateWarehouseOperations(uow);
			
			if (EmployeeIssueOperation == null) {
				EmployeeIssueOperation = new EmployeeIssueOperation (baseParameters);
			}

			EmployeeIssueOperation.Update(uow, baseParameters, askUser, this, signCardUid);
								
			uow.Save(EmployeeIssueOperation);
		}
		#endregion
	}
}

