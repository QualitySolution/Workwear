using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.Regulations;

namespace Workwear.Repository.Regulations
{
	public class ProtectionToolsRepository
	{
		public IList<ProtectionTools> GetProtectionToolsByName(IUnitOfWork uow, params string[] names)
		{
			return uow.Session.QueryOver<ProtectionTools>()
				.Where(x => x.Name.IsIn(names))
				.List();
		}
	}
}
