using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using workwear.Domain.Company;

namespace workwear.ViewModels.Company
{
	public class OrganizationViewModel : EntityTabViewModelBase<Organization>
	{
		public OrganizationViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
		}
	}
}
