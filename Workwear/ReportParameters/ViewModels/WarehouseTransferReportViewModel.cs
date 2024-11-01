using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Widgets;
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
			Title = "Отчет по перемещениям между складами";
			Identifier = "WarehouseTransferReport";
			if(FeaturesService.Available(WorkwearFeature.Warehouses)) {
				warehousesExpense = UoW.GetAll<Warehouse>().ToList();
				warehousesReceipt = UoW.GetAll<Warehouse>().ToList();
				warehousesExpense.Add(new Warehouse(){Id=-1,Name="Любой"});
				warehousesReceipt.Add(new Warehouse(){Id=-1, Name="Любой"});
			}

			if(FeaturesService.Available(WorkwearFeature.Owners)) {
				owners = UoW.GetAll<Owner>().ToList();
				owners.Add(new Owner(){Id=-1,Name = "Любой"});
			}

			selectExpenseWarehouse = warehousesExpense?.First();
			selectReceiptWarehouse = warehousesReceipt?.First();
			selectOwner = owners?.First();
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>() {
			{ "start_date", StartDate },
			{ "end_date", EndDate },
			{"allReceiptWarehouses", (SelectReceiptWarehouse as Warehouse)?.Id == -1},
			{"withoutReceiptWarehouse", SelectReceiptWarehouse.Equals(SpecialComboState.Not)},
			{"warehouse_receipt_id", (SelectReceiptWarehouse as Warehouse)?.Id ?? -1},
			{"allExpenseWarehouses", (SelectExpenseWarehouse as Warehouse)?.Id == -1},
			{"withoutExpenseWarehouse", SelectExpenseWarehouse.Equals(SpecialComboState.Not)},
			{"warehouse_expense_id", (SelectExpenseWarehouse as Warehouse)?.Id ?? -1},
			{"allOwners", (SelectOwner as Owner)?.Id == -1},
			{"withoutOwner", SelectOwner.Equals(SpecialComboState.Not)},
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
