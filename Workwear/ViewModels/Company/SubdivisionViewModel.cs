using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.Repository.Company;
using workwear.Tools.Features;
using workwear.ViewModels.Stock;

namespace workwear.ViewModels.Company
{
	public class SubdivisionViewModel : EntityDialogViewModelBase<Subdivision>
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

			NotifyConfiguration.Instance.BatchSubscribe(SubdivisionOperationChanged)
				.IfEntity<SubdivisionIssueOperation>()
				.AndWhere(x => x.Subdivision.Id == Entity.Id);
		}

		#region Свойства

		public IList<SubdivisionRecivedInfo> Items => SubdivisionRepository.ItemsBalance(UoW, Entity);

		#endregion

		#region Visible
		public bool VisibleWarehouse => featuresService.Available(WorkwearFeature.Warehouses);
		#endregion

		#region Controls

		public EntityEntryViewModel<Warehouse> EntryWarehouse;
		public EntityEntryViewModel<Subdivision> EntrySubdivisionViewModel;

		#endregion

		#region Обработка событий

		void SubdivisionOperationChanged(EntityChangeEvent[] changeEvents)
		{
			OnPropertyChanged(nameof(Items));
		}

		#endregion

		#region Действия View

		public void GiveItem()
		{
			if(UoW.IsNew && !Save())
				return;
			navigation.OpenViewModel<ExpenseObjectViewModel, IEntityUoWBuilder, Subdivision>(this, EntityUoWBuilder.ForCreate(), Entity);
		}

		public void ReturnItem()
		{
			navigation.OpenTdiTab<Dialogs.Stock.IncomeDocDlg, Subdivision>(this, Entity);
		}

		public void WriteOffItem()
		{
			navigation.OpenViewModel<WriteOffViewModel, IEntityUoWBuilder, Dictionary<Type, int>>(this, EntityUoWBuilder.ForCreate(), new Dictionary<Type, int>{ {typeof(Subdivision), Entity.Id }});
		}

		#endregion
		public override void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Dispose();
		}
	}
}
