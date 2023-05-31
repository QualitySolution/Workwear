using QS.BaseParameters;
using QS.ViewModels;
using Workwear.Models.Import;

namespace Workwear.ViewModels.Import
{
	public class SettingsNormsViewModel : ViewModelBase, IImportNormSettings
	{
		private readonly ParametersService parameters;
		/// <param name="parameters"> не является обязательным специально для тестов, можно передать null.</param>
		public SettingsNormsViewModel(ParametersService parameters)
		{
			this.parameters = parameters;
			listSeparator = parameters?.Dynamic.Import_ListSeparator(typeof(string)) ?? ",;\\/";
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
	}
}
