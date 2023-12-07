using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Postomats;
using workwear.Journal.ViewModels.ClothingService;
using CellLocation = Workwear.Domain.Postomats.CellLocation;

namespace Workwear.ViewModels.Postomats {
	public class PostomatDocumentViewModel : EntityDialogViewModelBase<PostomatDocument> {
		private readonly PostomatManagerService postomatService;
		private readonly IUserService userService;

		public PostomatDocumentViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			PostomatManagerService postomatService,
			ILifetimeScope autofacScope,
			IUserService userService,
			IValidator validator = null, UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.postomatService = postomatService ?? throw new ArgumentNullException(nameof(postomatService));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			Postomats = postomatService.GetPostomatList();
			if(Entity.TerminalId > 0)
				AllCells = postomatService.GetPostomat(Entity.TerminalId).Cells;
			
			var entryBuilder = new CommonEEVMBuilderFactory<PostomatDocument>(this, Entity, UoW, navigation, autofacScope);
	
			// if(Entity.Warehouse == null)
			// 	Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			//
			// WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse).MakeByType().Finish();
			Entity.Items.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(CanChangePostomat));
		}

		#region Постомат
		public IList<PostomatInfo> Postomats { get; }
		
		private PostomatInfo postomat;
		public virtual PostomatInfo Postomat {
			get => Postomats.FirstOrDefault(x => x.Id == Entity.TerminalId);
			set {
				if(SetField(ref postomat, value)) {
					Entity.TerminalId = value?.Id ?? 0;
					Entity.Postomat = postomat;
					AllCells = postomatService.GetPostomat(Entity.TerminalId).Cells;
				}
			}
		}

		private IList<CellInfo> AllCells = new List<CellInfo>();

		public IEnumerable<CellLocation> AvailableCellsFor(PostomatDocumentItem item) {
			if(!item.Location.IsEmpty)
				yield return new CellLocation(0,0, 0);
			yield return item.Location;
			foreach(var cell in AvailableCells()) 
				yield return cell;
		}
		
		public bool CanChangePostomat => Entity.Items.Count == 0;
		
		public IEnumerable<CellLocation> AvailableCells() {
			foreach(var cell in AllCells) {
				if(!Entity.Items.Any(x => x.LocationStorage == cell.Location.Storage 
				                          && x.LocationShelf == cell.Location.Shelf 
				                          && x.LocationCell == cell.Location.Cell))
					yield return new CellLocation(cell.Location);
			}
		}
		#endregion

		#region Команды View
		public void ReturnFromService() {
			var selectPage = NavigationManager.OpenViewModel<ClaimsJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectPage.ViewModel.Filter.SensitiveShowClosed = false;
			selectPage.ViewModel.Filter.ShowClosed = false;
			selectPage.ViewModel.OnSelectResult += ViewModel_OnSelectResult;
		}

		private void ViewModel_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			var serviceClaimsIds = e.GetSelectedObjects<ClaimsJournalNode>().Select(x => x.Id).ToArray();
			var serviceClaims = UoW.Session.QueryOver<ServiceClaim>()
				.Where(x => x.Id.IsIn(serviceClaimsIds))
				.Fetch(SelectMode.Fetch, x => x.Barcode)
				.Fetch(SelectMode.Fetch, x => x.Barcode.Nomenclature)
				.Fetch(SelectMode.Fetch, x => x.States)
				.List();
			foreach(var claim in serviceClaims) {
				Entity.AddItem(claim, AvailableCells().FirstOrDefault(), userService.GetCurrentUser());
			}
		}

		public void RemoveItem(PostomatDocumentItem item) {
			Entity.Items.Remove(item);
		}
		#endregion
		
		public override bool Save() {
			if(!Validate())
				return false;
			foreach(var item in Entity.Items) {
				UoW.Save(item.ServiceClaim);
			}
			UoW.Save(Entity);
			UoW.Commit();
			return true;
		}
	}
}
