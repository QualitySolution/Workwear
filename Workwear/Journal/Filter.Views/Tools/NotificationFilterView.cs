using QS.Views;
using workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Tools;

namespace workwear.Journal.Filter.Views.Tools
{
	public partial class NotificationFilterView : ViewBase<NotificationFilterViewModel>
	{
		public NotificationFilterView(NotificationFilterViewModel viewModel): base(viewModel)
		{
			this.Build();
			checkShowOnlyWork.Binding.AddBinding(ViewModel, vm => vm.ShowOnlyWork, w => w.Active).InitializeFromSource();
			yIssueType.ItemsEnum = typeof(AskIssueType);
			yIssueType.Binding.AddBinding(ViewModel, v => v.IsueType, w => w.SelectedItem).InitializeFromSource();
			ycheckShowOverdue.Binding.AddBinding(ViewModel, vm => vm.ShowOverdue, w => w.Active).InitializeFromSource();
			entitySubdivision.ViewModel = viewModel.SubdivisionEntry;
			datePeriodIssue.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartDateIssue, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDateIssue, w => w.EndDateOrNull)
				.InitializeFromSource();
		}
	}
}
