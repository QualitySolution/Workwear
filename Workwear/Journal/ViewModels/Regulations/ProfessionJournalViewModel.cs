using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Regulations;
using workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations
{
	public class ProfessionJournalViewModel : EntityJournalViewModelBase<Profession, ProfessionViewModel, ProfessionJournalNode>
	{
		public ProfessionJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<Profession> ItemsQuery(IUnitOfWork uow)
		{
			ProfessionJournalNode resultAlias = null;
			return uow.Session.QueryOver<Profession>()
				.Where(GetSearchCriterion<Profession>(
					x => x.Code,
					x => x.Name
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Code).WithAlias(() => resultAlias.Code)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<ProfessionJournalNode>());
		}
	}

	public class ProfessionJournalNode
	{
		public int Id { get; set; }
		public uint? Code { get; set; }
		public string Name { get; set; }
	}
}
