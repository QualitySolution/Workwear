using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Domain.Users;

namespace Workwear.Repository.Company {
	public class OrganizationRepository {
		/// <summary>
		///  Возвращает организацию по умолчанию, определенную в настройках пользователя, если не определени и огранизация единственния, то проставит её.
		/// </summary>
		public virtual Organization GetDefaultOrganization(IUnitOfWork uow, int idUser)
		{
			UserSettings settings = uow.Session.QueryOver<UserSettings>()
			.Where(x => x.User.Id == idUser).SingleOrDefault<UserSettings>();
			if(settings?.DefaultOrganization != null) 
				return settings.DefaultOrganization;

			var organizations = uow.GetAll<Organization>().Take(2).ToList();
			return organizations.Count == 1 ? organizations.First() : null; 
		}
	}
}
