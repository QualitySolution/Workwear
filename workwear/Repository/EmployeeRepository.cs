using System;
using NHibernate.Criterion;
using workwear.Domain;

namespace workwear.Repository
{
	public static class EmployeeRepository
	{

		public static QueryOver<EmployeeCard> ActiveEmployeesQuery ()
		{
			return QueryOver.Of<EmployeeCard> ().Where (e => e.DismissDate == null);
		}

	}
}

