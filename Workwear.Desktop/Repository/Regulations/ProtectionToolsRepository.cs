using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.Repository.Regulations
{
	public class ProtectionToolsRepository
	{
		/// <summary>
		/// Список номенклатур нормы с названием точно совпадающим с одним из заданного в списке names
		/// </summary>
		public IList<ProtectionTools> GetProtectionToolsByName(IUnitOfWork uow, params string[] names)
		{
			return uow.Session.QueryOver<ProtectionTools>()
				.Where(x => x.Name.IsIn(names))
				.List();
		}
		/// <summary>
		/// Список неархивных номенклатур нормы
		/// </summary>
		public IList<ProtectionTools> GetActiveProtectionTools(IUnitOfWork uow) {
			return uow.Session.QueryOver<ProtectionTools>()
				.Where(x => x.Archival == false)
				.List();
		}
		/// <summary>
		/// Номенклатуры нормы, в которых любая из номенклатур указана закупаемой.
		/// </summary>
		public IList<ProtectionTools> GetProtectionToolsWithSupplyNomenclature(IUnitOfWork uow, params Nomenclature[] nomenclatures) {
			var ids = nomenclatures.Select(x => x.Id).ToArray();
			return uow.Session.QueryOver<ProtectionTools>()
				.Where(x => x.SupplyNomenclatureUnisex.Id.IsIn(ids)
					|| x.SupplyNomenclatureMale.Id.IsIn(ids)
					|| x.SupplyNomenclatureFemale.Id.IsIn(ids))
				.List();
		}
	}
}
