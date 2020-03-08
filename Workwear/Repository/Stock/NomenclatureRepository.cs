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

		/// <summary>
		/// Получаем список подходящих номеклатур в том счисле и с учетом аналогов.
		/// </summary>
		public static Dictionary<ItemsType, List<Nomenclature>> GetSuitableNomenclatures(IUnitOfWork uow, ItemsType[] itemsTypes)
		{
			var results = new Dictionary<ItemsType, List<Nomenclature>>();
			var nomenclatures = uow.Session.QueryOver<Nomenclature>()
				.Where(n => n.Type.IsIn(itemsTypes))
				.List();

			foreach(var nomenclature in nomenclatures) {
				if(!results.ContainsKey(nomenclature.Type))
					results[nomenclature.Type] = new List<Nomenclature>();

				results[nomenclature.Type].Add(nomenclature);
			}
			return results;
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
