using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Tools.OverNorms.Models;

namespace Workwear.Tools.OverNorms 
{
	public interface IOverNormFactory 
	{
		OverNormModelBase CreateModel(IUnitOfWork uow, OverNormType type);
	}
}
