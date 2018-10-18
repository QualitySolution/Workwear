using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.Domain;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using workwear.Domain.Organization;
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
			Income incomeAlias = null;
			Expense expenseAlias = null;
			Writeoff writeoffAlias = null;
			StockDocumentsVMNode resultAlias = null;
			EmployeeCard employeeAlias = null;
			Facility facilityAlias = null;
			User authorAlias = null;

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
					.JoinQueryOver(() => incomeAlias.Facility, () => facilityAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => incomeAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   			.Select(() => incomeAlias.Id).WithAlias(() => resultAlias.Id)
				            .Select(() => incomeAlias.Date).WithAlias(() => resultAlias.Date)
					        .Select(() => incomeAlias.Operation).WithAlias(() => resultAlias.IncomeOperation)
					        .Select(() => facilityAlias.Name).WithAlias(() => resultAlias.Facility)
					        .Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
					        .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
					        .Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.EmployeeName)
					        .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
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
					.JoinQueryOver(() => expenseAlias.EmployeeCard, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => expenseAlias.Facility, () => facilityAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
							.Select(() => expenseAlias.Id).WithAlias(() => resultAlias.Id)
							.Select(() => expenseAlias.Date).WithAlias(() => resultAlias.Date)
					        .Select(() => expenseAlias.Operation).WithAlias(() => resultAlias.ExpenseOperation)
							.Select(() => facilityAlias.Name).WithAlias(() => resultAlias.Facility)
							.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
							.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
							.Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.EmployeeName)
							.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
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

				var writeoffList = writeoffQuery
					.JoinAlias(() => writeoffAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   			.Select(() => writeoffAlias.Id).WithAlias(() => resultAlias.Id)
							.Select(() => writeoffAlias.Date).WithAlias(() => resultAlias.Date)
							.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
					       )
				.TransformUsing(Transformers.AliasToBean<StockDocumentsVMNode>())
				.List<StockDocumentsVMNode>();

				writeoffList.ToList().ForEach(x => x.DocTypeEnum = StokDocumentType.WriteoffDoc);
				result.AddRange(writeoffList);
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
			typeof(Writeoff)
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

		public string DateString { get { return Date.ToShortDateString() + " " + Date.ToShortTimeString(); } }

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

		public string Facility { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Author { get; set; }

		public string EmployeeSurname { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }

		public string Employee { get { return StringWorks.PersonFullName(EmployeeSurname, EmployeeName, EmployeePatronymic); }}
	}
}
