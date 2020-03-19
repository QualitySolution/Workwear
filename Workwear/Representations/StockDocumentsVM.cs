using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Utilities.Text;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.JournalFilters;

namespace workwear.Representations
{
	public class StockDocumentsVM : RepresentationModelWithoutEntityBase<StockDocumentsVMNode>
	{
		public StockDocumentsFilter Filter
		{
			get
			{
				return RepresentationFilter as StockDocumentsFilter;
			}
			set
			{
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		public override void UpdateNodes()
		{
			StockDocumentsVMNode resultAlias = null;
			Income incomeAlias = null;
			Expense expenseAlias = null;
			Writeoff writeoffAlias = null;
			Transfer transferAlias = null;
			WriteoffItem writeoffItemAlias = null;
			EmployeeCard employeeAlias = null;
			Subdivision facilityAlias = null;
			UserBase authorAlias = null;
			Warehouse warehouseReceiptAlias = null;
			Warehouse warehouseExpenseAlias = null;
			MassExpense massExpenseAlias = null;

			List<StockDocumentsVMNode> result = new List<StockDocumentsVMNode>();

			if (Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == StokDocumentType.IncomeDoc )
			{
				var incomeQuery = UoW.Session.QueryOver<Income>(() => incomeAlias);
				if (Filter.RestrictStartDate.HasValue)
					incomeQuery.Where(o => o.Date >= Filter.RestrictStartDate.Value);
				if (Filter.RestrictEndDate.HasValue)
					incomeQuery.Where(o => o.Date < Filter.RestrictEndDate.Value.AddDays(1));

				var incomeList = incomeQuery
					.JoinQueryOver(() => incomeAlias.EmployeeCard, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => incomeAlias.Subdivision, () => facilityAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => incomeAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => incomeAlias.Warehouse, () => warehouseReceiptAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   			.Select(() => incomeAlias.Id).WithAlias(() => resultAlias.Id)
				            .Select(() => incomeAlias.Date).WithAlias(() => resultAlias.Date)
					        .Select(() => incomeAlias.Operation).WithAlias(() => resultAlias.IncomeOperation)
					        .Select(() => facilityAlias.Name).WithAlias(() => resultAlias.Facility)
					        .Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
					        .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
					        .Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.EmployeeName)
					        .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
							.Select(() => warehouseReceiptAlias.Name).WithAlias(() => resultAlias.ReceiptWarehouse)
						)
				.TransformUsing(Transformers.AliasToBean<StockDocumentsVMNode>())
				.List<StockDocumentsVMNode>();

				incomeList.ToList().ForEach(x => x.DocTypeEnum = StokDocumentType.IncomeDoc);
				result.AddRange(incomeList);
			}

			if (Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == StokDocumentType.ExpenseDoc)
			{
				var expenseQuery = UoW.Session.QueryOver<Expense>(() => expenseAlias);
				if (Filter.RestrictStartDate.HasValue)
					expenseQuery.Where(o => o.Date >= Filter.RestrictStartDate.Value);
				if (Filter.RestrictEndDate.HasValue)
					expenseQuery.Where(o => o.Date < Filter.RestrictEndDate.Value.AddDays(1));

				var expenseList = expenseQuery
					.JoinQueryOver(() => expenseAlias.Employee, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => expenseAlias.Subdivision, () => facilityAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseAlias.Warehouse, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
							.Select(() => expenseAlias.Id).WithAlias(() => resultAlias.Id)
							.Select(() => expenseAlias.Date).WithAlias(() => resultAlias.Date)
					        .Select(() => expenseAlias.Operation).WithAlias(() => resultAlias.ExpenseOperation)
							.Select(() => facilityAlias.Name).WithAlias(() => resultAlias.Facility)
							.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
							.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
							.Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.EmployeeName)
							.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
							.Select(() => warehouseExpenseAlias.Name).WithAlias(() => resultAlias.ExpenseWarehouse)
						   )
				.TransformUsing(Transformers.AliasToBean<StockDocumentsVMNode>())
				.List<StockDocumentsVMNode>();

				expenseList.ToList().ForEach(x => x.DocTypeEnum = StokDocumentType.ExpenseDoc);
				result.AddRange(expenseList);
			}

			if (Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == StokDocumentType.WriteoffDoc)
			{
				var writeoffQuery = UoW.Session.QueryOver<Writeoff>(() => writeoffAlias);
				if (Filter.RestrictStartDate.HasValue)
					writeoffQuery.Where(o => o.Date >= Filter.RestrictStartDate.Value);
				if (Filter.RestrictEndDate.HasValue)
					writeoffQuery.Where(o => o.Date < Filter.RestrictEndDate.Value.AddDays(1));

				var concatPrpjection = Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.Property(() => warehouseExpenseAlias.Name),
						Projections.Constant(", "));

				var writeoffList = writeoffQuery
					.JoinAlias(() => writeoffAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => writeoffAlias.Items, () => writeoffItemAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => writeoffItemAlias.Warehouse, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   			.SelectGroup(() => writeoffAlias.Id).WithAlias(() => resultAlias.Id)
							.Select(() => writeoffAlias.Date).WithAlias(() => resultAlias.Date)
							.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
							.Select(concatPrpjection).WithAlias(() => resultAlias.ExpenseWarehouse)
						   )
				.TransformUsing(Transformers.AliasToBean<StockDocumentsVMNode>())
				.List<StockDocumentsVMNode>();

				writeoffList.ToList().ForEach(x => x.DocTypeEnum = StokDocumentType.WriteoffDoc);
				result.AddRange(writeoffList);
			}

