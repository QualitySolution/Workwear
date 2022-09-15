using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using workwear.Journal.ViewModels.Company;

namespace workwear.ViewModels.Company
{
	public class DepartmentViewModel : EntityDialogViewModelBase<Department>
	{
		private readonly ILifetimeScope autofacScope;

		public DepartmentViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, ILifetimeScope autofacScope, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var builder = new CommonEEVMBuilderFactory<Department>(this, Entity, UoW, NavigationManager, autofacScope);

			EntrySubdivision = builder.ForProperty(x => x.Subdivision)
									.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
									.UseViewModelDialog<SubdivisionViewModel>()
									.Finish();
		}

		#region Controls

		public EntityEntryViewModel<Subdivision> EntrySubdivision;

		#endregion
	}
}
