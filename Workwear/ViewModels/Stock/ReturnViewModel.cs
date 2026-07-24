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
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.Filter.ViewModels.Company;
using workwear.Journal.ViewModels.ClothingService;
using Workwear.Journal.Filter.ViewModels.Regulations;
using Workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Journal.ViewModels.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Stock;
using Workwear.Models.Operations;
using Workwear.Repository.Operations;
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
			OverNormOperationRepository overNormOperationRepository,
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
			this.overNormOperationRepository = overNormOperationRepository ?? throw new ArgumentNullException(nameof(overNormOperationRepository));
			DutyNorm = UoW.GetInSession(dutyNorm);
			featuresService = autofacScope.Resolve<FeaturesService>();
			SetDocumentDateProperty(e => e.Date);
			
			if(featuresService.Available(WorkwearFeature.Owners))
				owners = UoW.GetAll<Owner>().ToList();
			if(Entity.Id == 0) {
				logger.Info("Создание Нового документа выдачи");
				Entity.CreatedbyUser = userService.GetCurrentUser();
				EmployeeCard = UoW.GetInSession(employee);
				Entity.Warehouse = UoW.GetInSession(warehouse) ?? stockRepository.GetDefaultWarehouse(featuresService, userService.CurrentUserId);
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
		private readonly IInteractiveService interactiveService;
		private readonly DutyNormRepository dutyNormRepository;
		private readonly OverNormOperationRepository overNormOperationRepository;
		
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
		public ServiceClaim ServiceClaim { get; }

		#endregion 
		
		#region Свойства для View
		public bool CanChangeDocDate => CanEdit && PermissionService.ValidatePresetPermission("can_change_document_date");
		public virtual bool CanAddEmployee => CanEdit;
		public virtual bool CanAddDutyNorms => CanEdit;
		public virtual bool CanAddOverNorm => CanEdit && featuresService.Available(WorkwearFeature.OverNorm);
		public virtual bool CanAddClaim => CanEdit && CanAddOverNorm;
		public virtual bool CanRemoveItem => CanEdit && SelectedItem != null;
		public virtual bool CanSetNomenclature => CanEdit && SelectedItem != null;
		public virtual bool CanEditItems => CanEdit && EmployeeCard != null;
		public virtual bool OwnersVisible => featuresService.Available(WorkwearFeature.Owners);
		public virtual bool WarehouseVisible => featuresService.Available(WorkwearFeature.Warehouses);
		public virtual bool ClaimVisible => featuresService.Available(WorkwearFeature.ClothingService) && CanAddOverNorm;
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
					OpenPageOptions.AsSlave,
					addingRegistrations: builder => { builder.RegisterInstance<Action<EmployeeBalanceFilterViewModel>>(
						filter => {
							filter.DateSensitive = false;
							filter.CheckShowWriteoffVisible = false;
							filter.SubdivisionSensitive = true;
							filter.EmployeeSensitive = true;
							filter.Date = Entity.Date;
							filter.CanChooseAmount = false;
						});
					});
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
					OpenPageOptions.AsSlave,
					addingRegistrations: builder => {
						builder.RegisterInstance<Action<DutyNormBalanceFilterViewModel>>(
							filter => {
								filter.DateSensitive = false;
								filter.SubdivisionSensitive =  DutyNorm == null;
								filter.DutyNormSensitive = DutyNorm == null;
								filter.Date = Entity.Date;
							});
					}
				);
			selectJournal.ViewModel.OnSelectResult += (sender, e) => AddFromDictionaryDutyNorm(
				e.GetSelectedObjects<DutyNormBalanceJournalNode>().ToDictionary(
					k => k.Id,
					v => v.Balance));
		}

		public void AddFromOverNorm() {
			var selectJournal = NavigationManager.OpenViewModel<OverNormIssuedJournalViewModel>(
				this,
				OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Employee = EmployeeCard;
			selectJournal.ViewModel.OnSelectResult += (sender, e) => AddFromOverNormNodes(
				e.GetSelectedObjects<OverNormIssuedJournalNode>());
		}

		public void AddFromClaim() {
			var selectJournal =
				NavigationManager.OpenViewModel<ClaimsJournalViewModel, ServiceClaim>(
					this,
					ServiceClaim,
					OpenPageOptions.AsSlave
				);
			selectJournal.ViewModel.Filter.ShowClosed = false;
			selectJournal.ViewModel.Filter.SensitiveShowClosed = false;
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += (sender, e) => AddFromClaimNodes(
				e.GetSelectedObjects<ClaimsJournalNode>().ToDictionary(
					k=>k.Id,
					v=>v.Balance
					));
		}

		public void AddFromClaimNodes(Dictionary<int, int> returningOperation) {
			Barcode barcodeAlias = null;
			var claims = UoW.Session.QueryOver<ServiceClaim>()
				.Where(x=>x.Id.IsIn(returningOperation.Select(i=>i.Key).ToList()))
				.JoinAlias(x=>x.Barcode,  () => barcodeAlias)
				.Fetch(SelectMode.Fetch, x => x.Employee)
				.Fetch(SelectMode.Fetch, () => barcodeAlias.Nomenclature)
				.Fetch(SelectMode.Fetch, () => barcodeAlias.Size)
				.Fetch(SelectMode.Fetch,() => barcodeAlias.Height)
				.List();
			var notAddedClaims = new List<int>();
			foreach(var claim  in claims) {
				var issuedOperation = overNormOperationRepository.GetActualIssuedOperation(claim, UoW);
				if(issuedOperation == null) {
					notAddedClaims.Add(claim.Id);
					continue;
				}
				Entity.AddItem(issuedOperation, 1, claim);
			}
			if(notAddedClaims.Any())
				interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"Заявки №{String.Join(", ", notAddedClaims)} не добавлены. Для их штрихкодов не найдена действующая выдача сверх нормы.");
			CalculateTotal();
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

		public void AddFromOverNormDictionary(Dictionary<int, int> returningOperation) {
			var operations = overNormOperationRepository.GetIssuedOperations(returningOperation.Keys, UoW);
			foreach(var operation in operations) {
				Entity.AddItem(operation, returningOperation[operation.Id]);
			}
			CalculateTotal();
		}

		private void AddFromOverNormNodes(IList<OverNormIssuedJournalNode> nodes) {
			foreach(var node in nodes) {
				var operation = overNormOperationRepository.GetIssuedOperation(node.Id, UoW);
				var availableBarcodeIds = overNormOperationRepository.GetAvailableBarcodeIdsForReturn(operation, UoW);
				if(availableBarcodeIds.Count > 1) {
					OpenOverNormBarcodeSelector(operation, availableBarcodeIds);
					continue;
				}

				var barcodes = availableBarcodeIds.Count == 1
					? UoW.Session.QueryOver<Barcode>()
						.Where(x => x.Id == availableBarcodeIds[0])
						.List()
					: null;
				Entity.AddItem(operation, availableBarcodeIds.Count == 1 ? 1 : node.Amount, barcodes: barcodes);
			}
			CalculateTotal();
		}

		private void OpenOverNormBarcodeSelector(OverNormOperation operation, IList<int> availableBarcodeIds) {
			var barcodeJournal = NavigationManager.OpenViewModel<BarcodeJournalViewModel>(
				this,
				OpenPageOptions.AsSlave,
				addingRegistrations: builder => {
					builder.RegisterInstance<Action<BarcodeJournalFilterViewModel>>(filter => {
						filter.CanUseFilter = false;
						filter.AllowedBarcodeIds = availableBarcodeIds;
					});
				});
			barcodeJournal.Tag = operation;
			barcodeJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			barcodeJournal.ViewModel.OnSelectResult += AddSelectedOverNormBarcodes;
		}

		private void AddSelectedOverNormBarcodes(object sender, JournalSelectedEventArgs e) {
			var page = NavigationManager.FindPage((BarcodeJournalViewModel)sender);
			var operation = (OverNormOperation)page.Tag;
			var barcodeIds = e.GetSelectedObjects<BarcodeJournalNode>()
				.Select(x => x.Id)
				.ToArray();
			var barcodes = UoW.Session.QueryOver<Barcode>()
				.WhereRestrictionOn(x => x.Id).IsIn(barcodeIds)
				.List();

			Entity.AddItem(operation, barcodes.Count, barcodes: barcodes);
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
			var itemsOverNorm = Entity.Items.Where(i => i.ReturnFrom == ReturnFrom.OverNorm).ToList();
			if(itemsOverNorm.Any()) {
				var operations = itemsOverNorm
					.Select(i => i.IssuedOverNormOperation)
					.ToArray();
				var writtenOff = CalculateOverNormReturned(operations);
				foreach(var item in itemsOverNorm) {
					var operation = item.IssuedOverNormOperation;
					item.MaxAmount = operation.WarehouseOperation.Amount - (writtenOff.ContainsKey(operation.Id) ? writtenOff[operation.Id] : 0);
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
			
			if (Entity.Id != 0)
			{
				var oldReturnClaims = UoW.Session.QueryOver<ReturnItem>()
					.Where(x => x.Document.Id == Entity.Id)
					.List();

				var currentReturnClaims = Entity.Items.Select(x => x.Id);

				var removedReturnClaims = oldReturnClaims
					.Where(x => !currentReturnClaims.Contains(x.Id) && x.ServiceClaim != null)
					.ToList();

				if (removedReturnClaims.Any())
				{
					logger.Info("Обнаружены удалённые строки возврата со стирки, обновляем статус...");

					foreach (var removedReturnClaim in removedReturnClaims)
					{
						removedReturnClaim.ServiceClaim.IsClosed = false;
						removedReturnClaim.ServiceClaim.ChangeState(ClaimState.AwaitIssue);

					}
					UoW.Commit();
				}
			}
			
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

		private Dictionary<int, int> CalculateOverNormReturned(OverNormOperation[] operations) {
			var currentReturnOperationIds = Entity.Items
				.Where(i => i.ReturnFrom == ReturnFrom.OverNorm)
				.Select(i => i.ReturnFromOverNormOperation?.Id ?? 0)
				.Where(id => id > 0)
				.ToArray();

			return overNormOperationRepository.GetReturnedAmounts(operations, currentReturnOperationIds, UoW);
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
