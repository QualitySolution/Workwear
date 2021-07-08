using System;
using QS.DomainModel.UoW;
using workwear.Domain.Company;

namespace workwear.Repository.Company
{
	public class PostRepository
	{
		public Post GetPostByName(IUnitOfWork uow, string name, Subdivision subdivision = null)
		{
			var query = uow.Session.QueryOver<Post>()
				.Where(x => x.Name == name);

			if(subdivision != null)
				query.Where(x => x.Subdivision.Id == subdivision.Id);

			return query
				.Take(1)
				.SingleOrDefault();
		}
	}
}
