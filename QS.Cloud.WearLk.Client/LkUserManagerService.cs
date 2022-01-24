using System;

namespace QS.Cloud.WearLk.Client
{
    public class LkUserManagerService : WearLkServiceBase
    {
        public LkUserManagerService(string sessionId) : base(sessionId)
        {
        }
        
        #region Запросы
        public string GetPassword(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Телефон должен быть указан", nameof(phone));
            
            var client = new LkUserManager.LkUserManagerClient(Channel);
            var request = new GetPasswordRequest
            {
                 Phone = phone
            };
            return client.GetPassword(request, Headers).Password;
        }
        
        public void SetPassword(string phone, string password)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Телефон должен быть указан", nameof(phone));
            
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль должен быть указан", nameof(password));
            
            var client = new LkUserManager.LkUserManagerClient(Channel);
            var request = new SetPasswordRequest
            {
                Phone = phone,
                Password = password
            };
            
            client.SetPassword(request, Headers);
        }
        
        public void ReplacePhone(string oldPhone, string newPhone)
        {
            if (string.IsNullOrWhiteSpace(oldPhone))
                throw new ArgumentException("Изменяемый телефон должен быть указан", nameof(oldPhone));
            
            var client = new LkUserManager.LkUserManagerClient(Channel);
            var request = new ReplacePhoneRequest()
            {
                OldPhone = oldPhone,
                NewPhone = newPhone
            };
            
            client.ReplacePhone(request, Headers);
        }
        
        public void RemovePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Телефон должен быть указан", nameof(phone));
            
            var client = new LkUserManager.LkUserManagerClient(Channel);
            var request = new RemovePhoneRequest()
            {
                Phone = phone,
            };
            
            client.RemovePhone(request, Headers);
        }
        
        #endregion
    }
}