using System;
using QS.ViewModels;
using QS.Views;
using workwear.ViewModels.Import;

namespace workwear.Views.Import
{
	public partial class SettingsMatchEmployeesView : ViewBase<SettingsMatchEmployeesViewModel>
	{
		public SettingsMatchEmployeesView(SettingsMatchEmployeesViewModel viewModel) : base(viewModel)
		{
			this.Build();
			checkConvertPersonnelNumber.Binding
				.AddBinding(ViewModel, v => v.ConvertPersonnelNumber, w => w.Active)
				.InitializeFromSource();
		}
	}
}
