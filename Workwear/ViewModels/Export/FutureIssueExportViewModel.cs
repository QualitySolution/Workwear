using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Utilities;
using QS.ViewModels.Control;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Company;
using Workwear.Models.Analytics;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Company;

namespace Workwear.ViewModels.Export {
	
	public class FutureIssueExportViewModel : UowDialogViewModelBase, IDialogDocumentation {
		public FeaturesService FeaturesService { get; }

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private readonly EmployeeIssueModel issueModel;
		private readonly FutureIssueModel futureIssueModel;
		private readonly BaseParameters baseParameters;
		private readonly SizeService sizeService;

		public FutureIssueExportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			EmployeeIssueModel issueModel,
			FutureIssueModel futureIssueModel,
			BaseParameters baseParameters,
			FeaturesService featuresService,
			SizeService sizeService,
			OrganizationRepository organizationRepository,
			UnitOfWorkProvider unitOfWorkProvider = null)
			: base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider) {
			FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.futureIssueModel = futureIssueModel ?? throw new ArgumentNullException(nameof(futureIssueModel));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			
			Title = "Прогноз выдач";
			RunVisible = runSensitive = true;
			DoneVisible = false;
			
			var entryBuilder = new CommonEEVMBuilderFactory<FutureIssueExportViewModel>(this, this, UoW, navigation) {
				AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope))
			};
			
			ResponsibleOrganizationEntryViewModel = entryBuilder.ForProperty(x => x.ExportOrganization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			var protectionToolsList = UoW.GetAll<ProtectionTools>().ToList();
			ChoiceProtectionToolsViewModel = new ChoiceListViewModel<ProtectionTools>(protectionToolsList);
			ChoiceProtectionToolsViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
			
			ExportOrganization = organizationRepository.GetDefaultOrganization(UoW, autofacScope.Resolve<IUserService>().CurrentUserId);
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("export.html#forecast-of-issues");
		public string ButtonTooltip => DocHelper.GetDialogDocTooltip(Title);
		#endregion

		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(ChoiceProtectionToolsViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(RunSensitive));
		}
		
		#region Поля и свойства
		public EntityEntryViewModel<Organization> ResponsibleOrganizationEntryViewModel { get; set; }
		public ChoiceListViewModel<ProtectionTools> ChoiceProtectionToolsViewModel;
		public IProgressBarDisplayable ProgressLocal;
        public IProgressBarDisplayable ProgressGlobal;
		public List<ColumnInfo> ColumnMap;
		
		private bool runSensitive;
		public virtual bool RunSensitive {
			get => runSensitive && (startDate <= endDate) && !ChoiceProtectionToolsViewModel.AllUnSelected;
			set => SetField(ref runSensitive, value);
		}
		public virtual bool RunVisible { get; set; }
		public virtual bool DoneVisible { get; set; }
		
		private Organization exportOrganization;
        public virtual Organization ExportOrganization {
            get => exportOrganization;
            set => SetField(ref exportOrganization, value);
        }
            
        private DateTime startDate = DateTime.Today.Date;
		[PropertyChangedAlso(nameof(RunSensitive))]
		public virtual DateTime StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}
		
		private DateTime endDate = DateTime.Today.AddMonths(1);
		[PropertyChangedAlso(nameof(RunSensitive))]
		public virtual DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}
		
		private bool moveDebt = true;
		public virtual bool MoveDebt {
			get => moveDebt;
			set => SetField(ref moveDebt, value);
		}
		
		private FutureIssueExportCost exportCost = FutureIssueExportCost.None;
		public FutureIssueExportCost ExportCost {
			get => exportCost;
			set => SetField(ref exportCost, value);
		}
		#endregion

		#region  Шаблон файла
		private List<ColumnInfo> configureColumnMap(ICellStyle cellStyleDate, ICellStyle cellStyleDateVirtual, ICellStyle cellStyleFinance) {
			var columnMap = new List<ColumnInfo>() {
				new ColumnInfo() {Label = 
					"Организация", FillCell = (cell, item) => {
					cell.SetCellValue(ExportOrganization?.Name ?? "");}},
				new ColumnInfo() {Label =
					"МВЗ.Код", FillCell = (cell, item) => {
					cell.SetCellValue(item.Subdivision?.Code ?? "");}},
				new ColumnInfo() { Label =
					"МВЗ", FillCell = (cell, item) => {
					cell.SetCellValue(item.Subdivision?.Name ?? "");}},
				new ColumnInfo() {Label =
					"Профессия.Код",FillCell = (cell, item) => {
					cell.SetCellValue(item.Post?.Code ?? "");}},
				new ColumnInfo() {Label =
					"Профессия", FillCell = (cell, item) => {
					cell.SetCellValue(item.Post?.Name ?? "");}},
				new ColumnInfo() {Label =
					"Табельный",FillCell = (cell, item) => {
					cell.SetCellValue(item.Employee.PersonnelNumber ?? "");}},
				new ColumnInfo() {Label =
					"Орг. единица", FillCell = (cell, item) => {
					cell.SetCellValue(item.Department?.Code ?? "");}},
				new ColumnInfo() {Label =
					"ФИО", FillCell = (cell, item) => {
					cell.SetCellValue(item.Employee.FullName);}},
				new ColumnInfo() {Label =
					"Пол", FillCell = (cell, item) => {
					cell.SetCellValue(item.Employee.Sex == Sex.M ? "М" : item.Employee.Sex == Sex.F ? "Ж" : "-");}},
				new ColumnInfo() {Label =
					"Норма.Код", FillCell = (cell, item) => {
					cell.SetCellValue(item.Norm?.Id ?? 0);}},
				new ColumnInfo() {Label =
					"Норма",FillCell = (cell, item) => {
					cell.SetCellValue(item.ProtectionTools.Name);}},
				new ColumnInfo() {Label =
					"Тип выдачи",FillCell = (cell, item) => {
					cell.SetCellValue(item.ItemsType?.IssueType.GetEnumTitle() ?? "");}},
				new ColumnInfo() {Label =
					"Артикул", FillCell = (cell, item) => {
					cell.SetCellValue(item.Nomenclature?.Number ?? "");}},
				new ColumnInfo() {Label =
					"Номенклатура", FillCell = (cell, item) => {
					cell.SetCellValue(item.Nomenclature?.Name ?? "");}},
				new ColumnInfo() {Label =
					"Характеристика", FillCell = (cell, item) => {
					cell.SetCellValue(item.Size?.Name + (item.Height != null ? (" / " + item.Height.Name) : ""));}},
				new ColumnInfo() {Label =
					"Количество\nпо норме", FillCell = (cell, item) => {
					cell.SetCellValue(item.NormItem.AmountText); },
					Type = CellType.Numeric},
				new ColumnInfo() {Label =
					"Срок\nиспользования", FillCell = (cell, item) => {
					cell.SetCellValue(item.NormItem.LifeText);}},
				new ColumnInfo() {Label =
					"Ограничения", FillCell = (cell, item) => {
					cell.SetCellValue(item.NormItem.NormCondition?.Name ?? "");}},
				new ColumnInfo() {Label =
					"Виртуальная", FillCell = (cell, item) => {
					cell.SetCellValue(item.VirtualLastIssue ? "+" : "");},},
				new ColumnInfo() {Label = 
					"Дата последней выдачи", FillCell = (cell, item) => { 
					if(item.LastIssueDate != null) 
						cell.SetCellValue(item.LastIssueDate.Value);},
					Type = CellType.Numeric,
					SetStyle = (cell, item, col) => { cell.CellStyle = item.VirtualLastIssue ? cellStyleDateVirtual : cellStyleDate; },
					Wight = 3000},
				new ColumnInfo() {Label =
					"Дата выдачи", FillCell = (cell, item) => {
					if(item.OperationDate != null)
						cell.SetCellValue(item.OperationDate.Value.Date);},
					Type = CellType.Numeric, 
					Style = cellStyleDate,
					Wight = 3000},
				new ColumnInfo() {Label =
					"Дата пропущенной выдачи", FillCell = (cell, item) => { 
					if(item.DelayIssueDate != null) 
						cell.SetCellValue(item?.DelayIssueDate?.ToShortDateString() ?? "");}, 
					Type = CellType.Numeric,
                    Style = cellStyleDate,
                    Wight = 3000},
				new ColumnInfo() {
					Label = "Цена",
					Visible = () => ExportCost != FutureIssueExportCost.None,
					FillCell = (cell, item) => {cell.SetCellValue((double)((GetCost(item) ?? 0) * 1.2M));},
					Type = CellType.Numeric,
					Style = cellStyleFinance,
					Wight = -1},
				new ColumnInfo() {
					Label = "Цена без НДС",
					Visible = () => ExportCost != FutureIssueExportCost.None,
					FillCell = (cell, item) => {cell.SetCellValue((double)(GetCost(item) ?? 0));},
					Type = CellType.Numeric,
					Style = cellStyleFinance,
					Wight = -1},
				new ColumnInfo() {Label =
					"Комментарий", FillCell = (cell, item) => {
					cell.SetCellValue("");}},
				new ColumnInfo() {Label =
					"Количество", FillCell = (cell, item) => {
					cell.SetCellValue(item.Amount);}},
				new ColumnInfo() {
					Label = "Сумма",
					Visible = () => ExportCost != FutureIssueExportCost.None,
					FillCell = (cell, item) => {cell.SetCellValue((double)((GetCost(item) ?? 0) * item.Amount * 1.2M));},
					Type = CellType.Numeric,
					Style = cellStyleFinance,
					Wight = -1},
				new ColumnInfo() {
					Label = "Сумма без НДС",
					Visible = () => ExportCost != FutureIssueExportCost.None,
					FillCell = (cell, item) => {cell.SetCellValue((double)((GetCost(item) ?? 0) * item.Amount));},
					Type = CellType.Numeric,
					Style = cellStyleFinance,
					Wight = -1}
				};
			int i = 0;
			return columnMap;
		}
		#endregion

		#region Создание файла
		public void Create() {
			RunSensitive = false;
			string filename = "";
			
			
			object[] param = new object[4];
			param[0] = "Отмена";
			param[1] = Gtk.ResponseType.Cancel;
			param[2] = "Сохранить";
			param[3] = Gtk.ResponseType.Accept;

			using(Gtk.FileChooserDialog fc = new Gtk.FileChooserDialog("Сохранить как", null, Gtk.FileChooserAction.Save, param)) {
				fc.CurrentName = (ChoiceProtectionToolsViewModel.AllSelected ? "П" : "Частичный п")
				                 + "рогноз выдач" 
				                 + (MoveDebt ? " (c долгами)" : "") 
				                 + " на " + startDate.ToShortDateString() + "-" + endDate.ToShortDateString()
				                 + " от " + DateTime.Now.ToShortDateString() + ".xlsx";
				if(fc.Run() == (int)Gtk.ResponseType.Accept) 
					filename = fc.Filename;
				fc.Destroy();
			}
			GtkHelper.WaitRedraw();
			if(String.IsNullOrEmpty(filename))
				return;
			if(!filename.ToLower().Trim().EndsWith(".xlsx"))
				filename += ".xlsx";
			
			using(FileStream fileStream = new FileStream(filename, FileMode.Create)) {
				var globalProgress = new ProgressPerformanceHelper(ProgressGlobal, 7, "Загрузка общих данных", showProgressText: true, logger: logger); 
				sizeService.RefreshSizes(UoW);
				
				IWorkbook workbook = new XSSFWorkbook();
				ISheet sheet = workbook.CreateSheet("Планируемые выдачи");

				#region Получение данных

				globalProgress.CheckPoint("Загрузка потребностей");
				var wearCardsItems = issueModel
					.LoadWearItemsForProtectionTools(ChoiceProtectionToolsViewModel.SelectedIds);
				var protectionTools = wearCardsItems.Select(x => x.ProtectionTools).Distinct();
				
				globalProgress.CheckPoint("Загрузка сотрудников");
				var employees = issueModel.LoadEmployeeFullInfo(wearCardsItems.Select(x => x.EmployeeCard.Id).ToArray());
					
				globalProgress.CheckPoint("Загрузка выдач");
				issueModel.FillWearReceivedInfo(wearCardsItems.ToArray(), progress: ProgressLocal);
				#endregion

				globalProgress.CheckPoint("Создание документа");
				#region Форматы и стили ячеек
				//Форматы ячеек. 
				IDataFormat dataFormater = workbook.CreateDataFormat();
				short dateFormat = dataFormater.GetFormat("dd.MM.yyyy");
				short moneyFormat = dataFormater.GetFormat($"#.## {baseParameters.UsedCurrency}");
				IFont fontFormatGrey = workbook.CreateFont();
				fontFormatGrey.Color = HSSFColor.Grey40Percent.Index;
				IFont fontFormatBold = workbook.CreateFont();
				fontFormatBold.IsBold = true;
                                
				ICellStyle cellStyleDateBase = workbook.CreateCellStyle();
				cellStyleDateBase.DataFormat = dateFormat;
				ICellStyle cellStyleDateVirtual = workbook.CreateCellStyle();
				cellStyleDateVirtual.DataFormat = dateFormat;
				cellStyleDateVirtual.SetFont(fontFormatGrey);
				cellStyleDateVirtual.SetFont(fontFormatGrey);
				ICellStyle cellStyleFinance = workbook.CreateCellStyle();
				cellStyleFinance.DataFormat = moneyFormat;
				#endregion
				
				//Шапка
				IFont fontHead = workbook.CreateFont();
				fontHead.IsBold = true;
				ICellStyle cellStyleHead = workbook.CreateCellStyle();
				cellStyleHead.SetFont(fontHead);
				cellStyleHead.WrapText = true;
				IRow row0 = sheet.CreateRow(0);
				row0.Height = 1000;

				ColumnMap = configureColumnMap(cellStyleDateBase, cellStyleDateVirtual, cellStyleFinance);
				foreach(var (column, index) in ColumnsWithIndex) {
					ICell cell = row0.CreateCell(index);
					cell.SetCellValue(column.Label);
					cell.CellStyle = cellStyleHead;
				}
				
				globalProgress.CheckPoint("Прогнозирование выдач");
				var featureIssues = futureIssueModel.CalculateIssues(StartDate, EndDate, MoveDebt, wearCardsItems, ProgressLocal);
				
				globalProgress.CheckPoint("Заполнение Excel файла");
				int rowIndex = 1;
				foreach(var issue in featureIssues) {
					IRow row = sheet.CreateRow(rowIndex++);
					foreach(var (column, index) in ColumnsWithIndex) {
						ICell cell = row.CreateCell(index);
						cell.SetCellType(column.Type);
						column.SetStyle(cell, issue, column);
						column.FillCell(cell, issue);
					}
				}
				globalProgress.CheckPoint("Сохранение документа");
				foreach(var (col, index) in ColumnsWithIndex)
					switch(col.Wight) {
						case 0 : break;
						case -1: sheet.AutoSizeColumn(index); break;
						default: sheet.SetColumnWidth(index, col.Wight);break;
					}
				workbook.Write(fileStream);
				workbook.Close();
				globalProgress.End();
			}
			RunVisible = false;
			DoneVisible = true;
			OnPropertyChanged(nameof(RunVisible));
			OnPropertyChanged(nameof(DoneVisible));
		}
		#endregion
		
		#region функции
		private decimal? GetCost(FutureIssue item) {
			switch(ExportCost) {
				case FutureIssueExportCost.None: return null;
				case FutureIssueExportCost.Assessed: return item.ProtectionTools.AssessedCost;
				case FutureIssueExportCost.Sale: return item.Nomenclature?.SaleCost;
				default: return null;
			}
		}

		private IEnumerable<(ColumnInfo, int)> ColumnsWithIndex => ColumnMap.Where(x => x.Visible()).Select((x, i) => (x, i));
		#endregion
	}

	public delegate void FillCellD <ICell, ExportRow> (ICell cell, ExportRow item) ;
	public delegate void FillCellS <ICell, ExportRow, ColumnInfo> (ICell cell, ExportRow item , ColumnInfo col) ;

	/// <summary>
	/// Данные о конкретной колонке экспорта
	/// </summary>
	public class ColumnInfo {
		public Func<bool> Visible { get; set; } = () => true;
		public string Label { get; set; }
		public int Wight { get; set; } //Умолчание 0
		public CellType Type { get; set; } = CellType.String;
		public ICellStyle Style { get; set; } = null;

		public FillCellD<ICell, FutureIssue> FillCell
			= (cell, item) => { cell.SetCellValue("*"); };
		public FillCellS<ICell, FutureIssue, ColumnInfo> SetStyle
			= (cell, item, col) => { cell.CellStyle = col.Style; };
			
	}

	public enum FutureIssueExportCost {
		[Display(Name = "Без стоимости")]
		None,
		[Display(Name = "Оценочная стоимость")]
		Assessed,
		[Display(Name = "Стоимость продажи")]
		Sale
	}
}
