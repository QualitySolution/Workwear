using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock.Documents;

namespace Workwear.ViewModels.Stock {
	public class ReturnViewModel : EntityDialogViewModelBase<Return> {
		public ReturnViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IValidator validator = null, UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
		}


		#region Проброс свойств документа
				
		#endregion
	}
}
