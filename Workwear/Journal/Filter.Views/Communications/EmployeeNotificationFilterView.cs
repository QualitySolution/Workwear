using QS.Views;
using Workwear.Domain.Stock;
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
			ycheckOffPeriod.Binding
				.AddBinding(ViewModel, vm => vm.ContainsPeriod, v => v.Active)
				.InitializeFromSource();
			ycheckBirthday.Binding
				.AddBinding(ViewModel, vm => vm.ContainsDateBirthPeriod, w=> w.Active)
				.InitializeFromSource();
			datePeriodBirth.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartDateBirth, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDateBirth, w => w.EndDateOrNull)
				.AddBinding(vm => vm.SensitiveDateBirth, v => v.Sensitive)
				.InitializeFromSource();
			ycheckStockAvailability.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CheckInStockAvailability, w => w.Active)
				.AddBinding(vm => vm.PeriodSensitive, w => w.Sensitive)
				.InitializeFromSource();
			ycomboListWarehouses.SetRenderTextFunc<Warehouse>(w => w.Name);
			ycomboListWarehouses.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CheckInStockAvailability, w => w.Sensitive)
				.AddBinding(vm => vm.Warehouses, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedWarehouse, w => w.SelectedItem)
				.InitializeFromSource();
			choiceProtectionToolsView.ViewModel = ViewModel.ChoiceProtectionToolsViewModel;
			choiceProtectionToolsView.Binding
				.AddBinding(ViewModel, vm => vm.PeriodSensitive, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
