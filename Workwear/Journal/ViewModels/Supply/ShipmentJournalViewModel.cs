using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.ViewModels.Extension;
using Workwear.Domain.Supply;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Supply;

namespace workwear.Journal.ViewModels.Supply {
	public class ShipmentJournalViewModel: EntityJournalViewModelBase<Shipment, ShipmentViewModel, ShipmentJournalNode>, IDialogDocumentation {
		public FeaturesService FeaturesService { get; }
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#planned-shipment");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(Shipment));
		#endregion
		public ShipmentJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			FeaturesService featuresService,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null
		) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) 
		{
			FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			
			TableSelectionMode = JournalSelectionMode.Multiple;
			
		}

		protected override IQueryOver<Shipment> ItemsQuery(IUnitOfWork uow) {
			ShipmentJournalNode resultAlias = null;
			Shipment shipmentAlias = null;
			UserBase authorAlias = null;

			var query = uow.Session.QueryOver<Shipment>(() => shipmentAlias);
			return query
				.Where(GetSearchCriterion(
					() => shipmentAlias.Id, 
						()=>authorAlias.Name,
						()=>shipmentAlias.Comment)
				)
				.JoinAlias(()=>shipmentAlias.CreatedbyUser, ()=>authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.StartPeriod).WithAlias(() => resultAlias.StartPeriod)
					.Select(x => x.EndPeriod).WithAlias(() => resultAlias.EndPeriod)
					.Select(x=>shipmentAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(x => authorAlias.Name).WithAlias(() => resultAlias.Author)
					.Select(x => x.CreationDate).WithAlias(() => resultAlias.CreationDate)
					.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
				).OrderBy(x => x.StartPeriod).Desc
				.TransformUsing(Transformers.AliasToBean<ShipmentJournalNode>());
		}
		
	}

	public class ShipmentJournalNode {
		[SearchHighlight]
		public int Id { get; set; }
		public DateTime StartPeriod { get; set; }
		public DateTime EndPeriod { get; set; }
		public ShipmentStatus Status { get; set; }
		public string StatusText => Status.GetEnumTitle();
		[SearchHighlight]
		public string Author { get; set; }
		public DateTime CreationDate { get; set; }
		[SearchHighlight]
		public String Comment { get; set; }
		
	}
}
