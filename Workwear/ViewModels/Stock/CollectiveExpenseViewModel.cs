using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NLog;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Dialog.ViewModels;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QSReport;
using workwear.Domain.Statements;
using workwear.Domain.Stock;
using workwear.Repository;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Tools.Features;
using workwear.ViewModels.Statements;

namespace workwear.ViewModels.Stock
{
	public class CollectiveExpenseViewModel : EntityDialogViewModelBase<CollectiveExpense>, ISelectItem
	{
		ILifetimeScope autofacScope;
		private readonly UserRepository userRepository;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public CollectiveExpenseItemsViewModel CollectiveExpenseItemsViewModel;
		IInteractiveQuestion interactive;
		private readonly CommonMessages commonMessages;
		private readonly FeaturesService featuresService;
		private readonly BaseParameters baseParameters;

		public CollectiveExpenseViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			IValidator validator,
			IUserService userService,
			UserRepository userRepository,
			IInteractiveQuestion interactive,
			StockRepository stockRepository,
			CommonMessages commonMessages,
			FeaturesService featuresService,
			BaseParameters baseParameters
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			this.interactive = interactive;
			this.commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			var entryBuilder = new CommonEEVMBuilderFactory<CollectiveExpense>(this, Entity, UoW, navigation, autofacScope);
			if(UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
			}

			if(Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);

			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse).MakeByType().Finish();

			var parameter = new TypedParameter(typeof(CollectiveExpenseViewModel), this);
			CollectiveExpenseItemsViewModel = this.autofacScope.Resolve<CollectiveExpenseItemsViewModel>(parameter);
			//Переопределяем параметры валидации
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));
		}

		#region EntityViewModels
		public EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		#endregion

		public override bool Save()
		{
			if(!Validate())
				return false;

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
			Entity.UpdateEmployeeWearItems(progress);
			progress.Add(text: "Сохранение в базу данных...");
			UoWGeneric.Commit();
			progress.Close();
			logger.Info("Ok");
			NavigationManager.ForceClosePage(progressPage, CloseSource.FromParentPage);
			return true;
		}

		private void IssuanceSheetOpen()
		{
			Save();
			MainClass.MainWin.NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}

		public void OpenIssuenceSheet()
		{
			if(UoW.HasChanges) {
				if(!interactive.Question("Сохранить документ выдачи перед открытием ведомости?") || !Save())
					return;
			}
			MainClass.MainWin.NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}

		public void CreateIssuenceSheet()
		{
			var userSettings = userRepository.GetCurrentUserSettings(UoW);
			Entity.CreateIssuanceSheet(userSettings);
		}

		public void PrintIssuenceSheet(IssuedSheetPrint doc)
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
