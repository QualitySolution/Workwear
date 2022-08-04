using QS.ViewModels;
using workwear.Models.Import;

namespace workwear.ViewModels.Import
{
	public class SettingsMatchEmployeesViewModel : ViewModelBase, IMatchEmployeesSettings
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
