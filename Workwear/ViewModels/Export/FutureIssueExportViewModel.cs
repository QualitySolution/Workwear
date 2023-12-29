using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NHibernate;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Tools;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Company;


namespace Workwear.ViewModels.Export {
	
	public class FutureIssueExportViewModel : UowDialogViewModelBase {

		private readonly EmployeeIssueModel issueModel;
		private readonly BaseParameters baseParameters;
		private readonly EmployeeRepository employeeRepository;
		private readonly SizeService _sizeService;

		public EntityEntryViewModel<Organization> ResponsibleOrganizationEntryViewModel { get; set; }

		public List<ColumnInfo> ColumnMap = new List<ColumnInfo>() {
			/*new ColumnInfo() { Label = 
				"Организация", FillCell = (cell, item) 
				=> { cell.SetCellValue(item.Organization?.Name ?? ""); }
				},
			new ColumnInfo() { Label =
				"МВЗ.Код" , FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Subdivision?.Code ?? "-"); } },
			*/
			new ColumnInfo() { Label =
				"МВЗ" , FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Subdivision?.Name ?? "-"); } },
  			/*new ColumnInfo() { Label = 
				"Профессия.Код" , //FillCell = (cell, item)  => { cell.SetCellValue(""); }
				}, 
			new ColumnInfo() { Label =
				"Профессия" , FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Post?.Name ?? "-"); } },
			new ColumnInfo() { Label = 
				"Табельный", FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Employee.PersonnelNumber ?? "-"); } },
			new ColumnInfo() { Label = 
				"Орг. единица", FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Department?.Code ?? "-"); } },*/
			new ColumnInfo() { Label =
				"ФИО", FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Employee.FullName); } },
			new ColumnInfo() { Label =
				"Норма.Код" , FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Norm?.Id ?? 0); } },
			new ColumnInfo() { Label =
				"Норма", FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Norm.Name); } },
			new ColumnInfo() { Label =
				"Потребность", FillCell = (cell, item)  
				=> { cell.SetCellValue(item.ProtectionTools.Name); } },
			/*new ColumnInfo() { Label = 
				"Артикул", FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Nomenclature?.Number ?? ""); } },
			new ColumnInfo() { Label = 
				"Номенклатура", FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Nomenclature?.Name ?? ""); } },
			new ColumnInfo() { Label = 
				"Характеристика", FillCell = (cell, item) 
				=> { cell.SetCellValue(""); } },*/
			new ColumnInfo() { Label = 
				"Количество\nпо норме", FillCell = (cell, item)  
				=> { cell.SetCellValue(item.NormItem.Amount); } },
			/*new ColumnInfo() { Label = 
				"Срок использования", FillCell = (cell, item)  
				=> { cell.SetCellValue("-"); } }, // ***************************************************************************/
			new ColumnInfo() { Label = 
				"Последняя выдача" , FillCell = (cell, item)  
				=> { cell.SetCellValue(item?.LasatIssueOperation
				?.OperationTime.ToShortDateString() ?? "-"); } }, // **************************************************************************
			new ColumnInfo() { Label = 
				"Дата выдачи" , FillCell = (cell, item)  
				=> { cell.SetCellValue(item?.OperationDate.Value.ToShortDateString() ?? "-"); } },
			/*new ColumnInfo() { Label = 
				"Цена" , FillCell = (cell, item)  
				=> { cell.SetCellValue((item.Nomenc lature?.SaleCost ?? 0 * 1.2M).ToString()); } },
			new ColumnInfo() { Label = 
				"Цена без НДС", FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Nomenclature?.SaleCost.ToString() ?? "0" ); } },
			new ColumnInfo() { Label = 
				"Комментарий" , FillCell = (cell, item)  
				=> { cell.SetCellValue(""); } },*/
			new ColumnInfo() { Label = 
				"Количество" , FillCell = (cell, item)  
				=> { cell.SetCellValue(item.Amount); } },
			/*new ColumnInfo() { Label = 
				"Сумма" , FillCell = (cell, item)  
				=> { cell.SetCellValue((item.Nomenclature?.SaleCost ?? 0 * item.Amount * 1.2M).ToString()); } },
			new ColumnInfo() { Label = "Сумма без НДС" , FillCell = (cell, item)  
				=> { cell.SetCellValue((item.Nomenclature?.SaleCost ?? 0 * 1.2M).ToString()); } },*/
		};
		
		public FutureIssueExportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			EmployeeIssueModel issueModel,
			BaseParameters baseParameters,
			EmployeeRepository employeeRepository,
			SizeService sizeService,
			IValidator validator = null,
			string UoWTitle = null,
			UnitOfWorkProvider unitOfWorkProvider = null)
			: base(unitOfWorkFactory, navigation, validator, UoWTitle, unitOfWorkProvider) {
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			
			var entryBuilder = new CommonEEVMBuilderFactory<FutureIssueExportViewModel>(this, this, UoW, navigation) {
				AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope))
			};
			
			ResponsibleOrganizationEntryViewModel = entryBuilder.ForProperty(x => x.ExportOrganization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			int i = 0;
			foreach(var col in ColumnMap) 
				col.Index = i++;

		}

		// Создание новой рабочей книги
		IWorkbook workbook = new XSSFWorkbook();
		
		private bool runSensetive;
		public virtual bool RunSensetive {
			get => runSensetive;
			set => SetField(ref runSensetive, value);
		}
		
		private Organization exportOrganization;
            public virtual Organization ExportOrganization {
                get => exportOrganization;
                set => SetField(ref exportOrganization, value);
            }

		private DateTime startDate = DateTime.Today;
		public virtual DateTime StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}
		
		private DateTime endDate = DateTime.Today.AddYears(1);
		public virtual DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}
		
		private bool showCredit;
		public virtual bool ShowCredit {
			get => showCredit;
			set => SetField(ref showCredit, value);
		}

		public void Create() {
			RunSensetive = false;
			
			File.Delete("/home/space/file.xlsx");
			FileStream fileStream = new FileStream("/home/space/file.xlsx", FileMode.Create);

			EmployeeCard employeeCardAlias = null;
			Norm normAlias = null;

			_sizeService.RefreshSizes(UoW);
			var employes = UoW.Session.QueryOver<EmployeeCard>( () => employeeCardAlias)
				.Fetch(SelectMode.Fetch, x => x.Subdivision)
				.Fetch(SelectMode.Fetch, x => x.Department)
				.Fetch(SelectMode.Fetch, x => x.Post)
				.Where(x => x.DismissDate == null)
				.List();
			
			var employeeIds = employes.Select(x => x.Id).ToArray();
			
			issueModel.PreloadEmployeeInfo(employeeIds);

			issueModel.PreloadWearItems(employeeIds);

			issueModel.FillWearReceivedInfo(employes.ToArray());
		
			var wearCardsItem = employes.SelectMany(x => x.WorkwearItems);
			var norms = UoW.Session.QueryOver<Norm>( () => normAlias)
				.Future();
			
			ISheet sheet = workbook.CreateSheet("Планируемые выдачи");
			
			//Шапка
			IFont fontHead = workbook.CreateFont();
			fontHead.IsBold = true;
			ICellStyle cellStyleHead = workbook.CreateCellStyle();
			cellStyleHead.SetFont(fontHead);
			IRow row0 = sheet.CreateRow(0);
			foreach(var column in ColumnMap) {
				ICell cell = row0.CreateCell(column.Index);
				cell.SetCellValue(column.Label);
				cell.CellStyle = cellStyleHead;
				//sheet.SetColumnWidth(column.Index, column.Label.Length * 256);
			}
			
			int i = 1;
			foreach(var item in wearCardsItem) {
				//номенклатура с максимальной стоимостью
				Nomenclature nomenclature = null;
				if(item.ProtectionTools?.Nomenclatures.Count > 0)
					nomenclature = item.ProtectionTools?.Nomenclatures.Aggregate((x, y) => (x?.SaleCost ?? 0.0M) >= (y?.SaleCost ?? 0.0M) ? x : y);
				var lastIsssue = item.LastIssueOperation(EndDate,baseParameters); 
				//учёт долга
				bool creditNeed = ShowCredit;

				item.UpdateNextIssue(null);
				while(item.NextIssue.HasValue && item.NextIssue < EndDate) {
					int need = item.CalculateRequiredIssue(baseParameters, (DateTime)item.NextIssue);
					if (need == 0 && !creditNeed)
						break;
					//создаём следующую выдачу
					var op = new EmployeeIssueOperation(baseParameters) {
						OperationTime = (DateTime)item.NextIssue,
						StartOfUse = item.NextIssue,
						Issued = need,
						Returned = 0,
						OverrideBefore = false,

						Employee = item.EmployeeCard,
						NormItem = item.ActiveNormItem,
						ProtectionTools = item.ProtectionTools,
						ExpiryByNorm = item.ActiveNormItem.CalculateExpireDate((DateTime)item.NextIssue),
					};
					op.AutoWriteoffDate = op.ExpiryByNorm; //Подстраховка

					//Создание строки выгрзки на эту выдачу
					if(op.OperationTime >= StartDate || creditNeed) {
						IRow row = sheet.CreateRow(i++);
						(new ExportRow() {
							Organization = ExportOrganization,
							EmployeeCardItem = item,
							OperationDate = op.OperationTime,
							Nomenclature = nomenclature,
							Amount = op.Issued,
							LasatIssueOperation = lastIsssue,
						}).SetRow(row, ColumnMap);
						lastIsssue = null; // Будет только в первой строке. 
						creditNeed = false;
					}
					item.Graph.AddOperations(new List<EmployeeIssueOperation> { op });
					item.UpdateNextIssue(null);
				}
			}
			
			workbook.Write(fileStream);
			fileStream.Close();
			workbook.Close();
			RunSensetive = true;
		}
	}

	public delegate void FillCellD <ICell, ExportRow> (ICell cell, ExportRow item) ;

	public class ColumnInfo {
		public int Index { get; set; }

		public string Label { get; set; }

		public FillCellD<ICell, ExportRow> FillCell
			= (cell, item) => { cell.SetCellValue("*"); };
	}
	public class ExportRow {
		public Organization Organization { get; set; }
		public EmployeeCardItem EmployeeCardItem { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public EmployeeIssueOperation LasatIssueOperation { get; set; }
		
		public EmployeeCard Employee => EmployeeCardItem.EmployeeCard;
		public Subdivision Subdivision => Employee.Subdivision;
		public Department Department => Employee.Department;
		public Post Post => Employee.Post;	
		public ProtectionTools ProtectionTools => EmployeeCardItem.ProtectionTools;
		public NormItem NormItem => EmployeeCardItem.ActiveNormItem;
		public Norm Norm => NormItem.Norm;

		public DateTime ? OperationDate { get; set; }
		public DateTime ? LastIssueDate { get; set; }
		public int Amount { get; set; }

		public virtual void SetRow(IRow row, List<ColumnInfo> columnMap) {
			foreach(var col in columnMap) {
				ICell cell = row.CreateCell(col.Index);
				col.FillCell(cell, this);
			}
		}
	}
}
