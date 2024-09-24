using System;
using System.Collections.Generic;
using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.Analytics {
	public class WarehouseForecastingViewModel : UowDialogViewModelBase
	{
		public WarehouseForecastingViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			StockRepository stockRepository,
			FeaturesService featuresService,
			UnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkFactory, navigation, UoWTitle: "Прогнозирование склада" , unitOfWorkProvider: unitOfWorkProvider)
		{
			Title = "Прогнозирование склада";
			
			var builder = new CommonEEVMBuilderFactory<WarehouseForecastingViewModel>(this, this, UoW, navigation, autofacScope);
			warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			WarehouseEntry = builder.ForProperty(x => x.Warehouse)
				.MakeByType()
				.Finish();
		}

		#region Свойства View
		public IProgressBarDisplayable Progress { get; set; }
		
		public EntityEntryViewModel<Warehouse> WarehouseEntry;

		private Warehouse warehouse;
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}

		private DateTime endDate = DateTime.Today.AddMonths(3);
		public virtual DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}
		
		private List<WarehouseForecastingItem> items = new List<WarehouseForecastingItem>();
		public virtual List<WarehouseForecastingItem> Items {
			get => items;
			set => SetField(ref items, value);
		}
		#endregion

		#region Действия

		public void Fill() {
			
		}

		#endregion
	}
}
