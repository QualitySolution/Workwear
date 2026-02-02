using System.Collections.Generic;
using System.Linq;
using QS.Cloud.Client;
using QS.Cloud.WearLk.Manage;


namespace QS.Cloud.WearLk.Client {
	public class ProductsManagerService : WearLkServiceBase {
		
		public ProductsManagerService(ISessionInfoProvider sessionInfoProvider) : base(sessionInfoProvider)
		{
		}
		
		#region Запросы
		
		public IList<Product> Products()
		{
			var client = new ProductsManager.ProductsManagerClient(Channel);
			var request = new ProductsRequest();
			return client.GetProducts(request, Headers).Products.ToList();
		}
		
		#endregion
		
		
	}
}
