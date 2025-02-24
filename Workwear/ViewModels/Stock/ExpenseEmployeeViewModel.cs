using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.IO;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NHibernate.SqlCommand;
using NLog;
using QS.Dialog;
using QS.DomainModel.Entity;
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
using Workwear.Models.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.Tools.User;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Statements;

namespace Workwear.ViewModels.Stock {
	public class ExpenseEmployeeViewModel : EntityDialogViewModelBase<Expense>, ISelectItem
	{
		private ILifetimeScope autofacScope;
		private readonly SizeService sizeService;
		private readonly CurrentUserSettings currentUserSettings;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public ExpenseDocItemsEmployeeViewModel DocItemsEmployeeViewModel;
		private IInteractiveService interactive;
		private readonly CommonMessages commonMessages;
		private readonly FeaturesService featuresService;
		private readonly BaseParameters baseParameters;
		private readonly IProgressBarDisplayable globalProgress;
		private readonly ModalProgressCreator modalProgressCreator;
		private readonly StockBalanceModel stockBalanceModel;
		private readonly EmployeeIssueModel issueModel;

		public ExpenseEmployeeViewModel(IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			IValidator validator,
			IUserService userService,
			CurrentUserSettings currentUserSettings,
			SizeService sizeService,
			IInteractiveService interactive,
			IProgressBarDisplayable globalProgress,
			ModalProgressCreator modalProgressCreator,
			StockBalanceModel stockBalanceModel,
			StockRepository stockRepository,
			CommonMessages commonMessages,
			FeaturesService featuresService,
			BaseParameters baseParameters,
			EmployeeIssueModel issueModel,
			EmployeeCard employee = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.currentUserSettings = currentUserSettings ?? throw new ArgumentNullException(nameof(currentUserSettings));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.globalProgress = globalProgress ?? throw new ArgumentNullException(nameof(globalProgress));
			this.modalProgressCreator = modalProgressCreator ?? throw new ArgumentNullException(nameof(modalProgressCreator));
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));

			var performance = new ProgressPerformanceHelper(globalProgress, employee == null ? 5u : 12u, "Загружаем размеры", logger);
			var ownersQuery = UoW.Session.QueryOver<Owner>().Future();
			this.sizeService.RefreshSizes(UoW); //Загружаем размеры
			performance.CheckPoint("Предварительная загрузка документа");
			var entryBuilder = new CommonEEVMBuilderFactory<Expense>(this, Entity, UoW, navigation, autofacScope);
			if(UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
			}
			else {
				//Предварительно загружаем все связанные сущности, чтобы не было дополнительных запросов.
				PreloadingDoc();
			}
			performance.CheckPoint("Заполняем сотрудника");

			if(employee != null) {
				Entity.Employee = UoW.Session.QueryOver<EmployeeCard>()
					.Where(x => x.Id == employee.Id)
					.Fetch(SelectMode.Fetch, x => x.Subdivision)
					.Fetch(SelectMode.Fetch, x => x.Subdivision.Warehouse)
					.SingleOrDefault();
				Entity.Warehouse = Entity.Employee.Subdivision?.Warehouse;
			}
			performance.CheckPoint("Заполняем склад");

			if(Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			stockBalanceModel.Warehouse = Entity.Warehouse;
			stockBalanceModel.OnDate = Entity.Date;
			if(employee != null) {
				performance.StartGroup("FillUnderreceived");
				FillUnderreceived(performance);
				performance.EndGroup();
			}

			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse)
									.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
									.UseViewModelDialog<WarehouseViewModel>()
									.Finish();
			EmployeeCardEntryViewModel = entryBuilder.ForProperty(x => x.Employee)
									.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
									.UseViewModelDialog<EmployeeViewModel>()
									.Finish();

			performance.CheckPoint("Создаем дочерние модели");
			var parameter = new TypedParameter(typeof(ExpenseEmployeeViewModel), this);
			var parameter2 = new TypedParameter(typeof(IList<Owner>), ownersQuery.ToList());
			DocItemsEmployeeViewModel = this.autofacScope.Resolve<ExpenseDocItemsEmployeeViewModel>(parameter, parameter2);
			Entity.PropertyChanged += EntityChange;
			
