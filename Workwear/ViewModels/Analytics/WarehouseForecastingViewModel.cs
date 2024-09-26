using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Utilities.Text;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Models.Analytics;
using Workwear.Models.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Analytics {
	public class WarehouseForecastingViewModel : UowDialogViewModelBase
	{
		private readonly EmployeeIssueModel issueModel;
		private readonly FutureIssueModel futureIssueModel;
		private readonly StockBalanceModel stockBalance;
		private readonly SizeService sizeService;

		public WarehouseForecastingViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			StockRepository stockRepository,
			FeaturesService featuresService,
			EmployeeIssueModel issueModel,
			FutureIssueModel futureIssueModel,
			StockBalanceModel stockBalance,
			SizeService sizeService,
			UnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkFactory, navigation, UoWTitle: "Прогнозирование склада" , unitOfWorkProvider: unitOfWorkProvider)
		{
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.futureIssueModel = futureIssueModel ?? throw new ArgumentNullException(nameof(futureIssueModel));
			this.stockBalance = stockBalance ?? throw new ArgumentNullException(nameof(stockBalance));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			Title = "Прогнозирование склада";
			
			var builder = new CommonEEVMBuilderFactory<WarehouseForecastingViewModel>(this, this, UoW, navigation, autofacScope);
			warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			WarehouseEntry = builder.ForProperty(x => x.Warehouse)
				.MakeByType()
				.Finish();
		}

		#region Свойства View
		public IProgressBarDisplayable ProgressTotal { get; set; }
		public IProgressBarDisplayable ProgressLocal { get; set; }
		
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
		
		private bool sensitiveFill = true;
		public virtual bool SensitiveFill {
			get => sensitiveFill;
			set => SetField(ref sensitiveFill, value);
		}
		#endregion

		#region Действия

		public void Fill() {
			SensitiveFill = false;
			stockBalance.Warehouse = Warehouse;
			ProgressTotal.Start(10, text:"Получение данных");
			ProgressLocal.Start(4, text:"Загрузка размеров");
			sizeService.RefreshSizes(UoW);
			ProgressLocal.Add(text: "Получение работающих сотрудников");
			var employees = UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.DismissDate == null)
				.List();
			var employeeIds = employees.Select(x => x.Id).ToArray();
			ProgressLocal.Add(text: "Получение норм");
			UoW.Session.QueryOver<Norm>()
				.Where(x => !x.Archival)
				.Fetch(SelectMode.Fetch, x => x.Items)
				.List();
			ProgressLocal.Add(text: "Заполняем сотрудников");
			issueModel.PreloadEmployeeInfo(employeeIds);
				
			ProgressLocal.Add(text: "Заполнение потребностей");
			issueModel.PreloadWearItems(employeeIds);
			ProgressLocal.Close(); 
			
			ProgressTotal.Add(text: "Получение выданных вещей");
			issueModel.FillWearReceivedInfo(employees.ToArray(), progress: ProgressLocal);

			ProgressTotal.Add(text: "Прогнозирование выдач");
			var wearCardsItems = employees.SelectMany(x => x.WorkwearItems).ToList();
			var featureIssues = futureIssueModel.CalculateIssues(DateTime.Today, EndDate, false, wearCardsItems, ProgressLocal);
			ProgressTotal.Add(text: "Получение складских остатков");
			var nomenclatures = featureIssues.SelectMany(x => x.ProtectionTools.Nomenclatures).Distinct().Where(x => !x.Archival).ToArray();
			stockBalance.AddNomenclatures(nomenclatures);
			ProgressTotal.Add(text: "Формируем прогноз");
			var groups = featureIssues.GroupBy(x => (x.ProtectionTools, x.Size, x.Height)).ToList();
			
			ProgressLocal.Start(groups.Count() + 2, text: "Суммирование");
			var result = new List<WarehouseForecastingItem>();
			foreach(var group in groups) {
				ProgressLocal.Add(text: group.Key.ProtectionTools.Name.EllipsizeMiddle(100));
				var stocks = stockBalance.ForNomenclature(group.Key.ProtectionTools.Nomenclatures.ToArray()).ToArray();
				var sex = stocks.OrderByDescending(x => x.Amount).FirstOrDefault()?.Position.Nomenclature.Sex ?? ClothesSex.Universal;
				if (sex == ClothesSex.Universal)
					result.Add(new WarehouseForecastingItem(group, stocks, ClothesSex.Universal));
				else {
					result.Add(new WarehouseForecastingItem(group, stocks, ClothesSex.Men));
					result.Add(new WarehouseForecastingItem(group, stocks, ClothesSex.Women));
				}
			}
			ProgressLocal.Add(text: "Сортировка");
			Items = result.OrderBy(x => x.ProtectionTool.Name).ThenBy(x => x.Size?.Name).ThenBy(x => x.Height?.Name).ToList();
			
			ProgressLocal.Close();
			ProgressTotal.Close();
			SensitiveFill = true;
		}

		#endregion
	}
}
