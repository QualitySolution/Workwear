using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using workwear.Tools.Import;

namespace workwear.ViewModels.Import 
{
	public class IncomeImportViewModel : UowDialogViewModelBase 
	{
		public IncomeImportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IInteractiveService interactiveService,
			Xml1CDocumentParser parser) : base(unitOfWorkFactory, navigation) 
		{
			this.interactiveService = interactiveService;
			this.parser = parser;
			parser.UnitOfWork = UoW;
			SelectFileVisible = true;
			Title = "Загрузка поступлений";
		}

		#region Events

		public event Action DocumentLoaded;

		#endregion

		#region services

		private readonly IInteractiveService interactiveService;
		private readonly Xml1CDocumentParser parser;

		#endregion

		#region public Property
		public List<DocumentViewModel> DocumentsViewModels { get; } = new List<DocumentViewModel>();

		private bool selectFileVisible;
		public bool SelectFileVisible {
			get => selectFileVisible;
			private set => SetField(ref selectFileVisible, value);
		}
		
		private bool selectDocumentVisible;
		public bool SelectDocumentVisible {
			get => selectDocumentVisible;
			private set => SetField(ref selectDocumentVisible, value);
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

		#endregion

		#region Methods

		public void LoadDocument(string filePatch) {
			if(filePatch.ToLower().EndsWith(".xml")) {
				parser.SetDocument(filePatch);
				foreach(var xml1CDocument in parser.ParseDocuments())
					DocumentsViewModels.Add(new DocumentViewModel(this) { document = xml1CDocument });
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
			SelectDocumentVisible = true;

			foreach(var documentViewModel in DocumentsViewModels) {
				documentViewModel.ItemViewModels = new List<DocumentItemViewModel>();
				foreach(var documentItem in parser.ParseDocumentItems(documentViewModel.document))
					documentViewModel.ItemViewModels.Add(new DocumentItemViewModel { Item = documentItem });
			}

			SelectDocument = DocumentsViewModels.First();
		}
		
		public void Cancel() => Close(true, CloseSource.Cancel);
		public void WandDownloadPropertyChange() => OnPropertyChanged(nameof(DocumentDownloadSensitive));

		#endregion
	}

	public class DocumentViewModel
	{
		private readonly IncomeImportViewModel master;
		public DocumentViewModel(IncomeImportViewModel masterViewModel) => master = masterViewModel;
		public Xml1CDocument document { get; set; }
		public string Title => document.DocumentType + " №:" + document.DocumentNumber;
		public string DocumentType => document.DocumentType;
		public uint Number => document.DocumentNumber;
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
		public string Nomenclature => Item?.Nomenclature.Name ?? Item?.NomenclatureFromCatalog;
		public string Size => Item?.Size.Name ?? Item?.CharacteristicFromCatalog;
		public string Height => Item?.Height.Name ?? Item?.CharacteristicFromCatalog;
		public int Amount => Item.Amount;
		public decimal Cost => Item.Cost;
	}
}
