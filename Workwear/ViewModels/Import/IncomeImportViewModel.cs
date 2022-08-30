using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Measurements;
using workwear.Models.Import;
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
			ILifetimeScope autofacScope,
			SizeService sizeService,
			IProgressBarDisplayable progressBar) : base(unitOfWorkFactory, navigation) 
		{
			this.interactiveService = interactiveService;
			this.parser = parser;
			SelectFileVisible = true;
			Title = "Загрузка поступлений";
			Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, userService.CurrentUserId);
			this.featuresService = featuresService;
			this.sizeService = sizeService;
			this.progressBar = progressBar;
			
			var builder = new CommonEEVMBuilderFactory<IncomeImportViewModel>(
				this, this, UoW, NavigationManager, autofacScope);
			EntryWarehouseViewModel = builder.ForProperty(x => x.Warehouse)
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
		private readonly SizeService sizeService;
		private readonly IProgressBarDisplayable progressBar;

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

		private DocumentViewModel selectDocumentViewModel;
		public DocumentViewModel SelectDocumentViewModel {
			get => selectDocumentViewModel;
			private set {
				if(SetField(ref selectDocumentViewModel, value)) {
					SelectDocumentItemViewModels = selectDocumentViewModel.ItemViewModels;
				} 
			}
		}

		private List<DocumentItemViewModel> selectDocumentItemViewModels;
		public List<DocumentItemViewModel> SelectDocumentItemViewModels {
			get => selectDocumentItemViewModels;
			private set => SetField(ref selectDocumentItemViewModels, value);
		}

		private Warehouse warehouse;

		private Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}
		public bool WarehouseSelectVisible => featuresService.Available(WorkwearFeature.Warehouses);

		#endregion

		#region Methods

		public void LoadDocument(string filePatch) {
			if(filePatch.ToLower().EndsWith(".xml")) {
				parser.SetData(filePatch, UoW);
				foreach(var xml1CDocument in parser.ParseDocuments()) {
					if(DocumentsViewModels.Any(x => x.Document.DocumentNumber == xml1CDocument.DocumentNumber)) {
						interactiveService.ShowMessage(
							ImportanceLevel.Info,
							$"Документ с номером {xml1CDocument.DocumentNumber} уже загружен");
						continue;
					}
					DocumentsViewModels.Add(new DocumentViewModel(this) { Document = xml1CDocument });
				}
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
			var count = parser.GetDocumentItemsCount(DocumentsViewModels.Select(x => x.Document));
			
			progressBar.Start(count,0, $"Загружаем {(DocumentsViewModels.Count == 1 ? "документ" : "документы")}");
			foreach(var documentViewModel in DocumentsViewModels) {
				documentViewModel.ItemViewModels = new List<DocumentItemViewModel>();
				foreach(var documentItem in parser.ParseDocumentItems(documentViewModel.Document, useAlternativeSize)) {
					progressBar.Add();
					documentViewModel.ItemViewModels.Add(new DocumentItemViewModel { Item = documentItem });
				}
			}
			progressBar.Close();
			SelectDocumentViewModel = DocumentsViewModels.First();
		}
		
		public void Cancel() => Close(true, CloseSource.Cancel);
		public void WandDownloadPropertyChange() => OnPropertyChanged(nameof(DocumentDownloadSensitive));
		
		public void CreateIncome() {
			progressBar.Start(DocumentsViewModels.Count, 0, "Создаём поступление");
			foreach(var document in DocumentsViewModels) {
				progressBar.Add();
				var page = (NavigationManager as ITdiCompatibilityNavigation)?.OpenTdiTab<IncomeDocDlg>(null, OpenPageOptions.IgnoreHash);
				var income = (page.TdiTab as IncomeDocDlg).Entity;
				income.Operation = IncomeOperations.Enter;
				income.Warehouse = Warehouse;
				income.Comment = document.Title;
				income.CreationDate = DateTime.Today;
				income.Date = document.Date ?? DateTime.Today;
				foreach(var item in document.ItemViewModels.Select(x => x.Item)) {
					if(item.Nomenclature is null)
						continue;
					income.AddItem(item.Nomenclature, item.Size, item.Height, item.Amount, null, item.Cost);
				}
			}
			progressBar.Close();
		}
		
		public void CreateNomenclature() {
			var openNomenclatureDialog = false;
				var nomenclatureTypes = new NomenclatureTypes(UoW, sizeService, true);
				var documentItemsWithoutNomenclature = DocumentsViewModels
					.SelectMany(d => d.ItemViewModels.Where(i => i.NomenclatureWarning))
					.GroupBy(x => x.Nomenclature)
					.Select(g => g.First())
					.ToList();
				progressBar.Start(documentItemsWithoutNomenclature.Count, 0,"Создаём номенклатуру");
				foreach(var notFoundNomenclature in documentItemsWithoutNomenclature) {
					progressBar.Add();
					var type = nomenclatureTypes.ParseNomenclatureName(notFoundNomenclature.Nomenclature);
					if(UInt32.TryParse(notFoundNomenclature.Article, out var result)) {
						if(type is null) {
							var page = NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(null,
								EntityUoWBuilder.ForCreate());
							page.ViewModel.Entity.Name = notFoundNomenclature.Nomenclature;
							page.ViewModel.Entity.Number = result;
							openNomenclatureDialog = true;
						}
						else {
							if(type.Id == 0)
									UoW.Save(type);
							var nomenclature = new Nomenclature {
								Name = notFoundNomenclature.Nomenclature,
								Number = result,
								Type = type,
								Comment = "Созданно при загрузке поступления из файла"
							};
							UoW.Save(nomenclature);
						}
					}
				}
				progressBar.Close();

				interactiveService.ShowMessage(ImportanceLevel.Info,
					openNomenclatureDialog
						? "Сохраните номенклатуру(ы) и повторите загрузку документа."
						: "Созданы новые номенклатуры, повторите загрузку документа.", "Загрузка документа");
				UoW.Commit();
				Close(false, CloseSource.Self);
		}

		public void SelectAll() {
			if(DocumentsViewModels.Any() && DocumentsViewModels.Select(x => x.WantDownload).First())
				foreach(var document in DocumentsViewModels)
					document.WantDownload = false;
			else
				foreach(var document in DocumentsViewModels)
					document.WantDownload = true;
		}

		#endregion

		#region EntryViewModel

		public readonly EntityEntryViewModel<Warehouse> EntryWarehouseViewModel;

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
		public DateTime? Date => Document.Date;
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
		public bool NomenclatureWarning => Item.Nomenclature == null;
		public bool SizeWarning => Nomenclature != null && Item.Size == null;
		public bool HeightWarning => Nomenclature != null && Item.Height == null;
		public string Article => Item.NomenclatureArticle;
		public bool CostWarning => Item.Cost == 0;
	}
}
