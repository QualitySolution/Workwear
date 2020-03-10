using System;
using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;

namespace workwear.ViewModels.Stock
{
	public class WarehouseViewModel : LegacyEntityDialogViewModelBase<Warehouse>
	{
		public EntityEntryViewModel<Subdivision> SubdivisionEntryViewModel;
		public ILifetimeScope AutofacScope;
		public ITdiCompatibilityNavigation navigationManager;
		public ITdiCompatibilityNavigation tdiNavigationManager;
		private readonly CommonMessages commonMessages;

		public WarehouseViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ITdiTab myTab, ITdiCompatibilityNavigation navigationManager, ILifetimeScope autofacScope, CommonMessages commonMessages) : base(uowBuilder, unitOfWorkFactory, myTab, navigationManager)
		{
			this.tdiNavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.commonMessages = commonMessages;
			var entryBuilder = new LegacyEEVMBuilderFactory<Warehouse>(this, TdiTab, Entity, UoW, navigationManager) {
				AutofacScope = AutofacScope
			};
		}
	}
}
