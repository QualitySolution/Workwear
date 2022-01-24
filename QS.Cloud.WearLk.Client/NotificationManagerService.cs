using System;
using System.Collections.Generic;
using System.Linq;
using QS.Cloud.WearLk.Manage;

namespace QS.Cloud.WearLk.Client
{
    public class NotificationManagerService : WearLkServiceBase
    {
        public NotificationManagerService(string sessionId) : base(sessionId)
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
        #endregion
    }
}