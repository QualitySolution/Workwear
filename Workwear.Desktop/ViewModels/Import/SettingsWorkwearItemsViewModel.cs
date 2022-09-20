using QS.ViewModels;
using Workwear.Models.Import;

namespace Workwear.ViewModels.Import
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
