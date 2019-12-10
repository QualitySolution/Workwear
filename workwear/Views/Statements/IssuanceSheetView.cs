using QS.Views.GtkUI;
using workwear.Domain.Statements;
using workwear.ViewModels.Statements;

namespace workwear.Views.Statements
{
	public partial class IssuanceSheetView : EntityTabViewBase<IssuanceSheetViewModel, IssuanceSheet>
	{
		public IssuanceSheetView(IssuanceSheetViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			dateOfPreparation.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			entityentryOrganization.ViewModel = ViewModel.OrganizationEntryViewModel;
			entityentrySubdivision.ViewModel = ViewModel.SubdivisionEntryViewModel;
			entityentryResponsiblePerson.ViewModel = ViewModel.ResponsiblePersonEntryViewModel;
			entityentryHeadOfDivisionPerson.ViewModel = ViewModel.HeadOfDivisionPersonEntryViewModel;
		}
	}
}