			if(Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == StokDocumentType.TransferDoc) {
				var transferQuery = UoW.Session.QueryOver<Transfer>(() => transferAlias);
				if(Filter.RestrictStartDate.HasValue)
					transferQuery.Where(o => o.Date >= Filter.RestrictStartDate.Value);
				if(Filter.RestrictEndDate.HasValue)
					transferQuery.Where(o => o.Date < Filter.RestrictEndDate.Value.AddDays(1));

				var transferList = transferQuery
					.JoinAlias(() => transferAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => transferAlias.WarehouseFrom, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => transferAlias.WarehouseTo, () => warehouseReceiptAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   			.Select(() => transferAlias.Id).WithAlias(() => resultAlias.Id)
							.Select(() => transferAlias.Date).WithAlias(() => resultAlias.Date)
							.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
							.Select(() => warehouseReceiptAlias.Name).WithAlias(() => resultAlias.ReceiptWarehouse)
							.Select(() => warehouseExpenseAlias.Name).WithAlias(() => resultAlias.ExpenseWarehouse)
						   )
				.TransformUsing(Transformers.AliasToBean<StockDocumentsVMNode>())
				.List<StockDocumentsVMNode>();

				transferList.ToList().ForEach(x => x.DocTypeEnum = StokDocumentType.TransferDoc);
				result.AddRange(transferList);
			}

			if(Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == StokDocumentType.MassExpense) {
				var transferQuery = UoW.Session.QueryOver<MassExpense>(() => massExpenseAlias);
				if(Filter.RestrictStartDate.HasValue)
					transferQuery.Where(o => o.Date >= Filter.RestrictStartDate.Value);
				if(Filter.RestrictEndDate.HasValue)
					transferQuery.Where(o => o.Date < Filter.RestrictEndDate.Value.AddDays(1));



				var massExpenseList = transferQuery
					.JoinAlias(() => massExpenseAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => massExpenseAlias.WarehouseFrom, () => warehouseExpenseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   			.Select(() => massExpenseAlias.Id).WithAlias(() => resultAlias.Id)
							.Select(() => massExpenseAlias.Date).WithAlias(() => resultAlias.Date)
							.Select(() => warehouseExpenseAlias.Name).WithAlias(() => resultAlias.ExpenseWarehouse)
							.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
							)

				.TransformUsing(Transformers.AliasToBean<StockDocumentsVMNode>())
				.List<StockDocumentsVMNode>();

				massExpenseList.ToList().ForEach(x => x.DocTypeEnum = StokDocumentType.MassExpense);
				result.AddRange(massExpenseList);

			}

			result.Sort((x, y) =>
			{
				if (x.Date < y.Date)
					return 1;
				if (x.Date == y.Date)
					return 0;
				return -1;
			});

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<StockDocumentsVMNode>.Create()
			.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Тип документа").SetDataProperty(node => node.DocTypeString)
		    .AddColumn("Операция").SetDataProperty(node => node.Operation)
			.AddColumn("Дата").SetDataProperty(node => node.DateString)
			.AddColumn("Склад").AddTextRenderer(x => x.Warehouse)
			.AddColumn("Автор").SetDataProperty(node => node.Author)
			.AddColumn("Детали").AddTextRenderer(node => node.Description)
			.Finish();

		public override IColumnsConfig ColumnsConfig
		{
			get { return columnsConfig; }
		}


		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(object updatedSubject)
		{
			return true;
		}

		#endregion

		public StockDocumentsVM(StockDocumentsFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public StockDocumentsVM() : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new StockDocumentsFilter(UoW);
		}

		public StockDocumentsVM(IUnitOfWork uow) : base (
			typeof(Income),
			typeof(Expense),
			typeof(Writeoff),
			typeof(Transfer)
		)
		{
			this.UoW = uow;
		}
	}

	public class StockDocumentsVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public StokDocumentType DocTypeEnum { get; set; }

		public string DocTypeString { get { return DocTypeEnum.GetEnumTitle(); } }

		public DateTime Date { get; set; }

		public string DateString { get { return Date.ToShortDateString(); } }

		public IncomeOperations IncomeOperation { get; set; }

		public ExpenseOperations ExpenseOperation { get; set; }

		public string Operation{
			get{
				switch(DocTypeEnum)
				{
					case StokDocumentType.IncomeDoc:
						return IncomeOperation.GetEnumTitle();
					case StokDocumentType.ExpenseDoc:
						return ExpenseOperation.GetEnumTitle();
					default:
						return null;
				}
			}
		}

		[UseForSearch]
		[SearchHighlight]
		public string Description
		{
			get
			{
				string text = String.Empty;
				if (!String.IsNullOrWhiteSpace(Employee))
					text += $"Сотрудник: {Employee} ";
				if (!String.IsNullOrWhiteSpace(Facility))
					text += $"Объект: {Facility} ";
				return text;
			}
		}

		public string ReceiptWarehouse { get; set; }
		public string ExpenseWarehouse { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Warehouse => ReceiptWarehouse == null && ExpenseWarehouse == null ? String.Empty :
			ReceiptWarehouse == null ? $" {ExpenseWarehouse} =>" : $"{ExpenseWarehouse} => {ReceiptWarehouse}";

		public string Facility { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Author { get; set; }

		public string EmployeeSurname { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }

		public string Employee { get { return PersonHelper.PersonFullName(EmployeeSurname, EmployeeName, EmployeePatronymic); }}
	}
}
