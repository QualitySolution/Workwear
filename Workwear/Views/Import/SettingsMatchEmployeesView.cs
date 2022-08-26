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

			checkSubdivisionLevelEnable.Binding
				.AddBinding(ViewModel, v => v.SubdivisionLevelEnable, w => w.Active)
				.InitializeFromSource();

			checkSubdivisionLevelReverse.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.SubdivisionLevelReverse, w => w.Active)
				.AddBinding(v => v.SubdivisionLevelEnable, w => w.Sensitive)
				.InitializeFromSource();

			entrySubdivisionLevelSeparator.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.SubdivisionLevelSeparator, w => w.Text)
				.AddBinding(v => v.SubdivisionLevelEnable, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
