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
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Tools;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Company;

namespace Workwear.ViewModels.Export {
	
	public class FutureIssueExportViewModel : UowDialogViewModelBase {

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private readonly EmployeeIssueModel issueModel;
		private readonly BaseParameters baseParameters;
		private readonly ModalProgressCreator localProgress;
		private readonly SizeService _sizeService;

		public FutureIssueExportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			EmployeeIssueModel issueModel,
			BaseParameters baseParameters,
			SizeService sizeService,
			OrganizationRepository organizationRepository,
			
			IValidator validator = null,
			string UoWTitle = null,
			UnitOfWorkProvider unitOfWorkProvider = null)
			: base(unitOfWorkFactory, navigation, validator, UoWTitle, unitOfWorkProvider) {
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			_sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			
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
		private bool runSensetive;
		public bool SensetiveLoad => startDate <= endDate;
		public EntityEntryViewModel<Organization> ResponsibleOrganizationEntryViewModel { get; set; }
		public IProgressBarDisplayable ProgressLocal;
        public IProgressBarDisplayable ProgressGlobal;
		public List<ColumnInfo> ColumnMap;
		
		public virtual bool RunSensetive {
			get => runSensetive;
			set => SetField(ref runSensetive, value);
		}
		
		private Organization exportOrganization;
        public virtual Organization ExportOrganization {
            get => exportOrganization;
            set => SetField(ref exportOrganization, value);
        }
            
        private DateTime startDate = DateTime.Today.Date;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}
		
