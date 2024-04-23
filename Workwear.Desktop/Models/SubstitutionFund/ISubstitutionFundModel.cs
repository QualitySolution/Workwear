using System.Collections.Generic;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Models.SubstitutionFund 
{
	public interface ISubstitutionFundModel 
	{
		int GetFreeBarcodesAmount(IUnitOfWork uow, Nomenclature nomenclature, Size size, Size height);
		IList<Barcode> GetFreeBarcodes(IUnitOfWork uow, Nomenclature nomenclature, Size size, Size height);
		IList<SubstituteFundOperation> GetCurrentlyInOperation(IUnitOfWork uow, Nomenclature nomenclature, Size size, Size height);
		void CreateOperation(IUnitOfWork uow, Nomenclature nomenclature, Size size, Size height);
		void DeleteOperation(IUnitOfWork uow, SubstituteFundOperation operation);
	}
}
