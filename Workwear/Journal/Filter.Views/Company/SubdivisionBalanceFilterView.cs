using QS.Views;
using Workwear.Domain.Users;
using workwear.Journal.Filter.ViewModels.Company;

namespace workwear.Journal.Filter.Views.Company
{
	public partial class SubdivisionBalanceFilterView : ViewBase<SubdivisionBalanceFilterViewModel>
	{
		public SubdivisionBalanceFilterView(SubdivisionBalanceFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}
		private void ConfigureDlg()
		{
			datepicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Date, w => w.Date)
				.AddBinding(vm => vm.SubdivisionSensitive, w => w.Sensitive)
				.InitializeFromSource();
			
			entitySubdivision.Binding
				.AddBinding(ViewModel, vm => vm.SubdivisionSensitive, w=> w.Sensitive)
				.InitializeFromSource();
			entitySubdivision.ViewModel = ViewModel.SubdivisionEntry;
			yenumcomboboxAmount.ItemsEnum = typeof(AddedAmount);
			yenumcomboboxAmount.Binding.
				AddBinding(ViewModel, v => v.CanChoiseAmount, w => w.Visible)
				.InitializeFromSource();
			yenumcomboboxAmount.Binding
				.AddBinding(ViewModel, v => v.AddAmount, w => w.SelectedItem)
				.InitializeFromSource();
			labelAmount.Binding
				.AddBinding(ViewModel, v => v.CanChoiseAmount, w => w.Visible)
				.InitializeFromSource();
		}
	}
}
