using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using Autofac;
using ClosedXML.Excel;
using Gamma.Utilities;
using NHibernate;
using NHibernate.SqlCommand;
using NLog;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels.Control;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Models.Analytics;
using Workwear.Models.Analytics.WarehouseForecasting;
using Workwear.Models.Operations;
using Workwear.Repository.Regulations;
using Workwear.Repository.Stock;
using Workwear.Repository.Supply;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Supply;

namespace Workwear.ViewModels.Analytics {
	public class WarehouseForecastingViewModel : UowDialogViewModelBase, IDialogDocumentation, IForecastColumnsModel
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		private readonly ILifetimeScope autofacScope;
		private readonly NomenclatureRepository nomenclatureRepository;
		private readonly ShipmentRepository shipmentRepository;
		private readonly EmployeeIssueModel issueModel;
		private readonly FeaturesService featuresService;
		private readonly FutureIssueModel futureIssueModel;
		private readonly DutyNormIssueModel dutyNormIssueModel;
		private readonly DutyNormRepository dutyNormRepository;
		private readonly StockBalanceModel stockBalance;
		private readonly SizeService sizeService;
		private readonly IFileDialogService fileDialogService;
		private readonly ProtectionToolsRepository protectionToolsRepository;

		public WarehouseForecastingViewModel(
			EmployeeIssueModel issueModel,
			FeaturesService featuresService,
			FutureIssueModel futureIssueModel,
			DutyNormIssueModel dutyNormIssueModel,
			DutyNormRepository dutyNormRepository,
			IFileDialogService fileDialogService,
			ILifetimeScope autofacScope,
			INavigationManager navigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			NomenclatureRepository nomenclatureRepository,
			ShipmentRepository shipmentRepository,
			SizeService sizeService,
			StockBalanceModel stockBalance,
			StockRepository stockRepository,
			UnitOfWorkProvider unitOfWorkProvider,
			ProtectionToolsRepository protectionToolsRepository
			) : base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.shipmentRepository = shipmentRepository ?? throw new ArgumentNullException(nameof(shipmentRepository));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.futureIssueModel = futureIssueModel ?? throw new ArgumentNullException(nameof(futureIssueModel));
			this.dutyNormIssueModel = dutyNormIssueModel ?? throw new ArgumentNullException(nameof(dutyNormIssueModel));
			this.dutyNormRepository = dutyNormRepository ?? throw new ArgumentNullException(nameof(dutyNormRepository));
			this.stockBalance = stockBalance ?? throw new ArgumentNullException(nameof(stockBalance));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			this.protectionToolsRepository = protectionToolsRepository ?? throw new ArgumentNullException(nameof(protectionToolsRepository));
			Title = "Прогнозирование склада";
			
			var builder = new CommonEEVMBuilderFactory<WarehouseForecastingViewModel>(this, this, UoW, navigation, autofacScope);
			warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			WarehouseEntry = builder.ForProperty(x => x.Warehouse)
				.MakeByType()
				.Finish();
			Granularity = Granularity.Weekly;
			ShowCreateShipment = featuresService.Available(WorkwearFeature.Shipment);
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#warehouse-forecast");
		public string ButtonTooltip => "Онлайн документация по прогнозированию складских запасов";
		#endregion

		#region Свойства View
		public IProgressBarDisplayable ProgressTotal { get; set; }
		public IProgressBarDisplayable ProgressLocal { get; set; }
		
		public readonly EntityEntryViewModel<Warehouse> WarehouseEntry;

		#region Выбор номенклатур

		private ChoiceListViewModel<ProtectionTools> choiceProtectionToolsViewModel;
		private ChoiceListViewModel<Nomenclature> choiceNomenclatureViewModel;
		public IChoiceListViewModel ChoiceGoodsViewModel {
			get {
				switch(NomenclatureType) {
					case ForecastingNomenclatureType.Nomenclature:
						if(choiceNomenclatureViewModel == null) {
							choiceNomenclatureViewModel =
								new ChoiceListViewModel<Nomenclature>(nomenclatureRepository.GetActiveNomenclatures());
							choiceNomenclatureViewModel.SelectionChanged += (sender, args) => ShowItemsList();
						}
						return choiceNomenclatureViewModel;
					case ForecastingNomenclatureType.ProtectionTools:
						if(choiceProtectionToolsViewModel == null) {
							choiceProtectionToolsViewModel = new ChoiceListViewModel<ProtectionTools>
								(protectionToolsRepository.GetActiveProtectionTools(UoW));
							choiceProtectionToolsViewModel.SelectionChanged += (sender, args) => ShowItemsList();
						}
						return choiceProtectionToolsViewModel;
					default:
						throw new NotImplementedException();
				}
			}
		}
		#endregion
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
					// Пересчёт только если дата вышла за пределы уже рассчитанного периода.
					if(employees != null && EndDate > lastForecastUntil)
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
				if(SetField(ref sensitiveSettings, value)) {
					SensitiveExport = SensitiveSettings;
					OnPropertyChanged(nameof(CanCreateShipment));
				}
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
	
