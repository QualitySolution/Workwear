using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Tools;

namespace Workwear.ViewModels.Company
{
	public class LeadersViewModel : EntityDialogViewModelBase<Leader>, IDialogDocumentation
	{
		private readonly ILifetimeScope autofacScope;

		public LeadersViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, ILifetimeScope autofacScope, IValidator validator) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var builder = new CommonEEVMBuilderFactory<Leader>(this, Entity, UoW, NavigationManager, autofacScope);

			EntryEmployee = builder.ForProperty(x => x.Employee).MakeByType().Finish();
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("organization.html#leaders");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion

		#region Controls
		public EntityEntryViewModel<EmployeeCard> EntryEmployee;
		#endregion
	}
}
