using QS.Views.Dialog;
using workwear.Tools;
using workwear.ViewModels.Tools;

namespace workwear.Views.Tools
{
	public partial class DataBaseSettingsView : SavedDialogViewBase<DataBaseSettingsViewModel>
	{
		public DataBaseSettingsView(DataBaseSettingsViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ycheckAutoWriteoff.Binding.AddBinding(ViewModel, v => v.DefaultAutoWriteoff, w => w.Active).InitializeFromSource();
			checkCheckBalances.Binding.AddBinding(ViewModel, v => v.CheckBalances, w => w.Active).InitializeFromSource();
			checkEmployeeSizeRanges.Binding.AddBinding(ViewModel, v => v.EmployeeSizeRanges, w => w.Active).InitializeFromSource();
			spbutAheadOfShedule.Binding.AddBinding(ViewModel, v => v.ColDayAheadOfShedule, w => w.ValueAsInt).InitializeFromSource();
			ComboShirtExpluatacion.ItemsEnum = typeof(ShiftExpluatacion);
			ComboShirtExpluatacion.Binding.AddBinding(ViewModel , v=> v.ShiftEpluatacion, w=>w.SelectedItem).InitializeFromSource();
			CommonButtonSubscription();
		}
	}
}
