using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gamma.Widgets;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using QS.ViewModels.Control;
using Workwear.Domain.Company;
using Workwear.Tools.Features;

namespace Workwear.ReportParameters.ViewModels {
	public class VacationsReportViewModel : ReportParametersUowViewModelBase {
		private readonly FeaturesService featuresService;

		public ChoiceListViewModel<Subdivision> ChoiceSubdivisionViewModel;

		public VacationsReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory,
			FeaturesService featuresService) : base(rdlViewerViewModel, uowFactory) {
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			Title = "Отчёт по отпускам";
			Identifier = "VacationsReport";

			VacationTypes = UoW.GetAll<VacationType>().ToList();

			var subdivisionsList = UoW.GetAll<Subdivision>().ToList();
			ChoiceSubdivisionViewModel = new ChoiceListViewModel<Subdivision>(subdivisionsList);
			ChoiceSubdivisionViewModel.ShowNullValue(true, "Без подразделения");
			ChoiceSubdivisionViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"start_date", StartDate},
			{"end_date", EndDate},
			{"vacation_type_id", (SelectVacationType as VacationType)?.Id ?? -1},
			{"subdivision_ids", ChoiceSubdivisionViewModel.SelectedIdsMod},
			{"without_subdivision", ChoiceSubdivisionViewModel.NullIsSelected},
			{"printPromo", featuresService.Available(WorkwearFeature.PrintPromo)},
		};

		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(ChoiceSubdivisionViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(SensitiveLoad));
		}

		#region Свойства
		private DateTime? startDate = new DateTime(DateTime.Today.Year, 1, 1);
		[PropertyChangedAlso(nameof(SensitiveLoad))]
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}

		private DateTime? endDate = new DateTime(DateTime.Today.Year, 12, 31);
		[PropertyChangedAlso(nameof(SensitiveLoad))]
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}

		public IList<VacationType> VacationTypes { get; }

		private object selectVacationType = SpecialComboState.All;
		public object SelectVacationType {
			get => selectVacationType;
			set => SetField(ref selectVacationType, value);
		}

		public bool SensitiveLoad => StartDate != null && EndDate != null && StartDate <= EndDate
		                             && !ChoiceSubdivisionViewModel.AllUnSelected;
		#endregion
	}
}
