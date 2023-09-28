using System;
using System.Collections.Generic;
using System.Linq;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Postomats;
using CellLocation = Workwear.Domain.Postomats.CellLocation;

namespace Workwear.ViewModels.Postomats {
	public class PostomatDocumentViewModel : EntityDialogViewModelBase<PostomatDocument> {
		private readonly PostomatManagerService postomatService;

		public PostomatDocumentViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			PostomatManagerService postomatService,
			IValidator validator = null, UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.postomatService = postomatService ?? throw new ArgumentNullException(nameof(postomatService));
			Postomats = postomatService.GetPostomatList();
			if(Entity.TerminalId > 0)
				AllCells = postomatService.GetPostomat(Entity.TerminalId).Cells;
		}

		#region Постомат
		public IList<PostomatInfo> Postomats { get; }
		
		private PostomatInfo postomat;
		public virtual PostomatInfo Postomat {
			get => Postomats.FirstOrDefault(x => x.Id == Entity.TerminalId);
			set {
				if(SetField(ref postomat, value)) {
					Entity.TerminalId = value?.Id ?? 0;
					AllCells = postomatService.GetPostomat(Entity.TerminalId).Cells;
				}
			}
		}

		private IList<CellInfo> AllCells = new List<CellInfo>();

		public IEnumerable<CellLocation> AvailableCellsFor(PostomatDocumentItem item) {
			if(!item.Location.IsEmpty)
				yield return new CellLocation(0,0, 0);
			yield return item.Location;
			foreach(var cell in AllCells) {
				if(!Entity.Items.Any(x => x.LocationStorage == cell.Location.Storage 
				                          && x.LocationShelf == cell.Location.Shelf 
				                          && x.LocationCell == cell.Location.Cell))
					yield return new CellLocation(cell.Location);
			}
		}
		#endregion

		#region Команды View

		public void RemoveItem(PostomatDocumentItem item) {
			Entity.Items.Remove(item);
		}

		#endregion
	}
}
