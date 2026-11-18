using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using QS.ViewModels.Control;
using Workwear.Domain.Stock;

namespace Workwear.ReportParameters.ViewModels {
	public class OverNormIssuedReportViewModel : ReportParametersUowViewModelBase {
		public OverNormIssuedReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory) : base(rdlViewerViewModel, uowFactory) {
			Title = "Числится вне нормы";
			Identifier = "OverNormIssuedReport";

			ChoiceNomenclatureViewModel = new ChoiceListViewModel<Nomenclature>(UoW.GetAll<Nomenclature>().ToList());
			ChoiceNomenclatureViewModel.PropertyChanged += (sender, args) => OnPropertyChanged(nameof(SensitiveLoad));
		}

		public ChoiceListViewModel<Nomenclature> ChoiceNomenclatureViewModel { get; }
		public bool SensitiveLoad => ReportDate != null && !ChoiceNomenclatureViewModel.AllUnSelected;

		private DateTime? reportDate = DateTime.Today;
		public DateTime? ReportDate {
			get => reportDate;
			set {
				if(SetField(ref reportDate, value))
					OnPropertyChanged(nameof(SensitiveLoad));
			}
		}

		private bool showComments;
		public bool ShowComments {
			get => showComments;
			set => SetField(ref showComments, value);
		}

		private bool showPhone = true;
		public bool ShowPhone {
			get => showPhone;
			set => SetField(ref showPhone, value);
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"nomenclature_ids", ChoiceNomenclatureViewModel.SelectedIdsMod},
			{"report_date", ReportDate},
			{"show_comments", ShowComments},
			{"show_phone", ShowPhone},
		};
	}
}
