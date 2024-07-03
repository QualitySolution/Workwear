using QS.Views.Dialog;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Tools;

namespace Workwear.Views.Tools
{
	public partial class DataBaseSettingsView : SavedDialogViewBase<DataBaseSettingsViewModel>
	{
		public DataBaseSettingsView(DataBaseSettingsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ylabelCollectiveIssueWithPersonal.Visible = ycheckCollectiveIssueWithPersonal.Visible = viewModel.CollectiveIssueWithPersonalVisible;
			ycheckAutoWriteoff.Binding.AddBinding(ViewModel, v => v.DefaultAutoWriteoff, w => w.Active).InitializeFromSource();
			checkCheckBalances.Binding.AddBinding(ViewModel, v => v.CheckBalances, w => w.Active).InitializeFromSource();
			spbutAheadOfShedule.Binding.AddBinding(ViewModel, v => v.ColDayAheadOfShedule, w => w.ValueAsInt).InitializeFromSource();
			ycheckCollectiveIssueWithPersonal.Binding.AddBinding(ViewModel, v => v.CollectiveIssueWithPersonal, w => w.Active).InitializeFromSource();
			ycheckCollapseDuplicateIssuanceSheet.Binding.AddBinding( ViewModel, v => v.CollapseDuplicateIssuanceSheet, w => w.Active).InitializeFromSource();
			ComboShirtExpluatacion.ItemsEnum = typeof(AnswerOptions);
			ComboShirtExpluatacion.Binding.AddBinding(ViewModel , v=> v.ShiftExpluatacion, w=>w.SelectedItem).InitializeFromSource();
			ComboExtendPeriod.ItemsEnum = typeof(AnswerOptions);
			ComboExtendPeriod.Binding.AddBinding(ViewModel, v => v.ExtendPeriod, w => w.SelectedItem).InitializeFromSource();
			yentryCurrency.Binding.AddBinding(ViewModel, v => v.UsedCurrency, w => w.Text).InitializeFromSource();
			CommonButtonSubscription();
		}
	}
}
