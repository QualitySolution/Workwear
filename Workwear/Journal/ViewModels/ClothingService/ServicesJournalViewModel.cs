using System;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Report.ViewModels;
using QS.Utilities;
using Workwear.Domain.ClothingService;
using Workwear.ReportParameters.ViewModels;
using Workwear.ViewModels.ClothingService;

namespace workwear.Journal.ViewModels.ClothingService {
	public class ServicesJournalViewModel : EntityJournalViewModelBase<Service, ServiceViewModel, ServiceJournalNode>
	{
		public ServicesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) 
		{
			UseSlider = true;
			
			NodeActionsList.Add(new JournalAction("Печать кодов",
				x => true,
				x => true,
				x => navigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ClothingServicesCodeViewModel))
			));
		}

		protected override IQueryOver<Service> ItemsQuery(IUnitOfWork uow)
		{
			ServiceJournalNode resultAlias = null;
			Service ServiceAlias = null;

			return uow.Session.QueryOver<Service>(() => ServiceAlias)
				.Where(GetSearchCriterion(
					() => ServiceAlias.Id,
					() => ServiceAlias.Name
				))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Cost).WithAlias(() => resultAlias.Cost)
					.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<ServiceJournalNode>());
		}
	}

	public class ServiceJournalNode 
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Cost { get; set; }
		public string CostText => Cost > 0 ? CurrencyWorks.GetShortCurrencyString (Cost) : String.Empty;
		public string Comment { get; set; }
	}
}
