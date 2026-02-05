using System.Collections.Generic;
using System.Threading;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;

namespace Workwear.Journal.ViewModels.Catalog {
	
	public class ProductJournalViewModel : JournalViewModelBase {
		private readonly ProductsManagerService productsManagerService;
		
		public ProductJournalViewModel(
			ProductsManagerService productsManagerService,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			this.productsManagerService = productsManagerService;
			Title = "Каталог продукции";
			SearchEnabled = false;
			
			DataLoader = new AnyDataLoader<Product>(GetNodes);
		}
		
		private IList<Product> GetNodes(CancellationToken token) {
			return productsManagerService.Products();
		}
	}
}
