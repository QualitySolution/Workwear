using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
			SendEmailRequest request = new SendEmailRequest();
			request.Messages.Add(messages);
			return client.SendEmail(request, Headers).Results;
		}
        
		public async Task<string> SendMessagesAsync(IEnumerable<EmailMessage> messages)
		{
			if (!messages.Any())
				throw new ArgumentException("Должно быть передано хотя бы одно сообщение", nameof(messages));
            
			EmailManager.EmailManagerClient client = new EmailManager.EmailManagerClient(Channel);
			SendEmailRequest request = new SendEmailRequest();
			request.Messages.Add(messages);
			var results = await client.SendEmailAsync(request, Headers).ConfigureAwait(false);
			return results.Results;
		}
		#endregion 
	}
}
