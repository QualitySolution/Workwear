using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using QS.Cloud.Client;
using QS.Cloud.WearLk.Manage;

namespace QS.Cloud.WearLk.Client {
	public class EmailManagerService : WearLkServiceBase 
	{
		public EmailManagerService(ISessionInfoProvider sessionInfoProvider) : base(sessionInfoProvider)
		{
		}
		
		#region Запросы
		public string SendMessages(IEnumerable<EmailMessage> messages, IProgress<int> progress = default, CancellationToken token = default)
		{
			if (!messages.Any())
				throw new ArgumentException("Должно быть передано хотя бы одно сообщение", nameof(messages));
            
			EmailManager.EmailManagerClient client = new EmailManager.EmailManagerClient(Channel);
			using(AsyncClientStreamingCall<SendEmailRequest, SendEmailResponse> call = client.SendEmail(Headers)) 
			{
				int i = 1;
				foreach(EmailMessage message in messages) 
				{
					token.ThrowIfCancellationRequested();
					call.RequestStream.WriteAsync(new SendEmailRequest() { Messages = message }).Wait();
					progress?.Report(i);
					i++;
				}

				call.RequestStream.CompleteAsync().Wait();
				SendEmailResponse response = call.GetAwaiter().GetResult();
				return response.Results;
			}
		}

		public async Task<string> SendMessagesAsync(IEnumerable<EmailMessage> messages, IProgress<int> progress = default, CancellationToken token = default)
		{
			if(!messages.Any())
				throw new ArgumentException("Должно быть передано хотя бы одно сообщение", nameof(messages));

			EmailManager.EmailManagerClient client = new EmailManager.EmailManagerClient(Channel);
			using(AsyncClientStreamingCall<SendEmailRequest, SendEmailResponse> call = client.SendEmail(Headers)) 
			{
				int i = 1;
				foreach(EmailMessage message in messages) 
				{
					token.ThrowIfCancellationRequested();
					await call.RequestStream.WriteAsync(new SendEmailRequest() { Messages = message });
					progress?.Report(i);
					i++;
				}
				
				await call.RequestStream.CompleteAsync();
				SendEmailResponse response = await call;
				return response.Results;
			}
		}
	}
		#endregion 
}

