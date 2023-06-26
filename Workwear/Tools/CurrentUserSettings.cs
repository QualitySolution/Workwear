using QS.Services;
using QSOrmProject.Users;
using Workwear.Domain.Users;
using Workwear.Repository.User;

namespace workwear.Tools
{
	public class CurrentUserSettings
	{
		UserSettingsManager<UserSettings> manager = new UserSettingsManager<UserSettings>();

		public CurrentUserSettings(IUserService userService, UserRepository userRepository)
		{
			manager.CreateUserSettings = uow => new UserSettings(userService.GetCurrentUser());
			manager.LoadUserSettings = userRepository.GetCurrentUserSettings;
		}

		public UserSettings Settings => manager.Settings;

		public void SaveSettings()
		{
			manager.SaveSettings();
		}
	}
}

