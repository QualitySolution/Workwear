using System.Collections.Generic;
using QS.Cloud.Client;
using QS.Cloud.Postomat.Manage;

namespace QS.Cloud.Postomat.Client {
	public class PostomatManagerService : CloudClientServiceBase {
		public PostomatManagerService(ISessionInfoProvider sessionInfoProvider)
			: base(sessionInfoProvider, "postomat.cloud.qsolution.ru", 4204) { }
		
		#region Запросы
		public IList<PostomatInfo> GetPostomatList() {
			var client = new PostomatManager.PostomatManagerClient(Channel);
			var request = new GetPostomatListRequest();
			return client.GetPostomatList(request, Headers).Postomats;
		}
		
		public GetPostomatResponse GetPostomat(uint id) {
			var client = new PostomatManager.PostomatManagerClient(Channel);
			var request = new GetPostomatRequest { Id = id };
			return client.GetPostomat(request, Headers);
		}
		#endregion

	}
}