		public bool CanCreateShipment => SensitiveSettings && NomenclatureType == ForecastingNomenclatureType.Nomenclature ;
		public bool ShowCreateShipment { get; }

		#region Visible
		public bool ShipmentColumnVisible => featuresService.Available(WorkwearFeature.Shipment) && NomenclatureType == ForecastingNomenclatureType.Nomenclature;
		public bool SuitableInStockVisible => NomenclatureType == ForecastingNomenclatureType.Nomenclature;
		#endregion

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
		
		private ForecastingNomenclatureType nomenclatureType = ForecastingNomenclatureType.Nomenclature;
		[PropertyChangedAlso(nameof(ChoiceGoodsViewModel))]
		public ForecastingNomenclatureType NomenclatureType {
			get => nomenclatureType;
			set {
				if(SetField(ref nomenclatureType, value)) {
					MakeForecast();
                    OnPropertyChanged(nameof(CanCreateShipment));
				}
			}
		}

		private ForecastingPriceType priceType = ForecastingPriceType.AssessedCost;
		public ForecastingPriceType PriceType {
			get => priceType;
			set => SetField(ref priceType, value);
		}

		private ForecastColumn[] forecastColumns;
		public ForecastColumn[] ForecastColumns {
			get => forecastColumns;
			set => SetField(ref forecastColumns, value);
		}
		
		#endregion

		#region Внутренние переменные

		private IForecastingModel forecastingModel {
			get {
				switch(NomenclatureType) {
					case ForecastingNomenclatureType.ProtectionTools:
						return autofacScope.Resolve<ProtectionToolsForecastingModel>(new TypedParameter(typeof(IForecastColumnsModel), this));
					case ForecastingNomenclatureType.Nomenclature:
						return autofacScope.Resolve<NomenclatureForecastingModel>(new TypedParameter(typeof(IForecastColumnsModel), this));
					default:
						throw new NotImplementedException();
				}
			}
		}
		private bool NomenclatureAllSelected => NomenclatureType == ForecastingNomenclatureType.Nomenclature 
			? (choiceNomenclatureViewModel?.AllSelected ?? true)
			: (choiceProtectionToolsViewModel?.AllSelected ?? true);
		
		IList<EmployeeCard> employees;
		IList<DutyNormItem> dutyNormItems;
		DateTime lastForecastUntil;
		private List<FutureIssue> futureIssues = new List<FutureIssue>();
		#endregion
		#region Действия

		public void Fill() {
			SensitiveSettings = false;
			SensitiveFill = false; //Специально отключаем навсегда, так как при повторном заполнении дублируются данные. Если нужно будет включить придется разбираться.
			stockBalance.Warehouse = Warehouse;
			
			LogMemory("Fill: начало");
			
			ProgressTotal.Start(11, text:"Получение данных");
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

			ProgressTotal.Add(text: "Загрузка дежурных норм");
			dutyNormItems = dutyNormRepository.AllItemsFor(uow: UoW);
			ProgressTotal.Add(text: "Заполнение дежурных норм");
			dutyNormIssueModel.FillDutyNormItems(dutyNormItems.ToArray(), progress: ProgressLocal);

			ProgressTotal.Add(text: "Прогнозирование выдач");
			MakeForecast();
			
			LogMemory("Fill: конец");
		}
		
		/// <summary>
		/// Предзагрузка данных для устранения N+1: supply-номенклатуры ProtectionTools,
		/// их ProtectionToolsNomenclatures и NomenclatureSizes.
		/// </summary>
		private void PreloadForecastingData() {
			var ptIds = futureIssues.Select(x => x.ProtectionTools.Id).Distinct().ToArray();
			if(!ptIds.Any()) return;

			// Загружаем supply-номенклатуры (References, без дублей строк) и ProtectionToolsNomenclatures (HasMany) — двумя Future-запросами
			var ptsFuture = UoW.Session.QueryOver<ProtectionTools>()
				.WhereRestrictionOn(p => p.Id).IsIn(ptIds)
				.Fetch(SelectMode.Fetch, p => p.SupplyNomenclatureUnisex)
				.Fetch(SelectMode.Fetch, p => p.SupplyNomenclatureMale)
				.Fetch(SelectMode.Fetch, p => p.SupplyNomenclatureFemale)
				.Future();

			ProtectionToolsNomenclature ptnAlias = null;
			UoW.Session.QueryOver<ProtectionTools>()
				.WhereRestrictionOn(p => p.Id).IsIn(ptIds)
				.JoinAlias(p => p.ProtectionToolsNomenclatures, () => ptnAlias, JoinType.LeftOuterJoin)
				.Fetch(SelectMode.Fetch, p => p.ProtectionToolsNomenclatures)
				.Fetch(SelectMode.Fetch, () => ptnAlias.Nomenclature)
				.Future();

			var pts = ptsFuture.ToList();

			// Загружаем NomenclatureSizes для всех supply-номенклатур, чтобы ResolveSizeForNomenclature не делал N+1
			var nomIds = pts
				.SelectMany(p => new[] { p.SupplyNomenclatureUnisex?.Id, p.SupplyNomenclatureMale?.Id, p.SupplyNomenclatureFemale?.Id })
				.Where(id => id.HasValue).Select(id => id.Value)
				.Distinct().ToArray();

			if(nomIds.Any()) {
				UoW.Session.QueryOver<Nomenclature>()
					.WhereRestrictionOn(n => n.Id).IsIn(nomIds)
					.Fetch(SelectMode.ChildFetch, n => n)
					.Fetch(SelectMode.Fetch, n => n.NomenclatureSizes)
					.List();
			}
		}