		private DateTime endDate = DateTime.Today.AddMonths(1);
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}
		#endregion

		#region  Шаблон файла
		private List<ColumnInfo> configureColumnMap(ICellStyle cellStyleDate, ICellStyle cellStyleDateVirtual, ICellStyle cellStyleFinance) {
			var columnMap = new List<ColumnInfo>() {
				new ColumnInfo() {Label = 
					"Организация", FillCell = (cell, item) => {
					cell.SetCellValue(item.Organization?.Name ?? "");}},
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
					if(item?.LasatIssueOperation?.OperationTime.Date != null) 
						cell.SetCellValue(item.LasatIssueOperation.OperationTime.Date);},
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
					"Дата пропущеной выдачи", FillCell = (cell, item) => { 
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
			RunSensetive = false;
			string filename = "";
			
			
			object[] param = new object[4];
			param[0] = "Отмена";
			param[1] = Gtk.ResponseType.Cancel;
			param[2] = "Сохранить";
			param[3] = Gtk.ResponseType.Accept;

			using(Gtk.FileChooserDialog fc = new Gtk.FileChooserDialog("Сохранить как", null, Gtk.FileChooserAction.Save, param)) {
				fc.CurrentName = "Прогноз выдач на " + startDate.ToShortDateString() + "-" + endDate.ToShortDateString()
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
				var globlProgress = new ProgressPerformanceHelper(ProgressGlobal, 8, nameof(issueModel.PreloadWearItems), logger: logger); 
				globlProgress.StartGroup("Загрузка общих данных");
				_sizeService.RefreshSizes(UoW);
				
				IWorkbook workbook = new XSSFWorkbook();
				ISheet sheet = workbook.CreateSheet("Планируемые выдачи");

				#region Получение данных
				var employes = UoW.Session.QueryOver<EmployeeCard>()
					.Fetch(SelectMode.Fetch, x => x.Subdivision)
					.Fetch(SelectMode.Fetch, x => x.Department)
					.Fetch(SelectMode.Fetch, x => x.Post)
					.Where(x => x.DismissDate == null)
					.List();

				var employeeIds = employes.Select(x => x.Id).ToArray();
				UoW.Session.QueryOver<Norm>()
					.Future();
				UoW.Session.QueryOver<NormItem>()
					.Future();
				
				globlProgress.StartGroup(nameof(issueModel.PreloadEmployeeInfo));
				issueModel.PreloadEmployeeInfo(employeeIds);
				
				globlProgress.StartGroup(nameof(issueModel.PreloadWearItems));
				issueModel.PreloadWearItems(employeeIds);
				
				globlProgress.StartGroup(nameof(issueModel.FillWearReceivedInfo));
				ProgressLocal.Close(); 
				issueModel.FillWearReceivedInfo(employes.ToArray(), progress: ProgressLocal);

				globlProgress.StartGroup("Создание документа");
				var wearCardsItems = employes.SelectMany(x => x.WorkwearItems);
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

				#region Формирование набора данных
				globlProgress.StartGroup("Перебор потребностей");				
				ProgressLocal.Start(wearCardsItems.Count());
				int i = 1;
				int gc = 0;
				
				foreach(var item in wearCardsItems) {
					if(gc++ > 10000) {
						GC.Collect();
						GC.WaitForPendingFinalizers();
						gc = 0;
					}
					
					ProgressLocal.Add(text:"Потребность " + item.EmployeeCard.ShortName);
					GtkHelper.WaitRedraw();
					
					DateTime? delayIssue = item.NextIssue < startDate ? item.NextIssue : null;
					DateTime? vacationEnd;
					//список созданных объектов операций
					List<EmployeeIssueOperation> virtualOperations = new List<EmployeeIssueOperation>(); 
					
					//отпускники
					var vacation = item.EmployeeCard.CurrentVacation(startDate);
					if(vacation != null)
						if(vacation.EndDate < EndDate)
							vacationEnd = vacation.EndDate;
						else 						
							continue;

					//номенклатура с максимальной стоимостью
					Nomenclature nomenclature = null;
					if(item.ProtectionTools?.Nomenclatures.Count > 0)
						nomenclature =
							item.ProtectionTools?.Nomenclatures.Aggregate((x, y) => (x?.SaleCost ?? 0.0M) >= (y?.SaleCost ?? 0.0M) ? x : y);

					item.UpdateNextIssue(null);
					while(item.NextIssue.HasValue && item.NextIssue < EndDate) {
						int need = item.CalculateRequiredIssue(baseParameters, (DateTime)item.NextIssue);
						if(need == 0)
							break;
						//Операцция приведшая к возникновению потребности
						var lastIsssue = item.Graph.GetWrittenOffOperation((DateTime)item.NextIssue);
						//создаём следующую виртуальную выдачу
						var op = new EmployeeIssueOperation(baseParameters) {
							OperationTime = (DateTime)item.NextIssue < startDate ? startDate : (DateTime)item.NextIssue,
							StartOfUse = item.NextIssue,
							Issued = need,
							Returned = 0,
							OverrideBefore = false,

							Employee = item.EmployeeCard,
							NormItem = item.ActiveNormItem,
							ProtectionTools = item.ProtectionTools,
						};
						op.ExpiryByNorm = item.ActiveNormItem.CalculateExpireDate(op.OperationTime);
						op.AutoWriteoffDate = op.ExpiryByNorm; //Подстраховка
						virtualOperations.Add(op);

						//Создание строки выгрузки на эту выдачу
						if(op.OperationTime >= StartDate) {
							IRow row = sheet.CreateRow(i++);
							(new ExportRow() {
								Organization = ExportOrganization,
								EmployeeCardItem = item,
								OperationDate = op.OperationTime,
								Nomenclature = nomenclature,
								Amount = op.Issued,
								LasatIssueOperation = lastIsssue,
								DelayIssueDate = delayIssue,
								VirtualLastIssue = virtualOperations.Any(o => o == lastIsssue)
							}).SetRow(row, ColumnMap);
							delayIssue = null;
						}

						var resetOperations = item.Graph.Intervals.Where(gi=> gi.StartDate == item.NextIssue)
							.Select(ai => ai.ActiveItems)
							.Select(x => x.Select(y => y.IssueOperation).Where(o => o.OverrideBefore));
						if(op.Issued < op.NormItem.Amount && resetOperations.Any())
							foreach (var opR in resetOperations.SelectMany(x => x).Select(y => y))
								if(opR != null)
									opR.Issued = op.NormItem.Amount;

						item.Graph.AddOperations(new List<EmployeeIssueOperation> { op });
						item.UpdateNextIssue(null);
					}
				}
				ProgressLocal.Close();
				#endregion
				globlProgress.StartGroup("Сохранение документа");
				foreach(var col in ColumnMap)
					switch(col.Wight) {
						case 0 : break;
						case -1: sheet.AutoSizeColumn(col.Index); break;
						default: sheet.SetColumnWidth(col.Index, col.Wight);break;
					}
				workbook.Write(fileStream);
				workbook.Close();
				globlProgress.End();
			}
			RunSensetive = true;
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
		
		private CellType type = CellType.String;
		public CellType Type {
			get => type;
			set => type = value;
		}
		
		private ICellStyle style = null;
		public ICellStyle Style {
			get => style;
			set => style = value;
		}

		public FillCellD<ICell, ExportRow> FillCell
			= (cell, item) => { cell.SetCellValue("*"); };
		public FillCellS<ICell, ExportRow, ColumnInfo> SetStyle
			= (cell, item, col) => { cell.CellStyle = col.Style; };
			
	}
	
	/// <summary>
	/// Строка экспорта. 
	/// </summary>
	public class ExportRow {
		public Organization Organization { get; set; }
		public EmployeeCardItem EmployeeCardItem { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public EmployeeIssueOperation LasatIssueOperation { get; set; }
		public bool VirtualLastIssue { get; set; }

		public EmployeeCard Employee => EmployeeCardItem.EmployeeCard;
		public Subdivision Subdivision => Employee.Subdivision;
		public Department Department => Employee.Department;
		public Post Post => Employee.Post;	
		public ProtectionTools ProtectionTools => EmployeeCardItem.ProtectionTools;
		public NormItem NormItem => EmployeeCardItem.ActiveNormItem;
		public Norm Norm => NormItem.Norm;

		public Size Size =>
			Employee.Sizes.FirstOrDefault(s => DomainHelper.EqualDomainObjects(s.SizeType, ProtectionTools.Type.SizeType))?.Size;
		public Size Height =>
			Employee.Sizes.FirstOrDefault(s => DomainHelper.EqualDomainObjects(s.SizeType, ProtectionTools.Type.HeightType))?.Size;

		public DateTime ? OperationDate { get; set; }
		public DateTime ? LastIssueDate { get; set; }
		public DateTime ? DelayIssueDate { get; set; }
		public int Amount { get; set; }

		public virtual void SetRow(IRow row, List<ColumnInfo> columnMap) {
			foreach(var col in columnMap) {
				ICell cell = row.CreateCell(col.Index);
				cell.SetCellType(col.Type);
				col.SetStyle(cell, this, col);
				col.FillCell(cell, this);
			}
		}
	}
}
