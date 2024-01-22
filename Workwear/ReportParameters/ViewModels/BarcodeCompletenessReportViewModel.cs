using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;

namespace Workwear.ReportParameters.ViewModels {
	public class BarcodeCompletenessReportViewModel : ReportParametersViewModelBase {
		
		public BarcodeCompletenessReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory)
			: base(rdlViewerViewModel) {
			UoW = uowFactory.CreateWithoutRoot();
			
			ChoiceProtectionToolsViewModel = new ChoiceProtectionToolsViewModel(UoW);
			ChoiceProtectionToolsViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
			ChoiceProtectionToolsViewModel.UnSelectAll();
			
			ChoiceSubdivisionViewModel = new ChoiceSubdivisionViewModel(UoW);
			ChoiceSubdivisionViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
		}

		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			//Двойная проверка страхует от несинхронных изменений незваний полей в разных классах.
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
			{"subdivision_ids", ChoiceSubdivisionViewModel.SelectedChoiceSubdivisionIds.Length == 0 ? 
				new [] {-1} : 
				ChoiceSubdivisionViewModel.SelectedChoiceSubdivisionIds},
			{"without_subdivision", ChoiceSubdivisionViewModel.NullIsSelected },
			{"protection_tools_ids", ChoiceProtectionToolsViewModel.SelectedProtectionToolsIds.Length == 0 ? 
				new [] {-1} :
				ChoiceProtectionToolsViewModel.SelectedProtectionToolsIds },
			{"show_employees", ShowEmployees },
			{"barcode_lag", BarcodeLag },
		};

		#region Параметры
		IUnitOfWork UoW;
		public override string Title => $"Отчёт по маркированной спецодежде от {reportDate?.ToString("dd MMMM yyyy") ?? "(выберите дату)"}";
		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}

		public bool VisibleShowEmployee => false; // ReportType == BarcodeCompletenessType.Flat;
		public bool VisibleShowSize => false; // ReportType == BarcodeCompletenessType.Flat;
		public bool VisibleShowSex => false; // ReportType == BarcodeCompletenessType.Flat;
		public bool VisibleGroupBySubdivision => false; // ReportType == BarcodeCompletenessType.Flat;
		public bool VisibleReportType => false; // Удалить из Вью
		
		public bool SensetiveLoad => ReportDate != null && !ChoiceProtectionToolsViewModel.AllUnSelected 
		                                                && !ChoiceSubdivisionViewModel.AllUnSelected;

		public ChoiceSubdivisionViewModel ChoiceSubdivisionViewModel;
		public ChoiceProtectionToolsViewModel ChoiceProtectionToolsViewModel;
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
			//[ReportIdentifier("BarcodeCompletenessReportFlat")]
			//[Display(Name = "Только данные")]
			//Flat
		}
	}
}
