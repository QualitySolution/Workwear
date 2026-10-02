using System.Collections.Generic;
using QS.BaseParameters;
using QS.Cloud.Client;
using QS.Cloud.WorkwearDictionary;

namespace QS.Cloud.WorkwearDictionary.Client {
	public class EtnDictionaryService : CloudClientServiceBase {
		private const string SerialNumberHeader = "x-application-serial-number";

		public EtnDictionaryService(ISessionInfoProvider sessionInfoProvider, ParametersService parametersService)
			: base(sessionInfoProvider, "dictionary.wear.cloud.qsolution.ru", 443) {
			if(parametersService.All.TryGetValue("serial_number", out var serialNumber) && !string.IsNullOrWhiteSpace(serialNumber))
				Headers.Add(SerialNumberHeader, serialNumber);
		}

		#region Запросы
		public GetNormsListResponse GetNormsList(int page, int pageSize, string searchQuery = null,
			App app = App.Posts, int parentId = 0, bool recursive = false) {
			var client = new ETNService.ETNServiceClient(Channel);
			var request = new GetNormsListRequest {
				Page = page,
				PageSize = pageSize,
				SearchQuery = searchQuery ?? string.Empty,
				App = app,
				ParentId = parentId,
				Recursive = recursive
			};
			return client.GetNormsList(request, Headers);
		}

		public GetNormResponse GetNormItems(int normId) => GetNormItems(new[] { normId });

		public GetNormResponse GetNormItems(IEnumerable<int> normIds, bool includeChildren = false,
			bool includeAdditional = false) {
			var client = new ETNService.ETNServiceClient(Channel);
			var request = new GetNormRequest {
				IncludeChildren = includeChildren,
				IncludeAdditional = includeAdditional
			};
			request.NormIds.AddRange(normIds);
			return client.GetNormItems(request, Headers);
		}
		#endregion
	}
}
