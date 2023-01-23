using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using QS.Cloud.WearLk.Manage;

namespace QS.Cloud.WearLk.Client 
{
	public class ClaimsManagerService : WearLkServiceBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
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

		public void Send(int claimId, string text) {
			var client = new ClaimManager.ClaimManagerClient(Channel);
			var request = new SendAnswerRequest { ClaimId = claimId, Text = text };
			client.SendAnswer(request, Headers);
		}
		
		public void CloseClaim(int claimId, string text) {
			var client = new ClaimManager.ClaimManagerClient(Channel);
			var request = new CloseClaimRequest { ClaimId = claimId, Text = text ?? String.Empty};
			client.CloseClaim(request, Headers);
		}
		#endregion

		#region Подписка
		public event EventHandler<ReceiveNeedForResponseCountEventArgs> NeedForResponseCountChanged;
		private DateTime? FailSince;

		private CancellationTokenSource cancellation;
		private Task responseReaderTask;
		public void SubscribeNeedForResponseCount() {
			cancellation = new CancellationTokenSource();
			SubscribeNeedForResponseCountConnect(cancellation.Token);
		}
		private void SubscribeNeedForResponseCountConnect(CancellationToken token)
		{
			var client = new ClaimManager.ClaimManagerClient(Channel);

			var request = new NeedForResponseCountRequest();
			var response = client.NeedForResponseCount(request, Headers);
			//var watcher = new NotificationConnectionWatcher(channel, OnChanalStateChanged);

			responseReaderTask = Task.Run(async () =>
				{
					while(await response.ResponseStream.MoveNext(token))
					{
						FailSince = null;
						var message = response.ResponseStream.Current;
						logger.Debug($"Количество необработанных обращений изменилось: " + message.Count);
						OnAppearedMessage(message.Count);
					}
					logger.Warn($"Соединение с QS.Cloud.WearLk.Manage.ClaimManager завершено.");
				}, token).ContinueWith(task =>
				{
					if(task.IsCanceled || (task.Exception?.InnerException as RpcException)?.StatusCode == StatusCode.Cancelled) {
						logger.Info($"Соединение с QS.Cloud.WearLk.Manage.ClaimManager отменено.");
					}
					else if (task.IsFaulted)
					{
						if (FailSince == null) 
							FailSince = DateTime.Now;
						var failedTime = (DateTime.Now - FailSince).Value;
						if(failedTime.Seconds < 10)
							Thread.Sleep(1000);
						else if(failedTime.Minutes < 10)
							Thread.Sleep(4000);
						else
							Thread.Sleep(30000);
						logger.Error(task.Exception);
						logger.Info($"Соединение с QS.Cloud.WearLk.Manage.ClaimManager разорвано... Пробуем соединиться.");
						SubscribeNeedForResponseCountConnect(new CancellationToken());
					} 
				});
		}
		
		protected virtual void OnAppearedMessage(uint count)
		{
			NeedForResponseCountChanged?.Invoke(this, new ReceiveNeedForResponseCountEventArgs{Count = count});
		}
		#endregion

		public override void Dispose() {
			cancellation.Cancel();
			base.Dispose();
		}
	}
	
	public class ReceiveNeedForResponseCountEventArgs : EventArgs
	{
		public uint Count { get; set; }
	}
}
