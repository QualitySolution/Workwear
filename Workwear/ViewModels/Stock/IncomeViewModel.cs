using System;
using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Tools.Features;

namespace workwear.ViewModels.Stock
{
	public class IncomeViewModel: EntityDialogViewModelBase<Income>
	{
		public IncomeViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IUserService userService,
			StockRepository stockRepository,
			FeaturesService featuresService,
			ILifetimeScope autofacScope,
			BaseParameters baseParameters,
			IInteractiveQuestion interactive,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
		}
	}
}
