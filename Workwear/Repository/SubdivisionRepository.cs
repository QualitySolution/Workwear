using NHibernate.Criterion;
using workwear.Domain.Company;

namespace workwear.Repository
{
	public static class SubdivisionRepository
	{

		public static QueryOver<Subdivision> ActiveObjectsQuery ()
		{
			return QueryOver.Of<Subdivision> ();
		}
	}
}

