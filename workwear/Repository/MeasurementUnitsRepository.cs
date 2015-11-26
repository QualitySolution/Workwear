using System;
using System.Collections.Generic;
using QSOrmProject;
using workwear.Domain;

namespace workwear.Repository
{
	public static class MeasurementUnitsRepository
	{
		public static IList<MeasurementUnits> GetActiveUnits(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<MeasurementUnits> ().List ();
		}
	}
}

