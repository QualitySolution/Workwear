using System;
using System.Collections.Generic;
using Autofac;
using NLog;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.Repository.Stock;
using workwear.Tools.Features;
using workwear.ViewModels.Company;
using workwear.ViewModels.Statements;

namespace workwear.ViewModels.Stock
{
	public class ExpenseObjectViewModel : EntityDialogViewModelBase<Expense>
	{
		private readonly ILifetimeScope autofacScope;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public ExpenseDocItemsObjectViewModel DocItemsObjectViewModel;
		IInteractiveQuestion interactive;
		private readonly StockRepository stockRepository;
		private readonly CommonMessages commonMessages;

		public ExpenseObjectViewModel(IEntityUoWBuilder uowBuilder,
									  IUnitOfWorkFactory unitOfWorkFactory,
									  INavigationManager navigation,
									  ILifetimeScope autofacScope,
									  IValidator validator,
									  IUserService userService,
									  IInteractiveQuestion interactive,
									  StockRepository stockRepository,
									  FeaturesService featutesService,
									  CommonMessages commonMessages,
									  Subdivision subdivision = null
									  )
		: base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			Entity.Date = DateTime.Today;
			this.interactive = interactive;
			this.stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			this.commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			if(subdivision != null) {
				Entity.Subdivision = subdivision;
				Entity.Warehouse = subdivision.Warehouse;
			}

			if(UoW.IsNew) {
				Entity.Operation = ExpenseOperations.Object;
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
			}

			if(Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featutesService);

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
		}

		#region EntityViewModels
		public EntityEntryViewModel<Warehouse> WarehouseExpenceViewModel;
		public EntityEntryViewModel<Subdivision> SubdivisionViewModel;
		#endregion

		public void CreateIssuanceSheet()
		{
			Entity.CreateIssuanceSheet();
		}

		public void OpenIssuanceSheetViewModel()
		{
			MainClass.MainWin.NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}

		public override bool Save()
		{
			if(!Validate())
				return false;

			logger.Info("Запись документа...");
			Entity.UpdateOperations(UoW, interactive);
			Entity.UpdateIssuanceSheet();
			if(Entity.IssuanceSheet != null)
				UoW.Save(Entity.IssuanceSheet);
			UoWGeneric.Save();

			logger.Info("Ok");
			return true;
		}

		private void IssuanceSheetOpen()
		{
			Save();
			MainClass.MainWin.NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}

		private void Print()
		{
			Save();
			var reportInfo = new ReportInfo {
				Title = String.Format("Ведомость №{0} (МБ-7)", Entity.Id),
				Identifier = "Statements.IssuanceSheet",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.IssuanceSheet.Id }
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		public void OpenIssuenceSheet()
		{
			if(UoW.HasChanges) {
				if(MessageDialogHelper.RunQuestionDialog("Сохранить документ выдачи перед открытием ведомости?"))
					Save();
				else
					return;
			}
			MainClass.MainWin.NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}

		public void CreateIssuenceSheet()
		{
			Entity.CreateIssuanceSheet();
		}

		public void PrintIssuenceSheet()
		{
			if(UoW.HasChanges) {
				if(commonMessages.SaveBeforePrint(Entity.GetType(), "ведомости"))
					Save();
				else
					return;
			}

			var reportInfo = new ReportInfo {
				Title = String.Format("Ведомость №{0} (МБ-7)", Entity.Id),
				Identifier = "Statements.IssuanceSheet",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.IssuanceSheet.Id }
				}
			};
			MainClass.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
	}
}
