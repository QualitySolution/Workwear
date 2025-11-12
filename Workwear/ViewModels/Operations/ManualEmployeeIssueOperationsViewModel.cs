using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Models.Operations;
using Workwear.Repository.Operations;
using Workwear.Tools;
using Workwear.Tools.Barcodes;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Operations 
{
	public class ManualEmployeeIssueOperationsViewModel : UowDialogViewModelBase, IDialogDocumentation
	{
		private readonly SizeService sizeService;
		private readonly BarcodeService barcodeService;
		private readonly IInteractiveQuestion interactive;
		private readonly ProtectionTools protectionTools;
		private readonly EmployeeCard employee;
		private readonly EmployeeIssueModel issueModel;
		private readonly BaseParameters baseParameters;

		public ManualEmployeeIssueOperationsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation,
			EmployeeIssueRepository repository,
			SizeService sizeService,
			BarcodeService barcodeService,
			ILifetimeScope autofacScope,
			IInteractiveQuestion interactive,
			EmployeeIssueModel issueModel,
			BaseParameters baseParameters,
			EmployeeCardItem cardItem = null,
			EmployeeIssueOperation selectOperation = null,
			ProtectionTools protectionTools = null,
			EmployeeCard employee = null,
			IValidator validator = null) : base(unitOfWorkFactory, navigation, validator, "Редактирование ручных операций",unitOfWorkProvider) 
		{
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			if(cardItem != null) {
				this.protectionTools = cardItem.ProtectionTools;
				EmployeeCardItem = UoW.GetById<EmployeeCardItem>(cardItem.Id);
				this.employee = EmployeeCardItem.EmployeeCard;
			}
			else if(selectOperation != null) {
				this.protectionTools = selectOperation.ProtectionTools;
				this.employee = selectOperation.Employee;
			}
			else {
				this.protectionTools = protectionTools;
				this.employee = employee;
			}
			
			if(this.employee == null || this.protectionTools == null)
				throw new ArgumentNullException(String.Join(" or ", nameof(selectOperation), nameof(cardItem), nameof(protectionTools), nameof(employee)));
			
			if(EmployeeCardItem == null)
				EmployeeCardItem = this.employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools.IsSame(protectionTools));
			
			Title = $"Ручные операции для {this.protectionTools.Name}";
			
			Operations = new ObservableList<EmployeeIssueOperation>(
				repository.GetAllManualIssue(UoW, this.employee, this.protectionTools)
					.OrderBy(x => x.OperationTime)
					.ToList());
			
			var entryBuilder = new CommonEEVMBuilderFactory<ManualEmployeeIssueOperationsViewModel>(this, this, UoW, navigation) {
				AutofacScope = autofacScope
			};
			NomenclatureEntryViewModel = entryBuilder.ForProperty(x => x.Nomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclatureJournalViewModel, NomenclatureFilterViewModel>(f => f.ProtectionTools = protectionTools)
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();
			NomenclatureEntryViewModel.IsEditable = false;
			
			//Исправляем ситуацию когда у операции пропала ссылка на норму, это может произойти в случает обновления нормы.
			if(EmployeeCardItem != null)
				foreach (var operation in Operations.Where(operation => operation.NormItem == null))
					operation.NormItem = EmployeeCardItem.ActiveNormItem;
			
			SelectOperation = selectOperation != null 
				? Operations.FirstOrDefault(x => x.Id == selectOperation.Id) 
				: Operations.FirstOrDefault();
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("employees.html#manual-issue");
		public string ButtonTooltip => DocHelper.GetDialogDocTooltip("Ручные операции");
		#endregion

		#region PublicProperty

		private IObservableList<EmployeeIssueOperation> operations;
		public IObservableList<EmployeeIssueOperation> Operations {
			get => operations;
			private set => SetField(ref operations, value);
		}

		private EmployeeCardItem employeeCardItem;
		[PropertyChangedAlso(nameof(CanAddOperation))]
		public EmployeeCardItem EmployeeCardItem {
			get => employeeCardItem;
			private set => SetField(ref employeeCardItem, value);
		}

		private EmployeeIssueOperation selectOperation;
		public EmployeeIssueOperation SelectOperation {
			get => selectOperation;
			set {
				changingOperation = true;

				if(SetField(ref selectOperation, value)) {
					NomenclatureEntryViewModel.IsEditable = SelectOperation != null;
					if(SelectOperation == null) 
						Issued = 0;

					OnPropertyChanged(nameof(VisibleSelectOperation));
					OnPropertyChanged(nameof(VisibleHeight));
					OnPropertyChanged(nameof(VisibleSize));
					OnPropertyChanged(nameof(Sizes));
					OnPropertyChanged(nameof(Heights));
					OnPropertyChanged(nameof(Size));
					OnPropertyChanged(nameof(Height));
					OnPropertyChanged(nameof(Nomenclature));
					OnPropertyChanged(nameof(CanEditOperation));
					OnPropertyChanged(nameof(VisibleBarcodes));
					OnPropertyChanged(nameof(Issued));
					OnPropertyChanged(nameof(WearPercent));
					OnPropertyChanged(nameof(IssueDate));
					OnPropertyChanged(nameof(AutoWriteoffDate));
					OnPropertyChanged(nameof(BarcodesText));
					changingOperation = false;
				}
			}
		}
		
		public EntityEntryViewModel<Nomenclature> NomenclatureEntryViewModel { get; private set; } 
		#endregion
		#region Проброс свойств операции
		
		public DateTime? IssueDate {
			get => SelectOperation?.OperationTime;
			set {
				if(changingOperation) 
					return;
				if(SelectOperation != null && SelectOperation.OperationTime != value) {
					SelectOperation.OperationTime = (DateTime)value;
					RecalculateDatesOfSelectedOperation();
					OnPropertyChanged(nameof(AutoWriteoffDate));
					OnPropertyChanged();
				}
			}
		}
		
		public DateTime? AutoWriteoffDate {
			get => SelectOperation?.AutoWriteoffDate;
			set {
				if(changingOperation) 
					return;
				if(SelectOperation != null && SelectOperation.AutoWriteoffDate != value) {
					SelectOperation.AutoWriteoffDate = value;
					SelectOperation.UseAutoWriteoff = !(value is null);
					OnPropertyChanged();
				}
			}
		}
		
		public int Issued {
			get => SelectOperation?.Issued ?? 0;
			set {
				if(changingOperation) 
					return;
				if(SelectOperation != null && SelectOperation.Issued != value) {
					SelectOperation.Issued = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(SensitiveCreateBarcodes));
					OnPropertyChanged(nameof(SensitiveBarcodesPrint));
				}
			}
		}

		public Nomenclature Nomenclature {
			get => SelectOperation?.Nomenclature;
			set {
				if(changingOperation) 
					return;
				if(SelectOperation == null || DomainHelper.IsSame(SelectOperation.Nomenclature, value))
					return;
				SelectOperation.Nomenclature = value;
				if(SelectOperation.WearSize != null && 
				   !SelectOperation.WearSize.SizeType.IsSame(SelectOperation.Nomenclature?.Type?.SizeType))
						SelectOperation.WearSize = null;
				if(SelectOperation.Height != null &&
				   !SelectOperation.Height.SizeType.IsSame(SelectOperation.Nomenclature?.Type?.HeightType))
						SelectOperation.Height = null;
				
				OnPropertyChanged();
				OnPropertyChanged(nameof(VisibleHeight));
				OnPropertyChanged(nameof(VisibleSize));
				OnPropertyChanged(nameof(VisibleBarcodes));
				OnPropertyChanged(nameof(Sizes));
				OnPropertyChanged(nameof(Heights));
				OnPropertyChanged(nameof(Size));
				OnPropertyChanged(nameof(Height));
				OnPropertyChanged(nameof(BarcodesText));
				OnPropertyChanged(nameof(SensitiveCreateBarcodes));
				OnPropertyChanged(nameof(SensitiveBarcodesPrint));
			}
		}

		public Size Size {
			get => SelectOperation?.WearSize;
			set {
				if(changingOperation) 
					return;
				SelectOperation.WearSize = value;
				OnPropertyChanged();
			}
		}

		public Size Height {
			get => SelectOperation?.Height;
			set {
				if(changingOperation) 
					return;
				SelectOperation.Height = value;
				OnPropertyChanged();
			}
		}

		public decimal WearPercent {
			get => (SelectOperation?.WearPercent ?? 0)  * 100;
			set {
				if(changingOperation) 
					return;
				if(SelectOperation != null && value != SelectOperation.WearPercent * 100 ) {
					SelectOperation.WearPercent = value / 100;
					RecalculateDatesOfSelectedOperation();
					OnPropertyChanged();
					OnPropertyChanged(nameof(AutoWriteoffDate));
					OnPropertyChanged(nameof(SensitiveCreateBarcodes));
                    OnPropertyChanged(nameof(SensitiveBarcodesPrint));
				}
			}
		}

		public bool OverrideBefore {
			get => SelectOperation?.OverrideBefore ?? false;
			set {
				if(changingOperation) 
					return;
				if(SelectOperation != null && value != SelectOperation.OverrideBefore){
					SelectOperation.OverrideBefore = value;;
                    OnPropertyChanged();
				}
			}
		}

		public string Comment {
			get => SelectOperation?.Comment;
			set {
				if(changingOperation) 
					return;
				if(SelectOperation != null && value != SelectOperation.Comment) {
					SelectOperation.Comment = value;
					OnPropertyChanged();
				}
			}
		}
		#endregion

		#region Заполняемые значения

		public IEnumerable<Size> Sizes => sizeService.GetSize(UoW, Nomenclature?.Type.SizeType, onlyUseInNomenclature: true)
			.Where(x => x.SizeType.Id != 1);
		public IEnumerable<Size> Heights => sizeService.GetSize(UoW, Nomenclature?.Type.HeightType, onlyUseInNomenclature: true)
			.Where(x => x.SizeType.Id == 1);
		#endregion
		
		#region Sensintive and Visibility
		public bool VisibleSelectOperation => SelectOperation != null;
		public bool VisibleHeight => SelectOperation?.Height != null ||
			(Nomenclature != null ? Nomenclature?.Type.HeightType != null : protectionTools?.Type.HeightType != null);
		public bool VisibleSize => SelectOperation?.WearSize != null ||
			(Nomenclature != null ? Nomenclature?.Type.SizeType != null : protectionTools?.Type.SizeType != null);
		public bool VisibleBarcodes => (Nomenclature?.UseBarcode ?? false) || (SelectOperation?.BarcodeOperations.Any() ?? false);
		public bool VisibleManualCalculate => EmployeeCardItem?.ActiveNormItem == null;
		public bool CanEditOperation => SelectOperation != null;
		public bool CanAddOperation => employee != null && protectionTools != null;
		public bool SensitiveCreateBarcodes => (Nomenclature?.UseBarcode ?? false) && (SelectOperation?.BarcodeOperations.Count ?? 0) != Issued;
		public bool SensitiveBarcodesPrint => Issued > 0 && ((Nomenclature?.UseBarcode ?? false) || (SelectOperation?.BarcodeOperations.Count ?? 0) > 0);

		public event Action<ProtectionTools> SaveChanged;

		#endregion

		#region Списание

		private int expiredMonths;

		public virtual int ExpiredMonths {
			get => expiredMonths;
			set => SetField(ref expiredMonths, value);
		}

		public void CalculateExpense() {
			AutoWriteoffDate = IssueDate?.AddMonths(ExpiredMonths);
		}

		#endregion
		
		#region Штрих коды

		private bool NeedCreateBarcodes => (SelectOperation?.BarcodeOperations.Count ?? 0) == 0;
		public string ButtonCreateOrRemoveBarcodesTitle => 
			(Nomenclature?.UseBarcode ?? false) && (SelectOperation?.BarcodeOperations.Count ?? 0) > Issued
				? "Обновить штрихкоды" : "Создать штрихкоды";
		
		public string BarcodesText {
			get {
				OnPropertyChanged(nameof(BarcodesColor));
				return NeedCreateBarcodes ?
					"необходимо создать" : String.Join("\n", SelectOperation.BarcodeOperations.Select(x => x.Barcode.Title));
			}
		}

		public string BarcodesColor => NeedCreateBarcodes ? "red" : null;
		public void ReleaseBarcodes() {
			if(SelectOperation.Id == 0)
				UoW.Save(SelectOperation);
			
			barcodeService.CreateBarcodeEAN13(UoW, new []{SelectOperation});
			UoW.Commit();
			OnPropertyChanged(nameof(SensitiveCreateBarcodes));
			OnPropertyChanged(nameof(ButtonCreateOrRemoveBarcodesTitle));
			OnPropertyChanged(nameof(BarcodesText));
		}

		public void PrintBarcodes() {
			if(SensitiveCreateBarcodes) {
				if(interactive.Question("Количество созданных штрих кодов отличается от необходимого. Обновить штрихкоды?"))
					ReleaseBarcodes();
				else
					return;
			}

			var reportInfo = new ReportInfo {
				Title = "Штрихкоды",
				Identifier = "Barcodes.BarcodeFromEmployeeIssue",
				Parameters = new Dictionary<string, object> {
					{"barcodes", SelectOperation.BarcodeOperations.Select(x => x.Barcode.Id).ToList()}
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}
		#endregion

		#region private

		private bool changingOperation = false;
		void RecalculateDatesOfSelectedOperation() {
			if(EmployeeCardItem is null)
				return;
			if(EmployeeCardItem.Graph is null) 
				issueModel.FillWearReceivedInfo(new[] { SelectOperation.Employee });
			SelectOperation.RecalculateDatesOfIssueOperation( EmployeeCardItem.Graph, baseParameters, interactive);
			AutoWriteoffDate = SelectOperation.AutoWriteoffDate;
		}
		#endregion

		public override bool Save() {
			Validations.Clear();
			Validations.AddRange(Operations.Select(x => new ValidationRequest(x)));
			if(!Validate())
				return false;
			
			foreach(var operation in Operations) {
				foreach(var baracode in operation.BarcodeOperations.Select(x => x.Barcode)) {
					if(!DomainHelper.EqualDomainObjects(baracode.Nomenclature, operation.Nomenclature))
						baracode.Nomenclature = operation.Nomenclature;
					if(!DomainHelper.EqualDomainObjects(baracode.Size, operation.WearSize))
						baracode.Size = operation.WearSize;
					if(!DomainHelper.EqualDomainObjects(baracode.Height, operation.Height))
						baracode.Height = operation.Height;
				}
				UoW.Save(operation);
			}

			UoW.Commit();
			SaveChanged?.Invoke(protectionTools);
			Close(false, CloseSource.Save);
			return true;
		}

		public void AddOnClicked() {
			var startDate = EmployeeCardItem?.NextIssue ?? DateTime.Today;
			var endDate = EmployeeCardItem?.ActiveNormItem?.CalculateExpireDate(startDate);
			var issue = new EmployeeIssueOperation {
				Employee = employee,
				Issued = EmployeeCardItem?.ActiveNormItem?.Amount ?? 1,
				ManualOperation = true,
				NormItem = EmployeeCardItem?.ActiveNormItem,
				ProtectionTools = protectionTools,
				Returned = 0,
				WearPercent = WearPercent,
				OperationTime =  startDate,
				StartOfUse = startDate,
				AutoWriteoffDate = endDate,
				ExpiryByNorm = endDate
			};
			//после создания чтобы не исправилось установкой даты.
			issue.UseAutoWriteoff = baseParameters.DefaultAutoWriteoff;
			
			if(!Operations.Any())
				issue.OverrideBefore = true;
			
			Operations.Add(issue);
			SelectOperation = issue;
		}

		public void DeleteOnClicked(EmployeeIssueOperation deleteOperation) {
			Operations.Remove(deleteOperation);
			UoW.Delete(deleteOperation);
		}
	}
}
