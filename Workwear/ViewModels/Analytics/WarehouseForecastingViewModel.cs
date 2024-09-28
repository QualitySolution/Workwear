﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
			UnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider)
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
			Granularity = Granularity.Weekly;
		}

		#region Свойства View
		public IProgressBarDisplayable ProgressTotal { get; set; }
		public IProgressBarDisplayable ProgressLocal { get; set; }
		
		public readonly EntityEntryViewModel<Warehouse> WarehouseEntry;

		private Warehouse warehouse;
		public Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}

		private DateTime endDate = DateTime.Today.AddMonths(3);
		public DateTime EndDate {
			get => endDate;
			set {
				if(SetField(ref endDate, value))
					RefreshColumns();
			}
		}

		private List<WarehouseForecastingItem> internalItems = new List<WarehouseForecastingItem>();
		protected List<WarehouseForecastingItem> InternalItems {
			get => internalItems;
			set { 
				if(SetField(ref internalItems, value))
					ShowItemsList(); 
			}
		}

		private List<WarehouseForecastingItem> items = new List<WarehouseForecastingItem>();
		public List<WarehouseForecastingItem> Items {
			get => items;
			set => SetField(ref items, value);
		}
		
		private bool sensitiveFill = true;
		public bool SensitiveFill {
			get => sensitiveFill;
			set => SetField(ref sensitiveFill, value);
		}
		
		private Granularity granularity;
		public Granularity Granularity {
			get => granularity;
			set { 
				if(SetField(ref granularity, value))
			         RefreshColumns();
			}
		}

		private WarehouseForecastingShowMode showMode;

		public WarehouseForecastingShowMode ShowMode {
			get => showMode;
			set {
				if(SetField(ref showMode, value))
					ShowItemsList();
			}
		}

		private ForecastColumn[] forecastColumns;
		public ForecastColumn[] ForecastColumns {
			get => forecastColumns;
			set => SetField(ref forecastColumns, value);
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
					result.Add(new WarehouseForecastingItem(this, group.Key, group.ToList(), stocks, ClothesSex.Universal));
				else {
					var mensIssues = group.Where(x => x.Employee.Sex == Sex.M).ToList();
					if (mensIssues.Any())
						result.Add(new WarehouseForecastingItem(this, group.Key, mensIssues, stocks, ClothesSex.Men));
					var womenIssues = group.Where(x => x.Employee.Sex == Sex.F).ToList();
					if(womenIssues.Any())
						result.Add(new WarehouseForecastingItem(this, group.Key, womenIssues, stocks, ClothesSex.Women));
				}
			}
			ProgressLocal.Add(text: "Сортировка");
			InternalItems = result.OrderBy(x => x.ProtectionTool.Name).ThenBy(x => x.Size?.Name).ThenBy(x => x.Height?.Name).ToList();
			
			ProgressLocal.Close();
			ProgressTotal.Close();
			SensitiveFill = true;
		}

		#endregion

		#region Private

		private void RefreshColumns() {
			var list = new List<ForecastColumn>();
			switch(Granularity) {
				case Granularity.Totally:
					list.Add(new ForecastColumn() { Title = "Потребность\nза весь период", StartDate = DateTime.Today, EndDate = EndDate });
					break;
				case Granularity.Monthly:
					var startMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
					while(startMonth <= EndDate) {
						var endMonth = startMonth.AddMonths(1).AddDays(-1);
						list.Add(new ForecastColumn() { Title = startMonth.ToString("yyyy\nMMMM"), StartDate = startMonth, EndDate = endMonth });
						startMonth = endMonth.AddDays(1);
					}
					break;
				case Granularity.Weekly:
					var startWeek = DateTime.Today.AddDays(DateTime.Today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1 - (int)DateTime.Today.DayOfWeek);
					while(startWeek <= EndDate) {
						var endWeek = startWeek.AddDays(6);
						list.Add(new ForecastColumn() { Title = $"c {startWeek:dd.MM}\nпо {endWeek:dd.MM}", StartDate = startWeek, EndDate = endWeek });
						startWeek = endWeek.AddDays(1);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			ForecastColumns = list.ToArray();
			foreach(var item in Items) {
				item.FillForecast();
			}
		}

		private void ShowItemsList() {
			switch(ShowMode) {
				case WarehouseForecastingShowMode.AllData:
					Items = InternalItems;
					break;
				case WarehouseForecastingShowMode.JustShortfall:
					Items = InternalItems.Where(x => x.ClosingBalance < 0).ToList();
					break;
				case WarehouseForecastingShowMode.JustSurplus:
					Items = InternalItems.Where(x => x.ClosingBalance > 0).ToList();
					break;
				default:
					throw new NotImplementedException();
			}
		}
		#endregion
	}

	public class ForecastColumn {
		public string Title { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}

	public enum Granularity {
		[Display(Name = "За весь период")]
		Totally,
		[Display(Name = "По месяцам")]
		Monthly,
		[Display(Name = "По неделям")]
		Weekly
	}

	public enum WarehouseForecastingShowMode {
		[Display(Name = "Все данные")]
		AllData,
		[Display(Name = "Только дефицит")]
		JustShortfall,
		[Display(Name = "Только излишки")]
		JustSurplus
	}
}
