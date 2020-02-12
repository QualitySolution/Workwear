using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using workwear.Domain.Company;
using workwear.ViewModels.Company;

namespace workwear.JournalViewModels.Company
{
	public class EmployeeJournalViewModel : EntityJournalViewModelBase<EmployeeCard, EmployeeViewModel, EmployeeJournalNode>
	{
		public EmployeeJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, 
										IDeleteEntityService deleteEntityService, ICurrentPermissionService currentPermissionService = null) 
										: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<EmployeeCard> ItemsQuery(IUnitOfWork uow)
		{
			EmployeeJournalNode resultAlias = null;
			return uow.Session.QueryOver<EmployeeCard>()
				.Where(GetSearchCriterion<EmployeeCard>(
					x => x.Id,
					x => x.CardNumber,
					x => x.PersonnelNumber,
					x => x.LastName,
					x => x.FirstName,
					x => x.Patronymic
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.CardNumber).WithAlias(() => resultAlias.CardNumber)
					.Select(x => x.PersonnelNumber).WithAlias(() => resultAlias.PersonnelNumber)
					.Select(x => x.FirstName).WithAlias(() => resultAlias.FirstName)
					.Select(x => x.LastName).WithAlias(() => resultAlias.LastName)
					.Select(x => x.Patronymic).WithAlias(() => resultAlias.Patronymic)
 					).TransformUsing(Transformers.AliasToBean<EmployeeJournalNode>());
		}
	}

	public class EmployeeJournalNode
	{
		public int Id { get; set; }
		public string CardNumber { get; set; }
		public string FullName { get; set; }
		public string PersonnelNumber { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string Patronymic { get; set; }
		public string Title => PersonHelper.PersonNameWithInitials(LastName, FirstName, Patronymic);
	}
}
