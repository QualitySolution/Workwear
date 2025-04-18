using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Stock;
using Workwear.Models.Operations;
using Workwear.Repository.Regulations;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.Stock {
	public class ReturnViewModel  : PermittingEntityDialogViewModelBase<Return>, IDialogDocumentation {
		public ReturnViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			IUserService userService,
			EmployeeIssueModel issueModel, 
			StockRepository stockRepository,
			StockBalanceModel stockBalanceModel,
			IInteractiveService interactiveService,
      DutyNormRepository dutyNormRepository,
			ICurrentPermissionService permissionService,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null,
			EmployeeCard employee = null,
			Warehouse warehouse = null,
      DutyNorm dutyNorm = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, permissionService, interactiveService, validator, unitOfWorkProvider)
		{
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.interactiveService = interactiveService;
			this.dutyNormRepository=dutyNormRepository ?? throw new ArgumentNullException(nameof(dutyNormRepository));
			DutyNorm = UoW.GetInSession(dutyNorm);
			featuresService = autofacScope.Resolve<FeaturesService>();
			SetDocumentDateProperty(e => e.Date);
			
			if(featuresService.Available(WorkwearFeature.Owners))
				owners = UoW.GetAll<Owner>().ToList();
			if(Entity.Id == 0) {
				logger.Info("Создание Нового документа выдачи");
				Entity.CreatedbyUser = userService.GetCurrentUser();
				EmployeeCard = UoW.GetInSession(employee);
				Entity.Warehouse = UoW.GetInSession(warehouse) ?? stockRepository.GetDefaultWarehouse(UoW, featuresService, userService.CurrentUserId);
			} 
			else 
				AutoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
			var entryBuilder = new CommonEEVMBuilderFactory<Return>(this, Entity, UoW, navigation, autofacScope);
			
			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			WarehouseEntryViewModel.IsEditable = CanEdit;

			CalculateTotal();
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock-documents.html#employee-return");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion

		#region Проброс свойств документа
		public virtual UserBase DocCreatedbyUser => Entity.CreatedbyUser;
		public virtual string DocComment { get => Entity.Comment; set => Entity.Comment = value;}
		public virtual IObservableList<ReturnItem> Items => Entity.Items;
		public virtual EmployeeCard EmployeeCard {
			set {
				if(value == null || value.Id == 0 || DomainHelper.EqualDomainObjects(EmployeeCard, value))
					return;
				UoW.GetById<EmployeeCard>(value.Id);
				issueModel.PreloadEmployeeInfo(value.Id);
				issueModel.PreloadWearItems(value.Id);
				issueModel.FillWearInStockInfo(new[] { value }, stockBalanceModel);
				issueModel.FillWearReceivedInfo(new[] { value });
				foreach(var item in Items) {
					item.EmployeeCard = value;
				}
			}
			get {
				foreach(var item in Items)
					return item.EmployeeCard;
				return null;
			}
		}
		#endregion

		#region Свойства ViewModel
		private readonly FeaturesService featuresService;
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		public readonly EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		private readonly EmployeeIssueModel issueModel;
		private readonly StockBalanceModel stockBalanceModel;
		private IInteractiveService interactiveService;
		private DutyNormRepository dutyNormRepository;
		
		private List<Owner> owners = new List<Owner>();
		public List<Owner> Owners => owners;
		
		private ReturnItem selectedItem;
		[PropertyChangedAlso(nameof(CanRemoveItem))]
		[PropertyChangedAlso(nameof(CanSetNomenclature))]
		public virtual ReturnItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}
		
		private string total;
		public string Total {
			get => total;
			set => SetField(ref total, value);
		}
		public DutyNorm DutyNorm { get; }

		#endregion 
		
		#region Свойства для View
		public virtual bool CanAddEmployee => CanEdit;
		public virtual bool CanAddDutyNorms => CanEdit;
		public virtual bool CanRemoveItem => CanEdit && SelectedItem != null;
		public virtual bool CanSetNomenclature => CanEdit && SelectedItem != null;
		public virtual bool CanEditItems => CanEdit && EmployeeCard != null;
		public virtual bool OwnersVisible => featuresService.Available(WorkwearFeature.Owners);
		public virtual bool WarehouseVisible => featuresService.Available(WorkwearFeature.Warehouses);
		public bool SensitiveDocNumber => CanEdit && !AutoDocNumber;
		
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

		#region Методы

		public void AddFromEmployee() {
			var selectJournal =
				NavigationManager.OpenViewModel<EmployeeBalanceJournalViewModel, EmployeeCard>(
					this,
					EmployeeCard,
					OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.DateSensitive = false;
			selectJournal.ViewModel.Filter.CheckShowWriteoffVisible = false;
			selectJournal.ViewModel.Filter.SubdivisionSensitive = true;
			selectJournal.ViewModel.Filter.EmployeeSensitive = true;
			selectJournal.ViewModel.Filter.Date = Entity.Date;
			selectJournal.ViewModel.Filter.CanChooseAmount = false;
			selectJournal.ViewModel.OnSelectResult += (sender, e) => AddFromDictionary(
				e.GetSelectedObjects<EmployeeBalanceJournalNode>().ToDictionary(
					k => k.Id,
					v => ((EmployeeBalanceJournalViewModel)sender).Filter.AddAmount == AddedAmount.One ? 1 :
						((EmployeeBalanceJournalViewModel)sender).Filter.AddAmount == AddedAmount.Zero ? 0 :
						v.Balance)
			);
		}

		public void AddFromDutyNorm() {
			var selectJournal =
				NavigationManager.OpenViewModel<DutyNormBalanceJournalViewModel, DutyNorm>(
					this,
					DutyNorm,
					OpenPageOptions.AsSlave
				);
			selectJournal.ViewModel.Filter.DateSensitive = false;
			selectJournal.ViewModel.Filter.SubdivisionSensitive =  DutyNorm == null;
			selectJournal.ViewModel.Filter.DutyNormSensitive = DutyNorm == null;
			selectJournal.ViewModel.Filter.Date = Entity.Date;
			selectJournal.ViewModel.OnSelectResult += (sender, e) => AddFromDictionaryDutyNorm(
				e.GetSelectedObjects<DutyNormBalanceJournalNode>().ToDictionary(
					k => k.Id,
					v => v.Balance));
		}

		/// <param name="returningOperation">Dictionary(operationId,amount)</param>
		public void AddFromDictionary(Dictionary<int, int> returningOperation) {
			var operations = UoW.Session.QueryOver<EmployeeIssueOperation>()
				.Where(x => x.Id.IsIn(returningOperation.Select(i => i.Key).ToList()))
				.Fetch(SelectMode.Fetch, x => x.NormItem)
				.Fetch(SelectMode.Fetch, x => x.IssuedOperation)
				.Fetch(SelectMode.Fetch, x => x.Employee)
				.Fetch(SelectMode.Fetch, x => x.Nomenclature)
				.Fetch(SelectMode.Fetch, x => x.WearSize)
				.Fetch(SelectMode.Fetch, x => x.Height)
				.List(); 
			foreach(var operation in operations) {
				Entity.AddItem(operation, returningOperation[operation.Id]);
			}
			CalculateTotal();
		}
		public void AddFromDictionaryDutyNorm(Dictionary<int, int> returningOperation) {
			var operations = UoW.Session.QueryOver<DutyNormIssueOperation>()
				.Where(x => x.Id.IsIn(returningOperation.Select(i => i.Key).ToList()))
				.Fetch(SelectMode.Fetch, x => x.DutyNormItem)
				.Fetch(SelectMode.Fetch, x => x.IssuedOperation)
				.Fetch(SelectMode.Fetch, x => x.DutyNorm)
				.Fetch(SelectMode.Fetch, x => x.Nomenclature)
				.Fetch(SelectMode.Fetch, x => x.WearSize)
				.Fetch(SelectMode.Fetch, x => x.Height)
				.List(); 
			foreach(var operation in operations) {
				Entity.AddItem(operation, returningOperation[operation.Id]);
			}
			CalculateTotal();
		}

		public void FillMaxAmount(DateTime? date = null) {
			var itemsEmp = Entity.Items.Where(i=>i.ReturnFrom==ReturnFrom.Employee).ToList();
			if(itemsEmp.Any()) {
				// для всех списаний/возвратов с сотрудника
				var operations = itemsEmp
					.Select(i=>i.ReturnFromEmployeeOperation)
					.Select(o=>o.IssuedOperation)
					.ToArray();
				var writtenOff=issueModel.CalculateWrittenOff(operations, UoW, date);
				foreach(var item in itemsEmp) {
					var operation = item.ReturnFromEmployeeOperation.IssuedOperation;
					item.MaxAmount = operation.Issued - (writtenOff.ContainsKey(operation.Id) ? writtenOff[operation.Id] : 0);
				}
			}
			var itemsDutyNorm = Entity.Items.Where(i=>i.ReturnFrom==ReturnFrom.DutyNorm).ToList();
			if(itemsDutyNorm.Any()) {
				// для всех списаний/возвратов с дежурной нормы
				var operations = itemsDutyNorm
					.Select(i=>i.ReturnFromDutyNormOperation)
					.Select(o=>o.IssuedOperation)
					.ToArray();
				var writtenOff=dutyNormRepository.CalculateWrittenOff(operations, UoW, date);
				foreach(var item in itemsDutyNorm) {
					var operation = item.ReturnFromDutyNormOperation.IssuedOperation;
					item.MaxAmount = operation.Issued - (writtenOff.ContainsKey(operation.Id) ? writtenOff[operation.Id] : 0);
				}
			}
		}
		
		public void DeleteItem(ReturnItem item) {
			Entity.RemoveItem(item); 
			OnPropertyChanged(nameof(CanRemoveItem));
			OnPropertyChanged(nameof(CanSetNomenclature));
			CalculateTotal();
		}
		
		public void SetNomenclature(ReturnItem item) {
			var selectJournal = NavigationManager.OpenViewModel<NomenclatureJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.ProtectionTools = item.IssuedEmployeeOnOperation?.ProtectionTools; 
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Single;
			selectJournal.ViewModel.OnSelectResult += (s, je) =>  SetNomenclatureSelected(item,je.SelectedObjects.First().GetId());
		}

		private void SetNomenclatureSelected(ReturnItem item, int selectedId) {
			item.Nomenclature = UoW.GetById<Nomenclature>(selectedId);
		}
		
		private void CalculateTotal() {
			Total = $"Позиций в документе: {Entity.Items.Count}  " +
			        $"Количество единиц: {Entity.Items.Sum(x => x.Amount)}";
		}

		public override bool Save() {
			logger.Info ("Запись документа...");

			FillMaxAmount(Entity.Date);
			Entity.UpdateOperations(UoW);
			if (Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;

			if(!base.Save()) {
            	logger.Info("Не Ок.");
            	return false;
            }

			var employeeOperations = Entity.Items.Where(r => r.ReturnFrom == ReturnFrom.Employee)
				.Select(w => w.ReturnFromEmployeeOperation)
				.Where(w => w.ProtectionTools != null)
				.ToArray();
			if(employeeOperations.Any()) {
				logger.Info("Обновляем записи о выданной одежде в карточке сотрудника...");
				Entity.UpdateEmployeeWearItems(UoW);
				UoW.Commit();
			}
			
			logger.Info ("Ok");
			return true;
		}
		public void PrintReturnDoc(ReturnDocReportEnum doc) 
		{
			
			if (UoW.HasChanges && !interactiveService.Question("Перед печатью документ будет сохранён. Продолжить?"))
				return;
			if (!Save())
				return;
			
			var reportInfo = new ReportInfo {
				Title = doc == ReturnDocReportEnum.ReturnSheet ? $"Возврат №{Entity.DocNumber ?? Entity.Id.ToString()}"
					: $"Ведомость №{Entity.DocNumber ?? Entity.Id.ToString()}",
				Identifier = doc.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id },
					{"printPromo", featuresService.Available(WorkwearFeature.PrintPromo)}
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
		public enum ReturnDocReportEnum
		{
			[Display(Name = "Лист возврата")]
			[ReportIdentifier("Documents.ReturnSheet")]
			ReturnSheet,
			[Display(Name = "Ведомость возврата книжная")]
			[ReportIdentifier("Statements.ReturnStatementVertical")]
			ReturnStatementVertical,
			[Display(Name = "Ведомость возврата альбомная")]
			[ReportIdentifier("Statements.ReturnStatementHorizontal")]
			ReturnStatementHorizontal
		}
		
		#endregion
		
	}
}
