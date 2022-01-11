using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Regulations;

namespace workwear.ViewModels.Regulations
{
	public class NormConditionViewModel : EntityDialogViewModelBase<NormCondition>
	{
		public NormConditionViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
		}
		public string Name { get; set; }

		public SexNormCondition sexNormCondition { get; set; }
	}
}
