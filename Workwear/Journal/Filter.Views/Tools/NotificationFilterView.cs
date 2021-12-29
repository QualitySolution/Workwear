using QS.Views;
using workwear.Journal.Filter.ViewModels.Tools;

namespace workwear.Journal.Filter.Views.Tools
{
	public partial class NotificationFilterView : ViewBase<NotificationFilterViewModel>
	{
		public NotificationFilterView(NotificationFilterViewModel viewModel): base(viewModel)
		{
			this.Build();
			ycheckShowOnlyOverdue.Binding.AddBinding(ViewModel, vm => vm.ShowOnlyOverdue, w => w.Active).InitializeFromSource();
			entitySubdivision.ViewModel = viewModel.SubdivisionEntry;
		}
	}
}
