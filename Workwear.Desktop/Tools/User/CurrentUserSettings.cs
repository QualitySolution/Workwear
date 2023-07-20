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
			changeWatcher?.BatchSubscribe(SettingChanged).ExcludeUow(uow)
				.IfEntity<UserSettings>().AndWhere(x => x.User.Id == userService.CurrentUserId);
		}
		
		/// <summary>
		/// Используйте только для тестов
		/// </summary>
		public CurrentUserSettings() {
		}

		private UserSettings userSettings;
		/// <summary>
		/// Настройки текущего пользователя.
		/// Если настройки содержать объекты доменной модели, такие как склад и организация по умолчанию.
		/// Не используйте их напрямую для сохранения в своих документах, подгружайте их в своих uow сессиях.
		/// </summary>
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

		public void SaveSettings()
		{
			uow.Save(userSettings);
			uow.Commit();
		}

		private void TryLoadSettings() {
			userSettings = uow.Session.QueryOver<UserSettings>()
					.Where(s => s.User.Id == userService.CurrentUserId)
					.SingleOrDefault();
		}

		private IUnitOfWork uow;
		
		#region Обновление
		private void SettingChanged(EntityChangeEvent[] changeevents) {
			uow.Session.Refresh(userSettings);
		}
		#endregion
		public void Dispose() {
			changeWatcher?.UnsubscribeAll(this);
			uow?.Dispose();
		}
	}
}