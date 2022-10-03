using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;

namespace Workwear.Repository.Sizes
{
    public class SizeRepository {
        public static IList<Size> GetSize(IUnitOfWork uow) => 
            uow.Session.QueryOver<Size>().List();

        public static IList<SizeType> GetSizeType(IUnitOfWork uow) => 
            uow.Session.QueryOver<SizeType>()
                .OrderBy(x => x.Position).Asc
                .List();
    }
}