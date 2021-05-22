using System;
using Grpc.Core;

namespace QS.Cloud.WearLk.Client
{
    public class LkUserManagerService: IDisposable
    {
        public static string ServiceAddress = "lk.wear.cloud.qsolution.ru";
        public static int ServicePort = 4201;
        
        private readonly Channel channel;
        private readonly string sessionId;

        private readonly Metadata headers;
        
        public LkUserManagerService(string sessionId)
        {
            channel = new Channel(ServiceAddress, ServicePort, ChannelCredentials.Insecure);
            this.sessionId = sessionId;
            headers = new Metadata {{"Authorization", $"Bearer {sessionId}"}};
        }
        
        #region Запросы
        public string GetPassword(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Телефон должен быть указан", nameof(phone));
            
            var client = new LkUserManager.LkUserManagerClient(channel);
            var request = new GetPasswordRequest
            {
                 Phone = phone
            };
            return client.GetPassword(request, headers).Password;
        }
        
        public void SetPassword(string phone, string password)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Телефон должен быть указан", nameof(phone));
            
            var client = new LkUserManager.LkUserManagerClient(channel);
            var request = new SetPasswordRequest
            {
                Phone = phone,
                Password = password
            };
            
            client.SetPassword(request, headers);
        }
        
        public void ReplacePhone(string oldPhone, string newPhone)
        {
            if (string.IsNullOrWhiteSpace(oldPhone))
                throw new ArgumentException("Изменяемый телефон должен быть указан", nameof(oldPhone));
            
            var client = new LkUserManager.LkUserManagerClient(channel);
            var request = new ReplacePhoneRequest()
            {
                OldPhone = oldPhone,
                NewPhone = newPhone
            };
            
            client.ReplacePhone(request, headers);
        }
        
        #endregion

        public void Dispose()
        {
            channel?.ShutdownAsync();
        }
    }
}