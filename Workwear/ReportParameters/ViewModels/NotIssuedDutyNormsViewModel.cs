using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.ViewModels.Control;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Repository.Regulations;
using Workwear.Tools.Features;

namespace Workwear.ReportParameters.ViewModels {
	public class NotIssuedDutyNormsViewModel : ReportParametersUowViewModelBase {
		private readonly FeaturesService featuresService;

		public NotIssuedDutyNormsViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			FeaturesService featuresService,
			ProtectionToolsRepository protectionToolsRepository,
			UnitOfWorkProvider unitOfWorkProvider = null
			) : base(rdlViewerViewModel, unitOfWorkFactory, unitOfWorkProvider)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			if(protectionToolsRepository == null) throw new ArgumentNullException(nameof(protectionToolsRepository));
			Title = "Справка по невыданному по дежурным нормам";

			var protectionToolsList = protectionToolsRepository.GetActiveProtectionTools(UoW);
			ChoiceProtectionToolsViewModel = new ChoiceListViewModel<ProtectionTools>(protectionToolsList);
			ChoiceProtectionToolsViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;

			var subdivisionsList = UoW.GetAll<Subdivision>().ToList();
			ChoiceSubdivisionViewModel = new ChoiceListViewModel<Subdivision>(subdivisionsList);
			ChoiceSubdivisionViewModel.ShowNullValue(true, "Без подразделения");
			ChoiceSubdivisionViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;

			warehousesList.Add(new Warehouse(){Id = -2, Name = "Не показывать"}); //По умолчанию
			var whs = UoW.GetAll<Warehouse>().ToList();
			if(!featuresService.Available(WorkwearFeature.Warehouses) || whs.Count == 1)
				warehousesList.Add(new Warehouse(){Id = -1, Name = "Показать"});
			else {
				warehousesList.AddRange(whs);
				warehousesList.Add(new Warehouse(){Id = -1, Name = "На всех складах"});
			}
			warehouse = warehousesList.First();
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"report_date", ReportDate },
			{"withoutsub", ChoiceSubdivisionViewModel.NullIsSelected },
			{"subdivisions", ChoiceSubdivisionViewModel.SelectedIdsMod },
			{"protection_tools_ids", ChoiceProtectionToolsViewModel.SelectedIdsMod },
			{"exclude_before", ExcludeBefore },
			{"show_stock", Warehouse.Id != -2 },
			{"all_warehouse", Warehouse.Id == -1 },
			{"warehouse_id", Warehouse.Id },
			{"exclude_zero_stock", ExcludeZeroStock },
			{"hide_worn", HideWorn },
			{"show_shipment", ShowShipment },
			{"printPromo", featuresService.Available(WorkwearFeature.PrintPromo) },
		};

		public override string Identifier {
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}

		#region Параметры
		private NotIssuedDutyNormsReportType reportType;
		[PropertyChangedAlso(nameof(VisibleShowShipment))]
		public virtual NotIssuedDutyNormsReportType ReportType {
			get => reportType;
			set => SetField(ref reportType, value);
		}

		private DateTime? reportDate = DateTime.Today;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? ReportDate {
			get => reportDate;
			set => SetField(ref reportDate, value);
		}

		private DateTime? excludeBefore;
		public virtual DateTime? ExcludeBefore {
			get => excludeBefore;
			set => SetField(ref excludeBefore, value);
		}

		private Warehouse warehouse;
		[PropertyChangedAlso(nameof(StockElementsSensetive))]
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}

		private List<Warehouse> warehousesList = new List<Warehouse>();
		public virtual List<Warehouse> WarehousesList {
			get => warehousesList;
			set => SetField(ref warehousesList, value);
		}

		private bool excludeZeroStock;
		public virtual bool ExcludeZeroStock {
			get => warehouse.Id != -2 && excludeZeroStock;
			set => SetField(ref excludeZeroStock, value);
		}

		private bool hideWorn;
		public virtual bool HideWorn {
			get => warehouse.Id != -2 && hideWorn;
			set => SetField(ref hideWorn, value);
		}

		private bool showShipment;
		public virtual bool ShowShipment {
			get => warehouse.Id != -2 && showShipment;
			set => SetField(ref showShipment, value);
		}
		#endregion

		#region Свойства
		public bool SensetiveLoad => ReportDate != null
		                             && !ChoiceProtectionToolsViewModel.AllUnSelected
		                             && !ChoiceSubdivisionViewModel.AllUnSelected;
		public bool StockElementsSensetive => warehouse.Id != -2;
		public bool VisibleShowShipment => featuresService.Available(WorkwearFeature.Shipment) && ReportType == NotIssuedDutyNormsReportType.Flat;

		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(ChoiceProtectionToolsViewModel.AllUnSelected))
				OnPropertyChanged(nameof(SensetiveLoad));
		}
		#endregion

		#region ViewModels
		public readonly ChoiceListViewModel<ProtectionTools> ChoiceProtectionToolsViewModel;
		public readonly ChoiceListViewModel<Subdivision> ChoiceSubdivisionViewModel;
		#endregion
	}

	public enum NotIssuedDutyNormsReportType {
		[ReportIdentifier("DutyNorms.NotIssuedDutyNorms")]
		[Display(Name = "Форматировано")]
		Common,
		[ReportIdentifier("DutyNorms.NotIssuedDutyNormsFlat")]
		[Display(Name = "Только данные")]
		Flat
	}
}
