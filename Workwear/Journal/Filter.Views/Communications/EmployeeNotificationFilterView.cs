using QS.Views;
using workwear.Journal.Filter.ViewModels.Communications;

namespace workwear.Journal.Filter.Views.Communications
{
	public partial class EmployeeNotificationFilterView : ViewBase<EmployeeNotificationFilterViewModel>
	{
		public EmployeeNotificationFilterView(EmployeeNotificationFilterViewModel viewModel): base(viewModel)
		{
			this.Build();
			checkShowOnlyWork.Binding.AddBinding(ViewModel, vm => vm.ShowOnlyWork, w => w.Active).InitializeFromSource();
			checkLkEnabled.Binding.AddBinding(ViewModel, vm => vm.ShowOnlyLk, w => w.Active).InitializeFromSource();
			yIssueType.ItemsEnum = typeof(AskIssueType);
			yIssueType.Binding.AddSource(viewModel)
				.AddBinding(v => v.IsueType, w => w.SelectedItem)
				.InitializeFromSource();
			ycheckShowOverdue.Binding.AddSource(viewModel)
				.AddBinding( vm => vm.ShowOverdue, w => w.Active)
				.AddBinding(vm => vm.PeriodSensitive, v => v.Sensitive)
				.InitializeFromSource();
			entitySubdivision.ViewModel = viewModel.SubdivisionEntry;
			datePeriodIssue.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartDateIssue, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDateIssue, w => w.EndDateOrNull)
				.AddBinding(vm => vm.PeriodSensitive, v => v.Sensitive)
				.InitializeFromSource();
			ycheckOffPeriod.Binding.AddBinding(ViewModel, vm => vm.ContainsPeriod, v => v.Active).InitializeFromSource();
		}
	}
}
