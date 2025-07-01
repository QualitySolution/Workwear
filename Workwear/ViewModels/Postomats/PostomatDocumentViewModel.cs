using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Postomats;
using Workwear.Journal.Filter.ViewModels.ClothingService;
using workwear.Journal.ViewModels.ClothingService;
using Workwear.ViewModels.ClothingService;
using Workwear.Tools;
using Workwear.Tools.Features;
using CellLocation = Workwear.Domain.Postomats.CellLocation;

namespace Workwear.ViewModels.Postomats {
	public class PostomatDocumentViewModel : EntityDialogViewModelBase<PostomatDocument>, IDialogDocumentation {
		private readonly PostomatManagerService postomatService;
		private readonly FeaturesService featuresService;
		private readonly IUserService userService;
		private readonly IInteractiveService interactive;

		public PostomatDocumentViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			PostomatManagerService postomatService,
			FeaturesService featuresService,
			IInteractiveService interactive,
			IUserService userService,
			IValidator validator = null, UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.postomatService = postomatService ?? throw new ArgumentNullException(nameof(postomatService));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			Postomats = postomatService.GetPostomatList(PostomatListType.Aso);
			Entity.Postomat = Postomats.FirstOrDefault(x => x.Id == Entity.TerminalId);
			if(Entity.TerminalId > 0) 
			{
				GetPostomatResponse postomatResponse = postomatService.GetPostomat(Entity.TerminalId);
				allCells = postomatResponse.Cells;
				foreach(PostomatDocumentItem item in Entity.Items) {
					var cell = allCells.FirstOrDefault(x => x.Location.Storage == item.LocationStorage
					                                       && x.Location.Shelf == item.LocationShelf
					                                       && x.Location.Cell == item.LocationCell);
					if(cell != null)
						item.Location = new CellLocation(cell.CellTitle, cell.Location);
				}
			}
			
			Entity.Items.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(CanChangePostomat));
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("postomat.html#postamat-refill-document");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		#region Постамат
		public IList<PostomatInfo> Postomats { get; }
		
		private PostomatInfo postomat;
		public virtual PostomatInfo Postomat {
			get => Postomats.FirstOrDefault(x => x.Id == Entity.TerminalId);
			set {
				if(SetField(ref postomat, value)) {
					Entity.TerminalId = value?.Id ?? 0;
					Entity.Postomat = postomat;
					allCells = postomatService.GetPostomat(Entity.TerminalId).Cells;
					OnPropertyChanged(nameof(CanAddItem));
				}
			}
		}

		private IList<CellInfo> allCells = new List<CellInfo>();

		public IEnumerable<CellLocation> AvailableCellsFor(PostomatDocumentItem item) {
			if(!item.Location.IsEmpty)
				yield return new CellLocation(null, 0, 0, 0);
			yield return item.Location;
			foreach(var cell in AvailableCells()) 
				yield return cell;
		}		
		
		public IEnumerable<CellLocation> AvailableCells() {
			foreach(var cell in allCells) {
				if(!cell.IsEmpty && cell.DocumentId != Entity.Id)
					continue; //Пропускаем занятые другими документами ячейки.
				if(!Entity.Items.Any(x => x.LocationStorage == cell.Location.Storage 
				                          && x.LocationShelf == cell.Location.Shelf 
				                          && x.LocationCell == cell.Location.Cell))
					yield return new CellLocation(cell.CellTitle, cell.Location);
			}
		}
		#endregion

		#region Свойства View
		public bool CanEdit => Entity.Status == DocumentStatus.New;
		public bool CanAddItem => Entity.Postomat != null;
		public bool CanChangePostomat => Entity.Items.Count == 0;
		public object CanUseBarcode => featuresService.Available(WorkwearFeature.Barcodes);

		#endregion

