using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear.Repository.Stock
{
	public static class NomenclatureRepository
	{
		public static IList<Nomenclature> GetNomenclaturesOfType(IUnitOfWork uow, ItemsType itemsType)
		{
			return uow.Session.QueryOver<Nomenclature>()
				.Where(n => n.Type.Id == itemsType.Id)
				.List();
		}
	}
}
