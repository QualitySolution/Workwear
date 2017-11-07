using QSOrmProject.Users;
using workwear.Domain.Users;

namespace workwear.Tools
{
	public static class CurrentUserSettings
	{
		static UserSettingsManager<UserSettings> manager = new UserSettingsManager<UserSettings>();

		static CurrentUserSettings()
		{
			manager.CreateUserSettings = uow => new UserSettings(Repository.UserRepository.GetMyUser(uow));
			manager.LoadUserSettings = uow => Repository.UserRepository.GetCurrentUserSettings(uow);
		}

		public static UserSettings Settings
		{
			get
			{
				return manager.Settings;
			}
		}

		public static void SaveSettings()
		{
			manager.SaveSettings();
		}
	}
}

