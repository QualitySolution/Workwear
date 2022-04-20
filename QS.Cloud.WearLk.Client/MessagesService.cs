using System;
using System.Collections.Generic;
using System.Linq;
using QS.Cloud.WearLk.Manage;

namespace QS.Cloud.WearLk.Client
{
    public class MessagesService : WearLkServiceBase
    {
        public MessagesService(string sessionId) : base(sessionId)
        {
        }

        #region Запросы
        public IList<MessageItem> GetMessages(string phone)
		{
			if(String.IsNullOrEmpty(phone))
				throw new ArgumentException("Телефон не должен быть пустым", nameof(phone));

			var client = new Messages.MessagesClient(Channel);
			var request = new GetMessagesRequest();
			request.Phone = phone;
			return client.GetMessages(request, Headers).Messages;
		}
		#endregion
	}
}