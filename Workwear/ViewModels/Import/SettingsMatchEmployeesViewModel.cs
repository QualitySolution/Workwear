using QS.BaseParameters;
using QS.ViewModels;
using workwear.Models.Import;

namespace workwear.ViewModels.Import
{
	public class SettingsMatchEmployeesViewModel : ViewModelBase, IMatchEmployeesSettings
	{
		private readonly ParametersService parameters;

		public SettingsMatchEmployeesViewModel(ParametersService parameters)
		{
			this.parameters = parameters;
			subdivisionLevelEnable = parameters.Dynamic.Import_SubdivisionLevelEnable(typeof(bool)) ?? false;
			subdivisionLevelSeparator = parameters.Dynamic.Import_SubdivisionLevelSeparator(typeof(string)) ?? "/";
			subdivisionLevelReverse = parameters.Dynamic.Import_SubdivisionLevelReverse(typeof(bool)) ?? false;
		}

		private bool convertPersonnelNumber;
		public virtual bool ConvertPersonnelNumber {
			get => convertPersonnelNumber;
			set => SetField(ref convertPersonnelNumber, value);
		}

		#region Уровни подразделение
		private bool subdivisionLevelEnable;
		public virtual bool SubdivisionLevelEnable {
			get => subdivisionLevelEnable;
			set {
				if(SetField(ref subdivisionLevelEnable, value))
					parameters.Dynamic.Import_SubdivisionLevelEnable = value;
			}
		}

		private string subdivisionLevelSeparator;
		public virtual string SubdivisionLevelSeparator {
			get => subdivisionLevelSeparator;
			set {
				if(SetField(ref subdivisionLevelSeparator, value))
					parameters.Dynamic.Import_SubdivisionLevelSeparator = value;
			}
		}

		private bool subdivisionLevelReverse;
		public virtual bool SubdivisionLevelReverse {
			get => subdivisionLevelReverse;
			set {
				if(SetField(ref subdivisionLevelReverse, value))
					parameters.Dynamic.Import_SubdivisionLevelReverse = value;
			}
		}
		#endregion
	}
}
