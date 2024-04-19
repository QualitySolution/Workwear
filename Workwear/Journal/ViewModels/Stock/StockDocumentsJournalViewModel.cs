﻿using System;
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
using Workwear.Domain.Company;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Models.Stock;
using Workwear.Tools.Features;

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
			CurrentPermissionService = currentPermissionService;
			DeleteEntityService = deleteEntityService;
			this.openStockDocumentsModel = openStockDocumentsModel ?? throw new ArgumentNullException(nameof(openStockDocumentsModel));
			FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			JournalFilter = Filter = autofacScope.Resolve<StockDocumentsFilterViewModel>(
				new TypedParameter(typeof(JournalViewModelBase), this));

			var dataLoader = new ThreadDataLoader<StockDocumentsJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(QueryIncomeDoc);
			dataLoader.AddQuery(QueryExpenseDoc);
			dataLoader.AddQuery(QueryCollectiveExpenseDoc);
			dataLoader.AddQuery(QueryWriteoffDoc);
			dataLoader.AddQuery(QueryTransferDoc);
			dataLoader.AddQuery(QueryCompletionDoc);
			dataLoader.AddQuery(QueryInspectionDoc);
			dataLoader.MergeInOrderBy(x => x.Date, true);
			dataLoader.MergeInOrderBy(x => x.CreationDate, true);
			DataLoader = dataLoader;

			CreateNodeActions();
			CreateDocumentsActions();

			UpdateOnChanges(typeof(Expense), typeof(CollectiveExpense), typeof(Income), 
				typeof(Writeoff), typeof(Transfer), typeof(Completion), typeof(Inspection));
		}

		#region Опциональные зависимости
		protected IDeleteEntityService DeleteEntityService;
		public ICurrentPermissionService CurrentPermissionService { get; set; }
		#endregion

		#region Формирование запроса
		private StockDocumentsJournalNode resultAlias = null;
		private EmployeeCard employeeAlias = null;
		private UserBase authorAlias = null;
		private Warehouse warehouseReceiptAlias = null;
		private Warehouse warehouseExpenseAlias = null;
		private IssuanceSheet issuanceSheetAlias = null;  

		protected IQueryOver<Income> QueryIncomeDoc(IUnitOfWork uow)
		{
			if(Filter.StockDocumentType != null && Filter.StockDocumentType != StockDocumentType.IncomeDoc)
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
				() => employeeAlias.Patronymic
			));

			incomeQuery
				.JoinQueryOver(() => incomeAlias.EmployeeCard, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => incomeAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => incomeAlias.Warehouse, () => warehouseReceiptAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.SelectList(list => list
			   			.Select(() => incomeAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => incomeAlias.Date).WithAlias(() => resultAlias.Date)
						.Select(() => incomeAlias.Operation).WithAlias(() => resultAlias.IncomeOperation)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
						.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
						.Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.EmployeeName)
						.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
						.Select(() => warehouseReceiptAlias.Name).WithAlias(() => resultAlias.ReceiptWarehouse)
						.Select(() => incomeAlias.Comment).WithAlias(() => resultAlias.Comment)
			            .Select(() => StockDocumentType.IncomeDoc).WithAlias(() => resultAlias.DocTypeEnum)
			            .Select(() => incomeAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
			            .Select(() => incomeAlias.Number).WithAlias(() => resultAlias.IncomeDocNubber)
					)
			.OrderBy(() => incomeAlias.Date).Desc
			.ThenBy(() => incomeAlias.CreationDate).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return incomeQuery;
		}

		protected IQueryOver<Expense> QueryExpenseDoc(IUnitOfWork uow)
		{
			if(Filter.StockDocumentType != null && Filter.StockDocumentType != StockDocumentType.ExpenseEmployeeDoc)
				return null;

			Expense expenseAlias = null;

			var expenseQuery = uow.Session.QueryOver<Expense>(() => expenseAlias);
			if(Filter.StartDate.HasValue)
				expenseQuery.Where(o => o.Date >= Filter.StartDate.Value);
			if(Filter.EndDate.HasValue)
				expenseQuery.Where(o => o.Date < Filter.EndDate.Value.AddDays(1));
			if(Filter.Warehouse != null)
				expenseQuery.Where(x => x.Warehouse == Filter.Warehouse);

			expenseQuery.Where(GetSearchCriterion(
				() => expenseAlias.Id,
				() => issuanceSheetAlias.Id,
				() => authorAlias.Name,
				() => employeeAlias.LastName,
				() => employeeAlias.FirstName,
				() => employeeAlias.Patronymic
			));

			expenseQuery
				.JoinQueryOver(() => expenseAlias.Employee, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseAlias.Warehouse, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => expenseAlias.IssuanceSheet, () => issuanceSheetAlias)
			.SelectList(list => list
						.Select(() => expenseAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => expenseAlias.Date).WithAlias(() => resultAlias.Date)
						.Select(() => issuanceSheetAlias.Id).WithAlias(() => resultAlias.IssueSheetId)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
						.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
						.Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.EmployeeName)
						.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
						.Select(() => warehouseExpenseAlias.Name).WithAlias(() => resultAlias.ExpenseWarehouse)
						.Select(() => expenseAlias.Comment).WithAlias(() => resultAlias.Comment)
						.Select(() => expenseAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
					   )
			.OrderBy(() => expenseAlias.Date).Desc
			.ThenBy(() => expenseAlias.CreationDate).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return expenseQuery;
		}

		protected IQueryOver<CollectiveExpense> QueryCollectiveExpenseDoc(IUnitOfWork uow)
		{
			if(Filter.StockDocumentType != null && Filter.StockDocumentType != StockDocumentType.CollectiveExpense)
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
				() => authorAlias.Name,
				() => issuanceSheetAlias.Id
			));

			collectiveExpenseQuery
				.JoinAlias(() => collectiveExpenseAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => collectiveExpenseAlias.Warehouse, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => collectiveExpenseAlias.IssuanceSheet, () => issuanceSheetAlias)
			.SelectList(list => list
						.Select(() => collectiveExpenseAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => collectiveExpenseAlias.Date).WithAlias(() => resultAlias.Date)
						.Select(() => issuanceSheetAlias.Id).WithAlias(() => resultAlias.IssueSheetId)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
						.Select(() => warehouseExpenseAlias.Name).WithAlias(() => resultAlias.ExpenseWarehouse)
						.Select(() => collectiveExpenseAlias.Comment).WithAlias(() => resultAlias.Comment)
						.Select(() => StockDocumentType.CollectiveExpense).WithAlias(() => resultAlias.DocTypeEnum)
						.Select(() => collectiveExpenseAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
					   )
			.OrderBy(() => collectiveExpenseAlias.Date).Desc
			.ThenBy(() => collectiveExpenseAlias.CreationDate).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return collectiveExpenseQuery;
		}
		protected IQueryOver<Transfer> QueryTransferDoc(IUnitOfWork uow)
		{
			if(Filter.StockDocumentType != null && Filter.StockDocumentType != StockDocumentType.TransferDoc)
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
						.Select(() => StockDocumentType.TransferDoc).WithAlias(() => resultAlias.DocTypeEnum)
			            .Select(() => transferAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
					   )
			.OrderBy(() => transferAlias.Date).Desc
			.ThenBy(() => transferAlias.CreationDate).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return transferQuery;
		}

		protected IQueryOver<Writeoff> QueryWriteoffDoc(IUnitOfWork uow, bool isCounting)
		{
			if(Filter.StockDocumentType != null && Filter.StockDocumentType != StockDocumentType.WriteoffDoc)
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

			var concatProjection = Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
					NHibernateUtil.String,
					Projections.Property(() => warehouseExpenseAlias.Name),
					Projections.Constant(", "));

			if(!isCounting) {
				writeoffQuery
					.JoinAlias(() => writeoffAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => writeoffAlias.Items, () => writeoffItemAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => writeoffItemAlias.Warehouse, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);
			}

			writeoffQuery
			.SelectList(list => list
			   			.SelectGroup(() => writeoffAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => writeoffAlias.Date).WithAlias(() => resultAlias.Date)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
						.Select(concatProjection).WithAlias(() => resultAlias.ExpenseWarehouse)
						.Select(() => StockDocumentType.WriteoffDoc).WithAlias(() => resultAlias.DocTypeEnum)
			            .Select(() => writeoffAlias.Comment).WithAlias(() => resultAlias.Comment)
			            .Select(() => writeoffAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
					   )
			.OrderBy(() => writeoffAlias.Date).Desc
			.ThenBy(() => writeoffAlias.CreationDate).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return writeoffQuery;
		}
		protected IQueryOver<Completion> QueryCompletionDoc(IUnitOfWork uow)
		{
			if(Filter.StockDocumentType != null && Filter.StockDocumentType != StockDocumentType.Completion)
				return null;
			Completion completionAlias = null;
			
			var completionQuery = uow.Session.QueryOver<Completion>(() => completionAlias);
			if(Filter.StartDate.HasValue)
				completionQuery.Where(o => o.Date >= Filter.StartDate.Value);
			if(Filter.EndDate.HasValue)
				completionQuery.Where(o => o.Date < Filter.EndDate.Value.AddDays(1));
			if(Filter.Warehouse != null)
				completionQuery.Where(x => x.SourceWarehouse == Filter.Warehouse || x.ResultWarehouse == Filter.Warehouse);

			completionQuery.Where(GetSearchCriterion(
				() => completionAlias.Id, () => authorAlias.Name));
			completionQuery
				.JoinAlias(() => completionAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => completionAlias.ResultWarehouse, () => warehouseReceiptAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => completionAlias.SourceWarehouse, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.SelectList(list => list
			   			.Select(() => completionAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => completionAlias.Date).WithAlias(() => resultAlias.Date)
			            .Select(() => warehouseReceiptAlias.Name).WithAlias(() => resultAlias.ReceiptWarehouse)
			            .Select(() => warehouseExpenseAlias.Name).WithAlias(() => resultAlias.ExpenseWarehouse)
						.Select(() => completionAlias.Comment).WithAlias(() => resultAlias.Comment)
			            .Select(() => StockDocumentType.Completion).WithAlias(() => resultAlias.DocTypeEnum)
			            .Select(() => completionAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
			            .Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
					)
			.OrderBy(() => completionAlias.Date).Desc
			.ThenBy(() => completionAlias.CreationDate).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());
			return completionQuery;
		}
		
		protected IQueryOver<Inspection> QueryInspectionDoc(IUnitOfWork uow) {
			if(Filter.StockDocumentType != null && Filter.StockDocumentType != StockDocumentType.InspectionDoc)
				return null;

			Inspection inspectionAlias = null;

			var inspectionQuery = uow.Session.QueryOver<Inspection>(() => inspectionAlias);
			if(Filter.StartDate.HasValue)
				inspectionQuery.Where(o => o.Date >= Filter.StartDate.Value);
			if(Filter.EndDate.HasValue)
				inspectionQuery.Where(o => o.Date < Filter.EndDate.Value.AddDays(1));

			inspectionQuery
				.JoinAlias(() => inspectionAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.SelectList(list => list
			   			.SelectGroup(() => inspectionAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => inspectionAlias.Date).WithAlias(() => resultAlias.Date)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
						.Select(() => StockDocumentType.InspectionDoc).WithAlias(() => resultAlias.DocTypeEnum)
			            .Select(() => inspectionAlias.Comment).WithAlias(() => resultAlias.Comment)
			            .Select(() => inspectionAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
					   )
			.OrderBy(() => inspectionAlias.Date).Desc
			.ThenBy(() => inspectionAlias.CreationDate).Desc
			.TransformUsing(Transformers.AliasToBean<StockDocumentsJournalNode>());

			return inspectionQuery;
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
			foreach(StockDocumentType docType in Enum.GetValues(typeof(StockDocumentType))) {
				switch (docType) {
					case StockDocumentType.CollectiveExpense when !FeaturesService.Available(WorkwearFeature.CollectiveExpense):
					case StockDocumentType.InspectionDoc when !FeaturesService.Available(WorkwearFeature.Inspection):
					case StockDocumentType.TransferDoc when !FeaturesService.Available(WorkwearFeature.Warehouses):
					case StockDocumentType.Completion when !FeaturesService.Available(WorkwearFeature.Completion):
						continue;
					default:
					{
						var insertDocAction = new JournalAction(
							docType.GetEnumTitle(),
							(selected) => true,
							(selected) => true,
							(selected) => openStockDocumentsModel.CreateDocumentDialog(this, docType)
						);
						addAction.ChildActionsList.Add(insertDocAction);
						break;
					}
				}
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
		public int Id { get; set; }

		public StockDocumentType DocTypeEnum { get; set; }

		public string DocTypeString => DocTypeEnum.GetEnumTitle();

		public DateTime Date { get; set; }

		public string DateString => Date.ToShortDateString();

		public IncomeOperations IncomeOperation { get; set; }

		public string Description {
			get {
				string text = String.Empty;
				if(!String.IsNullOrWhiteSpace(Employee))
					text += $"Сотрудник: {Employee} ";
				if(DocTypeEnum == StockDocumentType.IncomeDoc)
					text += $"Операция: {IncomeOperation.GetEnumTitle()}";
				if(!String.IsNullOrWhiteSpace(IncomeDocNubber))
					text += $" TН №: {IncomeDocNubber} ";
				return text;
			}
		}

		public string ReceiptWarehouse { get; set; }
		public string ExpenseWarehouse { get; set; }

		public string Warehouse => ReceiptWarehouse == null && ExpenseWarehouse == null ? String.Empty :
			ReceiptWarehouse == null ? $" {ExpenseWarehouse} =>" : $"{ExpenseWarehouse} => {ReceiptWarehouse}";

		public string Author { get; set; }

		public string EmployeeSurname { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }

		public string Employee => PersonHelper.PersonFullName(EmployeeSurname, EmployeeName, EmployeePatronymic);

		public string IncomeDocNubber { get; set; }
		public string Comment { get; set; }

		public int? IssueSheetId { get; set; }

		private DateTime? creationDate;
		public DateTime? CreationDate {
			get => creationDate ?? DateTime.MinValue;
			set => creationDate = value;
		}
		public string CreationDateString => creationDate?.ToString("g") ?? String.Empty;
	}
}
