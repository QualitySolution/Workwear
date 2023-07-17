using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NLog;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Tools;
using QS.Utilities.Debug;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear;
using Workwear.Domain.Company;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.User;
using Workwear.ViewModels.Statements;

namespace Workwear.ViewModels.Stock
{
	public class CollectiveExpenseViewModel : EntityDialogViewModelBase<CollectiveExpense>, ISelectItem
	{
		private ILifetimeScope autofacScope;
		private readonly CurrentUserSettings currentUserSettings;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public CollectiveExpenseItemsViewModel CollectiveExpenseItemsViewModel;
		private IInteractiveQuestion interactive;
		private readonly CommonMessages commonMessages;
		private readonly FeaturesService featuresService;
		private readonly BaseParameters baseParameters;
		private readonly IChangeMonitor<CollectiveExpenseItem> changeMonitor;

		public CollectiveExpenseViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			IValidator validator,
			IUserService userService,
			CurrentUserSettings currentUserSettings,
			IInteractiveQuestion interactive,
			StockRepository stockRepository,
			CommonMessages commonMessages,
			FeaturesService featuresService,
			BaseParameters baseParameters,
			IChangeMonitor<CollectiveExpenseItem> changeMonitor
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.currentUserSettings = currentUserSettings ?? throw new ArgumentNullException(nameof(currentUserSettings));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.changeMonitor = changeMonitor ?? throw new ArgumentNullException(nameof(changeMonitor));

			var performance = new PerformanceHelper("Диалог", logger);
			var entryBuilder = new CommonEEVMBuilderFactory<CollectiveExpense>(this, Entity, UoW, navigation, autofacScope);
			if (UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
			}
			else {
				UoW.Session.QueryOver<CollectiveExpense>()
					.Where(x => x.Id == Entity.Id)
					.Fetch(SelectMode.ChildFetch, x => x)
					.Fetch(SelectMode.Fetch, x => x.CreatedbyUser)
					.Fetch(SelectMode.Fetch, x => x.Warehouse)
					.SingleOrDefault();
			}
			performance.CheckPoint("Предзагрузка данных документа");
			
			changeMonitor.SubscribeAllChange(
					x => DomainHelper.EqualDomainObjects(x.Document, Entity))
				.TargetField(x => x.Employee);
			changeMonitor.AddSetTargetUnitOfWorks(UoW);

			performance.CheckPoint("entryBuilder и changeMonitor");
			if(Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);

			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse).MakeByType().Finish();
			TransferAgentEntryViewModel = entryBuilder.ForProperty(x => x.TransferAgent).MakeByType().Finish();
			
			performance.CheckPoint("Warehouse");
			performance.StartGroup("CollectiveExpenseItemsViewModel");
			var parameterModel = new TypedParameter(typeof(CollectiveExpenseViewModel), this);
			var parameterPerformance = new TypedParameter(typeof(PerformanceHelper), performance);
			CollectiveExpenseItemsViewModel = this.autofacScope.Resolve<CollectiveExpenseItemsViewModel>(parameterModel, parameterPerformance);
			performance.EndGroup();
			//Переопределяем параметры валидации
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));
			performance.CheckPoint("Конец");
			performance.PrintAllPoints(logger);
		}

		#region EntityViewModels
		public EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		public EntityEntryViewModel<EmployeeCard> TransferAgentEntryViewModel;
		#endregion

		public override bool Save()
		{
			if(!Validate())
				return false;
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;

			logger.Info("Запись документа...");
			var progressPage = NavigationManager.OpenViewModel<ProgressWindowViewModel>(this);
			var progress = progressPage.ViewModel.Progress;

			var employeeItemGroups = Entity.Items.GroupBy(x => x.Employee);
			//О подсчете прогресса, метод UpdateEmployeeWearItems использует умноженное на 2 количество сотрудников, как шагов прогресса.
			progress.Start(maxValue: employeeItemGroups.Count() * 2 + 2, text: "Подготовка...");
			Entity.CleanupItems();
			Entity.UpdateOperations(UoW, baseParameters, interactive);
			progress.Add(text: "Обновление ведомости...");
			Entity.UpdateIssuanceSheet();
			if(Entity.IssuanceSheet != null)
				UoW.Save(Entity.IssuanceSheet);

			UoWGeneric.Save();
			logger.Debug("Обновляем записи о выданной одежде в карточке сотрудника...");
			Entity.UpdateEmployeeWearItems(progress, changeMonitor.EntityIds.ToList());
			progress.Add(text: "Сохранение в базу данных...");
			UoWGeneric.Commit();
			progress.Close();
			logger.Info("Ok");
			NavigationManager.ForceClosePage(progressPage, CloseSource.FromParentPage);
			return true;
		}

		public void OpenIssuanceSheet()
		{
			if(UoW.HasChanges) {
				if(!interactive.Question("Сохранить документ выдачи перед открытием ведомости?") || !Save())
					return;
			}
			MainClass.MainWin.NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}

		public void CreateIssuanceSheet()
		{
			Entity.CreateIssuanceSheet(currentUserSettings.Settings);
		}

		public void PrintIssuanceSheet(IssuedSheetPrint doc)
		{
			if(UoW.HasChanges) {
				if(!commonMessages.SaveBeforePrint(Entity.GetType(), doc == IssuedSheetPrint.AssemblyTask ? "задания на сборку" : "ведомости") || !Save())
					return;
			}

			var reportInfo = new ReportInfo {
				Title = doc == IssuedSheetPrint.AssemblyTask ? $"Задание на сборку №{Entity.IssuanceSheet.Id}" : $"Ведомость №{Entity.IssuanceSheet.Id} (МБ-7)",
				Identifier = doc.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.IssuanceSheet.Id }
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		#region ISelectItem
		public void SelectItem(int id)
		{
			CollectiveExpenseItemsViewModel.SelectedItem = Entity.Items.First(x => x.Id == id);
		}
		#endregion
	}
}
