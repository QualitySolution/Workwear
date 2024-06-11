using NHibernate;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;

namespace Workwear.Repository.Company
{
	public class SubdivisionRepository
	{
		public IQueryOver<Subdivision,Subdivision> ActiveQuery (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Subdivision> ();
		}

		public Subdivision GetSubdivisionByName(IUnitOfWork uow, string name)
		{
			return uow.Session.QueryOver<Subdivision>()
				.Where(x => x.Name == name)
				.Take(1)
				.SingleOrDefault();
		}
	}
}