			if(UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
				logger.Info("Создание Нового документа выдачи");
			} else AutoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
			//Переопределяем параметры валидации
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));
			performance.End();
		}

		#region EntityViewModels
		public readonly EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		public readonly EntityEntryViewModel<EmployeeCard> EmployeeCardEntryViewModel;
		#endregion

		#region Свойства для View
		public bool IssuanceSheetCreateVisible => Entity.IssuanceSheet == null;
		public bool IssuanceSheetCreateSensitive => Entity.Employee != null;
		public bool IssuanceSheetOpenVisible => Entity.IssuanceSheet != null;
		public bool IssuanceSheetPrintVisible => Entity.IssuanceSheet != null;
		public bool SensitiveDocNumber => !AutoDocNumber;
		
		private bool autoDocNumber = true;
		[PropertyChangedAlso(nameof(DocNumberText))]
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		public bool AutoDocNumber {
			get => autoDocNumber;
			set => SetField(ref autoDocNumber, value);
		}

		public string DocNumberText {
			get => AutoDocNumber ? (Entity.Id == 0 ? "авто" : Entity.Id.ToString()) : Entity.DocNumberText;
			set { 
				if(!AutoDocNumber) 
					Entity.DocNumber = value; 
			}
		}
		#endregion

		#region Заполение документа

		private void PreloadingDoc() {
			var expenseQuery = UoW.Session.QueryOver<Expense>()
				.Fetch(SelectMode.ChildFetch, x => x)
				.Fetch(SelectMode.Skip, x => x.IssuanceSheet)
				.Fetch(SelectMode.Fetch, x => x.CreatedbyUser)
				.Fetch(SelectMode.Fetch, x => x.Employee)
				.Fetch(SelectMode.Fetch, x => x.Warehouse)
				.Fetch(SelectMode.Fetch, x => x.IssuanceSheet)
				.Where(x => x.Id == Entity.Id)
				.Future();
				
			ExpenseItem expenseItemAlias = null;
				
			UoW.Session.QueryOver<Expense>()
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

			expenseQuery.SingleOrDefault();
		}

		private void FillUnderreceived(PerformanceHelper performance)
		{
			Entity.Items.Clear();

			if(Entity.Employee == null)
				return;

			performance.CheckPoint("Предварительная загрузка сотрудника");
			issueModel.PreloadEmployeeInfo(Entity.Employee.Id);
			performance.CheckPoint("Предварительная загрузка потребностей");
			issueModel.PreloadWearItems(Entity.Employee.Id);
			performance.CheckPoint(nameof(Entity.Employee.FillWearReceivedInfo));
			issueModel.FillWearReceivedInfo(new []{Entity.Employee});
			performance.CheckPoint(nameof(issueModel.FillWearInStockInfo));
			issueModel.FillWearInStockInfo(Entity.Employee, stockBalanceModel);

			performance.CheckPoint("Заполняем строки документа");
			foreach(var item in Entity.Employee.WorkwearItems) {
				Entity.AddItem(item, baseParameters);
			}
		}

		private void UpdateAmounts() {
			foreach(var item in Entity.Items) {
				item.Amount = item.EmployeeCardItem?.CalculateRequiredIssue(baseParameters, Entity.Date) ?? 0;
			}
		}
		#endregion

		public bool SkipBarcodeCheck;
		
		public override bool Save()
		{
			logger.Info("Проверка документа...");
			if(!Validate()) {
				logger.Warn("Документ не корректен, сохранение отменено.");
				return false;
			}
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;
			
			if(AutoDocNumber)
				Entity.DocNumber = null;
			else if(String.IsNullOrWhiteSpace(Entity.DocNumber))
				Entity.DocNumber = Entity.DocNumberText;	
			
			if(!SkipBarcodeCheck && DocItemsEmployeeViewModel.SensitiveCreateBarcodes) {
				interactive.ShowMessage(ImportanceLevel.Error, "Перед окончательным сохранением необходимо обновить штрихкоды.");
				logger.Warn("Необходимо обновить штрихкоды.");
				return false;
			}

			//Так как сохранение достаточно сложное, рядом сохраняется еще два документа, при чтобы оно не ломалось из за зависимостей между объектами.
			//Придерживайтесь следующего порядка:
			// 1 - Из уже существующих документов удаляем строки которые удалены в основном.
			// 2 - Сохраняем основной документ, без закрытия транзакции.
			// 3 - Обрабатываем и сохраняем доп документы.

			var performance = new ProgressPerformanceHelper(modalProgressCreator, 6, "Очистка строк документа выдачи...", logger, true);
			Entity.CleanupItems();
			
			performance.CheckPoint("Предварительная запись ведомости...");
			Entity.CleanupIssuanceSheetItems();
			if(Entity.IssuanceSheet != null) {
				logger.Info("Предварительная запись ведомости...");
				UoW.Save(Entity.IssuanceSheet);
			}
			performance.CheckPoint("Запись документа выдачи...");
			Entity.UpdateOperations(UoW, baseParameters, interactive);
			UoWGeneric.Session.SaveOrUpdate(Entity); //Здесь сохраняем таким способом чтобы транзакция не закрылась.
			
			performance.CheckPoint("Запись ведомости...");
			Entity.UpdateIssuanceSheet();
			if(Entity.IssuanceSheet != null)
				UoW.Save(Entity.IssuanceSheet);

			performance.CheckPoint("Обновляем записи в карточке сотрудника...");
			Entity.UpdateEmployeeWearItems();
			performance.CheckPoint("Завершение транзакции...");
			UoWGeneric.Commit();
			performance.End();
			logger.Info($"Документ сохранен за {performance.TotalTime.TotalSeconds} сек.");
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
			var defaultOrganization = UoW.GetInSession(currentUserSettings.Settings.DefaultOrganization);
			var defaultLeader = UoW.GetInSession(currentUserSettings.Settings.DefaultLeader);
			var defaultResponsiblePerson = UoW.GetInSession(currentUserSettings.Settings.DefaultResponsiblePerson);
			Entity.CreateIssuanceSheet(defaultOrganization, defaultLeader, defaultResponsiblePerson);
		}

		public void PrintIssuanceSheet(IssuedSheetPrint doc)
		{
			if(UoW.HasChanges) {
				if(!commonMessages.SaveBeforePrint(Entity.GetType(), doc == IssuedSheetPrint.AssemblyTask ? "задания на сборку" : "ведомости") || !Save())
					return;
			}

			var reportInfo = new ReportInfo {
				Title = doc == IssuedSheetPrint.AssemblyTask ? $"Задание на сборку №{Entity.IssuanceSheet.DocNumber ?? Entity.IssuanceSheet.Id.ToString()}" 
					: $"Ведомость №{Entity.IssuanceSheet.DocNumber ?? Entity.IssuanceSheet.Id.ToString()} (МБ-7)",
				Identifier = doc.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.IssuanceSheet.Id },
					{"printPromo", featuresService.Available(WorkwearFeature.PrintPromo)}
				}
			};
			
			//Если пользователь не хочет сворачивать ФИО и табельник (настройка в базе)
			if((doc == IssuedSheetPrint.IssuanceSheet || doc == IssuedSheetPrint.IssuanceSheetVertical) && !baseParameters.CollapseDuplicateIssuanceSheet)
				reportInfo.Source = File.ReadAllText(reportInfo.GetPath()).Replace("<HideDuplicates>Data</HideDuplicates>", "<HideDuplicates></HideDuplicates>");

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
		#endregion

		public void EntityChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(Entity.Warehouse):
					stockBalanceModel.Warehouse = Entity.Warehouse;
					break;
				case nameof(Entity.Date):
					stockBalanceModel.OnDate = Entity.Date;
					if(interactive.Question("Обновить количество по потребности на новую дату документа?"))
						UpdateAmounts();
					break;
				case nameof(Entity.Employee):
					var performance = new ProgressPerformanceHelper(globalProgress, 6,"Обновление строк документа", logger);
					FillUnderreceived(performance);
					OnPropertyChanged(nameof(IssuanceSheetCreateSensitive));
					performance.End();
					break;
				case nameof(Entity.IssuanceSheet):
					OnPropertyChanged(nameof(IssuanceSheetCreateVisible));
					OnPropertyChanged(nameof(IssuanceSheetOpenVisible));
					OnPropertyChanged(nameof(IssuanceSheetPrintVisible));
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
