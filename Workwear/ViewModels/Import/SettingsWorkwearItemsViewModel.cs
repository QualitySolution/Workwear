using QS.ViewModels;
using workwear.Models.Import;

namespace workwear.ViewModels.Import
{
	public class SettingsWorkwearItemsViewModel : ViewModelBase, IMatchEmployeesSettings
	{
		public SettingsWorkwearItemsViewModel()
		{
		}

		private bool convertPersonnelNumber;
		public virtual bool ConvertPersonnelNumber {
			get => convertPersonnelNumber;
			set => SetField(ref convertPersonnelNumber, value);
		}

	}
}
