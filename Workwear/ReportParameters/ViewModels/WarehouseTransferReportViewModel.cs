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
				WarehousesExpense = UoW.GetAll<Warehouse>().ToList();
				WarehousesReceipt = UoW.GetAll<Warehouse>().ToList();
			}

			if(FeaturesService.Available(WorkwearFeature.Owners)) {
				Owners = UoW.GetAll<Owner>().ToList();
			}
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>() {
			{ "start_date", StartDate },
			{ "end_date", EndDate },
			{"allReceiptWarehouses", SelectReceiptWarehouse.Equals(SpecialComboState.All)},
			{"withoutReceiptWarehouses", SelectReceiptWarehouse.Equals(SpecialComboState.Not)},
			{"warehouse_receipt_id", (SelectReceiptWarehouse as Warehouse)?.Id ?? -1},
			{"allExpenseWarehouses", SelectExpenseWarehouse.Equals(SpecialComboState.All)},
			{"withoutExpenseWarehouses", SelectExpenseWarehouse.Equals(SpecialComboState.Not)},
			{"warehouse_expense_id", (SelectExpenseWarehouse as Warehouse)?.Id ?? -1},
			{"allOwners", SelectOwner.Equals(SpecialComboState.All)},
			{"withoutOwner", SelectOwner.Equals(SpecialComboState.Not)},
			{"owner_id", (SelectOwner as Owner)?.Id ?? -1},
		};

		private object selectExpenseWarehouse = SpecialComboState.All;
		public object SelectExpenseWarehouse {
			get => selectExpenseWarehouse;
			set => SetField(ref selectExpenseWarehouse, value);
		}
		
		private object selectReceiptWarehouse = SpecialComboState.All;
		public object SelectReceiptWarehouse {
			get => selectReceiptWarehouse;
			set => SetField(ref selectReceiptWarehouse, value);
		}
		
		private object selectOwner = SpecialComboState.All;

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
