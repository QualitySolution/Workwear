using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Regulations;
using Workwear.Tools;

namespace Workwear.ViewModels.Regulations
{
	public class ProfessionViewModel : EntityDialogViewModelBase<Profession>, IDialogDocumentation
	{

		public ProfessionViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("regulations.html#proffessions");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
	}
}
