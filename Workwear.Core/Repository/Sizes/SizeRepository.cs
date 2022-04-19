using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using workwear.Domain.Sizes;

namespace workwear.Repository.Sizes
{
    public class SizeRepository {
        public static IList<Size> GetSize(IUnitOfWork uow) => 
            uow.Session.QueryOver<Size>().List();

        public static IList<SizeType> GetSizeType(IUnitOfWork uow) => 
            uow.Session.QueryOver<SizeType>().List();
    }
}