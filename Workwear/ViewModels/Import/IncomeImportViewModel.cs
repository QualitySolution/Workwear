using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentNHibernate.Data;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.Repository.Stock;
using workwear.Tools.Features;
using workwear.Tools.Import;
using workwear.ViewModels.Stock;

namespace workwear.ViewModels.Import 
{
	public class IncomeImportViewModel : UowDialogViewModelBase 
	{
		public IncomeImportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IInteractiveService interactiveService,
			Xml1CDocumentParser parser,
			StockRepository stockRepository,
			FeaturesService featuresService,
			IUserService userService,
			ILifetimeScope autofacScope) : base(unitOfWorkFactory, navigation) 
		{
			this.interactiveService = interactiveService;
			this.parser = parser;
			SelectFileVisible = true;
			Title = "Загрузка поступлений";
			TargetWarehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, userService.CurrentUserId);
			this.featuresService = featuresService;
			
			var builder = new CommonEEVMBuilderFactory<IncomeImportViewModel>(
				this, this, UoW, NavigationManager, autofacScope);
			
			entryWarehouseViewModel = builder.ForProperty(x => x.TargetWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
		}

		#region Events

		public event Action DocumentLoaded;

		#endregion

		#region services

		private readonly IInteractiveService interactiveService;
		private readonly Xml1CDocumentParser parser;
		private readonly FeaturesService featuresService;

		#endregion

		#region public Property
		public List<DocumentViewModel> DocumentsViewModels { get; } = new List<DocumentViewModel>();

		private bool selectFileVisible;
		public bool SelectFileVisible {
			get => selectFileVisible;
			private set => SetField(ref selectFileVisible, value);
		}
		
		private bool documentHasBeenUploaded;
		public bool DocumentHasBeenUploaded {
			get => documentHasBeenUploaded;
			private set => SetField(ref documentHasBeenUploaded, value);
		}

		public bool DocumentDownloadSensitive => DocumentsViewModels.Any(d => d.WantDownload);

		private DocumentViewModel selectDocument;
		
		public DocumentViewModel SelectDocument {
			get => selectDocument;
			private set {
				if(SetField(ref selectDocument, value)) {
					SelectDocumentItems = selectDocument.ItemViewModels;
				} 
			}
		}

		private List<DocumentItemViewModel> selectDocumentItems;
		public List<DocumentItemViewModel> SelectDocumentItems {
			get => selectDocumentItems;
			set => SetField(ref selectDocumentItems, value);
		}

		private Warehouse targetWarehouse;
		public Warehouse TargetWarehouse {
			get => targetWarehouse;
			set => SetField(ref targetWarehouse, value);
		}

		public bool WarehouseSelectVisible => featuresService.Available(WorkwearFeature.Warehouses);

		#endregion

		#region Methods

		public void LoadDocument(string filePatch) {
			if(filePatch.ToLower().EndsWith(".xml")) {
				parser.SetData(filePatch, UoW);
				foreach(var xml1CDocument in parser.ParseDocuments())
					DocumentsViewModels.Add(new DocumentViewModel(this) { Document = xml1CDocument });
				if(DocumentsViewModels.Any())
					DocumentLoaded?.Invoke();
				else
					interactiveService.ShowMessage(ImportanceLevel.Warning, "В загруженом файле не обнаружены документы поступления");
			}
			else
				interactiveService.ShowMessage(ImportanceLevel.Error, "Формат файла не поддерживается");
		}

		public void ParseDocuments() {
			if(!DocumentsViewModels.Any(x => x.WantDownload))
				interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбран ни один из документов");
			DocumentsViewModels.RemoveAll(x => !x.WantDownload);
			SelectFileVisible = false;
			DocumentHasBeenUploaded = true;

			var useAlternativeSize = interactiveService.Question("Использовать альтернативные значения размеров?");

			foreach(var documentViewModel in DocumentsViewModels) {
				documentViewModel.ItemViewModels = new List<DocumentItemViewModel>();
				foreach(var documentItem in parser.ParseDocumentItems(documentViewModel.Document, useAlternativeSize))
					documentViewModel.ItemViewModels.Add(new DocumentItemViewModel { Item = documentItem });
			}

			SelectDocument = DocumentsViewModels.First();
		}
		
		public void Cancel() => Close(true, CloseSource.Cancel);
		public void WandDownloadPropertyChange() => OnPropertyChanged(nameof(DocumentDownloadSensitive));
		
		public void CreateIncome() {
			foreach(var document in DocumentsViewModels) {
				var income = new Income {
					Warehouse = TargetWarehouse
				};
				foreach(var item in document.ItemViewModels.Select(x => x.Item)) {
					if(item.Nomenclature is null)
						continue;
					income.AddItem(item.Nomenclature, item.Size, item.Height, item.Amount, null, item.Cost);
				}
				income.UpdateOperations(UoW, interactiveService);
				UoW.Save(income);
				UoW.Commit(); 
				(NavigationManager as ITdiCompatibilityNavigation)?.OpenTdiTab<IncomeDocDlg, int>(null, income.Id);
			}
		}

		#endregion

		#region EntryViewModel

		public EntityEntryViewModel<Warehouse> entryWarehouseViewModel;

		#endregion
	}

	public class DocumentViewModel
	{
		private readonly IncomeImportViewModel master;
		public DocumentViewModel(IncomeImportViewModel masterViewModel) => master = masterViewModel;
		public Xml1CDocument Document { get; set; }
		public string Title => Document.DocumentType + " №:" + Document.DocumentNumber;
		public string DocumentType => Document.DocumentType;
		public uint Number => Document.DocumentNumber;
		private bool wantDownload;
		public bool WantDownload {
			get => wantDownload;
			set {
				wantDownload = value;
				master.WandDownloadPropertyChange();
			}
		}

		public List<DocumentItemViewModel> ItemViewModels { get; set; }
	}

	public class DocumentItemViewModel
	{
		public Xml1CDocumentItem Item { get; set; }
		public string Nomenclature => Item.Nomenclature?.Name ?? Item.NomenclatureFromCatalog;
		public string Size => Item.Size?.Name ?? Item.SizeName;
		public string Height => Item.Height?.Name ?? Item.HeightName;
		public int Amount => Item.Amount;
		public decimal Cost => Item.Cost;
		public bool NomenclatureSelected => Item.Nomenclature != null;
		public bool SizeSelected => Item.Size != null;
		public bool HeightSelected => Item.Height != null;
		public string Article => Item.NomenclatureArticle;
		public bool CostIsZero => Item.Cost == 0;
	}
}
