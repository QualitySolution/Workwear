using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Journal;
using QS.Project.Services;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Tools;
using Workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company {
	public class CostCenterJournalViewModel : EntityJournalViewModelBase<CostCenter, CostCenterViewModel, CostCenterJournalNode>, IDialogDocumentation
	{
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("organization.html#mvz");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(CostCenter));
		#endregion
		public CostCenterJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
		}
		protected override IQueryOver<CostCenter> ItemsQuery(IUnitOfWork uow)
		{
			CostCenterJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<CostCenter>()
				.Where(GetSearchCriterion<CostCenter>(
					x => x.Id,
					x => x.Code,
					x => x.Name
				));

			 return query.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Code).WithAlias(() => resultAlias.Code)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					)
					.OrderBy(x => x.Code).Asc
					.ThenBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<CostCenterJournalNode>());
		}
	}
	public class CostCenterJournalNode {
		public int Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
	}
}
