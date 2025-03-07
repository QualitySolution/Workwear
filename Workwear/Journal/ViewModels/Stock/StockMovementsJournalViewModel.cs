using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Utilities;
using QS.Utilities.Text;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Models.Operations;
using workwear.Models.Stock;
using Workwear.Tools.Features;

namespace workwear.Journal.ViewModels.Stock
{
	public class StockMovmentsJournalViewModel : JournalViewModelBase
	{
		private readonly OpenStockDocumentsModel openDocuments;
		public readonly FeaturesService FeaturesService;

		public StockMovementsFilterViewModel Filter { get; private set; }

		public StockMovmentsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			FeaturesService featuresService,
			OpenStockDocumentsModel openDocuments) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.openDocuments = openDocuments ?? throw new ArgumentNullException(nameof(openDocuments));
			JournalFilter = Filter = autofacScope.Resolve<StockMovementsFilterViewModel>
				(new TypedParameter(typeof(JournalViewModelBase), this));

			var dataLoader = new ThreadDataLoader<StockMovementsJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			CreateNodeActions();

			UpdateOnChanges(typeof(WarehouseOperation));
			TabName = "Складские движения";
		}

		protected IQueryOver<WarehouseOperation> ItemsQuery(IUnitOfWork uow)
		{
			StockMovementsJournalNode resultAlias = null;

			WarehouseOperation warehouseOperationAlias = null;

			Expense expenseAlias = null;
			CollectiveExpense collectiveExpenseAlias = null;
			ExpenseDutyNorm expenseDutyNormAlias = null;
			Income incomeAlias = null;
			Transfer transferAlias = null;
			Writeoff writeoffAlias = null;
			Completion completionResultAlias = null;
			Completion completionSourceAlias = null;
			
			ExpenseItem expenseItemAlias = null;
			ExpenseDutyNormItem expenseDutyNormItemAlias = null;
			IncomeItem incomeItemAlias = null;
			Return returnAlias = null;
			ReturnItem returnItemAlias = null;
			TransferItem transferItemAlias = null;
			CollectiveExpenseItem collectiveExpenseItemAlias = null;
			WriteoffItem writeoffItemAlias = null;
			EmployeeCard employeeCardAlias = null;
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			CompletionResultItem completionResultItemAlias = null;
			CompletionSourceItem completionSourceItemAlias = null;
			IssuanceSheet issuanceSheetAlias = null;
			IssuanceSheetItem issuanceSheetItem = null;

			Nomenclature nomenclatureAlias = null;
			ItemsType itemTypesAlias = null;
			MeasurementUnits unitsAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			Owner ownerAlias = null;

			var queryStock = uow.Session.QueryOver(() => warehouseOperationAlias)
				.JoinAlias(() => warehouseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => warehouseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin);

			if(Filter.Warehouse != null)
				queryStock.Where(x => x.ReceiptWarehouse == Filter.Warehouse || x.ExpenseWarehouse == Filter.Warehouse);

			if(Filter.StartDate.HasValue)
				queryStock.Where(x => x.OperationTime >= Filter.StartDate.Value);

			if(Filter.EndDate.HasValue)
				queryStock.Where(x => x.OperationTime < Filter.EndDate.Value.AddDays(1));

			if(Filter.StockPosition != null) {
				queryStock.Where(x => x.Nomenclature == Filter.StockPosition.Nomenclature);
				queryStock.Where(x => x.WearSize == Filter.StockPosition.WearSize);
				queryStock.Where(x => x.Height == Filter.StockPosition.Height);
				queryStock.Where(x => x.WearPercent == Filter.StockPosition.WearPercent);
				queryStock.Where(x => x.Owner == Filter.StockPosition.Owner);
			}

			if(Filter.Nomenclature != null)
				queryStock.Where(x => x.Nomenclature == Filter.Nomenclature);

			if(Filter.Size != null)
				queryStock.Where(x => x.WearSize.Id == Filter.Size.Id);

			if(Filter.Height != null)
				queryStock.Where(x => x.Height.Id == Filter.Height.Id);
			
			if(Filter.Owner != null)
				switch(Filter.Owner.Id) {
					case -1: //все
						break;
					case 0: //без собственника 
						queryStock.Where(x => x.Owner == null);
						break;
					default: //с указаным сбственником
						queryStock.Where(x => x.Owner.Id == Filter.Owner.Id);
						break;
				}
			
			IProjection receiptProjection, expenseProjection;
			if(Filter.Warehouse != null) {
				receiptProjection = Projections.Conditional(
						Restrictions.Eq(Projections
							.Property<WarehouseOperation>(x => x.ReceiptWarehouse.Id), Filter.Warehouse.Id),
						Projections.Constant(true),
						Projections.Constant(false)
					);
				expenseProjection = Projections.Conditional(
					Restrictions.Eq(Projections
						.Property<WarehouseOperation>(x => x.ExpenseWarehouse.Id), Filter.Warehouse.Id),
					Projections.Constant(true),
					Projections.Constant(false)
				);

				switch(Filter.Direction) {
					case DirectionOfOperation.expense:
						queryStock.Where(x => x.ExpenseWarehouse.Id == Filter.Warehouse.Id);
						break;
					case DirectionOfOperation.receipt:
						queryStock.Where(x => x.ReceiptWarehouse.Id == Filter.Warehouse.Id);
						break;
				}
			}
			else {
				receiptProjection = Projections.Conditional(
						Restrictions.IsNotNull(Projections
							.Property<WarehouseOperation>(x => x.ReceiptWarehouse.Id)),
						Projections.Constant(true),
						Projections.Constant(false)
					);
				expenseProjection = Projections.Conditional(
					Restrictions.IsNotNull(Projections
						.Property<WarehouseOperation>(x => x.ExpenseWarehouse.Id)),
					Projections.Constant(true),
					Projections.Constant(false)
				);
				switch(Filter.Direction) {
					case DirectionOfOperation.expense:
						queryStock.Where(x => x.ReceiptWarehouse == null);
						break;
					case DirectionOfOperation.receipt:
						queryStock.Where(x => x.ExpenseWarehouse == null);
						break;
				}
			}

			queryStock
				.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Type, () => itemTypesAlias)
				.JoinAlias(() => itemTypesAlias.Units, () => unitsAlias)
				.JoinAlias(() => warehouseOperationAlias.Owner,  () => ownerAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => expenseItemAlias, () => expenseItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseItemAlias.ExpenseDoc, () => expenseAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => expenseDutyNormItemAlias, () => expenseDutyNormItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseDutyNormItemAlias.Document, () => expenseDutyNormAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => collectiveExpenseItemAlias, () => collectiveExpenseItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => collectiveExpenseItemAlias.Document, () => collectiveExpenseAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => incomeItemAlias, () => incomeItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => incomeItemAlias.Document, () => incomeAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => returnItemAlias, () => returnItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => returnItemAlias.Document, () => returnAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => transferItemAlias, () => transferItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => transferItemAlias.Document, () => transferAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => writeoffItemAlias, () => writeoffItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => writeoffItemAlias.Document, () => writeoffAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => employeeIssueOperationAlias, () => employeeIssueOperationAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => issuanceSheetItem, () => issuanceSheetItem.IssueOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => issuanceSheetItem.IssuanceSheet, () => issuanceSheetAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => employeeCardAlias, () => employeeIssueOperationAlias.Employee.Id == employeeCardAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => completionResultItemAlias, () => completionResultItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => completionResultItemAlias.Completion, () => completionResultAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => completionSourceItemAlias, () => completionSourceItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => completionSourceItemAlias.Completion, () => completionSourceAlias, JoinType.LeftOuterJoin)
				.Where(GetSearchCriterion(
					() => employeeCardAlias.FirstName,
					() => employeeCardAlias.LastName,
					() => employeeCardAlias.Patronymic,
					() => issuanceSheetAlias.Id,
					() => issuanceSheetAlias.DocNumber,
					() => nomenclatureAlias.Name
				));

			if(Filter.CollapseOperationItems) {
				queryStock.SelectList(list => list
					.SelectGroup(() => warehouseOperationAlias.Nomenclature.Id)
					.Select(() => warehouseOperationAlias.OperationTime).WithAlias(() => resultAlias.OperationTime)
					.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
					.Select(receiptProjection).WithAlias(() => resultAlias.Receipt)
					.Select(expenseProjection).WithAlias(() => resultAlias.Expense)
					.SelectSum(() => warehouseOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => employeeCardAlias.FirstName).WithAlias(() => resultAlias.EmployeeName)
					.Select(() => employeeCardAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
					.Select(() => employeeCardAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
					.SelectCount(() => employeeCardAlias.Id).WithAlias(() => resultAlias.NumberOfCollapsedRows)
					//Ссылки
					.SelectGroup(() => expenseItemAlias.ExpenseDoc.Id).WithAlias(() => resultAlias.ExpenseId)
					.SelectGroup(() => expenseAlias.DocNumber).WithAlias(() => resultAlias.ExpenseDocNumber)
					.SelectGroup(() => expenseDutyNormItemAlias.Document.Id).WithAlias(() => resultAlias.ExpenseDutyNormId)
					.SelectGroup(() => expenseDutyNormAlias.DocNumber).WithAlias(() => resultAlias.ExpenseDutyNormDocNumber)
					.SelectGroup(() => collectiveExpenseItemAlias.Document.Id).WithAlias(() => resultAlias.CollectiveExpenseId)
					.SelectGroup(() => collectiveExpenseAlias.DocNumber).WithAlias(() => resultAlias.CollectiveExpenseDocNumber)
					.SelectGroup(() => incomeItemAlias.Document.Id).WithAlias(() => resultAlias.IncomeId)
					.SelectGroup(() => incomeAlias.DocNumber).WithAlias(() => resultAlias.IncomeDocNumber)
					.SelectGroup(() => returnItemAlias.Document.Id).WithAlias(() => resultAlias.ReturnId)
					.SelectGroup(() => returnAlias.DocNumber).WithAlias(() => resultAlias.ReturnDocNumber)
					.SelectGroup(() => transferItemAlias.Document.Id).WithAlias(() => resultAlias.TransferId)
					.SelectGroup(() => transferAlias.DocNumber).WithAlias(() => resultAlias.TransferDocNumber)
					.SelectGroup(() => writeoffItemAlias.Document.Id).WithAlias(() => resultAlias.WriteoffId)
					.SelectGroup(() => writeoffAlias.DocNumber).WithAlias(() => resultAlias.WriteoffDocNumber)
					.SelectGroup(() => completionResultItemAlias.Completion.Id).WithAlias(() => resultAlias.CompletionFromResultId)
					.SelectGroup(() => completionResultAlias.DocNumber).WithAlias(() => resultAlias.CompletionFromResultDocNumber)
					.SelectGroup(() => completionSourceItemAlias.Completion.Id).WithAlias(() => resultAlias.CompletionFromSourceId)
					.SelectGroup(() => completionSourceAlias.DocNumber).WithAlias(() => resultAlias.CompletionFromSourceDocNumber)
					
					.SelectGroup(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSizeName)
					.SelectGroup(() => heightAlias.Name).WithAlias(() => resultAlias.HeightName)
					.SelectGroup(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
					.SelectGroup(() => ownerAlias.Name).WithAlias(() => resultAlias.OwnerName)
				);
			}
			else {
				queryStock.SelectList(list => list
					.Select(() => warehouseOperationAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => warehouseOperationAlias.Id).WithAlias(() => resultAlias.OperationId)
					.Select(() => warehouseOperationAlias.OperationTime).WithAlias(() => resultAlias.OperationTime)
					.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
					.Select(receiptProjection).WithAlias(() => resultAlias.Receipt)
					.Select(expenseProjection).WithAlias(() => resultAlias.Expense)
					.Select(() => warehouseOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSizeName)
					.Select(() => heightAlias.Name).WithAlias(() => resultAlias.HeightName)
					.Select(() => ownerAlias.Name).WithAlias(() => resultAlias.OwnerName)
					.Select(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
					//Ссылки
					.Select(() => expenseItemAlias.Id).WithAlias(() => resultAlias.ExpenseItemId)
					.Select(() => expenseItemAlias.ExpenseDoc.Id).WithAlias(() => resultAlias.ExpenseId)
					.Select(() => expenseAlias.DocNumber).WithAlias(() => resultAlias.ExpenseDocNumber)
					.Select(() => expenseDutyNormItemAlias.Id).WithAlias(() => resultAlias.ExpenseDutyNormItemId)
					.Select(() => expenseDutyNormItemAlias.Document.Id).WithAlias(() => resultAlias.ExpenseDutyNormId)
					.Select(() => expenseDutyNormAlias.DocNumber).WithAlias(() => resultAlias.ExpenseDutyNormDocNumber)
					.Select(() => collectiveExpenseItemAlias.Id).WithAlias(() => resultAlias.CollectiveExpenseItemId)
					.Select(() => collectiveExpenseItemAlias.Document.Id).WithAlias(() => resultAlias.CollectiveExpenseId)
					.Select(() => collectiveExpenseAlias.DocNumber).WithAlias(() => resultAlias.CollectiveExpenseDocNumber)
					.Select(() => issuanceSheetAlias.Id).WithAlias(() => resultAlias.IssuanceSheetId)
					.Select(() => issuanceSheetAlias.DocNumber).WithAlias(() => resultAlias.IssueSheetNumber)
					.Select(() => incomeItemAlias.Id).WithAlias(() => resultAlias.IncomeItemId)
					.Select(() => incomeItemAlias.Document.Id).WithAlias(() => resultAlias.IncomeId)
					.Select(() => incomeAlias.DocNumber).WithAlias(() => resultAlias.IncomeDocNumber)
					.Select(() => returnItemAlias.Id).WithAlias(() => resultAlias.ReturnItemId)
					.Select(() => returnItemAlias.Document.Id).WithAlias(() => resultAlias.ReturnId)
					.Select(() => returnAlias.DocNumber).WithAlias(() => resultAlias.ReturnDocNumber)
					.Select(() => transferItemAlias.Id).WithAlias(() => resultAlias.TransferItemId)
					.Select(() => transferItemAlias.Document.Id).WithAlias(() => resultAlias.TransferId)
					.Select(() => transferAlias.DocNumber).WithAlias(() => resultAlias.TransferDocNumber)
					.Select(() => writeoffItemAlias.Id).WithAlias(() => resultAlias.WriteoffItemId)
					.Select(() => writeoffItemAlias.Document.Id).WithAlias(() => resultAlias.WriteoffId)
					.Select(() => writeoffAlias.DocNumber).WithAlias(() => resultAlias.WriteoffDocNumber)
					.Select(() => employeeCardAlias.FirstName).WithAlias(() => resultAlias.EmployeeName)
					.Select(() => employeeCardAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
					.Select(() => employeeCardAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
					.Select(() => completionResultItemAlias.Id).WithAlias(() => resultAlias.CompletionResultItemId)
					.Select(() => completionResultItemAlias.Completion.Id).WithAlias(() => resultAlias.CompletionFromResultId)
					.Select(() => completionResultAlias.DocNumber).WithAlias(() => resultAlias.CompletionFromResultDocNumber)
					.Select(() => completionSourceItemAlias.Id).WithAlias(() => resultAlias.CompletionSourceItemId)
					.Select(() => completionSourceItemAlias.Completion.Id).WithAlias(() => resultAlias.CompletionFromSourceId)
					.Select(() => completionSourceAlias.DocNumber).WithAlias(() => resultAlias.CompletionFromSourceDocNumber)
				);
			}

			return queryStock
				.OrderBy(x => x.OperationTime).Desc
				.ThenBy(x => x.Id).Asc
				.TransformUsing(Transformers.AliasToBean<StockMovementsJournalNode>());
		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();
			var openDocument = new JournalAction("Открыть документ",
					(selected) => selected.Cast<StockMovementsJournalNode>().Any(x => x.DocumentId.HasValue),
					(selected) => true,
					(selected) => OpenDocument(selected.Cast<StockMovementsJournalNode>().ToArray())
					);
			NodeActionsList.Add(openDocument);
			RowActivatedAction = openDocument;
		}

		void OpenDocument(StockMovementsJournalNode[] nodes)
		{
			foreach(var node in nodes.Where(x => x.DocumentId.HasValue))
				openDocuments.EditDocumentDialog(this, node);
		}

		public override string FooterInfo => $"<span foreground=\"Green\" weight=\"ultrabold\">+</span> {SumReceipt}  " +
		                                     $"<span foreground=\"Red\" weight=\"ultrabold\">-</span> {SumExpense}  " +
		                                     "Сальдо: " +
		                                     (SumExpense > SumReceipt ? $"<span foreground=\"Red\" weight=\"ultrabold\">-</span>{SumExpense-SumReceipt} " : $"{SumReceipt-SumExpense} ") +
		                                     $"| Загружено: {DataLoader.Items.Count} шт.";

		protected IEnumerable<StockMovementsJournalNode> Nodes => DataLoader.Items.Cast<StockMovementsJournalNode>();
		private int SumReceipt => Nodes.Where(x => x.Receipt).Sum(x => x.Amount);
		private int SumExpense => Nodes.Where(x => x.Expense).Sum(x => x.Amount);
	}

	public class StockMovementsJournalNode : OperationToDocumentReference
	{
		public int Id { get; set; }
		public int? OperationId { get; set; }
		public int? IssuanceSheetId { get; set; }
		public string IssueSheetNumber { get; set; }
		public string IssueSheetNumberText => IssueSheetNumber ?? IssuanceSheetId.ToString();
		public DateTime OperationTime { get; set; }
		public string UnitsName { get; set; }
		public bool Receipt { get; set; }
		public bool Expense { get; set; }
		public decimal WearPercent { get; set; }
		public int Amount { get; set; }
		public string WearSizeName { get; set; }
		public string HeightName { get; set; }
		public string OwnerName { get; set; }
		public string NomenclatureName { get; set; }
		public string AmountText => $"{Direction} {Amount} {UnitsName}";
		public string OperationTimeText => OperationTime.ToString("g");
		public string WearPercentText => WearPercent.ToString("P0");
		public string DocumentText => DocumentType != null ? DocumentTitle : null;
		public string Direction {
			get {
				if(Expense && Receipt)
					return "<span foreground=\"Blue\" weight=\"ultrabold\">±</span>";
				if(Receipt)
					return "<span foreground=\"Green\" weight=\"ultrabold\">+</span>";
				if(Expense)
					return "<span foreground=\"Red\" weight=\"ultrabold\">-</span>";
				return "<span foreground=\"Fuchsia\" weight=\"ultrabold\">?</span>";
			}
		}

		public string RowTooltip => OperationId.HasValue ? $"ИД складской операции: {OperationId}" : null;
		public string EmployeeSurname { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }
		public int NumberOfCollapsedRows { get; set; }

		public string Employee =>
			NumberOfCollapsedRows > 1 ? 
				NumberToTextRus.FormatCase(NumberOfCollapsedRows, 
					"{0} сотрудник", "{0} сотрудника", "{0} сотрудников") 
				: PersonHelper.PersonFullName(EmployeeSurname, EmployeeName, EmployeePatronymic);
	}
}
