using System;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Tools.OverNorms.Models;
using Workwear.Tools.OverNorms.Models.Impl;

namespace Workwear.Tools.OverNorms.Impl 
{
	public class OverNormFactory : IOverNormFactory
	{
		public OverNormModelBase CreateModel(IUnitOfWork uow, OverNormType type) 
		{
			if(uow == null) throw new ArgumentNullException(nameof(uow));

			switch(type) 
			{
				case OverNormType.Repair:
					return new RepairModel(uow);
				case OverNormType.Substitute:
					return new SubstituteFundModel(uow);
				case OverNormType.Guest:
					return new GuestModel(uow);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
	}
}
