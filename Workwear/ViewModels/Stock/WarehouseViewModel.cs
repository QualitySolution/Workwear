using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Stock;
using Workwear.Tools;

namespace Workwear.ViewModels.Stock
{
	public class WarehouseViewModel : EntityDialogViewModelBase<Warehouse>, IDialogDocumentation
	{
		public WarehouseViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigationManager) : base(uowBuilder, unitOfWorkFactory, navigationManager)
		{

		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#warehouses");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
	}
}
