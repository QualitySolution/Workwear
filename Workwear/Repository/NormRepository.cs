using System.Collections.Generic;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Regulations;

namespace workwear.Repository
{
	public static class NormRepository
	{
		public static IList<Norm> GetNormForPost(IUnitOfWork uow, Post post)
		{
			Post postAliace = null;

			return uow.Session.QueryOver<Norm> ()
				.JoinQueryOver (n => n.Professions, () => postAliace)
				.Where (p => p.Id == post.Id)
				.List ();
		}
	}
}

