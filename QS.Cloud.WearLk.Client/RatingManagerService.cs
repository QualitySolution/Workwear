using System.Collections.Generic;
using QS.Cloud.WearLk.Manage;

namespace QS.Cloud.WearLk.Client 
{
	public class RatingManagerService : WearLkServiceBase
	{
		public RatingManagerService(string sessionId) : base(sessionId)
		{
		}

		#region Запросы

		public IList<Rating> GetRatings(int nomenclatureId) 
		{
			var client = new RatingManager.RatingManagerClient(Channel);
			var request = new GetRatingsRequest { NomenclatureId = nomenclatureId };
			return client.GetRatings(request, Headers).Ratings;
		}

		#endregion
	}
}
