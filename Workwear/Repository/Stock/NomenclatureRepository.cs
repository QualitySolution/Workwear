using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using workwear.Domain.Stock;
using NHibernate.Criterion;

namespace workwear.Repository.Stock
{
	public class NomenclatureRepository
	{
		public IList<Nomenclature> GetNomenclatureByName(IUnitOfWork uow, params string[] names)
		{
			return uow.Session.QueryOver<Nomenclature>()
				.Where(x => x.Name.IsIn(names))
				.List();
		}

		#region Статические
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
		#endregion
	}
}
