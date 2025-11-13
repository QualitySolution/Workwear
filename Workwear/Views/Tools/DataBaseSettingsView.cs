using QS.Views.Dialog;
using Workwear.Domain.Stock;
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
				.AddBinding(v => v.CanEdit, w => w.IsEditable)
				.AddBinding(v => v.EditLockDateVisible, w => w.Visible)
				.InitializeFromSource();
			ylabelCollectiveIssueWithPersonal.Visible = ycheckCollectiveIssueWithPersonal.Visible = viewModel.CollectiveIssueWithPersonalVisible;
			ycheckAutoWriteoff.Binding.AddSource(ViewModel)
				.AddBinding(v => v.DefaultAutoWriteoff, w => w.Active)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .InitializeFromSource();
		   
			checkCheckBalances.Binding.AddSource(ViewModel)
			    .AddBinding(v => v.CheckBalances, w => w.Active)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .InitializeFromSource();
		   
		    spbutAheadOfShedule.Binding.AddSource(ViewModel)
			    .AddBinding(v => v.ColDayAheadOfShedule, w => w.ValueAsInt)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .InitializeFromSource();
		   
		    ycheckCollectiveIssueWithPersonal.Binding.AddSource(ViewModel)
			    .AddBinding(v => v.CollectiveIssueWithPersonal, w => w.Active)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .InitializeFromSource();
		   
		    ycheckCollapseDuplicateIssuanceSheet.Binding.AddSource(ViewModel)
			    .AddBinding(v => v.CollapseDuplicateIssuanceSheet, w => w.Active)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .InitializeFromSource();
		   
		    ComboShirtExpluatacion.ItemsEnum = typeof(AnswerOptions);
		    ComboShirtExpluatacion.Binding.AddSource(ViewModel)
			    .AddBinding(v => v.ShiftExpluatacion, w => w.SelectedItem)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .InitializeFromSource();
		   
		    ComboExtendPeriod.ItemsEnum = typeof(AnswerOptions);
		    ComboExtendPeriod.Binding.AddSource(ViewModel)
			    .AddBinding(v => v.ExtendPeriod, w => w.SelectedItem)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .InitializeFromSource();

		    Combo_markingType.Visible = ylabel_markingType.Visible = ViewModel.MarkingVisible;
		    Combo_markingType.ItemsEnum = typeof(BarcodeTypes);
		    Combo_markingType.Binding.AddSource(ViewModel)
			    .AddBinding(v => v.ClothingMarkingType, w => w.SelectedItem)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .InitializeFromSource();
		    
		    yentryCurrency.Binding.AddSource(ViewModel)
			    .AddBinding(v => v.UsedCurrency, w => w.Text)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .InitializeFromSource();
		   
		    ycheckbuttonIssue.Binding.AddSource(ViewModel)
			    .AddBinding(v => v.IsDocNumberInIssueSign, w => w.Active)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .InitializeFromSource();
		   
		    ycheckbuttonReturn.Binding.AddSource(ViewModel)
			    .AddBinding(v => v.IsDocNumberInReturnSign, w => w.Active)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .InitializeFromSource();
		    
		    ylabelstartDateOfOperations.Binding.AddBinding(ViewModel, v=>v.StartDateOfOperationsVisible, w=>w.Visible)
			    .InitializeFromSource();

		    startDateOfOperations.Binding.AddSource(ViewModel)
			    .AddBinding(v => v.StartDateOfOperations, w => w.DateOrNull)
			    .AddBinding(v => v.CanEdit, w => w.Sensitive)
			    .AddBinding(v => v.StartDateOfOperationsVisible, w => w.Visible)
			    .InitializeFromSource();
			    
		    
			CommonButtonSubscription();
		}
	}
}
