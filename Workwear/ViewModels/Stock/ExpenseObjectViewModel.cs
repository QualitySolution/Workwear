using System;
using Autofac;
using NLog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using QS.ViewModels.Control.EEVM;
using workwear.Journal.ViewModels.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.ViewModels.Company;
using workwear.ViewModels.Statements;
using QS.Services;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Report;
using System.Collections.Generic;
using QS.Report.ViewModels;

namespace workwear.ViewModels.Stock
{
	public class ExpenseObjectViewModel : EntityDialogViewModelBase<Expense>
	{
		private readonly ILifetimeScope autofacScope;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public ExpenseDocItemsObjectViewModel DocItemsObjectViewModel;

		public ExpenseObjectViewModel(IEntityUoWBuilder uowBuilder,
									  IUnitOfWorkFactory unitOfWorkFactory,
									  INavigationManager navigation,
									  ILifetimeScope autofacScope,
									  IValidator validator,
									  IUserService userService,
									  IInteractiveQuestion interactive,
									  Subdivision subdivision = null
									  )
		: base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			Entity.Date = DateTime.Today;

				Entity.Operation = ExpenseOperations.Object;
				Entity.Subdivision = subdivision;
				Entity.Warehouse = subdivision.Warehouse;


			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var entryBuilder = new CommonEEVMBuilderFactory<Expense>(this, Entity, UoW, navigation, autofacScope);

			if(UoW.IsNew)
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);

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
			logger.Info("Запись документа...");
			var valid = new QSValidator<Expense>(UoWGeneric.Root);

			var ask = new GtkQuestionDialogsInteractive();
			Entity.UpdateOperations(UoW, ask);
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
				if(QSOrmProject.CommonDialogs.SaveBeforePrint(Entity.GetType(), "ведомости"))
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
