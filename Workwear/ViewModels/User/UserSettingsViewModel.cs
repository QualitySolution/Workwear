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
using Workwear.Domain.Stock;
using Workwear.Domain.Users;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.User
{
	public class UserSettingsViewModel : EntityDialogViewModelBase<UserSettings>, IDialogDocumentation
	{
		public EntityEntryViewModel<Warehouse> WarehouseFromEntryViewModel;
		public EntityEntryViewModel<Leader> LeaderFromEntryViewModel;
		public EntityEntryViewModel<Organization> OrganizationFromEntryViewModel;
		public EntityEntryViewModel<Leader> ResponsiblePersonFromEntryViewModel;
		
		private readonly FeaturesService featuresService;

		public UserSettingsViewModel
		(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory,
			 INavigationManager navigation, ILifetimeScope autofacScope, FeaturesService featuresService, IValidator validator = null) 
		: base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
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

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("settings.html#user-settings");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		#region Свойства для View

		public bool VisibleWarehouse => featuresService.Available(WorkwearFeature.Warehouses);

		#endregion
	}
}
