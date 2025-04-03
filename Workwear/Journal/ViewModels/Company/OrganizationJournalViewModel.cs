using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Tools;
using Workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{
	public class OrganizationJournalViewModel : EntityJournalViewModelBase<Organization, OrganizationViewModel, OrganizationJournalNode>, IDialogDocumentation
	{
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("organization.html#organizations");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(Organization));
		#endregion
		public OrganizationJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<Organization> ItemsQuery(IUnitOfWork uow)
		{
			OrganizationJournalNode resultAlias = null;
			return uow.Session.QueryOver<Organization>()
				.Where(GetSearchCriterion<Organization>(
					x => x.Id, 
					x => x.Name,
					x => x.Address
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Address).WithAlias(() => resultAlias.Address)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<OrganizationJournalNode>());
		}
	}

	public class OrganizationJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
	}
}
