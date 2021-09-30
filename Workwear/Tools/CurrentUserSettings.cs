using QSOrmProject.Users;
using workwear.Domain.Users;
using workwear.Repository;

namespace workwear.Tools
{
	public static class CurrentUserSettings
	{
		static UserSettingsManager<UserSettings> manager = new UserSettingsManager<UserSettings>();

		static CurrentUserSettings()
		{
			var userRepository = new UserRepository();
			manager.CreateUserSettings = uow => new UserSettings(UserRepository.GetMyUser(uow));
			manager.LoadUserSettings = userRepository.GetCurrentUserSettings;
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

