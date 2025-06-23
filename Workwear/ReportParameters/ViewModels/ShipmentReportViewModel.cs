using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using QS.ViewModels.Control;
using QS.ViewModels.Extension;
using Workwear.Domain.Regulations;
using Workwear.Domain.Supply;

namespace Workwear.ReportParameters.ViewModels {
	public class ShipmentReportViewModel : ReportParametersViewModelBase {
		IUnitOfWork UoW;
		public ShipmentReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory) : 
			base(rdlViewerViewModel) {

			Title = $"Отчет по планируемым поставкам";
			UoW = uowFactory.CreateWithoutRoot();

			var nomenclatureList = UoW.GetAll<Norm>().ToList();
			ChoiceNormViewModel = new ChoiceListViewModel<Norm>(nomenclatureList);
			ChoiceNormViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;

			var shipmentList = UoW.GetAll<Shipment>().ToList();
			ChoiceShipmentViewModel = new ChoiceListViewModel<Shipment>(shipmentList);
			ChoiceShipmentViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;

		}
		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"shipment_ids", ChoiceShipmentViewModel.SelectedIdsMod},
			{"nomenclature_ids", ChoiceNormViewModel.SelectedIdsMod},
		};

		#region Параметры
		#endregion
		

		#region ViewModels
		public ChoiceListViewModel<Norm> ChoiceNormViewModel;
		public ChoiceListViewModel<Shipment> ChoiceShipmentViewModel;
		#endregion

		#region Свойства
		public bool SensetiveLoad => !ChoiceNormViewModel.AllUnSelected && !ChoiceShipmentViewModel.AllUnSelected;
		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(ChoiceNormViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
			if(nameof(ChoiceShipmentViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
		}
		#endregion
	}
}

