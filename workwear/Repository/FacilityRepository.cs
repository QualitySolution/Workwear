using NHibernate.Criterion;
using workwear.Domain.Company;

namespace workwear.Repository
{
	public static class FacilityRepository
	{

		public static QueryOver<Facility> ActiveObjectsQuery ()
		{
			return QueryOver.Of<Facility> ();
		}
	}
}

