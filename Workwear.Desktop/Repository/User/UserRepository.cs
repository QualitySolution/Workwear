using System;
using QS.DomainModel.UoW;
using QS.Services;
using Workwear.Domain.Users;

namespace Workwear.Repository.User
{
	public class UserRepository
	{
		private readonly IUserService userService;

		public UserRepository(IUserService userService) {
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		/// <summary>
		/// По возможности не используйте напрямую этот метод, для получения настроек используйте класс CurrentUserSettings
		/// </summary>
		public UserSettings GetCurrentUserSettings(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<UserSettings>()
				.Where(s => s.User.Id == userService.CurrentUserId)
				.SingleOrDefault();
		}

	}
}

