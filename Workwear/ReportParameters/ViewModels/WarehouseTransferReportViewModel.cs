using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Widgets;
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
				Warehouses = UoW.GetAll<Warehouse>().ToList();
			}

			if(FeaturesService.Available(WorkwearFeature.Owners)) {
				Owners = UoW.GetAll<Owner>().ToList();
			}
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>() {
			{ "start_date", StartDate },
			{ "end_date", EndDate },
			{"allReceiptWarehouses", SelectWarehouse.Equals(SpecialComboState.All)},
			{"withoutReceiptWarehouses", SelectWarehouse.Equals(SpecialComboState.Not)},
			{"warehouse_receipt_id", (SelectWarehouse as Warehouse)?.Id ?? -1},
			{"allExpenseWarehouses", SelectWarehouse.Equals(SpecialComboState.All)},
			{"withoutExpenseWarehouses", SelectWarehouse.Equals(SpecialComboState.Not)},
			{"warehouse_expense_id", (SelectWarehouse as Warehouse)?.Id ?? -1},
			{"allOwners", SelectOwner.Equals(SpecialComboState.All)},
			{"withoutOwner", SelectOwner.Equals(SpecialComboState.Not)},
			{"owner_id", (SelectOwner as Owner)?.Id ?? -1},
		};

		private object selectWarehouse = SpecialComboState.All;
		public object SelectWarehouse {
			get => selectWarehouse;
			set => SetField(ref selectWarehouse, value);
		}
		
		private object selectOwner = SpecialComboState.All;

		public object SelectOwner {
			get => selectOwner;
			set => SetField(ref selectOwner, value);
		}
		
		private DateTime? startDate = DateTime.Now.AddMonths(-1);
		public virtual DateTime? StartDate {
			get => startDate;
			set {
				if(SetField(ref startDate, value))
					OnPropertyChanged(nameof(SensetiveLoad));
			}
		}

		private DateTime? endDate = DateTime.Now;
		public virtual DateTime? EndDate {
			get => endDate;
			set {
				if(SetField(ref endDate, value))
					OnPropertyChanged(nameof(SensetiveLoad));
			}
		}
		
		private IList<Warehouse> warehouses;
		public IList<Warehouse> Warehouses {
			get => warehouses;
			set => SetField(ref warehouses, value);
		}

		private IList<Owner> owners;
		public IList<Owner> Owners {
			get => owners;
			set => SetField(ref owners, value);
		}

		public bool SensetiveLoad => (StartDate != null && EndDate != null && startDate <= endDate);
	}
}
