using System;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock 
{
	public class OwnerJournalViewModel : EntityJournalViewModelBase<Owner, OwnerViewModel, OwnerJournalNode> 
	{
		public OwnerJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<Owner> ItemsQuery(IUnitOfWork uow) 
		{
			OwnerJournalNode resultAlias = null;
			return uow.Session.QueryOver<Owner>()
				.Where(GetSearchCriterion<Owner>(x => x.Name))
				
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Description).WithAlias(() => resultAlias.Description)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<OwnerJournalNode>());
		}
	}

	public class OwnerJournalNode 
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string ShortDescription {
			get {
				if(String.IsNullOrEmpty(Description))
					return String.Empty;
				if(Description.Length <= 100)
					return Description;
				return Description.Remove(100, Description.Length - 101) + "...";
			}
		}
	}
}
