using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using ClosedXML.Excel;
using Gamma.Utilities;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.Utilities.Text;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Models.Analytics;
using Workwear.Models.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Analytics {
	public class WarehouseForecastingViewModel : UowDialogViewModelBase, IDialogDocumentation
	{
		private readonly EmployeeIssueModel issueModel;
		private readonly FutureIssueModel futureIssueModel;
		private readonly StockBalanceModel stockBalance;
		private readonly SizeService sizeService;
		private readonly IFileDialogService fileDialogService;

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
			IFileDialogService fileDialogService,
			UnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider)
		{
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.futureIssueModel = futureIssueModel ?? throw new ArgumentNullException(nameof(futureIssueModel));
			this.stockBalance = stockBalance ?? throw new ArgumentNullException(nameof(stockBalance));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			Title = "Прогнозирование склада";
			
			var builder = new CommonEEVMBuilderFactory<WarehouseForecastingViewModel>(this, this, UoW, navigation, autofacScope);
			warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			WarehouseEntry = builder.ForProperty(x => x.Warehouse)
				.MakeByType()
				.Finish();
			Granularity = Granularity.Weekly;
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#warehouse-forecast");
		public string ButtonTooltip => "Онлайн документация по прогнозированию складских запасов";
		#endregion

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
				if(SetField(ref endDate, value)) {
					if(EndDate > lastForecastUntil || employees != null)
						MakeForecast();
					RefreshColumns();
				}
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
		
		private bool sensitiveSettings = true;
		public bool SensitiveSettings {
			get => sensitiveSettings;
			set { 
				if(SetField(ref sensitiveSettings, value))
					SensitiveExport = SensitiveSettings;
			}
		}

		private bool sensitiveExport = false;
		[PropertyChangedAlso(nameof(SensitiveFill))]
		public bool SensitiveExport {
			get => sensitiveExport;
			set => SetField(ref sensitiveExport, value);
		}
		
		private bool sensitiveFill = true;
		public bool SensitiveFill {
			get => sensitiveFill && SensitiveSettings;
			set {
				SetField(ref sensitiveFill, value);
				WarehouseEntry.IsEditable = value;
			}
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

		#region Внутренние переменные
		IList<EmployeeCard> employees;
		DateTime lastForecastUntil;
		List<FutureIssue> futureIssues = new List<FutureIssue>();
		#endregion
		#region Действия

		public void Fill() {
			SensitiveSettings = false;
			SensitiveFill = false; //Специально отключаем навсегда, так как при повторном заполнении дублируются данные. Если нужно будет включить придется разбираться.
			stockBalance.Warehouse = Warehouse;
			ProgressTotal.Start(4, text:"Получение данных");
			ProgressLocal.Start(4, text:"Загрузка размеров");
			sizeService.RefreshSizes(UoW);
			ProgressLocal.Add(text: "Получение работающих сотрудников");
			employees = UoW.Session.QueryOver<EmployeeCard>()
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
			MakeForecast();
		}
		
		private void MakeForecast() {
			if(employees == null)
				return;
			SensitiveSettings = false;
			if(!ProgressTotal.IsStarted)
				ProgressTotal.Start(2, text: "Прогнозирование выдач");
			
			var wearCardsItems = employees.SelectMany(x => x.WorkwearItems).ToList();
			var issues = futureIssueModel.CalculateIssues(DateTime.Today, EndDate, true, wearCardsItems, ProgressLocal);
			futureIssues.AddRange(issues);
			lastForecastUntil = EndDate;
			ProgressTotal.Add(text: "Получение складских остатков");
			var nomenclatures = issues.SelectMany(x => x.ProtectionTools.Nomenclatures).Distinct().Where(x => !x.Archival).ToArray();
			stockBalance.AddNomenclatures(nomenclatures);
			ProgressTotal.Add(text: "Формируем прогноз");
			var groups = futureIssues.GroupBy(x => (x.ProtectionTools, x.Size, x.Height)).ToList();
			
			ProgressLocal.Start(groups.Count() + 1, text: "Суммирование");
			var result = new List<WarehouseForecastingItem>();
			foreach(var group in groups) {
				ProgressLocal.Add(text: group.Key.ProtectionTools.Name.EllipsizeMiddle(100));
				var stocks = stockBalance.ForNomenclature(group.Key.ProtectionTools.Nomenclatures.ToArray()).ToArray();
				SupplyType supplyType; 
				if(group.Key.ProtectionTools.SupplyType == SupplyType.Unisex && group.Key.ProtectionTools.SupplyNomenclatureUnisex != null)
					supplyType = SupplyType.Unisex;
				else if(group.Key.ProtectionTools.SupplyType == SupplyType.TwoSex && (group.Key.ProtectionTools.SupplyNomenclatureMale != null || group.Key.ProtectionTools.SupplyNomenclatureFemale != null))
					supplyType = SupplyType.TwoSex;
				else
					supplyType = (stocks.OrderByDescending(x => x.Amount).FirstOrDefault()?.Position.Nomenclature.Sex ?? ClothesSex.Universal) == ClothesSex.Universal ? SupplyType.Unisex : SupplyType.TwoSex;
				if (supplyType == SupplyType.Unisex)
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
			SensitiveSettings = true;
		}

		public void ExportToExcel() {
			var settings = new DialogSettings();
			settings.FileFilters.Add(new DialogFileFilter("Excel 2007 ", "*.xlsx"));
			settings.Title = "Сохранить прогноз склада";
			settings.DefaultFileExtention = "xlsx";
			settings.PlatformType = DialogPlatformType.Crossplatform;
			string sheetName;
			switch(ShowMode) {
				case WarehouseForecastingShowMode.JustShortfall:
					settings.FileName = $"Заявка на поставку по {EndDate:dd.MM.yyyy}";
					sheetName = "Заявка на поставку";
					break;
				case WarehouseForecastingShowMode.JustSurplus:
					settings.FileName = $"Излишки склада на {EndDate:dd.MM.yyyy}";
					sheetName = "Излишки склада";
					break;
				default:
					settings.FileName = $"Прогноз склада до {EndDate:dd.MM.yyyy}";
					sheetName = "Прогноз склада";
					break;
			};
			settings.FileName += ".xlsx";
			var file = fileDialogService.RunSaveFileDialog(settings);
			if(!file.Successful)
				return;
			
			SensitiveSettings = false;
			ProgressLocal.Start(Items.Count + 1, text: "Сохранение");
			using (var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add(sheetName);
				//Создаем заголовки
				worksheet.Cell("A1").Value = "Номенклатура нормы";
				worksheet.Cell("B1").Value = "Номенклатура";
				worksheet.Cell("C1").Value = "Размер";
				worksheet.Cell("D1").Value = "Рост";
				worksheet.Cell("E1").Value = "Пол";
				worksheet.Cell("F1").Value = "В наличии";
				worksheet.Cell("G1").Value = "Просрочено";
				int col = 8;
				foreach(var column in ForecastColumns) {
					worksheet.Cell(1, col).Value = column.Title;
					col++;
				}
				worksheet.Cell(1, col).Value = "Остаток без \nпросроченной";
				col++;
				worksheet.Cell(1, col).Value = "Остаток c \nпросроченной";
				ProgressLocal.Add();
				
				//Заполняем данными
				int row = 2;
				foreach(var item in Items) {
					worksheet.Cell(row, 1).Value = item.ProtectionTool.Name;
					worksheet.Cell(row, 2).Value = item.Nomenclature?.Name;
					worksheet.Cell(row, 3).Value = item.Size?.Name;
					worksheet.Cell(row, 4).Value = item.Height?.Name;
					worksheet.Cell(row, 5).Value = item.Sex.GetEnumShortTitle();
					worksheet.Cell(row, 6).Value = item.InStock;
					worksheet.Cell(row, 7).Value = item.Unissued;
					col = 8;
					for(int i = 0; i < ForecastColumns.Length; i++) {
						worksheet.Cell(row, col).Value = item.Forecast[i];
						col++;
					}
					worksheet.Cell(row, col).Value = item.InStock - item.Forecast.Sum();
					col++;
					worksheet.Cell(row, col).Value = item.InStock - item.Unissued - item.Forecast.Sum();
					row++;
					ProgressLocal.Add();
				}

				workbook.SaveAs(file.Path);
			}
			ProgressLocal.Close();
			SensitiveSettings = true;
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
						list.Add(new ForecastColumn() { Title = $"с {startWeek:dd.MM}\nпо {endWeek:dd.MM}", StartDate = startWeek, EndDate = endWeek });
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
