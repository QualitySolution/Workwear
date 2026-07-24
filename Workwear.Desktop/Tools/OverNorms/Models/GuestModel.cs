using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Tools.OverNorms.Models
{
	/// <summary>
	/// Модель для создания операций с гостевого склада
	/// </summary>
	public class GuestModel : OverNormModelBase 
	{
		private readonly IUnitOfWork uow;

		public GuestModel(IUnitOfWork uow)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public override bool CanUseWithBarcodes => true;
		public override bool CanUseWithoutBarcodes => false;
		public override bool RequiresEmployeeIssueOperation => false;

		public override OverNorm CreateDocument(IList<OverNormParam> @params, Warehouse expenseWarehouse, UserBase createdByUser = null, string docNumber = null, string comment = null) 
		{
			if (@params == null) throw new ArgumentNullException(nameof(@params));
			if (!@params.Any()) throw new ArgumentException(nameof(@params));
			if (RequiresEmployeeIssueOperation && @params.Any(x => x.EmployeeIssueOperation == null)) throw new InvalidOperationException("Необходимо заполнить операции выдачи сотруднику");
			if (UseBarcodes && @params.Any(x => !x.Barcodes.Any())) throw new InvalidOperationException("При использовании штрихкодов заполните их");
			if (expenseWarehouse == null) throw new ArgumentNullException(nameof(expenseWarehouse));
			
			OverNorm document = new OverNorm() {
				Warehouse = expenseWarehouse,
				Comment = comment,
				Type = OverNormType.Guest,
				CreatedbyUser = createdByUser,
				DocNumber = docNumber,
			};
			
			foreach (OverNormParam param in @params) 
				AddItem(document, param, expenseWarehouse);
			
			return document;
		}

		public override void WriteOffOperation(OverNormOperation operation, Warehouse receiptWarehouse, UserBase createdByUser = null, string docNumber = null, string comment = null) 
		{
			if (operation == null) throw new ArgumentNullException(nameof(operation));
			if (receiptWarehouse == null) throw new ArgumentNullException(nameof(receiptWarehouse));

			WarehouseOperation newWarehouseOp = new WarehouseOperation() {
				ReceiptWarehouse = receiptWarehouse,
				Amount = operation.WarehouseOperation.Amount,
				Nomenclature = operation.WarehouseOperation.Nomenclature,
				WearSize = operation.WarehouseOperation.WearSize,
				Height = operation.WarehouseOperation.Height
			};
			
			OverNormOperation writeOff = CreateOperationWithBarcodes(newWarehouseOp, operation.Employee, operation.BarcodeOperations.Select(x => x.Barcode));
			writeOff.ReturnFromOperation = operation;
		}

		public override void AddOperation(OverNorm document, OverNormParam param, Warehouse expenseWarehouse) 
		{
			if (document == null) throw new ArgumentNullException(nameof(document));
			if (RequiresEmployeeIssueOperation && param.EmployeeIssueOperation == null) throw new InvalidOperationException("Необходимо заполнить операцию выдачи сотруднику");
			if (UseBarcodes && !param.Barcodes.Any()) throw new InvalidOperationException("При использовании штрихкодов заполните их");
			if (expenseWarehouse == null) throw new ArgumentNullException(nameof(expenseWarehouse));
			
			AddItem(document, param, expenseWarehouse);
		}

		public override void UpdateOperation(OverNormItem item, OverNormParam param) 
		{
			if (item == null) throw new ArgumentNullException(nameof(item));
			if (RequiresEmployeeIssueOperation && param.EmployeeIssueOperation == null) throw new InvalidOperationException("Необходимо заполнить операцию выдачи сотруднику");
			if (UseBarcodes && param.Amount != param.Barcodes.Count) throw new InvalidOperationException("При использовании штрихкодов заполните их");
			
			item.OverNormOperation.LastUpdate = DateTime.Now;
			item.OverNormOperation.Type = OverNormType.Guest;
			item.OverNormOperation.Employee = param.Employee;
			item.OverNormOperation.SubstitutedIssueOperation = RequiresEmployeeIssueOperation ? param.EmployeeIssueOperation : item.OverNormOperation.SubstitutedIssueOperation;
			item.OverNormOperation.Nomenclature = param.Nomenclature;
			item.OverNormOperation.WearSize = param.Size;
			item.OverNormOperation.Height = param.Height;
			item.OverNormOperation.WearPercent = param.WearPercent;
			
			if(param.Amount > 0 && item.OverNormOperation.WarehouseOperation == null) 
				item.OverNormOperation.WarehouseOperation = new WarehouseOperation() { ExpenseWarehouse = item.Document.Warehouse };
			item.OverNormOperation.WarehouseOperation.Amount = param.Amount;
			item.OverNormOperation.WarehouseOperation.StockPosition = param.StockPosition;
			UpdateBarcodeOperations(item.OverNormOperation, param.Barcodes);
		}
		
		private void AddItem(OverNorm document, OverNormParam param, Warehouse expenseWarehouse)
		{
			WarehouseOperation newWarehouseOp = new WarehouseOperation {
				ExpenseWarehouse = expenseWarehouse,
				Amount = param.Amount,
				StockPosition = param.StockPosition
			};

			OverNormOperation newOverNormOp;
			if (UseBarcodes && param.Barcodes.Any()) 
				newOverNormOp = CreateOperationWithBarcodes(newWarehouseOp, param.Employee,  param.Barcodes);
			else {
				newOverNormOp = new OverNormOperation() {
					WarehouseOperation = newWarehouseOp,
					Type = OverNormType.Guest,
					Employee = param.Employee
				};
			}
			
			document.AddItem(newOverNormOp);
		}
		
		private OverNormOperation CreateOperationWithBarcodes(WarehouseOperation newWarehouseOp, EmployeeCard employee, IEnumerable<Barcode> barcodes) 
		{
			OverNormOperation newOverNormOp = new OverNormOperation() {
				WarehouseOperation = newWarehouseOp,
				Type = OverNormType.Guest,
				Employee = employee,
				Nomenclature = newWarehouseOp.Nomenclature,
				WearSize = newWarehouseOp.WearSize,
				Height = newWarehouseOp.Height,
				WearPercent = newWarehouseOp.WearPercent,
			};
			
			AddBarcodeOperations(newOverNormOp, barcodes);
			return newOverNormOp;
		}
	}
}
