using QS.Views;
using Workwear.Domain.Supply;
using Workwear.Journal.Filter.ViewModels.Supply;

namespace Workwear.Journal.Filter.Views.Supply {
	public partial class ShipmentJournalFilterView : ViewBase<ShipmentJournalFilterViewModel> {
		public ShipmentJournalFilterView(ShipmentJournalFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			
			enumcomboStatus.ItemsEnum = typeof(ShipmentStatus); 
			enumcomboStatus.Binding
				.AddBinding(ViewModel, v => v.Status, w => w.SelectedItemOrNull).InitializeFromSource();
			ycheckbuttonNotFullOrdered.Binding
				.AddBinding(ViewModel, vm => vm.NotFullOrdered, w => w.Active).InitializeFromSource();
		}
	}
}
