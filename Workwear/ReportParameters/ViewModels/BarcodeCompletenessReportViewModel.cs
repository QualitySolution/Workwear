using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.ViewModels.Control;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Tools;

namespace Workwear.ReportParameters.ViewModels {
	public class BarcodeCompletenessReportViewModel : ReportParametersViewModelBase, IDialogDocumentation {
		
		public BarcodeCompletenessReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory)
			: base(rdlViewerViewModel) {
			UoW = uowFactory.CreateWithoutRoot();

			Nomenclature nomenclatureAlias = null;

			var protectionToolsList =  UoW.Session.QueryOver<ProtectionTools>()
				.Left.JoinAlias(x => x.Nomenclatures, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.UseBarcode)
				.List();
			ChoiceProtectionToolsViewModel = new ChoiceListViewModel<ProtectionTools>(protectionToolsList);
			ChoiceProtectionToolsViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
			ChoiceProtectionToolsViewModel.UnSelectAll();
			
			var subdivisionsList = UoW.GetAll<Subdivision>().ToList();
			ChoiceSubdivisionViewModel = new ChoiceListViewModel<Subdivision>(subdivisionsList);
			ChoiceSubdivisionViewModel.ShowNullValue(true, "Без подраздеения");
			ChoiceSubdivisionViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("reports.html#barcode-completeness");
		public string ButtonTooltip => DocHelper.GetReportDocTooltip("Отчёт по покрытию маркировкой");
		#endregion
		
		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			//Двойная проверка страхует от несинхронных изменений названий полей в разных классах.
			if(nameof(ChoiceSubdivisionViewModel.AllUnSelected) == e.PropertyName 
			   || nameof(ChoiceProtectionToolsViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"report_date", ReportDate },
			{"exclude_in_vacation", ExcludeInVacation },
			{"show_sex", ShowSex },
			{"show_size", ShowSize },
			{"group_by_subdivision", GroupBySubdivision },
			{"subdivision_ids", ChoiceSubdivisionViewModel.SelectedIdsMod},
			{"without_subdivision", ChoiceSubdivisionViewModel.NullIsSelected },
			{"protection_tools_ids", ChoiceProtectionToolsViewModel.SelectedIdsMod },
			{"show_employees", ShowEmployees },
			{"barcode_lag", BarcodeLag },
		};

		#region Параметры
		IUnitOfWork UoW;
		public override string Title => $"Отчёт по покрытию маркировкой от {reportDate?.ToString("dd MMMM yyyy") ?? "(выберите дату)"}";
		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}

		public bool VisibleShowEmployee => ReportType == BarcodeCompletenessType.Flat;
		public bool VisibleShowSize => ReportType == BarcodeCompletenessType.Flat;
		public bool VisibleShowSex => ReportType == BarcodeCompletenessType.Flat;
		public bool VisibleGroupBySubdivision => ReportType == BarcodeCompletenessType.Flat;
		
		public bool SensetiveLoad => ReportDate != null && !ChoiceProtectionToolsViewModel.AllUnSelected 
		                                                && !ChoiceSubdivisionViewModel.AllUnSelected;
		public ChoiceListViewModel<Subdivision> ChoiceSubdivisionViewModel;
		public ChoiceListViewModel<ProtectionTools> ChoiceProtectionToolsViewModel;
		#endregion
		
		#region Свойства
		private DateTime? reportDate = DateTime.Today;
		public virtual DateTime? ReportDate {
			get => reportDate;
		}
		
		private bool excludeInVacation;
		public virtual bool ExcludeInVacation {
			get => excludeInVacation;
			set => SetField(ref excludeInVacation, value);
		}

		private bool showSex;
		public virtual bool ShowSex {
			get => showSex;
			set => SetField(ref showSex, value);
		}

		private bool showSize;
		public virtual bool ShowSize {
			get => showSize;
			set => SetField(ref showSize, value);
		}
		
		private bool groupBySubdivision;
		public virtual bool GroupBySubdivision {
			get => groupBySubdivision;
			set => SetField(ref groupBySubdivision, value);
		}
		private BarcodeCompletenessType reportType;
		public virtual BarcodeCompletenessType ReportType {
			get => reportType;
			set {
				SetField(ref reportType, value); 
				OnPropertyChanged(nameof(VisibleShowEmployee));
				OnPropertyChanged(nameof(VisibleShowSize));
				OnPropertyChanged(nameof(VisibleShowSex));
				OnPropertyChanged(nameof(VisibleGroupBySubdivision));
			}
		}

		private int barcodeLag; 
			public virtual int BarcodeLag {
        		get => barcodeLag;
        		set => SetField(ref barcodeLag, value);
        	}	
			
		private bool showEmployees;
		public virtual bool ShowEmployees {
			get => showEmployees;
			set => SetField(ref showEmployees, value);
		}
		#endregion
		
		public enum BarcodeCompletenessType {
			[ReportIdentifier("BarcodeCompletenessReport")]
			[Display(Name = "Форматировано")]
			Common,
			[ReportIdentifier("BarcodeCompletenessReportFlat")]
			[Display(Name = "Только данные")]
			Flat
		}
	}
}
