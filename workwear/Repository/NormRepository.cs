using System;
using System.Collections.Generic;
using QSOrmProject;
using workwear.Domain;
using NHibernate.Criterion;

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

