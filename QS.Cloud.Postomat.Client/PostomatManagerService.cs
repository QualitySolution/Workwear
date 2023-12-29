using System.Collections.Generic;
using System.Threading;
using QS.Cloud.Client;
using QS.Cloud.Postomat.Manage;

namespace QS.Cloud.Postomat.Client {
	public class PostomatManagerService : CloudClientServiceBase {
		public PostomatManagerService(ISessionInfoProvider sessionInfoProvider)
			: base(sessionInfoProvider, "postomat.cloud.qsolution.ru", 4204) { }
		
		#region Запросы
		public IList<PostomatInfo> GetPostomatList(PostomatListType listType) {
			var client = new PostomatManager.PostomatManagerClient(Channel);
			var request = new GetPostomatListRequest();
			request.ListType = listType;
			return client.GetPostomatList(request, Headers).Postomats;
		}
		
		public IList<FullnessInfo> GetFullness(CancellationToken token) {
			var client = new PostomatManager.PostomatManagerClient(Channel);
			var request = new GetFullnessRequest();
			return client.GetFullness(request, Headers, cancellationToken: token).Fullness;
		}
		
		public GetPostomatResponse GetPostomat(uint id) {
			var client = new PostomatManager.PostomatManagerClient(Channel);
			var request = new GetPostomatRequest { Id = id };
			return client.GetPostomat(request, Headers);
		}
		#endregion

	}
}
