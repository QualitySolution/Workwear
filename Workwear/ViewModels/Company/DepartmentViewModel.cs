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
using workwear.Journal.ViewModels.Company;
using Workwear.Tools;

namespace Workwear.ViewModels.Company
{
	public class DepartmentViewModel : EntityDialogViewModelBase<Department>, IDialogDocumentation
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
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("organization.html#departments");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion

		#region Controls

		public EntityEntryViewModel<Subdivision> EntrySubdivision;

		#endregion
	}
}
