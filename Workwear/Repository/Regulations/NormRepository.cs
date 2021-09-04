using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Regulations;

namespace workwear.Repository.Regulations
{
	public class NormRepository
	{
		public IList<Norm> GetNormsForPost(IUnitOfWork uow, params Post[] posts)
		{
			Post postAliace = null;

			var postsIds = posts.Select(x => x.Id).Distinct().ToArray();

			return uow.Session.QueryOver<Norm>()
				.JoinQueryOver(n => n.Posts, () => postAliace)
				.Where(p => p.Id.IsIn(postsIds))
				.List();
		}
	}
}

