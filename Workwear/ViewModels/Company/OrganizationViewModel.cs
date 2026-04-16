using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Tools;

namespace Workwear.ViewModels.Company
{
	public class OrganizationViewModel : EntityDialogViewModelBase<Organization>, IDialogDocumentation
	{
		public OrganizationViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IValidator validator) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("organization.html#organizations");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
	}
}
