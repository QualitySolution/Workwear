using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;

namespace Workwear.ReportParameters.ViewModels {
	public class ProvisionReportViewModel : ReportParametersViewModelBase {
		
		public ProvisionReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory)
			: base(rdlViewerViewModel) {
			UoW = uowFactory.CreateWithoutRoot();
			
			ChoiceProtectionToolsViewModel = new ChoiceProtectionToolsViewModel(UoW);
			ChoiceProtectionToolsViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
			
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
			{"show_stock", ShowStock },
		};

		#region Параметры
		IUnitOfWork UoW;
		public override string Title => $"Отчёт по обеспечености сотрудников на {reportDate?.ToString("dd MMMM yyyy") ?? "(выберите дату)"}";
		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}

		public bool VisibleShowStock => ReportType == ProvisionReportType.Flat;
		public bool VisibleShowEmployee => ReportType == ProvisionReportType.Flat;
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
		private ProvisionReportType reportType;
		public virtual ProvisionReportType ReportType {
			get => reportType;
			set {
				SetField(ref reportType, value); 
				OnPropertyChanged(nameof(VisibleShowStock));
				OnPropertyChanged(nameof(VisibleShowEmployee));
			}
		}

		private bool showStock;
		public virtual bool ShowStock {
			get => showStock;
			set => SetField(ref showStock, value);
		}
		private bool showEmployees;
		public virtual bool ShowEmployees {
			get => showEmployees;
			set => SetField(ref showEmployees, value);
		}
		#endregion
		
		public enum ProvisionReportType {
			[ReportIdentifier("ProvisionReport")]
			[Display(Name = "Форматировано")]
			Common,
			[ReportIdentifier("ProvisionReportFlat")]
			[Display(Name = "Только данные")]
			Flat
		}
	}
}