		#region Команды View
		public void AddFromScan() {
			UnitOfWorkProvider.UoW.Session.QueryOver<Service>();
			NavigationManager.OpenViewModel<ClothingAddViewModel, PostomatDocumentViewModel>
				(this, this, addingRegistrations: c => c.RegisterInstance(UnitOfWorkProvider));
		}
		public void ReturnFromService() {
			var selectPage = NavigationManager.OpenViewModel<ClaimsJournalViewModel>(this, OpenPageOptions.AsSlave,
				model => model.ExcludeInDocs = true,
				addingRegistrations: builder => {
					builder.RegisterInstance<Action<ClaimsJournalFilterViewModel>>(
						filter => {
							filter.SensitiveShowClosed = false;
							filter.ShowClosed = false;
							filter.Postomat = Postomat;
						});
				});
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectPage.ViewModel.OnSelectResult += ViewModel_OnSelectResult;
		}

		private void ViewModel_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			var serviceClaimsIds = e.GetSelectedObjects<ClaimsJournalNode>().Select(x => x.Id).ToArray();

			UoW.Session.QueryOver<ServiceClaim>()
				.Where(x => x.Id.IsIn(serviceClaimsIds))
				.Fetch(SelectMode.Skip, x => x)
				.Fetch(SelectMode.Fetch, x => x.States)
				.Future();
			
			var serviceClaims = UoW.Session.QueryOver<ServiceClaim>()
				.Where(x => x.Id.IsIn(serviceClaimsIds))
				.Fetch(SelectMode.Fetch, x => x.Barcode)
				.Fetch(SelectMode.Fetch, x => x.Barcode.Nomenclature)
				.List();
			foreach(var claim in serviceClaims) {
				Entity.AddItem(claim, AvailableCells().FirstOrDefault(), userService.GetCurrentUser());
			}
		}

		public void RemoveItem(PostomatDocumentItem item) {
			if(item.Id == 0)
				item.ServiceClaim.States.RemoveAll(s => s.Id == 0);
			else if(interactive.Question("Строка уже была сохранена, при удалении нужно проставить новый статус заявке. Продолжить?"))
				NavigationManager.OpenViewModel<ClothingMoveViewModel, ServiceClaim>(this, item.ServiceClaim);
			else 
				return;
			Entity.Items.Remove(item);
		}
		#endregion

		#region Save and Print
		public override bool Save()
		{
			if(!Validate())
				return false;
			foreach(var item in Entity.Items) {
				UoW.Save(item.ServiceClaim);
			}
			UoW.Save(Entity);
			UoW.Commit();
			return true;
		}
		
		#region Print
		public void Print(PostomatPrintType type) 
		{
			if(!Entity.Items.Any()) 
			{
				interactive.ShowMessage(ImportanceLevel.Warning, "Нет данных для печати. Заполните документ");
				return;
			}
			
			Save();
			switch(type) 
			{
				case PostomatPrintType.Document: PrintDocument(type);
					return;
				case PostomatPrintType.Stickers: PrintStickers(type);
					return;
				default: return;
			}
		}

		private void PrintDocument(PostomatPrintType type) 
		{
			ReportInfo reportInfo = new ReportInfo
			{
				Title = $"Ведомость на загрузку №{Entity.Id} от {Entity.CreateTime:d}",
				Identifier = type.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> 
				{
					{ "id",  Entity.Id },
					{ "postomat_location", Postomat?.Location },
					{ "responsible_person", userService.GetCurrentUser().Name }
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
		
		private void PrintStickers(PostomatPrintType type) 
		{
			ReportInfo reportInfo = new ReportInfo
			{
				Title = $"Этикетки для загрузки №{Entity.Id} от {Entity.CreateTime:d}",
				Identifier = type.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> 
				{
					{ "id",  Entity.Id }
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
		#endregion

		public enum PostomatPrintType 
		{
			[Display(Name = "Ведомость на загрузку")]
			[ReportIdentifier("Documents.PostomatIssueSheet")]
			Document,
			[Display(Name = "Этикетки")]
			[ReportIdentifier("Documents.PostomatIssueStickers")]
			Stickers
		}
		#endregion
	}
}
