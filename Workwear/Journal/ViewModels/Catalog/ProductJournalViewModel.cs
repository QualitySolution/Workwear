using System.Collections.Generic;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;

namespace workwear.Journal.ViewModels.Tools {
	
	public class ProductJournalViewModel : JournalViewModelBase {
		private IList<Product> catalogProducts;
		
		public ProductJournalViewModel(
			ProductsManagerService productsManagerService,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			catalogProducts = productsManagerService.Products();
			Title = "Каталог продукции";
		}
		
		private IList<Product> GetNodes() {
			return catalogProducts;
		}
	}
}
