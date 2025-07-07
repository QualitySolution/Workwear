using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Stock;
using Workwear.Tools;

namespace Workwear.ViewModels.Stock 
{
	public class OwnerViewModel : EntityDialogViewModelBase<Owner>, IDialogDocumentation
	{
		public OwnerViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#owners");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
	}
}
