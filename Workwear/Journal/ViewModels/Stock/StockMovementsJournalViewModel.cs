using System;
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
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Models.Operations;
using workwear.Models.Stock;

namespace workwear.Journal.ViewModels.Stock
{
	public class StockMovmentsJournalViewModel : JournalViewModelBase
	{
		private readonly OpenStockDocumentsModel openDocuments;

		public StockMovementsFilterViewModel Filter { get; private set; }

		public StockMovmentsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			OpenStockDocumentsModel openDocuments) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			AutofacScope = autofacScope;
			this.openDocuments = openDocuments ?? throw new ArgumentNullException(nameof(openDocuments));
			JournalFilter = Filter = AutofacScope.Resolve<StockMovementsFilterViewModel>
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
			ExpenseItem expenseItemAlias = null;
			IncomeItem incomeItemAlias = null;
			TransferItem transferItemAlias = null;
			CollectiveExpenseItem collectiveExpenseItemAlias = null;
			WriteoffItem writeOffItemAlias = null;
			EmployeeCard employeeCardAlias = null;
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			CompletionResultItem completionResultItemAlias = null;
			CompletionSourceItem completionSourceItemAlias = null;

			Nomenclature nomenclatureAlias = null;
			ItemsType itemTypesAlias = null;
			MeasurementUnits unitsAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;

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
			}

			if(Filter.Nomenclature != null)
				queryStock.Where(x => x.Nomenclature == Filter.Nomenclature);

			if(Filter.Size != null)
				queryStock.Where(x => x.WearSize.Id == Filter.Size.Id);

			if(Filter.Height != null)
				queryStock.Where(x => x.Height.Id == Filter.Height.Id);

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
				.JoinEntityAlias(() => expenseItemAlias, () => expenseItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseItemAlias.ExpenseDoc, () => expenseAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => collectiveExpenseItemAlias, () => collectiveExpenseItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => incomeItemAlias, () => incomeItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => transferItemAlias, () => transferItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => writeOffItemAlias, () => writeOffItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => employeeIssueOperationAlias, () => employeeIssueOperationAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => employeeCardAlias, () => employeeIssueOperationAlias.Employee.Id == employeeCardAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => completionResultItemAlias, () => completionResultItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => completionSourceItemAlias, () => completionSourceItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.Where(GetSearchCriterion(
					() => employeeCardAlias.FirstName,
					() => employeeCardAlias.LastName,
					() => employeeCardAlias.Patronymic
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

					//Ссылки
					.SelectGroup(() => expenseItemAlias.ExpenseDoc.Id).WithAlias(() => resultAlias.ExpenceId)
					.Select(() => expenseAlias.Operation).WithAlias(() => resultAlias.ExpenseOperation)
					.SelectGroup(() => collectiveExpenseItemAlias.Document.Id).WithAlias(() => resultAlias.CollectiveExpenseId)
					.SelectCount(() => employeeCardAlias.Id).WithAlias(() => resultAlias.NumberOfCollapsedRows)
					.SelectGroup(() => incomeItemAlias.Document.Id).WithAlias(() => resultAlias.IncomeId)
					.SelectGroup(() => transferItemAlias.Document.Id).WithAlias(() => resultAlias.TransferItemId)
					.SelectGroup(() => writeOffItemAlias.Document.Id).WithAlias(() => resultAlias.WriteoffId)
					.SelectGroup(() => completionResultItemAlias.Completion.Id).WithAlias(() => resultAlias.CompletionFromResultId)
					.SelectGroup(() => completionSourceItemAlias.Completion.Id).WithAlias(() => resultAlias.CompletionFromSourceId)

					.SelectGroup(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSizeName)
					.SelectGroup(() => heightAlias.Name).WithAlias(() => resultAlias.HeightName)
					.SelectGroup(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
				);
			}
			else {
				queryStock.SelectList(list => list
					.Select(() => warehouseOperationAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => warehouseOperationAlias.OperationTime).WithAlias(() => resultAlias.OperationTime)
					.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
					.Select(receiptProjection).WithAlias(() => resultAlias.Receipt)
					.Select(expenseProjection).WithAlias(() => resultAlias.Expense)
					.Select(() => warehouseOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSizeName)
					.Select(() => heightAlias.Name).WithAlias(() => resultAlias.HeightName)
					.Select(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
					//Ссылки
					.Select(() => expenseItemAlias.Id).WithAlias(() => resultAlias.ExpenceItemId)
					.Select(() => expenseItemAlias.ExpenseDoc.Id).WithAlias(() => resultAlias.ExpenceId)
					.Select(() => expenseAlias.Operation).WithAlias(() => resultAlias.ExpenseOperation)
					.Select(() => collectiveExpenseItemAlias.Id).WithAlias(() => resultAlias.CollectiveExpenseItemId)
					.Select(() => collectiveExpenseItemAlias.Document.Id).WithAlias(() => resultAlias.CollectiveExpenseId)
					.Select(() => incomeItemAlias.Id).WithAlias(() => resultAlias.IncomeItemId)
					.Select(() => incomeItemAlias.Document.Id).WithAlias(() => resultAlias.IncomeId)
					.Select(() => transferItemAlias.Id).WithAlias(() => resultAlias.TransferId)
					.Select(() => transferItemAlias.Document.Id).WithAlias(() => resultAlias.TransferItemId)
					.Select(() => writeOffItemAlias.Id).WithAlias(() => resultAlias.WriteoffItemId)
					.Select(() => writeOffItemAlias.Document.Id).WithAlias(() => resultAlias.WriteoffId)
					.Select(() => employeeCardAlias.FirstName).WithAlias(() => resultAlias.EmployeeName)
					.Select(() => employeeCardAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
					.Select(() => employeeCardAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
					.Select(() => completionResultItemAlias.Id).WithAlias(() => resultAlias.CompletionResultItemId)
					.Select(() => completionSourceItemAlias.Id).WithAlias(() => resultAlias.CompletionSourceItemId)
					.Select(() => completionSourceItemAlias.Completion.Id).WithAlias(() => resultAlias.CompletionFromSourceId)
					.Select(() => completionResultItemAlias.Completion.Id).WithAlias(() => resultAlias.CompletionFromResultId)
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
			var updateStatusAction = new JournalAction("Открыть документ",
					(selected) => selected.Cast<StockMovementsJournalNode>().Any(x => x.DocumentId.HasValue),
					(selected) => true,
					(selected) => OpenDocument(selected.Cast<StockMovementsJournalNode>().ToArray())
					);
			NodeActionsList.Add(updateStatusAction);
		}

		void OpenDocument(StockMovementsJournalNode[] nodes)
		{
			foreach(var node in nodes.Where(x => x.DocumentId.HasValue))
				openDocuments.EditDocumentDialog(this, node);
		}
	}

	public class StockMovementsJournalNode : OperationToDocumentReference
	{
		public int Id { get; set; }
		public DateTime OperationTime { get; set; }
		public string UnitsName { get; set; }
		public bool Receipt { get; set; }
		public bool Expense { get; set; }
		public decimal WearPercent { get; set; }
		public int Amount { get; set; }
		public string WearSizeName { get; set; }
		public string HeightName { get; set; }
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
