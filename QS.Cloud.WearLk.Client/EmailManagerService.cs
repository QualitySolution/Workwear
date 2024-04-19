using System;
using System.Collections.Generic;
using System.Linq;
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
		public string SendMessages(IEnumerable<EmailMessage> messages)
		{
			if (!messages.Any())
				throw new ArgumentException("Должно быть передано хотя бы одно сообщение", nameof(messages));
            
			EmailManager.EmailManagerClient client = new EmailManager.EmailManagerClient(Channel);
			using(AsyncClientStreamingCall<SendEmailRequest, SendEmailResponse> call = client.SendEmail(Headers)) 
			{
				foreach(EmailMessage message in messages) 
				{
					call.RequestStream.WriteAsync(new SendEmailRequest() { Messages = message }).ConfigureAwait(false).GetAwaiter().GetResult();
				}

				call.RequestStream.CompleteAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				SendEmailResponse response = call.ConfigureAwait(false).GetAwaiter().GetResult();
				return response.Results;
			}
		}

		public async Task<string> SendMessagesAsync(IEnumerable<EmailMessage> messages)
		{
			if(!messages.Any())
				throw new ArgumentException("Должно быть передано хотя бы одно сообщение", nameof(messages));

			EmailManager.EmailManagerClient client = new EmailManager.EmailManagerClient(Channel);
			using(AsyncClientStreamingCall<SendEmailRequest, SendEmailResponse> call = client.SendEmail(Headers)) 
			{
				foreach(EmailMessage message in messages) 
				{
					await call.RequestStream.WriteAsync(new SendEmailRequest() { Messages = message }).ConfigureAwait(false);
				}
				
				await call.RequestStream.CompleteAsync().ConfigureAwait(false);
				SendEmailResponse response = await call.ConfigureAwait(false);
				return response.Results;
			}
		}
	}
		#endregion 
}

