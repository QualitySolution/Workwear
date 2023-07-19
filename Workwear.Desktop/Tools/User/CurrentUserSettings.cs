using System;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Services;
using Workwear.Domain.Users;

namespace Workwear.Tools.User
{
	public class CurrentUserSettings : IDisposable
	{
		private readonly IUserService userService;
		private readonly IEntityChangeWatcher changeWatcher;

		public CurrentUserSettings(IUnitOfWorkFactory unitOfWorkFactory, IUserService userService, IEntityChangeWatcher changeWatcher)
		{
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.changeWatcher = changeWatcher;
			uow = unitOfWorkFactory.CreateWithoutRoot();
			changeWatcher?.BatchSubscribe(SettingChanged).IfEntity<UserSettings>().AndWhere(x => x.User.Id == userService.CurrentUserId);
		}
		
		/// <summary>
		/// Используйте только для тестов
		/// </summary>
		public CurrentUserSettings() {
		}

		private UserSettings userSettings;
		public virtual UserSettings Settings {
			get {
				if(userSettings == null) {
					TryLoadSettings();
					if(userSettings == null) {
						userSettings = new UserSettings(userService.GetCurrentUser());
					}
				}
				return userSettings;
			}
		}

		private bool selfSave = false;
		
		public void SaveSettings()
		{
			selfSave = true;
			uow.Save(userSettings);
			uow.Commit();
			selfSave = false;
		}

		private void TryLoadSettings() {
			userSettings = uow.Session.QueryOver<UserSettings>()
					.Where(s => s.User.Id == userService.CurrentUserId)
					.SingleOrDefault();
		}

		private IUnitOfWork uow;
		
		#region Обновление
		private void SettingChanged(EntityChangeEvent[] changeevents) {
			if(selfSave)
				return;
			uow.Session.Refresh(userSettings);
		}
		#endregion
		public void Dispose() {
			changeWatcher?.UnsubscribeAll(this);
			uow?.Dispose();
		}
	}
}