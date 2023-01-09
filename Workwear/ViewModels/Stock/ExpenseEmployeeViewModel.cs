using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.Utilities;
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
using QSReport;
using workwear.Domain.Company;
using workwear.Domain.Statements;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.Repository;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Tools.Features;
using workwear.ViewModels.Company;
using workwear.ViewModels.Statements;

namespace workwear.ViewModels.Stock
{
	public class ExpenseEmployeeViewModel : EntityDialogViewModelBase<Expense>, ISelectItem
	{
		ILifetimeScope autofacScope;
		private readonly UserRepository userRepository;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public ExpenseDocItemsEmployeeViewModel DocItemsEmployeeViewModel;
		IInteractiveQuestion interactive;
		private readonly CommonMessages commonMessages;
		private readonly FeaturesService featuresService;
		private readonly BaseParameters baseParameters;
		private readonly IProgressBarDisplayable progress;

		public ExpenseEmployeeViewModel(IEntityUoWBuilder uowBuilder, 
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
			BaseParameters baseParameters,
			IProgressBarDisplayable progress,
			EmployeeCard employee = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			this.interactive = interactive;
			this.commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
			var entryBuilder = new CommonEEVMBuilderFactory<Expense>(this, Entity, UoW, navigation, autofacScope);
			if(UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
				Entity.Operation = ExpenseOperations.Employee;
			}
			if(Entity.Operation != ExpenseOperations.Employee)
				throw new InvalidOperationException("Диалог предназначен только для операций выдачи сотруднику.");

			if(employee != null) {
				Entity.Employee = UoW.GetById<EmployeeCard>(employee.Id);
				Entity.Warehouse = Entity.Employee.Subdivision?.Warehouse;
			}

			if(Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			if(employee != null)
				FillUnderreceived();

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
			DocItemsEmployeeViewModel = this.autofacScope.Resolve<ExpenseDocItemsEmployeeViewModel>(parameter);
			Entity.PropertyChanged += EntityChange;

			//Переопределяем параметры валидации
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));
		}

		#region EntityViewModels
		public EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		public EntityEntryViewModel<EmployeeCard> EmployeeCardEntryViewModel;
		#endregion

		private void FillUnderreceived()
		{
			Entity.ObservableItems.Clear();

			if(Entity.Employee == null)
				return;

			Entity.Employee.FillWearInStockInfo(UoW, baseParameters, Entity.Warehouse, Entity.Date, onlyUnderreceived: false);

			foreach(var item in Entity.Employee.WorkwearItems) {
				Entity.AddItem(item, baseParameters);
			}
		}

		public void FillAktNumber()
		{
			foreach(var item in Entity.WriteOffDoc.Items)
				foreach(var i in Entity.Items)
					if(item.Nomenclature == i.Nomenclature)
						i.AktNumber = item.AktNumber;
		}

		public override bool Save()
		{
			progress.Start(7);
			if(!Validate()) {
				progress.Close();
				return false;
			}
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;

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

		public void OpenIssuenceSheet()
		{
			if(UoW.HasChanges) {
				if(!MessageDialogHelper.RunQuestionDialog("Сохранить документ выдачи перед открытием ведомости?") || !Save())
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

		public void EntityChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Employee)) {
				FillUnderreceived();
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
