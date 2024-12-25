using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using Gamma.Widgets;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using Workwear.Domain.Stock;
using Workwear.Tools.Features;

namespace Workwear.ReportParameters.ViewModels {
	public class StockAllWearViewModel : ReportParametersViewModelBase{
		public StockAllWearViewModel(RdlViewerViewModel rdlViewerViewModel,
		IUnitOfWorkFactory unitOfWorkFactory,
		FeaturesService featuresService)
		: base(rdlViewerViewModel)
		{
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			base.Title = "Складская ведомость";
			//warehouse 
			Warehouses=UoW.GetAll<Warehouse>().ToList();
		}
		
		IUnitOfWork UoW;
		private readonly FeaturesService featuresService;
		public bool VisibleWarehouse => featuresService.Available(WorkwearFeature.Warehouses);
		public bool VisibleSumm => ReportType == StockAllWearReportType.Flat && featuresService.Available(WorkwearFeature.Selling);
		
		
		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"report_date", ReportDate },
			{"warehouse_id", (SelectWarehouse as Warehouse)?.Id ?? -1},
			{"allWarehouses",SelectWarehouse.Equals(SpecialComboState.All)},
			{"ownerVisible", featuresService.Available(WorkwearFeature.Owners)},
			{"showSumm", ShowSumm},
			{"showSex", ShowSex},
			{"warehouse_name", (SelectWarehouse as Warehouse)?.Name ?? " "},
			
		};
		
		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}
		private object selectWarehouse =SpecialComboState.All;

		public object SelectWarehouse {
			get => selectWarehouse;
			set => SetField(ref selectWarehouse, value);
		}
		
		private Warehouse warehouse;
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}
 
		private StockAllWearReportType reportType;
		public virtual StockAllWearReportType ReportType {
			get => reportType;
			set {
				SetField(ref reportType, value); 
				OnPropertyChanged(nameof(VisibleSumm));
			}
		}
		
		private DateTime? reportDate = DateTime.Today;
		public virtual DateTime? ReportDate {
			get => reportDate;
			set => SetField(ref reportDate, value);
		}
		private bool showSumm;
		public virtual bool ShowSumm {
			get=> showSumm;
			set=> SetField(ref showSumm, value);
		}

		private bool showSex;
		public virtual bool ShowSex {
			get => showSex;
			set=>SetField(ref showSex, value);
		}
		private IList<Warehouse> warehouses;
		public IList<Warehouse> Warehouses {
			get => warehouses;
			set => SetField(ref warehouses, value); 
		}
	}
	
	public enum StockAllWearReportType {
		[ReportIdentifier("StockBallanceReport")]
		[Display(Name = "Форматировано")]
		Common,
		[ReportIdentifier("StockBallanceReportFlat")]
		[Display(Name = "Только данные")]
		Flat
	}
}
