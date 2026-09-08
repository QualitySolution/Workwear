using QS.BaseParameters;
using QS.Cloud.Client;
using QS.Cloud.WorkwearDictionary.Grpc.Contracts;

namespace QS.Cloud.WorkwearDictionary.Client {
	public class EtnDictionaryService : CloudClientServiceBase {
		private const string SerialNumberHeader = "x-application-serial-number";

		public EtnDictionaryService(ISessionInfoProvider sessionInfoProvider, ParametersService parametersService)
			: base(sessionInfoProvider, "dictionary.wear.cloud.qsolution.ru", 443) {
			if(parametersService.All.TryGetValue("serial_number", out var serialNumber) && !string.IsNullOrWhiteSpace(serialNumber))
				Headers.Add(SerialNumberHeader, serialNumber);
		}

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
