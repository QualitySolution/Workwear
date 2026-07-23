using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using QS.ViewModels.Control;
using Workwear.Domain.ClothingService;

namespace Workwear.ReportParameters.ViewModels {
	public class ClothingServicesCodeViewModel : ReportParametersViewModelBase {
		public ClothingServicesCodeViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory)
			: base(rdlViewerViewModel)
		{ 
			var UoW = uowFactory.CreateWithoutRoot();
			
			var serviceList = UoW.GetAll<Service>().ToList();
			ChoiceServiceViewModel = new ChoiceListViewModel<Service>(serviceList);
		}

		public ChoiceListViewModel<Service> ChoiceServiceViewModel;

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"ids", ChoiceServiceViewModel.SelectedIds },
		};

		public override string Title => "Коды услуг обслуживания";
		public override string Identifier => "ClothingService.ClothingServiceCodes";
	}
}
