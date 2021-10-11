using System;
using QS.ViewModels;

namespace workwear.ViewModels.Import
{
	public class SettingsWorkwearItemsViewModel : ViewModelBase
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
