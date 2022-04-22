using QS.ViewModels;

namespace workwear.ViewModels.Import
{
	public class SettingsMatchEmployeesViewModel : ViewModelBase
	{
		public SettingsMatchEmployeesViewModel()
		{
		}

		private bool convertPersonnelNumber;
		public virtual bool ConvertPersonnelNumber {
			get => convertPersonnelNumber;
			set => SetField(ref convertPersonnelNumber, value);
		}

	}
}
