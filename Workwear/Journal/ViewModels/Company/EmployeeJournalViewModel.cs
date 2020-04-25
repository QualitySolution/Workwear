﻿using System;
using Autofac;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using workwear.Dialogs.Organization;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Journal.Filter.ViewModels.Company;
using workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{
	public class EmployeeJournalViewModel : EntityJournalViewModelBase<EmployeeCard, EmployeeViewModel, EmployeeJournalNode>
	{
		private readonly ITdiCompatibilityNavigation tdiNavigationManager;

		/// <summary>
		/// Для хранения пользовательской информации как в WinForms
		/// </summary>
		public object Tag;

		public EmployeeFilterViewModel Filter { get; private set; }

		public EmployeeJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, ITdiCompatibilityNavigation navigationManager, 
										IDeleteEntityService deleteEntityService, ILifetimeScope autofacScope, ICurrentPermissionService currentPermissionService = null) 
										: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = false;

			AutofacScope = autofacScope;
			JournalFilter = Filter = AutofacScope.Resolve<EmployeeFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
			this.tdiNavigationManager = navigationManager;
		}

		protected override IQueryOver<EmployeeCard> ItemsQuery(IUnitOfWork uow)
		{
			EmployeeJournalNode resultAlias = null;

			Post postAlias = null;
			Subdivision facilityAlias = null;
			EmployeeCard employeeAlias = null;

			var employees = uow.Session.QueryOver<EmployeeCard>(() => employeeAlias);
			if(Filter.ShowOnlyWork)
				employees.Where(x => x.DismissDate == null);

			return employees
				.Where(GetSearchCriterion<EmployeeCard>(
					x => x.Id,
					x => x.CardNumber,
					x => x.PersonnelNumber,
					x => x.LastName,
					x => x.FirstName,
					x => x.Patronymic
					))
				.JoinAlias(() => employeeAlias.Post, () => postAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeAlias.Subdivision, () => facilityAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.CardNumber).WithAlias(() => resultAlias.CardNumber)
					.Select(x => x.PersonnelNumber).WithAlias(() => resultAlias.PersonnelNumber)
					.Select(x => x.FirstName).WithAlias(() => resultAlias.FirstName)
					.Select(x => x.LastName).WithAlias(() => resultAlias.LastName)
					.Select(x => x.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => employeeAlias.DismissDate).WithAlias(() => resultAlias.DismissDate)
					.Select(() => postAlias.Name).WithAlias(() => resultAlias.Post)
	   				.Select(() => facilityAlias.Name).WithAlias(() => resultAlias.Subdivision)
 					)
				.OrderBy(() => employeeAlias.LastName).Asc
				.ThenBy(() => employeeAlias.FirstName).Asc
				.ThenBy(() => employeeAlias.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<EmployeeJournalNode>());
		}

		protected override void CreateEntityDialog()
		{
			tdiNavigationManager.OpenTdiTab<EmployeeCardDlg>(this);
		}

		protected override void EditEntityDialog(EmployeeJournalNode node)
		{
			tdiNavigationManager.OpenTdiTab<EmployeeCardDlg, int>(this, node.Id);
		}
	}

	public class EmployeeJournalNode
	{
		public int Id { get; set; }
		[SearchHighlight]
		public string CardNumber { get; set; }

		[SearchHighlight]
		public string CardNumberText {
			get {
				return CardNumber ?? Id.ToString();
			}
		}

		[SearchHighlight]
		public string PersonnelNumber { get; set; }

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Patronymic { get; set; }

		[SearchHighlight]
		public string FIO {
			get {
				return String.Join(" ", LastName, FirstName, Patronymic);
			}
		}

		public string Post { get; set; }

		public string Subdivision { get; set; }

		public bool Dismiss { get { return DismissDate.HasValue; } }

		public DateTime? DismissDate { get; set; }
		public string Title => PersonHelper.PersonNameWithInitials(LastName, FirstName, Patronymic);
	}
}
