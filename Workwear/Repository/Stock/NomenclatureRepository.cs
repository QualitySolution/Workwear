using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using NHibernate.Criterion;

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

		public static IList<ItemsType> GetTypesOfNomenclatures(IUnitOfWork uow, Nomenclature[] nomenclatures)
		{
			var ids = nomenclatures.Select(n => n.Id).ToArray();
			return uow.Session.QueryOver<Nomenclature>()
				.Where(n => n.Id.IsIn(ids))
				.JoinQueryOver(x => x.Type)
				.Select(x => x.Type)
				.List<ItemsType>();
		}
	}
}
