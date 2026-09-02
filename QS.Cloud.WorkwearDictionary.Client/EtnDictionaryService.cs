using QS.Cloud.Client;
using QS.Cloud.WorkwearDictionary.Grpc.Contracts;

namespace QS.Cloud.WorkwearDictionary.Client {
	public class EtnDictionaryService : CloudClientServiceBase {
		//TODO Уточнить адрес и порт сервиса ЕТН, когда он будет задеплоен.
		public EtnDictionaryService(ISessionInfoProvider sessionInfoProvider)
			: base(sessionInfoProvider, "cloud.qsolution.ru", 0000) { }

		#region Запросы
		public GetNormsListResponse GetNormsList(int page, int pageSize, string searchQuery = null) {
			var client = new ETNService.ETNServiceClient(Channel);
			var request = new GetNormsListRequest {
				Page = page,
				PageSize = pageSize,
				SearchQuery = searchQuery ?? string.Empty
			};
			return client.GetNormsList(request, Headers);
		}

		public GetNormResponse GetNormItems(int normId) {
			var client = new ETNService.ETNServiceClient(Channel);
			var request = new GetNormRequest { Id = normId };
			return client.GetNormItems(request, Headers);
		}
		#endregion
	}
}
