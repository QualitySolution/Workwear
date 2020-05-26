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
using workwear.Journal.ViewModels.Stock;
using workwear.Repository.Company;
using workwear.ViewModels.Stock;

namespace workwear.ViewModels.Company
{
	public class SubdivisionViewModel : EntityDialogViewModelBase<Subdivision>
	{
		private readonly ITdiCompatibilityNavigation navigation;
		private readonly ILifetimeScope autofacScope;

		public SubdivisionViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ITdiCompatibilityNavigation navigation, IValidator validator, ILifetimeScope autofacScope) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var builder = new CommonEEVMBuilderFactory<Subdivision>(this, Entity, UoW, NavigationManager, autofacScope);

			EntryWarehouse = builder.ForProperty(x => x.Warehouse)
									.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
									.UseViewModelDialog<WarehouseViewModel>()
									.Finish();

			NotifyConfiguration.Instance.BatchSubscribe(SubdivisionOperationChanged)
				.IfEntity<SubdivisionIssueOperation>()
				.AndWhere(x => x.Subdivision.Id == Entity.Id);
		}

		#region Свойства

		public IList<SubdivisionRecivedInfo> Items => SubdivisionRepository.ItemsBalance(UoW, Entity);

		#endregion

		#region Controls

		public EntityEntryViewModel<Warehouse> EntryWarehouse;

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

			navigation.OpenTdiTab<ExpenseDocDlg, Subdivision>(this, Entity);
		}

		public void ReturnItem()
		{
			navigation.OpenTdiTab<IncomeDocDlg, Subdivision>(this, Entity);
		}

		public void WriteOffItem()
		{
			navigation.OpenTdiTab<WriteOffDocDlg, Subdivision>(this, Entity);
		}

		#endregion

		public override void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Dispose();
		}
	}
}
