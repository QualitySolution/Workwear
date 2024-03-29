﻿using QS.BaseParameters;
using QS.ViewModels;
using Workwear.Models.Import;

namespace Workwear.ViewModels.Import
{
	public class SettingsMatchEmployeesViewModel : ViewModelBase, IMatchEmployeesSettings
	{
		private readonly ParametersService parameters;
		/// <param name="parameters"> не является обязательным специально для тестов, можно передать null.</param>
		public SettingsMatchEmployeesViewModel(ParametersService parameters)
		{
			this.parameters = parameters;
			subdivisionLevelEnable = parameters?.Dynamic.Import_SubdivisionLevelEnable(typeof(bool)) ?? false;
			subdivisionLevelSeparator = parameters?.Dynamic.Import_SubdivisionLevelSeparator(typeof(string)) ?? "/";
			subdivisionLevelReverse = parameters?.Dynamic.Import_SubdivisionLevelReverse(typeof(bool)) ?? false;
		}

		private bool convertPersonnelNumber;
		public virtual bool ConvertPersonnelNumber {
			get => convertPersonnelNumber;
			set => SetField(ref convertPersonnelNumber, value);
		}
		
		private bool dontCreateNewEmployees;
		public virtual bool DontCreateNewEmployees {
			get => dontCreateNewEmployees;
			set => SetField(ref dontCreateNewEmployees, value);
		}

		#region Уровни подразделение
		private bool subdivisionLevelEnable;
		public virtual bool SubdivisionLevelEnable {
			get => subdivisionLevelEnable;
			set {
				if(SetField(ref subdivisionLevelEnable, value) && parameters != null)
					parameters.Dynamic.Import_SubdivisionLevelEnable = value;
			}
		}

		private string subdivisionLevelSeparator;
		public virtual string SubdivisionLevelSeparator {
			get => subdivisionLevelSeparator;
			set {
				if(SetField(ref subdivisionLevelSeparator, value) && parameters != null)
					parameters.Dynamic.Import_SubdivisionLevelSeparator = value;
			}
		}

		private bool subdivisionLevelReverse;
		public virtual bool SubdivisionLevelReverse {
			get => subdivisionLevelReverse;
			set {
				if(SetField(ref subdivisionLevelReverse, value) && parameters != null)
					parameters.Dynamic.Import_SubdivisionLevelReverse = value;
			}
		}
		#endregion
	}
}
