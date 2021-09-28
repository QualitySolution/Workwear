using System;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using QS.ViewModels.Resolve;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Models.Stock;
using workwear.Tools.Features;
using workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock
{
	public class StockDocumentsJournalViewModel : JournalViewModelBase
	{
		public readonly FeaturesService FeaturesService;
		private readonly OpenStockDocumentsModel openStockDocumentsModel;

		public StockDocumentsFilterViewModel Filter { get; private set; }

		public StockDocumentsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigation,
			OpenStockDocumentsModel openStockDocumentsModel,
			ILifetimeScope autofacScope, 
			FeaturesService featuresService,
			ICurrentPermissionService currentPermissionService = null, 
			IDeleteEntityService deleteEntityService = null)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			Title = "Журнал документов";
			AutofacScope = autofacScope;
			CurrentPermissionService = currentPermissionService;
			DeleteEntityService = deleteEntityService;
			this.openStockDocumentsModel = openStockDocumentsModel ?? throw new ArgumentNullException(nameof(openStockDocumentsModel));
			this.FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			JournalFilter = Filter = AutofacScope.Resolve<StockDocumentsFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			var dataLoader = new ThreadDataLoader<StockDocumentsJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(QueryIncomeDoc);
			dataLoader.AddQuery(QueryExpenseDoc);
			dataLoader.AddQuery(QueryCollectiveExpenseDoc);
			dataLoader.AddQuery(QueryWriteoffDoc);
			dataLoader.AddQuery(QueryMassExpenseDoc);
			dataLoader.AddQuery(QueryTransferDoc);
			dataLoader.MergeInOrderBy(x => x.Date, true);
			DataLoader = dataLoader;

			CreateNodeActions();
			CreateDocumentsActions();

			UpdateOnChanges(typeof(Expense), typeof(CollectiveExpense), typeof(Income), typeof(Writeoff), typeof(MassExpense), typeof(Transfer));
		}

		#region Опциональные зависимости
		protected IDeleteEntityService DeleteEntityService;
		public ICurrentPermissionService CurrentPermissionService { get; set; }
		#endregion

		#region Формирование запроса
		private StockDocumentsJournalNode resultAlias = null;
		private EmployeeCard employeeAlias = null;
		private Subdivision subdivisionAlias = null;
		private UserBase authorAlias = null;
		private Warehouse warehouseReceiptAlias = null;
		private Warehouse warehouseExpenseAlias = null;

		protected IQueryOver<Income> QueryIncomeDoc(IUnitOfWork uow)
		{
			if(Filter.StokDocumentType != null && Filter.StokDocumentType != StokDocumentType.IncomeDoc)
				return null;

			Income incomeAlias = null;

			var incomeQuery = uow.Session.QueryOver<Income>(() => incomeAlias);
			if(Filter.StartDate.HasValue)
				incomeQuery.Where(o => o.Date >= Filter.StartDate.Value);
			if(Filter.EndDate.HasValue)
				incomeQuery.Where(o => o.Date < Filter.EndDate.Value.AddDays(1));
			if(Filter.Warehouse != null)
				incomeQuery.Where(x => x.Warehouse == Filter.Warehouse);

			incomeQuery.Where(GetSearchCriterion(
				() => incomeAlias.Id,
				() => authorAlias.Name,
				() => employeeAlias.LastName,
				() => employeeAlias.FirstName,
				() => employeeAlias.Patronymic,
				() => subdivisionAlias.Name
			));

			incomeQuery
				.JoinQueryOver(() => incomeAlias.EmployeeCard, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => incomeAlias.Subdivision, () => subdivisionAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => incomeAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => incomeAlias.Warehouse, () => warehouseReceiptAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.SelectList(list => list
			   			.Select(() => incomeAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => incomeAlias.Date).WithAlias(() => resultAlias.Date)
						.Select(() => incomeAlias.Operation).WithAlias(() => resultAlias.IncomeOperation)
						.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
						.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
						.Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.EmployeeName)
						.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
						.Select(() => warehouseReceiptAlias.Name).WithAlias(() => resultAlias.ReceiptWarehouse)
						.Select(() => incomeAlias.Comment).WithAlias(() => resultAlias.Comment)
			            .Select(() => StokDocumentType.IncomeDoc).WithAlias(() => resultAlias.DocTypeEnum)
					)
			.OrderBy(() => incomeAlias.Date).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return incomeQuery;
		}

		protected IQueryOver<Expense> QueryExpenseDoc(IUnitOfWork uow)
		{
			if(Filter.StokDocumentType != null && Filter.StokDocumentType != StokDocumentType.ExpenseEmployeeDoc && Filter.StokDocumentType != StokDocumentType.ExpenseObjectDoc)
				return null;

			Expense expenseAlias = null;

			var expenseQuery = uow.Session.QueryOver<Expense>(() => expenseAlias);
			if(Filter.StartDate.HasValue)
				expenseQuery.Where(o => o.Date >= Filter.StartDate.Value);
			if(Filter.EndDate.HasValue)
				expenseQuery.Where(o => o.Date < Filter.EndDate.Value.AddDays(1));
			if(Filter.StokDocumentType == StokDocumentType.ExpenseEmployeeDoc)
				expenseQuery.Where(x => x.Operation == ExpenseOperations.Employee);
			if(Filter.StokDocumentType == StokDocumentType.ExpenseObjectDoc)
				expenseQuery.Where(x => x.Operation == ExpenseOperations.Object);
			if(Filter.Warehouse != null)
				expenseQuery.Where(x => x.Warehouse == Filter.Warehouse);

			expenseQuery.Where(GetSearchCriterion(
				() => expenseAlias.Id,
				() => authorAlias.Name,
				() => employeeAlias.LastName,
				() => employeeAlias.FirstName,
				() => employeeAlias.Patronymic,
				() => subdivisionAlias.Name
			));

			expenseQuery
				.JoinQueryOver(() => expenseAlias.Employee, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => expenseAlias.Subdivision, () => subdivisionAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseAlias.Warehouse, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.SelectList(list => list
						.Select(() => expenseAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => expenseAlias.Date).WithAlias(() => resultAlias.Date)
						.Select(() => expenseAlias.Operation).WithAlias(() => resultAlias.ExpenseOperation)
						.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
						.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
						.Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.EmployeeName)
						.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
						.Select(() => warehouseExpenseAlias.Name).WithAlias(() => resultAlias.ExpenseWarehouse)
						.Select(() => expenseAlias.Comment).WithAlias(() => resultAlias.Comment)
					   )
			.OrderBy(() => expenseAlias.Date).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return expenseQuery;
		}

		protected IQueryOver<CollectiveExpense> QueryCollectiveExpenseDoc(IUnitOfWork uow)
		{
			if(Filter.StokDocumentType != null && Filter.StokDocumentType != StokDocumentType.CollectiveExpense)
				return null;

			CollectiveExpense collectiveExpenseAlias = null;

			var collectiveExpenseQuery = uow.Session.QueryOver<CollectiveExpense>(() => collectiveExpenseAlias);
			if(Filter.StartDate.HasValue)
				collectiveExpenseQuery.Where(o => o.Date >= Filter.StartDate.Value);
			if(Filter.EndDate.HasValue)
				collectiveExpenseQuery.Where(o => o.Date < Filter.EndDate.Value.AddDays(1));
			if(Filter.Warehouse != null)
				collectiveExpenseQuery.Where(x => x.Warehouse == Filter.Warehouse);

			collectiveExpenseQuery.Where(GetSearchCriterion(
				() => collectiveExpenseAlias.Id,
				() => authorAlias.Name
			));

			collectiveExpenseQuery
				.JoinAlias(() => collectiveExpenseAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => collectiveExpenseAlias.Warehouse, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.SelectList(list => list
						.Select(() => collectiveExpenseAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => collectiveExpenseAlias.Date).WithAlias(() => resultAlias.Date)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
						.Select(() => warehouseExpenseAlias.Name).WithAlias(() => resultAlias.ExpenseWarehouse)
						.Select(() => collectiveExpenseAlias.Comment).WithAlias(() => resultAlias.Comment)
						.Select(() => StokDocumentType.CollectiveExpense).WithAlias(() => resultAlias.DocTypeEnum)
					   )
			.OrderBy(() => collectiveExpenseAlias.Date).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return collectiveExpenseQuery;
		}

		protected IQueryOver<MassExpense> QueryMassExpenseDoc(IUnitOfWork uow)
		{
			if(Filter.StokDocumentType != null && Filter.StokDocumentType != StokDocumentType.MassExpense)
				return null;

			MassExpense massExpenseAlias = null;

			var massExpenseQuery = uow.Session.QueryOver<MassExpense>(() => massExpenseAlias);
			if(Filter.StartDate.HasValue)
				massExpenseQuery.Where(o => o.Date >= Filter.StartDate.Value);
			if(Filter.EndDate.HasValue)
				massExpenseQuery.Where(o => o.Date < Filter.EndDate.Value.AddDays(1));
			if(Filter.Warehouse != null)
				massExpenseQuery.Where(x => x.WarehouseFrom == Filter.Warehouse);

			massExpenseQuery.Where(GetSearchCriterion(
				() => massExpenseAlias.Id,
				() => authorAlias.Name
			));

			massExpenseQuery
				.JoinAlias(() => massExpenseAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => massExpenseAlias.WarehouseFrom, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.SelectList(list => list
			   			.Select(() => massExpenseAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => massExpenseAlias.Date).WithAlias(() => resultAlias.Date)
						.Select(() => warehouseExpenseAlias.Name).WithAlias(() => resultAlias.ExpenseWarehouse)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
			            .Select(() => massExpenseAlias.Comment).WithAlias(() => resultAlias.Comment)
						.Select(() => StokDocumentType.MassExpense).WithAlias(() => resultAlias.DocTypeEnum)
						)
			.OrderBy(() => massExpenseAlias.Date).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return massExpenseQuery;
		}

		protected IQueryOver<Transfer> QueryTransferDoc(IUnitOfWork uow)
		{
			if(Filter.StokDocumentType != null && Filter.StokDocumentType != StokDocumentType.TransferDoc)
				return null;

			Transfer transferAlias = null;

			var transferQuery = uow.Session.QueryOver<Transfer>(() => transferAlias);
			if(Filter.StartDate.HasValue)
				transferQuery.Where(o => o.Date >= Filter.StartDate.Value);
			if(Filter.EndDate.HasValue)
				transferQuery.Where(o => o.Date < Filter.EndDate.Value.AddDays(1));
			if(Filter.Warehouse != null)
				transferQuery.Where(x => x.WarehouseFrom == Filter.Warehouse || x.WarehouseTo == Filter.Warehouse);

			transferQuery.Where(GetSearchCriterion(
				() => transferAlias.Id,
				() => authorAlias.Name
			));

			transferQuery
				.JoinAlias(() => transferAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => transferAlias.WarehouseFrom, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => transferAlias.WarehouseTo, () => warehouseReceiptAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.SelectList(list => list
			   			.Select(() => transferAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => transferAlias.Date).WithAlias(() => resultAlias.Date)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
						.Select(() => warehouseReceiptAlias.Name).WithAlias(() => resultAlias.ReceiptWarehouse)
						.Select(() => warehouseExpenseAlias.Name).WithAlias(() => resultAlias.ExpenseWarehouse)
			            .Select(() => transferAlias.Comment).WithAlias(() => resultAlias.Comment)
						.Select(() => StokDocumentType.TransferDoc).WithAlias(() => resultAlias.DocTypeEnum)
					   )
			.OrderBy(() => transferAlias.Date).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return transferQuery;
		}

		protected IQueryOver<Writeoff> QueryWriteoffDoc(IUnitOfWork uow, bool isCounting)
		{
			if(Filter.StokDocumentType != null && Filter.StokDocumentType != StokDocumentType.WriteoffDoc)
				return null;

			WriteoffItem writeoffItemAlias = null;
			Writeoff writeoffAlias = null;

			var writeoffQuery = uow.Session.QueryOver<Writeoff>(() => writeoffAlias);
			if(Filter.StartDate.HasValue)
				writeoffQuery.Where(o => o.Date >= Filter.StartDate.Value);
			if(Filter.EndDate.HasValue)
				writeoffQuery.Where(o => o.Date < Filter.EndDate.Value.AddDays(1));
			if(Filter.Warehouse != null)
				writeoffQuery.Where(() => writeoffItemAlias.Warehouse == Filter.Warehouse);

			writeoffQuery.Where(GetSearchCriterion(
				() => writeoffAlias.Id,
				() => authorAlias.Name
			));

			var concatPrpjection = Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
					NHibernateUtil.String,
					Projections.Property(() => warehouseExpenseAlias.Name),
					Projections.Constant(", "));

			if(!isCounting) {
				writeoffQuery
				.JoinAlias(() => writeoffAlias.Items, () => writeoffItemAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => writeoffItemAlias.Warehouse, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);
			}

			writeoffQuery
				.JoinAlias(() => writeoffAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.SelectList(list => list
			   			.SelectGroup(() => writeoffAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => writeoffAlias.Date).WithAlias(() => resultAlias.Date)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
						.Select(concatPrpjection).WithAlias(() => resultAlias.ExpenseWarehouse)
						.Select(() => StokDocumentType.WriteoffDoc).WithAlias(() => resultAlias.DocTypeEnum)
			            .Select(() => writeoffAlias.Comment).WithAlias(() => resultAlias.Comment)
					   )
			.OrderBy(() => writeoffAlias.Date).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return writeoffQuery;
		}
		#endregion
		#region Действия
		void CreateDocumentsActions()
		{
			var addAction = new JournalAction("Добавить",
					(selected) => true,
					(selected) => true,
					null,
					"Insert"
					);
			NodeActionsList.Add(addAction);
			foreach(StokDocumentType docType in Enum.GetValues(typeof(StokDocumentType))) {
				if(docType is StokDocumentType.MassExpense && !FeaturesService.Available(WorkwearFeature.MassExpense))
					continue;
				if(docType is StokDocumentType.CollectiveExpense && !FeaturesService.Available(WorkwearFeature.CollectiveExpense))
					continue;
				if(docType is StokDocumentType.TransferDoc && !FeaturesService.Available(WorkwearFeature.Warehouses))
					continue;
				var insertDocAction = new JournalAction(
					docType.GetEnumTitle(),
					(selected) => true,
					(selected) => true,
					(selected) => openStockDocumentsModel.CreateDocumentDialog(this, docType)
				);
				addAction.ChildActionsList.Add(insertDocAction);
			}

			var editAction = new JournalAction("Изменить",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => selected.Cast<StockDocumentsJournalNode>().ToList()
						.ForEach(n => openStockDocumentsModel.EditDocumentDialog(this, n.DocTypeEnum, n.Id))
					);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
				RowActivatedAction = editAction;

			var deleteAction = new JournalAction("Удалить",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => DeleteEntities(selected.Cast<StockDocumentsJournalNode>().ToArray()),
					"Delete"
					);
			NodeActionsList.Add(deleteAction);
		}

		protected virtual void DeleteEntities(StockDocumentsJournalNode[] nodes)
		{
			foreach(var node in nodes) {
				var doctype = StockDocument.GetDocClass(node.DocTypeEnum);
				DeleteEntityService.DeleteEntity(doctype, DomainHelper.GetId(node));
			}
		}
		#endregion
	}

	public class StockDocumentsJournalNode
	{
		private ExpenseOperations _expenseOperation;

		public int Id { get; set; }

		public StokDocumentType DocTypeEnum { get; set; }

		public string DocTypeString => DocTypeEnum.GetEnumTitle();

		public DateTime Date { get; set; }

		public string DateString => Date.ToShortDateString();

		public IncomeOperations IncomeOperation { get; set; }

		public ExpenseOperations ExpenseOperation {
			get => _expenseOperation;
			set {
				_expenseOperation = value;
				if(ExpenseOperation == ExpenseOperations.Employee)
					DocTypeEnum = StokDocumentType.ExpenseEmployeeDoc;
				if(ExpenseOperation == ExpenseOperations.Object)
					DocTypeEnum = StokDocumentType.ExpenseObjectDoc;
			}
		}

		public string Operation {
			get {
				switch(DocTypeEnum) {
					case StokDocumentType.IncomeDoc:
						return IncomeOperation.GetEnumTitle();
					default:
						return null;
				}
			}
		}

		public string Description {
			get {
				string text = String.Empty;
				if(!String.IsNullOrWhiteSpace(Employee))
					text += $"Сотрудник: {Employee} ";
				if(!String.IsNullOrWhiteSpace(Subdivision))
					text += $"Объект: {Subdivision} ";
				return text;
			}
		}

		public string ReceiptWarehouse { get; set; }
		public string ExpenseWarehouse { get; set; }

		public string Warehouse => ReceiptWarehouse == null && ExpenseWarehouse == null ? String.Empty :
			ReceiptWarehouse == null ? $" {ExpenseWarehouse} =>" : $"{ExpenseWarehouse} => {ReceiptWarehouse}";

		public string Subdivision { get; set; }

		public string Author { get; set; }

		public string EmployeeSurname { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }

		public string Employee => PersonHelper.PersonFullName(EmployeeSurname, EmployeeName, EmployeePatronymic);

		public string Comment { get; set; }
	}
}
