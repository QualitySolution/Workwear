using System;
using Grpc.Core;

namespace QS.Cloud.WearLk.Client
{
    public class LkUserManagerService: IDisposable
    {
        public static readonly string ServiceAddress = "lk.wear.cloud.qsolution.ru";
        public static readonly int ServicePort = 4201;

        private readonly string sessionId;

        private readonly Metadata headers;
        
        public LkUserManagerService(string sessionId)
        {
            this.sessionId = sessionId;
            headers = new Metadata {{"Authorization", $"Bearer {sessionId}"}};
        }

        private Channel channel;
        private Channel Channel {
            get {
                if(channel == null || channel.State == ChannelState.Shutdown)
                    channel = new Channel(ServiceAddress, ServicePort, ChannelCredentials.Insecure);
                if (channel.State == ChannelState.TransientFailure)
                    channel.ConnectAsync();
                return channel;
            }
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
            
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль должен быть указан", nameof(password));
            
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
        
        public void RemovePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Телефон должен быть указан", nameof(phone));
            
            var client = new LkUserManager.LkUserManagerClient(channel);
            var request = new RemovePhoneRequest()
            {
                Phone = phone,
            };
            
            client.RemovePhone(request, headers);
        }
        
        #endregion

        public void Dispose()
        {
            channel?.ShutdownAsync();
        }
    }
}