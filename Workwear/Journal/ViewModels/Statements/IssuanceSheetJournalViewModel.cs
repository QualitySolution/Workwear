﻿using System;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Company;
using Workwear.Domain.Statements;
using workwear.Journal.Filter.ViewModels.Statements;
using Workwear.ViewModels.Statements;

namespace workwear.Journal.ViewModels.Statements
{
	public class IssuanceSheetJournalViewModel : EntityJournalViewModelBase<IssuanceSheet, IssuanceSheetViewModel, IssuanceSheetJournalNode>
	{
		public IssuanceSheetFilterViewModel Filter { get; private set; }

		public IssuanceSheetJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService,
			ILifetimeScope autofacScope,
			INavigationManager navigationManager = null, 
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
		) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			JournalFilter = Filter = autofacScope
				.Resolve<IssuanceSheetFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
		}

		protected override IQueryOver<IssuanceSheet> ItemsQuery(IUnitOfWork uow)
		{
			IssuanceSheetJournalNode resultAlias = null;

			IssuanceSheet issuanceSheetAlias = null;
			IssuanceSheetItem issuanceSheetItemAlias = null;
			Organization organizationAlias = null;
			Subdivision subdivisionAlias = null;
			EmployeeCard employeeCardAlias = null;

			var employeesSubquery = QueryOver.Of<IssuanceSheetItem>(() => issuanceSheetItemAlias)
				.Where(() => issuanceSheetItemAlias.IssuanceSheet.Id == issuanceSheetAlias.Id)
				.JoinQueryOver(x => x.Employee, () => employeeCardAlias)
				.Select(CustomProjections
					.GroupConcat(Projections
						.Property(() => employeeCardAlias.LastName), useDistinct: true, separator: ", "));

			var query = uow.Session.QueryOver<IssuanceSheet>(() => issuanceSheetAlias);

			if(Filter.StartDate != null)
				query.Where(x => x.Date >= Filter.StartDate);
			if(Filter.EndDate != null)
				query.Where(x => x.Date <= Filter.EndDate);

			return query
				.Where(GetSearchCriterion(
					() => issuanceSheetAlias.Id,
					() => issuanceSheetAlias.DocNumber,
					() => issuanceSheetAlias.Expense.Id,
					() => issuanceSheetAlias.CollectiveExpense.Id,
					() => organizationAlias.Name,
					() => subdivisionAlias.Name,
					() => subdivisionAlias.Code
					))
				.Left.JoinAlias(s => s.Organization, () => organizationAlias)
				.Left.JoinAlias(s => s.Subdivision, () => subdivisionAlias)
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.DocNumber).WithAlias(() => resultAlias.DocNumber)
					.Select(x => x.Date).WithAlias(() => resultAlias.Date)
					.Select(() => organizationAlias.Name).WithAlias(() => resultAlias.Organigation)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					.Select(() => subdivisionAlias.Code).WithAlias(() => resultAlias.SubdivisionCode)
					.Select(x => x.Expense.Id).WithAlias(() => resultAlias.DocExpenseId)
					.Select(x => x.CollectiveExpense.Id).WithAlias(() => resultAlias.DocCollectiveExpenseId)
					.SelectSubQuery(employeesSubquery).WithAlias(() => resultAlias.Employees)
				)
				.OrderBy(() => issuanceSheetAlias.Date).Desc
				.ThenBy(() => issuanceSheetAlias.Id).Desc
				.TransformUsing(Transformers.AliasToBean<IssuanceSheetJournalNode>());
		}
	}

	public class IssuanceSheetJournalNode
	{
		public int Id { get; set; }
		public string DocNumber { get; set; }
		public string DocNumberText => String.IsNullOrWhiteSpace(DocNumber) ? Id.ToString() : DocNumber;
		public DateTime Date { get; set; }
		public string Organigation { get; set; }
		public string SubdivisionCode { get; set; }
		public string Subdivision { get; set; }

		public int? DocExpenseId { get; set; }
		public int? DocMassExpenseId { get; set; }
		public int? DocCollectiveExpenseId { get; set; }
		public string Document { 
			get{
				if(DocExpenseId != null)
					return $"Выдача №{DocExpenseId}";
				if(DocMassExpenseId != null)
					return $"Массовая выдача №{DocMassExpenseId}";
				if(DocCollectiveExpenseId != null)
					return $"Коллективная выдача №{DocCollectiveExpenseId}";
				return null;
			} }

		public string Employees { get; set; }
	}
}
