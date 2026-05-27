using QS.Report.ViewModels;
using System.Collections.Generic;
using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.ViewModels.Control;
using Workwear.Domain.Stock;
using Workwear.Tools;

namespace Workwear.ReportParameters.ViewModels {
	public class RentActViewModel : ReportParametersViewModelBase {
		public RentActViewModel(RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory)
			: base(rdlViewerViewModel) {
			var UoW = uowFactory.CreateWithoutRoot();
			Title = "Акт об оказанных услугах";
			Identifier = "Rent.RentAct";

			var nomenclatureToolsList = UoW.GetAll<Nomenclature>().ToList();
			ChoiceNomenclatureViewModel = new ChoiceListViewModel<Nomenclature>(nomenclatureToolsList);
			ChoiceNomenclatureViewModel.PropertyChanged += (s, e) => OnPropertyChanged(nameof(SensitiveLoad));
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>() {
			{ "start_date", StartDate },
			{ "end_date", EndDate }
		};
		
		private DateTime? startDate = DateTime.Today.AddMonths(-1);
		public virtual DateTime? StartDate {
			get => startDate;
			set {
				if(SetField(ref startDate, value))
					OnPropertyChanged(nameof(SensitiveLoad));
			}
		}
		private DateTime? endDate = DateTime.Today;
		public virtual DateTime? EndDate {
			get => endDate;
			set {
				if(SetField(ref endDate, value))
					OnPropertyChanged(nameof(SensitiveLoad));
			}
		}

		public bool SensitiveLoad => StartDate != null && EndDate != null && !ChoiceNomenclatureViewModel.AllUnSelected;

		public ChoiceListViewModel<Nomenclature> ChoiceNomenclatureViewModel;
	}
}

