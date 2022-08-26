using System.Collections.Generic;
using QS.Cloud.WearLk.Manage;

namespace QS.Cloud.WearLk.Client 
{
	public class ClaimsManagerService : WearLkServiceBase
	{
		public ClaimsManagerService(string sessionId) : base(sessionId)
		{
		}

		#region Запросы

		public IList<Claim> GetClaims(uint size, uint offset, bool showClosed) {
			var client = new ClaimManager.ClaimManagerClient(Channel);
			var request = new GetClaimsRequest{PageSize = size, ItemsSkipped = offset, ShowClosed = showClosed};
			return client.GetClaims(request, Headers).Claims;
		}

		public IList<ClaimMessage> GetMessages(int id) {
			var client = new ClaimManager.ClaimManagerClient(Channel);
			var request = new GetClaimRequest { Id = id };
			return client.GetClaim(request, Headers).Messages;
		}

		public void SetChanges(Claim claim) {
			var client = new ClaimManager.ClaimManagerClient(Channel);
			var request = new EditClaimRequest { ClaimId = claim.Id, ClaimState = claim.ClaimState, Title = claim.Title };
			client.EditClaim(request, Headers);
		}

		public void Send(int claimId, string text) {
			var client = new ClaimManager.ClaimManagerClient(Channel);
			var request = new SendAnswerRequest { ClaimId = claimId, Text = text };
			client.SendAnswer(request, Headers);
		}

		#endregion
	}
}
