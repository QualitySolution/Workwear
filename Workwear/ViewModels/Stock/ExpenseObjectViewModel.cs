using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autofac;
using NLog;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Tools.Features;
using workwear.ViewModels.Company;

namespace workwear.ViewModels.Stock
{
	public class ExpenseObjectViewModel : EntityDialogViewModelBase<Expense>
	{
		private readonly ILifetimeScope autofacScope;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public ExpenseDocItemsObjectViewModel DocItemsObjectViewModel;
		IInteractiveQuestion interactive;
		private readonly StockRepository stockRepository;
		private readonly BaseParameters baseParameters;

		public ExpenseObjectViewModel(IEntityUoWBuilder uowBuilder,
									  IUnitOfWorkFactory unitOfWorkFactory,
									  INavigationManager navigation,
									  ILifetimeScope autofacScope,
									  IValidator validator,
									  IUserService userService,
									  IInteractiveQuestion interactive,
									  StockRepository stockRepository,
									  FeaturesService featutesService,
									  BaseParameters baseParameters,
									  Subdivision subdivision = null
									  )
		: base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			Entity.Date = DateTime.Today;
			this.interactive = interactive;
			this.stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			if(subdivision != null) {
				Entity.Subdivision = subdivision;
				Entity.Warehouse = subdivision.Warehouse;
			}

			if(UoW.IsNew) {
				Entity.Operation = ExpenseOperations.Object;
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
			}
			if(Entity.Operation != ExpenseOperations.Object)
				throw new InvalidOperationException("Диалог предназначен только для операций выдачи на подразделение.");

			if(Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featutesService, autofacScope.Resolve<IUserService>().CurrentUserId);

			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var entryBuilder = new CommonEEVMBuilderFactory<Expense>(this, Entity, UoW, navigation, autofacScope);

			WarehouseExpenceViewModel = entryBuilder.ForProperty(x => x.Warehouse)
									.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
									.UseViewModelDialog<WarehouseViewModel>()
									.Finish();

			SubdivisionViewModel = entryBuilder.ForProperty(x => x.Subdivision)
								.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
								.UseViewModelDialog<SubdivisionViewModel>()
								.Finish();
								
			var parameter = new TypedParameter(typeof(ExpenseObjectViewModel), this);
			DocItemsObjectViewModel = this.autofacScope.Resolve<ExpenseDocItemsObjectViewModel>(parameter);

			//Переопределяем параметры валидации
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));
		}

		#region EntityViewModels
		public EntityEntryViewModel<Warehouse> WarehouseExpenceViewModel;
		public EntityEntryViewModel<Subdivision> SubdivisionViewModel;
		#endregion

		public override bool Save()
		{
			if(!Validate())
				return false;

			logger.Info("Запись документа...");
			Entity.UpdateOperations(UoW, baseParameters, interactive);
			UoWGeneric.Save();

			logger.Info("Ok");
			return true;
		}
	}
}
