using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company {
	public class EmployeeGroupJournalViewModel : EntityJournalViewModelBase<EmployeeGroup, EmployeeGroupViewModel, EmployeeGroupJournalNode>{
		public EmployeeGroupJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null) 
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<EmployeeGroup> ItemsQuery(IUnitOfWork uow) {
			{
				EmployeeGroupJournalNode resultAlias = null;
				EmployeeGroup employeeGroupAlias = null;
				EmployeeGroupItem itemAlias = null;
                				
				var itemsSubQuery = QueryOver.Of<EmployeeGroupItem>(() => itemAlias)
					.Where(() => itemAlias.Group.Id == employeeGroupAlias.Id)
					.ToRowCountQuery();
				
				return uow.Session.QueryOver<EmployeeGroup>(() => employeeGroupAlias)
					.Where(GetSearchCriterion<EmployeeGroup>(
						x => x.Id, 
						x => x.Name,
						x => x.Comment
					))
					.SelectList((list) => list
						.Select(x => x.Id).WithAlias(() => resultAlias.Id)
						.Select(x => x.Name).WithAlias(() => resultAlias.Name)
						.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
						.SelectSubQuery(itemsSubQuery).WithAlias(() => resultAlias.Count)
					).OrderBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<EmployeeGroupJournalNode>());
			}
		}
	}
	
	public class EmployeeGroupJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Comment { get; set; }
		public int Count { get; set; }
	}
}
