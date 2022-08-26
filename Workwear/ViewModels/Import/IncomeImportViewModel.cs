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
			Title = "Загрузка поступлений";
		}

		#region Events

		public event Action DocumentLoaded;
		public event Action DocumentParsed;

		#endregion

		#region services

		private readonly IInteractiveService interactiveService;
		private readonly Xml1CDocumentParser parser;

		#endregion

		#region public Property
		public List<DocumentViewModel> Documents { get; } = new List<DocumentViewModel>();

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

		public bool DocumentDownloadSensitive => Documents.Any(d => d.WantDownload);

		private DocumentViewModel selectDocument;
		[PropertyChangedAlso(nameof(SelectDocumentItems))]
		public DocumentViewModel SelectDocument {
			get => selectDocument;
			private set => SetField(ref selectDocument, value);
		}

		public List<DocumentItemViewModel> SelectDocumentItems => selectDocument.ItemViewModels;

		#endregion

		#region Methods

		public void LoadDocument(string filePatch) {
			if(filePatch.ToLower().EndsWith(".xml")) {
				foreach(var xml1CDocument in parser.ParseDocuments(filePatch))
					Documents.Add(new DocumentViewModel(this) { document = xml1CDocument });
				if(Documents.Any())
					DocumentLoaded?.Invoke();
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
			SelectDocumentVisible = true;

			foreach(var document in Documents) {
				document.ItemViewModels = new List<DocumentItemViewModel>();
				foreach(var documentItem in parser.ParseDocumentItems(document.document))
					document.ItemViewModels.Add(new DocumentItemViewModel { Item = documentItem });
			}

			SelectDocument = Documents.First();
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
		public string Title { get; set; } = "2";
	}
}
