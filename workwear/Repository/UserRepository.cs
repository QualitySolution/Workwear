using System;
using workwear.Domain;
using QSOrmProject;

namespace workwear.Repository
{
	public static class UserRepository
	{
		public static User GetMyUser(IUnitOfWork uow)
		{
			return uow.GetById<User> (QSProjectsLib.QSMain.User.Id);
		}
	}
}

