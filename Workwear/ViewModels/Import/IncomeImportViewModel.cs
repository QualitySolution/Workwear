using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
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
		}

		#region Events

		public event Action documentLoaded;
		public event Action documentParsed;

		#endregion

		#region Private fields

		private readonly IInteractiveService interactiveService;
		private readonly Xml1CDocumentParser parser;

		#endregion

		#region public Property

		private List<DocumentViewModel> documents = new List<DocumentViewModel>();
		public List<DocumentViewModel> Documents {
			get => documents;
			private set {
				if(SetField(ref documents, value)) 
					OnPropertyChanged(nameof(DocumentListVisible));
			}
		}
		
		private bool selectFileVisible;
		public bool SelectFileVisible {
			get => selectFileVisible;
			set => SetField(ref selectFileVisible, value);
		}
		
		public bool DocumentListVisible => Documents.Any();

		private DocumentViewModel selectDocument;
		public DocumentViewModel SelectDocument {
			get => selectDocument;
			set {
				if(SetField(ref selectDocument, value));
					OnPropertyChanged(nameof(SelectDocumentItems));
			}
		}

		public List<DocumentItemViewModel> SelectDocumentItems => selectDocument.ItemViewModels;

		#endregion

		#region Methods

		public void LoadDocument(string filePatch) {
			if(filePatch.ToLower().EndsWith(".xml")) {
				foreach(var xml1CDocument in parser.ParseDocuments(filePatch))
					Documents.Add(new DocumentViewModel { document = xml1CDocument });
				if(Documents.Any())
					documentLoaded?.Invoke();
				else
					interactiveService.ShowMessage(ImportanceLevel.Warning, "В загруженом файле не обнаружены документы поступления");
			}
			else
				interactiveService.ShowMessage(ImportanceLevel.Error, "Формат файла не поддерживается");
		}

		public void ParseDocuments() {
			if(!Documents.Any(x => x.WantDownload))
				interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбран ни один из документов");
			Documents.RemoveAll(x => !x.WantDownload);
			SelectFileVisible = false;

			foreach(var document in Documents) {
				document.ItemViewModels = new List<DocumentItemViewModel>();
				foreach(var documentItem in parser.ParseDocumentItems(document.document))
					document.ItemViewModels.Add(new DocumentItemViewModel { Item = documentItem });
			}

			SelectDocument = Documents.First();
		}

		#endregion
	}

	public class DocumentViewModel : PropertyChangedBase
	{
		public Xml1CDocument document { get; set; }
		public string Title => document.DocumentType;
		public uint Number => document.DocumentNumber;
		private bool wantDownload;
		public bool WantDownload {
			get => wantDownload;
			set => SetField(ref wantDownload, value);
		}
		
		public List<DocumentItemViewModel> ItemViewModels { get; set; }
	}

	public class DocumentItemViewModel : PropertyChangedBase 
	{
		public Xml1CDocumentItem Item { get; set; }
		public string Title { get; set; } = "2";
	}
}
