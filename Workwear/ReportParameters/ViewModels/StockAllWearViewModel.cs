using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using Gamma.Widgets;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools.Features;

namespace Workwear.ReportParameters.ViewModels {
	public class StockAllWearViewModel : ReportParametersViewModelBase{
		public StockAllWearViewModel(RdlViewerViewModel rdlViewerViewModel,
		IUnitOfWorkFactory unitOfWorkFactory,
		INavigationManager navigation,
		ILifetimeScope autofacScope,
		StockRepository stockRepository,
		FeaturesService featuresService)
		: base(rdlViewerViewModel)
		{
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			var builder = new CommonEEVMBuilderFactory<StockAllWearViewModel>(rdlViewerViewModel, this, UoW, navigation, autofacScope);
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			
			WarehouseEntry = builder.ForProperty(x => x.Warehouse).MakeByType().Finish();
			//warehouse 
			WarehouseEntry.Entity = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId) 
				?? UoW.GetAll<Warehouse>().First();
			base.Title = "Складская ведомость";
			Warehouses=UoW.GetAll<Warehouse>().ToList();
		}
		
		IUnitOfWork UoW;
		private readonly FeaturesService featuresService;
		public bool VisibleWarehouse => featuresService.Available(WorkwearFeature.Warehouses);
		public bool VisibleSumm => ReportType == StockAllWearReportType.Flat && featuresService.Available(WorkwearFeature.Selling);
		public EntityEntryViewModel<Warehouse> WarehouseEntry;
		
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
