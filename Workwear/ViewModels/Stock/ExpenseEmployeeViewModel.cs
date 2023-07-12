using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NHibernate.SqlCommand;
using NLog;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Utilities.Debug;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear;
using Workwear.Domain.Company;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Operations;
using Workwear.Repository.Stock;
using Workwear.Repository.User;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Statements;

namespace Workwear.ViewModels.Stock {
	public class ExpenseEmployeeViewModel : EntityDialogViewModelBase<Expense>, ISelectItem
	{
		private ILifetimeScope autofacScope;
		private readonly UserRepository userRepository;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public ExpenseDocItemsEmployeeViewModel DocItemsEmployeeViewModel;
		private IInteractiveService interactive;
		private readonly CommonMessages commonMessages;
		private readonly FeaturesService featuresService;
		private readonly BaseParameters baseParameters;
		private readonly IProgressBarDisplayable progress;
		private readonly EmployeeIssueRepository issueRepository;

		public ExpenseEmployeeViewModel(IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			IValidator validator,
			IUserService userService,
			UserRepository userRepository,
			IInteractiveService interactive,
			StockRepository stockRepository,
			CommonMessages commonMessages,
			FeaturesService featuresService,
			BaseParameters baseParameters,
			IProgressBarDisplayable progress,
			EmployeeIssueRepository issueRepository,
			EmployeeCard employee = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
			this.issueRepository = issueRepository ?? throw new ArgumentNullException(nameof(issueRepository));
			this.issueRepository.RepoUow = UoW;

			var performance = new PerformanceHelper("Диалог", logger);
			var ownersQuery = UoW.Session.QueryOver<Owner>().Future();
			var entryBuilder = new CommonEEVMBuilderFactory<Expense>(this, Entity, UoW, navigation, autofacScope);
			if(UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
				Entity.Operation = ExpenseOperations.Employee;
			}
			else {
				//Предварительно загружаем все связанные сущности, чтобы не было дополнительных запросов.
				var expenceQuery = UoW.Session.QueryOver<Expense>()
					.Fetch(SelectMode.ChildFetch, x => x)
					.Fetch(SelectMode.Skip, x => x.IssuanceSheet)
					.Fetch(SelectMode.Fetch, x => x.CreatedbyUser)
					.Fetch(SelectMode.Fetch, x => x.Employee)
					.Fetch(SelectMode.Fetch, x => x.Warehouse)
					.Fetch(SelectMode.Fetch, x => x.IssuanceSheet)
					.Fetch(SelectMode.Fetch, x => x.WriteOffDoc)
					.Where(x => x.Id == Entity.Id)
					.Future();
				
				ExpenseItem expenseItemAlias = null;
				
				var itemsQuery = UoW.Session.QueryOver<Expense>()
					.Where(x => x.Id == Entity.Id)
					.Fetch(SelectMode.ChildFetch, x => x)
					.Left.JoinAlias(x => x.Items, () => expenseItemAlias)
					.Fetch(SelectMode.Fetch, () => expenseItemAlias.Nomenclature)
					.Fetch(SelectMode.Fetch, () => expenseItemAlias.Nomenclature.Type)
					.Fetch(SelectMode.Fetch, () => expenseItemAlias.ProtectionTools)
					.Fetch(SelectMode.Fetch, () => expenseItemAlias.EmployeeIssueOperation)
					.Fetch(SelectMode.Fetch, () => expenseItemAlias.WarehouseOperation)
					.Future();

				if(featuresService.Available(WorkwearFeature.Barcodes))
					UoW.Session.QueryOver<ExpenseItem>()
						.Where(x => x.ExpenseDoc.Id == Entity.Id)
						.Fetch(SelectMode.ChildFetch, x => x)
						.Fetch(SelectMode.Skip, x => x.IssuanceSheetItem)
						.JoinQueryOver(x => x.EmployeeIssueOperation, JoinType.LeftOuterJoin)
						.Fetch(SelectMode.ChildFetch, x => x)
						.JoinQueryOver(x => x.BarcodeOperations, JoinType.LeftOuterJoin)
						.Fetch(SelectMode.Fetch, x => x)
						.Future();

				expenceQuery.SingleOrDefault();
			}
			
			
			if(Entity.Operation != ExpenseOperations.Employee)
				throw new InvalidOperationException("Диалог предназначен только для операций выдачи сотруднику.");

			if(employee != null) {
				Entity.Employee = UoW.GetById<EmployeeCard>(employee.Id);
				Entity.Warehouse = Entity.Employee.Subdivision?.Warehouse;
			}

			if(Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			if(employee != null) {
				performance.StartGroup("FillUnderreceived");
				FillUnderreceived(performance);
				performance.EndGroup();
			}

			if(Entity.WriteOffDoc != null)
				FillAktNumber();

			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse)
									.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
									.UseViewModelDialog<WarehouseViewModel>()
									.Finish();
			EmployeeCardEntryViewModel = entryBuilder.ForProperty(x => x.Employee)
									.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
									.UseViewModelDialog<EmployeeViewModel>()
									.Finish();

			var parameter = new TypedParameter(typeof(ExpenseEmployeeViewModel), this);
			var parameter2 = new TypedParameter(typeof(IList<Owner>), ownersQuery.ToList());
			DocItemsEmployeeViewModel = this.autofacScope.Resolve<ExpenseDocItemsEmployeeViewModel>(parameter, parameter2);
			Entity.PropertyChanged += EntityChange;

			//Переопределяем параметры валидации
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));
			performance.CheckPoint("Завершено создание модели");
			performance.PrintAllPoints(logger);
		}

		#region EntityViewModels
		public EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		public EntityEntryViewModel<EmployeeCard> EmployeeCardEntryViewModel;
		#endregion

		#region Свойства для View
		public bool IssuanceSheetCreateVisible => Entity.IssuanceSheet == null;
		public bool IssuanceSheetCreateSensitive => Entity.Employee != null;
		public bool IssuanceSheetOpenVisible => Entity.IssuanceSheet != null;
		public bool IssuanceSheetPrintVisible => Entity.IssuanceSheet != null;
		public bool WriteoffOpenVisible => Entity.WriteOffDoc != null;
		public string WriteoffDocNumber => (Entity.WriteOffDoc?.Id > 0) ? Entity.WriteOffDoc?.Id.ToString() : null;
		#endregion

		#region Заполение документа
		private void FillUnderreceived(PerformanceHelper performance)
		{
			Entity.ObservableItems.Clear();

			if(Entity.Employee == null)
				return;

			Entity.Employee.FillWearReceivedInfo(issueRepository);
			performance.CheckPoint(nameof(Entity.Employee.FillWearReceivedInfo));
			Entity.Employee.FillWearInStockInfo(UoW, baseParameters, Entity.Warehouse, Entity.Date, onlyUnderreceived: false);
			performance.CheckPoint(nameof(Entity.Employee.FillWearInStockInfo));

			foreach(var item in Entity.Employee.WorkwearItems) {
				Entity.AddItem(item, baseParameters);
			}
			performance.CheckPoint("Заполняем строки документа");
		}

		private void UpdateAmounts() {
			foreach(var item in Entity.Items) {
				item.Amount = item.EmployeeCardItem?.CalculateRequiredIssue(baseParameters, Entity.Date) ?? 0;
			}
		}
		#endregion

		public void FillAktNumber()
		{
			foreach(var item in Entity.WriteOffDoc.Items)
				foreach(var i in Entity.Items)
					if(item.Nomenclature == i.Nomenclature)
						i.AktNumber = item.AktNumber;
		}

		public bool SkipBarcodeCheck;
		
		public override bool Save()
		{
			progress.Start(7);
			if(!Validate()) {
				progress.Close();
				return false;
			}
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;

			if(!SkipBarcodeCheck && DocItemsEmployeeViewModel.SensitiveCreateBarcodes) {
				interactive.ShowMessage(ImportanceLevel.Error, "Перед окончательным сохранением необходимо обновить штрихкоды.");
				return false;
			}

			//Так как сохранение достаточно сложное, рядом сохраняется еще два документа, при чтобы оно не ломалось из за зависимостей между объектами.
			//Придерживайтесь следующего порядка:
			// 1 - Из уже существующих документов удаляем строки которые удалены в основном.
			// 2 - Сохраняем основной документ, без закрытия транзакции.
			// 3 - Обрабатываем и сохраняем доп документы.
			
			logger.Info("Обработка строк документа выдачи...");
			progress.Add();
			Entity.CleanupItems();
			Entity.CleanupItemsWriteOff();
			progress.Add();
			if(Entity.Items.Any(x => x.IsWriteOff) && Entity.WriteOffDoc == null) {
				Entity.WriteOffDoc = new Writeoff();
				Entity.WriteOffDoc.Date = Entity.Date;
				Entity.WriteOffDoc.CreatedbyUser = Entity.CreatedbyUser;
			}
			if(Entity.WriteOffDoc != null) {
				logger.Info("Предварительная запись списания...");
				UoW.Save(Entity.WriteOffDoc);
			}
			progress.Add();
			Entity.CleanupIssuanceSheetItems();
			if(Entity.IssuanceSheet != null) {
				logger.Info("Предварительная запись ведомости...");
				UoW.Save(Entity.IssuanceSheet);
			}
			progress.Add();
			logger.Info("Запись документа выдачи...");
			Entity.UpdateOperations(UoW, baseParameters, interactive);
			UoWGeneric.Session.SaveOrUpdate(Entity); //Здесь сохраняем таким способом чтобы транзакция не закрылась.
			
			progress.Add();
			Entity.UpdateIssuanceSheet();
			if(Entity.IssuanceSheet != null)
				UoW.Save(Entity.IssuanceSheet);

			progress.Add();
			Entity.UpdateIssuedWriteOffOperation();
			if(Entity.WriteOffDoc != null)
				UoW.Save(Entity.WriteOffDoc);

			progress.Add();
			logger.Info("Обновляем записи о выданной одежде в карточке сотрудника...");
			Entity.UpdateEmployeeWearItems();
			UoWGeneric.Commit();
			logger.Info("Ok");
			progress.Close();
			return true;
		}

		#region Ведомость
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
			var userSettings = userRepository.GetCurrentUserSettings(UoW);
			Entity.CreateIssuanceSheet(userSettings);
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
		#endregion
		#region Списание
		public void OpenWriteoff() {
			if(UoW.HasChanges) {
				if(!interactive.Question("В документе были изменения. Сохранить документ выдачи перед открытием связанного списания?") || !Save())
					return;
			}
			MainClass.MainWin.NavigationManager.OpenViewModel<WriteOffViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.WriteOffDoc.Id));
		}
		#endregion

		public void EntityChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(Entity.Date):
					if(interactive.Question("Обновить количество по потребности на новую дату документа?"))
						UpdateAmounts();
					break;
				case nameof(Entity.Employee):
					var performance = new PerformanceHelper("Обновление строк документа", logger);
					FillUnderreceived(performance);
					OnPropertyChanged(nameof(IssuanceSheetCreateSensitive));
					performance.PrintAllPoints(logger);
					break;
				case nameof(Entity.IssuanceSheet):
					OnPropertyChanged(nameof(IssuanceSheetCreateVisible));
					OnPropertyChanged(nameof(IssuanceSheetOpenVisible));
					OnPropertyChanged(nameof(IssuanceSheetPrintVisible));
					break;
				case nameof(Entity.WriteOffDoc):
					OnPropertyChanged(nameof(WriteoffOpenVisible));
					break;
			}
		}

		#region ISelectItem
		public void SelectItem(int id)
		{
			DocItemsEmployeeViewModel.SelectedItem = Entity.Items.FirstOrDefault(x => x.Id == id);
		}
		#endregion
	}
}
