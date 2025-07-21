using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;

namespace Workwear.Repository.Regulations
{
	public class NormRepository
	{
		public IUnitOfWork RepoUow;

		public NormRepository(IUnitOfWork uow = null) {
			RepoUow = uow;
		}
		public IList<Norm> GetNormsForPost(IUnitOfWork uow, params Post[] posts)
		{
			Post postAliace = null;

			var postsIds = posts.Select(x => x.Id).Distinct().ToArray();

			return uow.Session.QueryOver<Norm>()
				.JoinQueryOver(n => n.Posts, () => postAliace)
				.Where(p => p.Id.IsIn(postsIds))
				.List();
		}

		public IList<Norm> GetNormsUseProtectionTools( int protectionToolsId, IUnitOfWork uow = null) {
			
			NormItem normItemAlias = null;

			return (uow ?? RepoUow).Session.QueryOver<Norm>()
				.JoinAlias(x => x.Items, () => normItemAlias)
				.Where(() => normItemAlias.ProtectionTools.Id == protectionToolsId)
				.List();
		}
	}
}