		private void MakeForecast() {
			if(employees == null)
				return;
			SensitiveSettings = false;
			if(!ProgressTotal.IsStarted)
				ProgressTotal.Start(7, text: "Прогнозирование выдач сотрудникам");

			// Инициализирует нужный ChoiceListViewModel до обращения к forecastingModel.
			var _ = ChoiceGoodsViewModel;

			LogMemory("MakeForecast: начало");
			
			var wearCardsItems = employees.SelectMany(x => x.WorkwearItems).ToList();

			var issues = futureIssueModel.CalculateIssues(DateTime.Today, EndDate, true, wearCardsItems, ProgressLocal);
			futureIssues.AddRange(issues);

			ProgressTotal.Add(text: "Прогнозирование выдач по дежурным нормам");
			if(dutyNormItems.Any()) {
				var dutyIssues = futureIssueModel.CalculateDutyNormIssues(DateTime.Today, EndDate, true, dutyNormItems, ProgressLocal);
				futureIssues.AddRange(dutyIssues);
			}

			lastForecastUntil = EndDate;
			ProgressTotal.Add(text: "Получение складских остатков");
			var nomenclatures = NomenclatureType == ForecastingNomenclatureType.ProtectionTools 
				? futureIssues.SelectMany(x => x.ProtectionTools.Nomenclatures).Distinct().Where(x => !x.Archival).ToArray()
				: choiceNomenclatureViewModel.SelectedEntities.ToArray();
			stockBalance.AddNomenclatures(nomenclatures);

			ProgressTotal.Add(text: "Предзагрузка данных");
			PreloadForecastingData();
			ProgressTotal.Add(text: "Формируем прогноз");
			var result = forecastingModel.MakeForecastingItems(ProgressLocal, futureIssues);
			
			ProgressTotal.Add(text: "Заполнение заказанного");
			if(featuresService.Available(WorkwearFeature.Shipment)) {
				var ordered = shipmentRepository.GetOrderedItems();
				foreach(var item in result) {
					item.ShipmentItems.AddRange(ordered.Where(x => x.Nomenclature.IsSame(item.Nomenclature)
					                                                        && DomainHelper.IsSame(x.WearSize, item.Size) 
					                                                        && DomainHelper.IsSame(x.Height, item.Height)));
				}
			}

			ProgressTotal.Add(text: "Сортировка");
			InternalItems = result.OrderBy(x => x.Name).ThenBy(x => x.Size?.Name).ThenBy(x => x.Height?.Name).ToList();
			
			ProgressTotal.Close();
			LogMemory("MakeForecast: конец");
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
				int col = 1;
				if(NomenclatureType == ForecastingNomenclatureType.Nomenclature)
					worksheet.Column(col).Hide();
				worksheet.Column(col).Width = 50;
				worksheet.Cell(1, col++).Value = "Номенклатура нормы";
				worksheet.Column(col).Width = 50;
				worksheet.Cell(1, col++).Value = "Номенклатура";
				worksheet.Cell(1, col++).Value = "Размер";
				worksheet.Cell(1, col++).Value = "Рост";
				worksheet.Cell(1, col++).Value = "Пол";
				if(PriceType == ForecastingPriceType.None)
					worksheet.Column(col).Hide();
				worksheet.Cell(1, col++).Value = "Цена";
				worksheet.Cell(1, col++).Value = "В наличии";
				worksheet.Cell(1, col++).Value = "Просрочено";
				foreach(var column in ForecastColumns) {
					worksheet.Cell(1, col++).Value = column.Title;
				}
				if(ShipmentColumnVisible) {
					worksheet.Cell(1, col++).Value = "Заказано";
					worksheet.Cell(1, col++).Value = "Дата поступления";
					worksheet.Cell(1, col++).Value = "Причина расхождений";
				}
				worksheet.Cell(1, col++).Value = "Остаток без \nпросроченной";
				worksheet.Cell(1, col++).Value = "Остаток c \nпросроченной";
				ProgressLocal.Add();
				
				//Заполняем данными
				int row = 2;
				foreach(var item in Items) {
					col = 1;
					worksheet.Cell(row, col++).Value = item.ProtectionTool?.Name;
					worksheet.Cell(row, col++).Value = 
						NomenclatureType == ForecastingNomenclatureType.ProtectionTools ? item.Nomenclature?.Name : item.Name;
					worksheet.Cell(row, col++).Value = item.Size?.Name;
					worksheet.Cell(row, col++).Value = item.Height?.Name;
					worksheet.Cell(row, col++).Value = item.Sex.GetEnumShortTitle();
					worksheet.Cell(row, col++).Value = item.GetPrice(PriceType);
					worksheet.Cell(row, col++).Value = item.InStock;
					worksheet.Cell(row, col++).Value = item.Unissued;
					for(int i = 0; i < ForecastColumns.Length; i++) {
						worksheet.Cell(row, col++).Value = item.Forecast[i];
					}
					if(ShipmentColumnVisible) {
						worksheet.Cell(row, col++).Value = item.TotalOrdered;
						worksheet.Cell(row, col++).Value = String.Join(";", item.ShipmentItems.Select(s => s.Shipment.PeriodTitle));
						worksheet.Cell(row, col++).Value = String.Join(";", item.ShipmentItems.Select(s => s.DiffCause));
					}
					worksheet.Cell(row, col++).Value = item.InStock - item.Forecast.Sum();
					worksheet.Cell(row, col++).Value = item.InStock - item.Unissued - item.Forecast.Sum();
					row++;
					ProgressLocal.Add();
				}

				workbook.SaveAs(file.Path);
			}
			ProgressLocal.Close();
			SensitiveSettings = true;
		}

		public void CreateShipment(ShipmentCreateType eItemEnum) => 
			NavigationManager.OpenViewModel<ShipmentViewModel, IEntityUoWBuilder, List<WarehouseForecastingItem>, ShipmentParams>
				(null, EntityUoWBuilder.ForCreate(), Items, new ShipmentParams(eItemEnum, EndDate));

		#endregion

		#region Private

		private void LogMemory(string label) {
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var gcManaged = GC.GetTotalMemory(false);
			var workingSet = Process.GetCurrentProcess().WorkingSet64;
			Logger.Info("[Память] {0}: управляемая куча = {1:N0} КБ, рабочий набор = {2:N0} КБ",
				label,
				gcManaged / 1024,
				workingSet / 1024);
		}

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
			
			OnPropertyChanged(nameof(CanCreateShipment));
		}

		private void ShowItemsList() {
			IEnumerable<WarehouseForecastingItem> filtered = InternalItems;
			if(!NomenclatureAllSelected) {
				if(NomenclatureType == ForecastingNomenclatureType.ProtectionTools) {
					var selectedPt = new HashSet<ProtectionTools>(choiceProtectionToolsViewModel.SelectedEntities);
					filtered = filtered.Where(x => x.ProtectionTool != null && selectedPt.Contains(x.ProtectionTool));
				} else {
					var selectedNoms = new HashSet<Nomenclature>(choiceNomenclatureViewModel.SelectedEntities);
					filtered = filtered.Where(x => x.Nomenclature != null && selectedNoms.Contains(x.Nomenclature));
				}
			}

			switch(ShowMode) {
				case WarehouseForecastingShowMode.AllData:
					Items = filtered.ToList();
					break;
				case WarehouseForecastingShowMode.JustShortfall:
					Items = filtered.Where(x => x.WithDebt < 0).ToList();
					break;
				case WarehouseForecastingShowMode.JustSurplus:
					Items = filtered.Where(x => x.WithDebt > 0).ToList();
					break;
				default:
					throw new NotImplementedException();
			}
		}
		#endregion
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
	
	public enum ForecastingNomenclatureType {
		[Display(Name = "Складская номенклатура")]
		Nomenclature,
		[Display(Name = "Номенклатура нормы")]
		ProtectionTools
	}	
	
	public enum ShipmentCreateType {
		[Display(Name = "Без долга")]
		WithoutDebt,
		[Display(Name = "С долгом")]
		WithDebt
	}
	public class ShipmentParams
	{
		public ShipmentCreateType Type { get; }
		public DateTime EndDate { get; }

		public ShipmentParams(ShipmentCreateType type, DateTime endDate)
		{
			Type = type;
			EndDate = endDate;
		}
	}

}
