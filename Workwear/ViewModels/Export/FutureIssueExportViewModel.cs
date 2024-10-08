using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NHibernate;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Utilities;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Company;
using Workwear.Models.Analytics;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Tools;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Company;

namespace Workwear.ViewModels.Export {
	
	public class FutureIssueExportViewModel : UowDialogViewModelBase {

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
			SizeService sizeService,
			OrganizationRepository organizationRepository,
			UnitOfWorkProvider unitOfWorkProvider = null)
			: base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider) {
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.futureIssueModel = futureIssueModel ?? throw new ArgumentNullException(nameof(futureIssueModel));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			
			Title = "Прогноз выдач";
			
			var entryBuilder = new CommonEEVMBuilderFactory<FutureIssueExportViewModel>(this, this, UoW, navigation) {
				AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope))
			};
			
			ResponsibleOrganizationEntryViewModel = entryBuilder.ForProperty(x => x.ExportOrganization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			ExportOrganization = organizationRepository.GetDefaultOrganization(UoW, autofacScope.Resolve<IUserService>().CurrentUserId);
		}

		#region Поля и свойства
		private bool runSensitive;
		public bool SensitiveLoad => startDate <= endDate;
		public EntityEntryViewModel<Organization> ResponsibleOrganizationEntryViewModel { get; set; }
		public IProgressBarDisplayable ProgressLocal;
        public IProgressBarDisplayable ProgressGlobal;
		public List<ColumnInfo> ColumnMap;
		
		public virtual bool RunSensitive {
			get => runSensitive;
			set => SetField(ref runSensitive, value);
		}
		
		private Organization exportOrganization;
        public virtual Organization ExportOrganization {
            get => exportOrganization;
            set => SetField(ref exportOrganization, value);
        }
            
        private DateTime startDate = DateTime.Today.Date;
		[PropertyChangedAlso(nameof(SensitiveLoad))]
		public virtual DateTime StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}
		
		private DateTime endDate = DateTime.Today.AddMonths(1);
		[PropertyChangedAlso(nameof(SensitiveLoad))]
		public virtual DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}
		
		private bool noDebt;
		public virtual bool NoDebt {
			get => noDebt;
			set => SetField(ref noDebt, value);
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
					cell.SetCellValue(item.NormItem.Amount + " " + item.NormItem.ProtectionTools.Type.Units.Name);},
					Type = CellType.Numeric},
				new ColumnInfo() {Label =
					"Срок\nиспользования", FillCell = (cell, item) => {
					cell.SetCellValue(item.NormItem.LifeText);}},
				new ColumnInfo() {Label =
					"Виртуальная", FillCell = (cell, item) => {
					cell.SetCellValue(item.VirtualLastIssue ? "+" : "");},},
				new ColumnInfo() {Label = 
					"Дата последней выдачи", FillCell = (cell, item) => { 
					if(item?.LastIssueOperation?.OperationTime.Date != null) 
						cell.SetCellValue(item.LastIssueOperation.OperationTime.Date);},
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
				new ColumnInfo() {Label =
					"Цена", FillCell = (cell, item) => {
					cell.SetCellValue((double)((item.Nomenclature?.SaleCost ?? 0) * 1.2M));},
					Type = CellType.Numeric,
					Style = cellStyleFinance,
					Wight = -1},
				new ColumnInfo() {Label = 
					"Цена без НДС", FillCell = (cell, item) => {
					cell.SetCellValue((double)(item.Nomenclature?.SaleCost ?? 0));},
					Type = CellType.Numeric,
					Style = cellStyleFinance,
					Wight = -1},
				new ColumnInfo() {Label =
					"Комментарий", FillCell = (cell, item) => {
					cell.SetCellValue("");}},
				new ColumnInfo() {Label =
					"Количество", FillCell = (cell, item) => {
					cell.SetCellValue(item.Amount);}},
				new ColumnInfo() {Label =
					"Сумма", FillCell = (cell, item) => {
					cell.SetCellValue((double)((item.Nomenclature?.SaleCost ?? 0) * item.Amount * 1.2M));},
					Type = CellType.Numeric,
					Style = cellStyleFinance,
					Wight = -1},
				new ColumnInfo() {Label =
					"Сумма без НДС", FillCell = (cell, item) => {
					cell.SetCellValue((double)((item.Nomenclature?.SaleCost ?? 0) * item.Amount));},
					Type = CellType.Numeric,
					Style = cellStyleFinance,
					Wight = -1}
				};
			int i = 0;
			foreach(var col in columnMap) 
				col.Index = i++;
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
				fc.CurrentName = "Прогноз выдач" + (NoDebt ? "(без долгов)" : "") + " на " + startDate.ToShortDateString() + "-" + endDate.ToShortDateString()
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
				var globalProgress = new ProgressPerformanceHelper(ProgressGlobal, 8, "Загрузка общих данных", logger: logger); 
				sizeService.RefreshSizes(UoW);
				
				IWorkbook workbook = new XSSFWorkbook();
				ISheet sheet = workbook.CreateSheet("Планируемые выдачи");

				#region Получение данных
				var employees = UoW.Session.QueryOver<EmployeeCard>()
					.Fetch(SelectMode.Fetch, x => x.Subdivision)
					.Fetch(SelectMode.Fetch, x => x.Department)
					.Fetch(SelectMode.Fetch, x => x.Post)
					.Where(x => x.DismissDate == null)
					.List();

				var employeeIds = employees.Select(x => x.Id).ToArray();
				UoW.Session.QueryOver<Norm>()
					.Fetch(SelectMode.Fetch, x => x.Items)
					.List();
				
				globalProgress.CheckPoint(nameof(issueModel.PreloadEmployeeInfo));
				issueModel.PreloadEmployeeInfo(employeeIds);
				
				globalProgress.CheckPoint(nameof(issueModel.PreloadWearItems));
				issueModel.PreloadWearItems(employeeIds);
				
				globalProgress.CheckPoint(nameof(issueModel.FillWearReceivedInfo));
				ProgressLocal.Close(); 
				issueModel.FillWearReceivedInfo(employees.ToArray(), progress: ProgressLocal);

				globalProgress.CheckPoint("Создание документа");
				var wearCardsItems = employees.SelectMany(x => x.WorkwearItems).ToList();
				#endregion

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
				
				foreach(var column in ColumnMap) {
					ICell cell = row0.CreateCell(column.Index);
					cell.SetCellValue(column.Label);
					cell.CellStyle = cellStyleHead;
				}
				
				globalProgress.CheckPoint("Прогнозирование выдач");
				var featureIssues = futureIssueModel.CalculateIssues(StartDate, EndDate, NoDebt, wearCardsItems, ProgressLocal);
				
				globalProgress.CheckPoint("Заполнение Excel файла");
				int i = 1;
				foreach(var issue in featureIssues) {
					IRow row = sheet.CreateRow(i++);
					foreach(var col in ColumnMap) {
						ICell cell = row.CreateCell(col.Index);
						cell.SetCellType(col.Type);
						col.SetStyle(cell, issue, col);
						col.FillCell(cell, issue);
					}
				}
				globalProgress.CheckPoint("Сохранение документа");
				foreach(var col in ColumnMap)
					switch(col.Wight) {
						case 0 : break;
						case -1: sheet.AutoSizeColumn(col.Index); break;
						default: sheet.SetColumnWidth(col.Index, col.Wight);break;
					}
				workbook.Write(fileStream);
				workbook.Close();
				globalProgress.End();
			}
			RunSensitive = true;
		}
		#endregion
	}

	public delegate void FillCellD <ICell, ExportRow> (ICell cell, ExportRow item) ;
	public delegate void FillCellS <ICell, ExportRow, ColumnInfo> (ICell cell, ExportRow item , ColumnInfo col) ;

	/// <summary>
	/// Данные о конкретной колонке экспорта
	/// </summary>
	public class ColumnInfo {
		public int Index { get; set; }
		public string Label { get; set; }
		public int Wight { get; set; } //Умолчание 0
		public CellType Type { get; set; } = CellType.String;
		public ICellStyle Style { get; set; } = null;

		public FillCellD<ICell, FutureIssue> FillCell
			= (cell, item) => { cell.SetCellValue("*"); };
		public FillCellS<ICell, FutureIssue, ColumnInfo> SetStyle
			= (cell, item, col) => { cell.CellStyle = col.Style; };
			
	}
	
	
}
