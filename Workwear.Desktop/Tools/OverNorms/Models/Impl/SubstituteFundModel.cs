﻿using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.OverNorms.Impl;

namespace Workwear.Tools.OverNorms.Models.Impl 
{
	/// <summary>
	/// Модель для создания операций с подменного фонда
	/// </summary>
	public class SubstituteFundModel : OverNormModelBase 
	{
		private readonly IUnitOfWork uow;

		public SubstituteFundModel(IUnitOfWork uow) 
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		//public override bool Editable { get; }

		public override bool CanUseWithBarcodes => true;

		public override bool CanUseWithoutBarcodes => false;

		public override bool RequiresEmployeeIssueOperation => true;

		public override OverNorm CreateDocument(IList<OverNormParam> @params, Warehouse expenseWarehouse, UserBase createdByUser = null, string docNumber = null, string comment = null) 
		{
			if (@params == null) throw new ArgumentNullException(nameof(@params));
			if (!@params.Any()) throw new ArgumentException(nameof(@params));
			if (RequiresEmployeeIssueOperation && @params.Any(x => x.EmployeeIssueOperation == null)) throw new InvalidOperationException("Необходимо заполнить операции выдачи сотруднику");
			if (UseBarcodes && @params.Any(x => !x.Barcodes.Any())) throw new InvalidOperationException("При использовании штрихкодов заполните их");
			if (expenseWarehouse == null) throw new ArgumentNullException(nameof(expenseWarehouse));

			OverNorm document = new OverNorm() 
			{
				Warehouse = expenseWarehouse,
				Comment = comment,
				Type = OverNormType.Substitute,
				CreatedbyUser = createdByUser,
				DocNumber = docNumber
			};

			foreach (OverNormParam param in @params) 
			{
				AddItem(document, param, expenseWarehouse);
			}
			
			return document;
		}

		public override void WriteOffOperation(OverNormOperation operation, Warehouse receiptWarehouse, UserBase createdByUser = null, string docNumber = null, string comment = null) 
		{
			if (operation == null) throw new ArgumentNullException(nameof(operation));
			if (receiptWarehouse == null) throw new ArgumentNullException(nameof(receiptWarehouse));

			WarehouseOperation newWarehouseOp = new WarehouseOperation() 
			{
				ReceiptWarehouse = receiptWarehouse,
				Amount = operation.WarehouseOperation.Amount,
				Nomenclature = operation.WarehouseOperation.Nomenclature,
				WearSize = operation.WarehouseOperation.WearSize,
				Height = operation.WarehouseOperation.Height
			};

			OverNormOperation writeOff = CreateOperationWithBarcodes(newWarehouseOp, operation.Employee, operation.SubstitutedIssueOperation, operation.BarcodeOperations.Select(x => x.Barcode));
			operation.ReturnFromOperation = writeOff;
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
			item.OverNormOperation.Employee = param.Employee;
			item.OverNormOperation.SubstitutedIssueOperation = param.EmployeeIssueOperation ?? item.OverNormOperation.SubstitutedIssueOperation;

			item.OverNormOperation.WarehouseOperation.OperationTime = DateTime.Now;
			item.OverNormOperation.WarehouseOperation.Amount = param.Amount;
			item.OverNormOperation.WarehouseOperation.Nomenclature = param.Nomenclature;
			item.OverNormOperation.WarehouseOperation.WearSize = param.Size;
			item.OverNormOperation.WarehouseOperation.Height = param.Height;

			List<int> currentBarcodeIds = item.OverNormOperation.BarcodeOperations.Select(bo => bo.Barcode.Id).ToList();
			if (!UseBarcodes) 
			{
				item.OverNormOperation.BarcodeOperations.Clear();
			}
			else if (UseBarcodes && (param.Barcodes.Any(b => !currentBarcodeIds.Contains(b.Id)) || param.Barcodes.Count != currentBarcodeIds.Count)) 
			{
				int amountToUpdate = item.OverNormOperation.BarcodeOperations.Count > param.Amount
					? param.Amount
					: item.OverNormOperation.BarcodeOperations.Count;   
				if (item.OverNormOperation.BarcodeOperations.Count > param.Amount) 
				{
					int count = item.OverNormOperation.BarcodeOperations.Count;
					for (int i = count - 1; i >= count - param.Amount; i--) 
					{
						item.OverNormOperation.BarcodeOperations.RemoveAt(i);
					}
				}
				else 
				{
					FillOverNormOperation(item.OverNormOperation, param.Barcodes.Skip(amountToUpdate));
				}
				
				for (int i = 0; i < amountToUpdate; i++) 
				{
					BarcodeOperation bo = item.OverNormOperation.BarcodeOperations[i];
					if (bo.Barcode.Id != param.Barcodes[i].Id) 
					{
						bo.Barcode = param.Barcodes[i];
					}
				}
			}
		}

		private void AddItem(OverNorm document, OverNormParam param, Warehouse expenseWarehouse) 
		{
			WarehouseOperation newWarehouseOp = new WarehouseOperation 
			{
				ExpenseWarehouse = expenseWarehouse,
				Amount = param.Amount,
				Nomenclature = param.Nomenclature,
				WearSize = param.Size,
				Height = param.Height
				//TODO: owner
			};

			OverNormOperation newOverNormOp =
				CreateOperationWithBarcodes(newWarehouseOp, param.Employee, param.EmployeeIssueOperation, param.Barcodes);
			document.AddItem(newOverNormOp, param);
		}

		private OverNormOperation CreateOperationWithBarcodes(WarehouseOperation newWarehouseOp, EmployeeCard employee, EmployeeIssueOperation employeeIssueOperation, IEnumerable<Barcode> barcodes) 
		{
			OverNormOperation newOverNormOp = new OverNormOperation() 
			{
				SubstitutedIssueOperation = employeeIssueOperation,
				WarehouseOperation = newWarehouseOp,
				Type = OverNormType.Substitute,
				Employee = employee
			};

			FillOverNormOperation(newOverNormOp, barcodes);
			return newOverNormOp;
		}

		private void FillOverNormOperation(OverNormOperation overNormOp, IEnumerable<Barcode> barcodes) 
		{
			foreach (Barcode substituteBarcode in barcodes) 
			{
				BarcodeOperation barcodeOperation = new BarcodeOperation() 
				{
					Barcode = substituteBarcode,
					OverNormOperation = overNormOp
				};

				overNormOp.BarcodeOperations.Add(barcodeOperation);
			}
		}
	}
}
