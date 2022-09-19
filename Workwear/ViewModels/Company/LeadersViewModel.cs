using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;

namespace Workwear.ViewModels.Company
{
	public class LeadersViewModel : EntityDialogViewModelBase<Leader>
	{
		private readonly ILifetimeScope autofacScope;

		public LeadersViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, ILifetimeScope autofacScope, IValidator validator) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var builder = new CommonEEVMBuilderFactory<Leader>(this, Entity, UoW, NavigationManager, autofacScope);

			EntryEmployee = builder.ForProperty(x => x.Employee).MakeByType().Finish();
		}

		#region Controls
		public EntityEntryViewModel<EmployeeCard> EntryEmployee;
		#endregion
	}
}
