using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Measurements;
using Workwear.Repository.Operations;
using Workwear.Tools.Barcodes;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Operations 
{
	public class ManualEmployeeIssueOperationsViewModel : UowDialogViewModelBase 
	{
		private readonly SizeService sizeService;
		private readonly BarcodeService barcodeService;
		private readonly IInteractiveQuestion interactive;
		private readonly ProtectionTools protectionTools;
		public ManualEmployeeIssueOperationsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			EmployeeIssueRepository repository,
			SizeService sizeService,
			BarcodeService barcodeService,
			ILifetimeScope autofacScope,
			IInteractiveQuestion interactive,
			EmployeeCardItem cardItem = null,
			EmployeeIssueOperation selectOperation = null,
			IValidator validator = null) : base(unitOfWorkFactory, navigation, validator, "Редактирование ручных операций") 
		{
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			if(cardItem != null) {
				Title = cardItem.ProtectionTools.Name;
				protectionTools = cardItem.ProtectionTools;
				EmployeeCardItem = UoW.GetById<EmployeeCardItem>(cardItem.Id);
				Operations = new GenericObservableList<EmployeeIssueOperation>(
					repository.GetAllManualIssue(UoW, EmployeeCardItem.EmployeeCard, EmployeeCardItem.ProtectionTools)
						.OrderBy(x => x.OperationTime)
						.ToList());
			}
			else if(selectOperation != null) {
				Title = selectOperation.ProtectionTools?.Name;
				protectionTools = selectOperation.ProtectionTools;
				Operations = new GenericObservableList<EmployeeIssueOperation>(
					repository.GetAllManualIssue(UoW, selectOperation.Employee, selectOperation.ProtectionTools)
						.OrderBy(x => x.OperationTime)
						.ToList());
			}
			else
				throw new ArgumentNullException(nameof(selectOperation) + nameof(cardItem));
			
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
				? Operations.First(x => x.Id == selectOperation.Id) 
				: Operations.FirstOrDefault();
		}

		#region PublicProperty

		private GenericObservableList<EmployeeIssueOperation> operations;
		public GenericObservableList<EmployeeIssueOperation> Operations {
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
		[PropertyChangedAlso(nameof(CanEditOperation))]
		[PropertyChangedAlso(nameof(Nomenclature))]
		public EmployeeIssueOperation SelectOperation {
			get => selectOperation;
			set {
				if(SetField(ref selectOperation, value)) {
					NomenclatureEntryViewModel.IsEditable = SelectOperation != null;
					if(value != null) {
						DateTime = value.OperationTime;
						Issued = value.Issued;
						OverrideBefore = value.OverrideBefore;
					}
					else
						Issued = 0;
					OnPropertyChanged(nameof(VisibleBarcodes));
				}
			}
		}
		
		public EntityEntryViewModel<Nomenclature> NomenclatureEntryViewModel { get; private set; } 
		#endregion
		#region Проброс свойст операции
		
		private DateTime dateTime;
		public DateTime DateTime {
			get => dateTime;
			set {
				if(SetField(ref dateTime, value) && SelectOperation.StartOfUse != value) {
					RecalculateDatesOfSelectedOperation();
				}
			}
		}

		private int issued;
		public int Issued {
			get => issued;
			set {
				if(SetField(ref issued, value)) {
					
					if(SelectOperation != null) {
						SelectOperation.Issued = value;
						RecalculateDatesOfSelectedOperation();
					}
					OnPropertyChanged(nameof(SensitiveCreateBarcodes));
					OnPropertyChanged(nameof(SensitiveBarcodesPrint));
				}
			}
		}

		public Nomenclature Nomenclature {
			get => SelectOperation?.Nomenclature;
			set {
				SelectOperation.Nomenclature = value;
				if(Size != null && !Size.SizeType.IsSame(Nomenclature.Type.SizeType))
					Size = null;
				if(Height != null && !Height.SizeType.IsSame(Nomenclature.Type.HeightType))
					Height = null;
				
				OnPropertyChanged();
				OnPropertyChanged(nameof(VisibleHeight));
				OnPropertyChanged(nameof(VisibleSize));
				OnPropertyChanged(nameof(VisibleBarcodes));
				OnPropertyChanged(nameof(Sizes));
				OnPropertyChanged(nameof(Heights));
				OnPropertyChanged(nameof(BarcodesText));
				OnPropertyChanged(nameof(BarcodesColor));
			}
		}

		public Size Size {
			get => SelectOperation?.WearSize;
			set {
				SelectOperation.WearSize = value;
				OnPropertyChanged();
			}
		}

		public Size Height {
			get => SelectOperation?.Height;
			set {
				SelectOperation.Height = value;
				OnPropertyChanged();
			}
		}

		private bool overrideBefore;
		public bool OverrideBefore {
			get => overrideBefore;
			set {
				if(SetField(ref overrideBefore, value))
					if(SelectOperation != null)
						SelectOperation.OverrideBefore = value;
			}
		}
		
		#endregion

		#region Заполняемые значения

		public IEnumerable<Size> Sizes => sizeService.GetSize(UoW, Nomenclature?.Type.SizeType, onlyUseInNomenclature: true);
		public IEnumerable<Size> Heights => sizeService.GetSize(UoW, Nomenclature?.Type.HeightType, onlyUseInNomenclature: true);

		#endregion
		
		#region Sensintive and Visibility
		public bool VisibleHeight => Nomenclature?.Type.HeightType != null;
		public bool VisibleSize => Nomenclature?.Type.SizeType != null;
		public bool VisibleBarcodes => (Nomenclature?.UseBarcode ?? false) || (SelectOperation?.BarcodeOperations.Any() ?? false);
		public bool CanEditOperation => SelectOperation != null;
		public bool CanAddOperation => EmployeeCardItem != null;
		public bool SensitiveCreateBarcodes => (Nomenclature?.UseBarcode ?? false) && (SelectOperation?.BarcodeOperations.Count ?? 0) != Issued;
		public bool SensitiveBarcodesPrint => Issued > 0 && ((Nomenclature?.UseBarcode ?? false) || (SelectOperation?.BarcodeOperations.Count ?? 0) > 0);

		public event Action<ProtectionTools> SaveChanged;

		#endregion
		
		#region Штрих коды

		private bool NeedCreateBarcodes => (SelectOperation?.BarcodeOperations.Count ?? 0) == 0;
		public string ButtonCreateOrRemoveBarcodesTitle => 
			(Nomenclature?.UseBarcode ?? false) && (SelectOperation?.BarcodeOperations.Count ?? 0) > Issued
				? "Обновить штрихкоды" : "Создать штрихкоды";
		public string BarcodesText => NeedCreateBarcodes ? "необходимо создать" 
			: String.Join("\n", SelectOperation.BarcodeOperations.Select(x => x.Barcode.Title));

		public string BarcodesColor => NeedCreateBarcodes ? "red" : null;
		public void ReleaseBarcodes() {
			if(SelectOperation.Id == 0)
				UoW.Save(SelectOperation);
			
			barcodeService.CreateOrRemove(UoW, new []{SelectOperation});
			UoW.Commit();
			OnPropertyChanged(nameof(SensitiveCreateBarcodes));
			OnPropertyChanged(nameof(ButtonCreateOrRemoveBarcodesTitle));
			OnPropertyChanged(nameof(BarcodesText));
			OnPropertyChanged(nameof(BarcodesColor));
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
					{"operations", new int[] {SelectOperation.Id}}
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}
		#endregion

		#region private
		void RecalculateDatesOfSelectedOperation() {
			SelectOperation.OperationTime = DateTime;
			SelectOperation.StartOfUse = DateTime;
			SelectOperation.ExpiryByNorm = SelectOperation.NormItem?.CalculateExpireDate(DateTime, SelectOperation.Issued);
			if(SelectOperation.UseAutoWriteoff)
				SelectOperation.AutoWriteoffDate = SelectOperation.ExpiryByNorm;
		}

		#endregion

		public override bool Save() {
			Validations.Clear();
			Validations.AddRange(Operations.Select(x => new ValidationRequest(x)));
			if(!Validate())
				return false;
			foreach(var operation in Operations)
				UoW.Save(operation);
			UoW.Commit();
			SaveChanged?.Invoke(protectionTools);
			Close(false, CloseSource.Save);
			return true;
		}

		public void AddOnClicked() {
			if(EmployeeCardItem == null)
				throw new ArgumentNullException(nameof(EmployeeCardItem));
			var startDate = EmployeeCardItem.NextIssue ?? DateTime.Today;
			var endDate = EmployeeCardItem.ActiveNormItem?.CalculateExpireDate(startDate);
			var issue = new EmployeeIssueOperation {
				Employee = EmployeeCardItem.EmployeeCard,
				Issued = EmployeeCardItem.ActiveNormItem?.Amount ?? 1,
				ManualOperation = true,
				NormItem = EmployeeCardItem.ActiveNormItem,
				ProtectionTools = EmployeeCardItem.ProtectionTools,
				Returned = 0,
				WearPercent = 0m,
				UseAutoWriteoff = true,
				OperationTime =  startDate,
				StartOfUse = startDate,
				AutoWriteoffDate = endDate,
				ExpiryByNorm = endDate
			};
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
