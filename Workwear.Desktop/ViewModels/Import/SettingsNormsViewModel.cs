using QS.BaseParameters;
using QS.ViewModels;

namespace Workwear.ViewModels.Import
{
	public class SettingsNormsViewModel : ViewModelBase, IImportNormSettings
	{
		private readonly ParametersService parameters;
		/// <param name="parameters"> не является обязательным специально для тестов, можно передать null.</param>
		public SettingsNormsViewModel(ParametersService parameters)
		{
			this.parameters = parameters;
			listSeparator = parameters?.Dynamic.Import_ListSeparator(typeof(string)) ?? "/";
			wearoutToName = parameters?.Dynamic.Import_WearoutToName(typeof(bool)) ?? false;
		}

		private bool wearoutToName;
		public virtual bool WearoutToName {
			get => wearoutToName;
			set {
				if(SetField(ref wearoutToName, value) && parameters != null)
					parameters.Dynamic.Import_WearoutToName = value;
			}
		}

		#region Списки
		private string listSeparator;
		public virtual string ListSeparator {
			get => listSeparator;
			set {
				if(SetField(ref listSeparator, value) && parameters != null)
					parameters.Dynamic.Import_ListSeparator = value;
			}
		}
		#endregion
		
		#region Иерархия подразделений подразделений
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
