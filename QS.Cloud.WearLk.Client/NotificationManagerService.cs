using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QS.Cloud.Client;
using QS.Cloud.WearLk.Manage;

namespace QS.Cloud.WearLk.Client
{
    public class NotificationManagerService : WearLkServiceBase
    {
        public NotificationManagerService(ISessionInfoProvider sessionInfoProvider) : base(sessionInfoProvider)
        {
        }

        #region Запросы
        public string SendMessages(IEnumerable<OutgoingMessage> messages)
        {
            if (!messages.Any())
                throw new ArgumentException("Должно быть передано хотя бы одно сообщение", nameof(messages));
            
            var client = new NotificationManager.NotificationManagerClient(Channel);
            var request = new SendMessageRequest();
            request.Messages.Add(messages);
            return client.SendMessage(request, Headers).Results;
        }
        
        public async Task<string> SendMessagesAsync(IEnumerable<OutgoingMessage> messages)
        {
	        if (!messages.Any())
		        throw new ArgumentException("Должно быть передано хотя бы одно сообщение", nameof(messages));
            
	        var client = new NotificationManager.NotificationManagerClient(Channel);
	        var request = new SendMessageRequest();
	        request.Messages.Add(messages);
	        var results = await client.SendMessageAsync(request, Headers);
	        return results.Results;
        }

		public IList<UserStatusInfo> GetStatuses(IEnumerable<string> phones)
		{
			if(!phones.Any())
				return new List<UserStatusInfo>();

			var client = new NotificationManager.NotificationManagerClient(Channel);
			var request = new GetUsersStatusRequest();
			request.Phones.Add(phones);
			return client.GetUsersStatus(request, Headers).Statuses;
		}
		#endregion
	}
}
