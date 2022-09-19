using QS.Views.Dialog;
using Workwear.Tools;
using Workwear.ViewModels.Tools;

namespace Workwear.Views.Tools
{
	public partial class DataBaseSettingsView : SavedDialogViewBase<DataBaseSettingsViewModel>
	{
		public DataBaseSettingsView(DataBaseSettingsViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ycheckAutoWriteoff.Binding.AddBinding(ViewModel, v => v.DefaultAutoWriteoff, w => w.Active).InitializeFromSource();
			checkCheckBalances.Binding.AddBinding(ViewModel, v => v.CheckBalances, w => w.Active).InitializeFromSource();
			spbutAheadOfShedule.Binding.AddBinding(ViewModel, v => v.ColDayAheadOfShedule, w => w.ValueAsInt).InitializeFromSource();
			ComboShirtExpluatacion.ItemsEnum = typeof(AnswerOptions);
			ComboShirtExpluatacion.Binding.AddBinding(ViewModel , v=> v.ShiftExpluatacion, w=>w.SelectedItem).InitializeFromSource();
			ComboExtendPeriod.ItemsEnum = typeof(AnswerOptions);
			ComboExtendPeriod.Binding.AddBinding(ViewModel, v => v.ExtendPeriod, w => w.SelectedItem).InitializeFromSource();
			CommonButtonSubscription();
		}
	}
}
