using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using Gamma.Widgets;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.ViewModels.Control;
using QS.ViewModels.Extension;
using Workwear.Domain.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;

namespace Workwear.ReportParameters.ViewModels {
	public class StockAllWearViewModel : ReportParametersUowViewModelBase, IDialogDocumentation {
		public StockAllWearViewModel(RdlViewerViewModel rdlViewerViewModel,
		IUnitOfWorkFactory unitOfWorkFactory,
		FeaturesService featuresService)
		: base(rdlViewerViewModel, unitOfWorkFactory)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			base.Title = "Складская ведомость";
			Warehouses=UoW.GetAll<Warehouse>().ToList();
			ChoiceNomenclatureViewModel = new ChoiceListViewModel<Nomenclature>(UoW.GetAll<Nomenclature>().ToList());
			allNomenclatureItems = ChoiceNomenclatureViewModel.Items.ToList();
			ChoiceNomenclatureViewModel.PropertyChanged += (s, e) => OnPropertyChanged(nameof(SensitiveLoad));
			
			if(featuresService.Available(WorkwearFeature.Owners)) {
				Owners = UoW.GetAll<Owner>().ToList();
				ShowOwners = UoW.GetAll<Owner>().Any();
			}
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("reports.html#stock-list");
		public string ButtonTooltip => DocHelper.GetReportDocTooltip(Title);
		#endregion
		
		private readonly FeaturesService featuresService;
		public IList<Owner> Owners { get; } = new List<Owner>();
		public bool ShowOwners { get; }
		public bool VisibleWarehouse => featuresService.Available(WorkwearFeature.Warehouses);
		
		public bool VisibleBarcodes => featuresService.Available(WorkwearFeature.Barcodes) && ReportType == StockAllWearReportType.Flat;
		public bool VisibleSumm => ReportType == StockAllWearReportType.Flat && featuresService.Available(WorkwearFeature.Selling);
		public bool VisibleFlatOptions => ReportType == StockAllWearReportType.Flat;
		public ChoiceListViewModel<Nomenclature> ChoiceNomenclatureViewModel { get; }
		private readonly IList<ISelectableEntity> allNomenclatureItems;
		
		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"report_date", ReportDate },
			{"warehouse_id", (SelectWarehouse as Warehouse)?.Id ?? -1},
			{"allWarehouses",SelectWarehouse.Equals(SpecialComboState.All)},
			{"ownerVisible", ShowOwners},
			{"allOwners", SelectOwner.Equals(SpecialComboState.All)},
			{"withoutOwner", SelectOwner.Equals(SpecialComboState.Not)},
			{"ownerId", (SelectOwner as Owner)?.Id ?? -1},
			{"showSumm", ShowSumm},
			{"showSex", ShowSex},
			{"show_barcode", ShowBarcodes},
			{"show_over_norm", ShowOverNorm},
			{"show_service", ShowService},
			{"nomenclature_ids", OnlyMarkable ? ChoiceNomenclatureViewModel.SelectedIds : ChoiceNomenclatureViewModel.SelectedIdsMod},
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
		
		private object selectOwner = SpecialComboState.All;
		public object SelectOwner {
			get => selectOwner;
			set => SetField(ref selectOwner, value);
		}
		
		private Warehouse warehouse;
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}
 
		private StockAllWearReportType reportType;
		[PropertyChangedAlso(nameof(VisibleBarcodes))]
		public virtual StockAllWearReportType ReportType {
			get => reportType;
			set {
				SetField(ref reportType, value); 
				if(value != StockAllWearReportType.Flat)
					OnlyMarkable = false;
				OnPropertyChanged(nameof(VisibleSumm));
				OnPropertyChanged(nameof(VisibleFlatOptions));
			}
		}
		
		private DateTime? reportDate = DateTime.Today;
		public virtual DateTime? ReportDate {
			get => reportDate;
			set {
				if(SetField(ref reportDate, value))
					OnPropertyChanged(nameof(SensitiveLoad));
			}
		}
		private bool showSumm;
		public virtual bool ShowSumm {
			get=> showSumm;
			set=> SetField(ref showSumm, value);
		}
		private bool showBarcodes;
		public virtual bool ShowBarcodes {
			get=> showBarcodes;
			set=> SetField(ref showBarcodes, value);
		}
		private bool showSex;
		private bool onlyMarkable;
		public bool OnlyMarkable {
			get => onlyMarkable;
			set {
				if(!SetField(ref onlyMarkable, value))
					return;
				ChoiceNomenclatureViewModel.Items.Clear();
				//При проставлении очищаем скрываемые позиции 
				foreach(var item in allNomenclatureItems) {
					if(value && !((SelectableEntity<Nomenclature>)item).Entity.UseBarcode) {
						item.Select = false;
						continue;
					}
					ChoiceNomenclatureViewModel.Items.Add(item);
				}
				OnPropertyChanged(nameof(SensitiveLoad));
			}
		}
		
		private bool showOverNorm;
		public bool ShowOverNorm {
			get => showOverNorm;
			set => SetField(ref showOverNorm, value);
		}
		private bool showService;
		public bool ShowService {
			get => showService;
			set => SetField(ref showService, value);
		}
		public virtual bool ShowSex {
			get => showSex;
			set=>SetField(ref showSex, value);
		}
		private IList<Warehouse> warehouses;
		public IList<Warehouse> Warehouses {
			get => warehouses;
			set => SetField(ref warehouses, value); 
		}
		public bool SensitiveLoad => ReportDate != null && !ChoiceNomenclatureViewModel.AllUnSelected;

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
