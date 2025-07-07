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
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Company
{
	public class SubdivisionViewModel : EntityDialogViewModelBase<Subdivision>, IDialogDocumentation
	{
		private readonly ITdiCompatibilityNavigation navigation;
		private readonly ILifetimeScope autofacScope;
		private readonly FeaturesService featuresService;

		public SubdivisionViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ITdiCompatibilityNavigation navigation, 
			IValidator validator, 
			ILifetimeScope autofacScope,
			FeaturesService featuresService
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			var builder = new CommonEEVMBuilderFactory<Subdivision>(this, Entity, UoW, NavigationManager, autofacScope);

			EntryWarehouse = builder.ForProperty(x => x.Warehouse)
									.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
									.UseViewModelDialog<WarehouseViewModel>()
									.Finish();
			
			EntrySubdivisionViewModel = builder.ForProperty(x => x.ParentSubdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("organization.html#mvz");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion

		#region Visible
		public bool VisibleWarehouse => featuresService.Available(WorkwearFeature.Warehouses);
		#endregion

		#region Controls
		public EntityEntryViewModel<Warehouse> EntryWarehouse;
		public EntityEntryViewModel<Subdivision> EntrySubdivisionViewModel;
		#endregion
	}
}
