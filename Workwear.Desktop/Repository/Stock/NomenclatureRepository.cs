using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Stock;
using NHibernate.Criterion;

namespace Workwear.Repository.Stock
{
	public class NomenclatureRepository
	{
		private UnitOfWorkProvider unitOfWorkProvider;
		private IUnitOfWork UoW => unitOfWorkProvider?.UoW;

		public NomenclatureRepository(UnitOfWorkProvider unitOfWorkProvider) {
			this.unitOfWorkProvider = unitOfWorkProvider;
		}
		
		public IList<Nomenclature> GetActiveNomenclatures() {
			return UoW.Session.QueryOver<Nomenclature>()
				.Where(x => x.Archival == false)
				.List();
		}

		public IList<Nomenclature> GetNomenclatureByName(IUnitOfWork uow, params string[] names)
		{
			return uow.Session.QueryOver<Nomenclature>()
				.Where(x => x.Name.IsIn(names))
				.List();
		}
	}
}
