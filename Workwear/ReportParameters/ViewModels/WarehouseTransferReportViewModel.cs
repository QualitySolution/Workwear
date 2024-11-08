using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using Workwear.Domain.Stock;
using Workwear.Tools.Features;

namespace Workwear.ReportParameters.ViewModels {
	public class WarehouseTransferReportViewModel : ReportParametersUowViewModelBase {
		public readonly FeaturesService FeaturesService;
		public WarehouseTransferReportViewModel(RdlViewerViewModel rdlViewerViewModel, 
			IUnitOfWorkFactory unitOfWorkFactory, FeaturesService featuresService) : base(rdlViewerViewModel, unitOfWorkFactory) {
			FeaturesService = featuresService;
			Title = "Отчет по складским операциям";
			Identifier = "WarehouseTransferReport";
			if(FeaturesService.Available(WorkwearFeature.Warehouses)) {
				warehousesExpense = UoW.GetAll<Warehouse>().ToList();
				warehousesReceipt = UoW.GetAll<Warehouse>().ToList();
				warehousesExpense.Insert(0, new Warehouse(){Id=-1,Name="Любой"});
				warehousesReceipt.Insert(0,new Warehouse(){Id=-1, Name="Любой"});
				warehousesExpense.Insert(1, new Warehouse(){Id=-2, Name="Без склада"});
				warehousesReceipt.Insert(1, new Warehouse(){Id = -2, Name = "Без склада"});
			}

			if(FeaturesService.Available(WorkwearFeature.Owners)) {
				owners = UoW.GetAll<Owner>().ToList();
				owners.Insert(0,new Owner(){Id=-1,Name = "Любой"});
				owners.Insert(1, new Owner(){Id = -2,Name = "Без собственника"});
			}

			selectExpenseWarehouse = warehousesExpense?.First();
			selectReceiptWarehouse = warehousesReceipt?.First();
			selectOwner = owners?.First();
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>() {
			{ "start_date", StartDate },
			{ "end_date", EndDate },
			{"allReceiptWarehouses", (SelectReceiptWarehouse as Warehouse)?.Id == -1},
			{"withoutReceiptWarehouse", (SelectReceiptWarehouse as Warehouse)?.Id == -2},
			{"warehouse_receipt_id", (SelectReceiptWarehouse as Warehouse)?.Id ?? -1},
			{"allExpenseWarehouses", (SelectExpenseWarehouse as Warehouse)?.Id == -1},
			{"withoutExpenseWarehouse", (SelectExpenseWarehouse as Warehouse)?.Id ==-2},
			{"warehouse_expense_id", (SelectExpenseWarehouse as Warehouse)?.Id ?? -1},
			{"allOwners", (SelectOwner as Owner)?.Id == -1},
			{"withoutOwner", (SelectOwner as Owner)?.Id == -2},
			{"owner_id", (SelectOwner as Owner)?.Id ?? -1},
		};

		private object selectExpenseWarehouse;
		public object SelectExpenseWarehouse {
			get => selectExpenseWarehouse;
			set => SetField(ref selectExpenseWarehouse, value);
		}

		private object selectReceiptWarehouse;
		public object SelectReceiptWarehouse {
			get => selectReceiptWarehouse;
			set => SetField(ref selectReceiptWarehouse, value);
		}

		private object selectOwner;

		public object SelectOwner {
			get => selectOwner;
			set => SetField(ref selectOwner, value);
		}
		
		private DateTime? startDate = DateTime.Now.AddMonths(-1);
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
			
		}
		private DateTime? endDate = DateTime.Now;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}
		
		private IList<Warehouse> warehousesExpense;
		public IList<Warehouse> WarehousesExpense {
			get => warehousesExpense;
			set => SetField(ref warehousesExpense, value);
		}
		
		private IList<Warehouse> warehousesReceipt;
		public IList<Warehouse> WarehousesReceipt {
			get => warehousesReceipt;
			set => SetField(ref warehousesReceipt, value);
		}

		private IList<Owner> owners;
		public IList<Owner> Owners {
			get => owners;
			set => SetField(ref owners, value);
		}
		public bool SensetiveLoad => StartDate != null && EndDate != null && startDate <= endDate;
	}
}
