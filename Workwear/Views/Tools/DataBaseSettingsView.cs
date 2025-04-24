using QS.Views.Dialog;
using Workwear.Tools;
using Workwear.ViewModels.Tools;

namespace Workwear.Views.Tools {
	public partial class DataBaseSettingsView : SavedDialogViewBase<DataBaseSettingsViewModel>
	{
		public DataBaseSettingsView(DataBaseSettingsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			labelEditLockDate.Binding.AddBinding(ViewModel, v => v.EditLockDateVisible, w => w.Visible).InitializeFromSource();
			dateEditLock.Binding.AddSource(ViewModel)
				.AddBinding(v => v.EditLockDate, w => w.DateOrNull)
				.AddBinding(v => v.EditLockDateVisible, w => w.Visible)
				.InitializeFromSource();
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
			ycheckbuttonIssue.Binding.AddBinding(ViewModel, v=>v.IsDocNumberInIssueSign,w=>w.Active).InitializeFromSource();
			ycheckbuttonReturn.Binding.AddBinding(ViewModel, v=>v.IsDocNumberInReturnSign, w=>w.Active).InitializeFromSource();
			CommonButtonSubscription();
		}
	}
}
