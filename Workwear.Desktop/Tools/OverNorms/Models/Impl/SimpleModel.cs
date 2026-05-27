using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Tools.OverNorms.Models.Impl 
{
	/// <summary>
	/// Модель для создания операций остановочного ремонта
	/// </summary>
	public class SimpleModel : OverNormModelBase
	{
		private readonly IUnitOfWork uow;

		public SimpleModel(IUnitOfWork uow)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

//public override bool Editable { get; }

		public override bool CanUseWithBarcodes => true;
		
		public override bool CanUseWithoutBarcodes => true;
		
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
				Type = OverNormType.Simple,
				CreatedbyUser = createdByUser,
				DocNumber = docNumber,
			};
			
			foreach (OverNormParam param in @params) 
				AddItems(document, param, expenseWarehouse);
			
			return document;
		}

		public override void WriteOffOperation(OverNormOperation operation, Warehouse receiptWarehouse, UserBase createdByUser = null, string docNumber = null, string comment = null) 
		{
////1289			
			throw new InvalidOperationException("Разовые операции не списываются");
		}

		public override void AddOperation(OverNorm document, OverNormParam param, Warehouse expenseWarehouse) 
		{
			if (document == null) throw new ArgumentNullException(nameof(document));
			if (RequiresEmployeeIssueOperation && param.EmployeeIssueOperation == null) throw new InvalidOperationException("Необходимо заполнить операцию выдачи сотруднику");
			if (UseBarcodes && !param.Barcodes.Any()) throw new InvalidOperationException("При использовании штрихкодов заполните их");
			if (expenseWarehouse == null) throw new ArgumentNullException(nameof(expenseWarehouse));
			
			WarehouseOperation newWarehouseOp = new WarehouseOperation {
				ExpenseWarehouse = expenseWarehouse,
				Amount = param.Amount,
				Nomenclature = param.Nomenclature,
				WearSize = param.Size,
				Height = param.Height,
				Owner = param.Owner
			};
			
			OverNormOperation overNormOp = new OverNormOperation() {
				WarehouseOperation = newWarehouseOp,
				Type = OverNormType.Simple,
				Employee = param.Employee,
				Nomenclature = param.Nomenclature,
				WearSize = param.Size,
				Height = param.Height,
				WearPercent = param.WearPercent,
				Barcodes = param.Barcodes,
			};
			
			foreach(var barcode in param.Barcodes) 
				barcode.BarcodeOperations.Add(new BarcodeOperation(){Barcode = barcode, OverNormOperation = overNormOp});
			document.AddItem(overNormOp, param);
		}
		
		public override void UpdateOperation(OverNormItem item, OverNormParam param) 
		{
			if (item == null) throw new ArgumentNullException(nameof(item));
			if (RequiresEmployeeIssueOperation && param.EmployeeIssueOperation == null) throw new InvalidOperationException("Необходимо заполнить операцию выдачи сотруднику");
			if (UseBarcodes && param.Amount != param.Barcodes.Count) throw new InvalidOperationException("При использовании штрихкодов заполните их");

			item.OverNormOperation.LastUpdate = DateTime.Now;
			item.OverNormOperation.Employee = param.Employee;
			item.OverNormOperation.Barcodes = param.Barcodes;
			item.OverNormOperation.BarcodeOperations = param.Barcodes.SelectMany(b => b.BarcodeOperations) as IList<BarcodeOperation>;
			
			item.OverNormOperation.WarehouseOperation.Amount = param.Amount;
			item.OverNormOperation.WarehouseOperation.Nomenclature = param.Nomenclature;
			item.OverNormOperation.WarehouseOperation.WearSize = param.Size;
			item.OverNormOperation.WarehouseOperation.Height = param.Height;
			item.OverNormOperation.WarehouseOperation.WearPercent = param.WearPercent;
			item.OverNormOperation.WarehouseOperation.Owner = param.Owner;
		}
		
		private void AddItems(OverNorm document, OverNormParam param, Warehouse expenseWarehouse)
		{
			WarehouseOperation newWarehouseOp = new WarehouseOperation 
			{
				ExpenseWarehouse = expenseWarehouse,
				Amount = param.Amount,
				Nomenclature = param.Nomenclature,
				WearSize = param.Size,
				Height = param.Height,
				Owner = param.Owner,
				WearPercent = param.WearPercent
			};

			OverNormOperation newOverNormOp = new OverNormOperation() 
			{
				WarehouseOperation = newWarehouseOp,
				Type = OverNormType.Simple,
				Employee = param.Employee
			};
			
			document.AddItem(newOverNormOp);
		}
	}
}
