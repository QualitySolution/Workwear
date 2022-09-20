using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock;

namespace Workwear.ViewModels.Stock
{
	public class WarehouseViewModel : EntityDialogViewModelBase<Warehouse>
	{
		public WarehouseViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigationManager) : base(uowBuilder, unitOfWorkFactory, navigationManager)
		{

		}
	}
}
