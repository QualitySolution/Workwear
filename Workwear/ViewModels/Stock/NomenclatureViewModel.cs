using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;

namespace workwear.ViewModels.Stock
{
	public class NomenclatureViewModel : EntityDialogViewModelBase<Nomenclature>
	{
		public NomenclatureViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			//FIXME Заглушка пока не используется.
		}
	}
}
