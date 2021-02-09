using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Domain.Users;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.Tools.Features;
using workwear.ViewModels.Company;
using workwear.ViewModels.Stock;

namespace workwear.ViewModels.User
{
	public class UserSettingsViewModel : EntityDialogViewModelBase<UserSettings>
	{
		public EntityEntryViewModel<Warehouse> WarehouseFromEntryViewModel;
		public EntityEntryViewModel<Leader> LeaderFromEntryViewModel;
		public EntityEntryViewModel<Organization> OrganizationFromEntryViewModel;
		public EntityEntryViewModel<Leader> ResponsiblePersonFromEntryViewModel;

		public ILifetimeScope AutofacScope;
		private readonly FeaturesService featuresService;

		public UserSettingsViewModel
		(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory,
			 INavigationManager navigation, ILifetimeScope autofacScope, FeaturesService featuresService, IValidator validator = null) 
		: base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			var entryBuilder = new CommonEEVMBuilderFactory<UserSettings>(this, Entity, UoW, navigation, autofacScope);

			WarehouseFromEntryViewModel = entryBuilder.ForProperty(x => x.DefaultWarehouse)
							 .UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
							 .UseViewModelDialog<WarehouseViewModel>()
							 .Finish();

			LeaderFromEntryViewModel = entryBuilder.ForProperty(x => x.DefaultLeader)
				 .UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
				 .UseViewModelDialog<LeadersViewModel>()
				 .Finish();

			ResponsiblePersonFromEntryViewModel = entryBuilder.ForProperty(x => x.DefaultResponsiblePerson)
				 .UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
				 .UseViewModelDialog<LeadersViewModel>()
				 .Finish();

			OrganizationFromEntryViewModel = entryBuilder.ForProperty(x => x.DefaultOrganization)
				 .UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				 .UseViewModelDialog<OrganizationViewModel>()
				 .Finish();
		}

		#region Свойства для View

		public bool VisibleWarehouse => featuresService.Available(WorkwearFeature.Warehouses);

		#endregion
	}
}
