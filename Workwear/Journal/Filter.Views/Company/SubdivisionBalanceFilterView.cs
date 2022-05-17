using QS.Views;
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
		}
	}
}
