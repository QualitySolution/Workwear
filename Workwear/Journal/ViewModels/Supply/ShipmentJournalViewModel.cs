using System;
using Autofac;
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
using Workwear.Journal.Filter.ViewModels.Supply;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Supply;

namespace workwear.Journal.ViewModels.Supply {
	public class ShipmentJournalViewModel: EntityJournalViewModelBase<Shipment, ShipmentViewModel, ShipmentJournalNode>, IDialogDocumentation {
		public FeaturesService FeaturesService { get; }
		public ShipmentJournalFilterViewModel Filter { get; }
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#planned-shipment");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(Shipment));
		#endregion
		public ShipmentJournalViewModel(
			ILifetimeScope autofacScope,
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
			
			JournalFilter = Filter = autofacScope.Resolve<ShipmentJournalFilterViewModel>(
				new TypedParameter(typeof(JournalViewModelBase), this));
		}

		protected override IQueryOver<Shipment> ItemsQuery(IUnitOfWork uow) {
			ShipmentJournalNode resultAlias = null;
			Shipment shipmentAlias = null;
			UserBase authorAlias = null;

			var query = uow.Session.QueryOver<Shipment>(() => shipmentAlias)
				.Where(GetSearchCriterion(
					() => shipmentAlias.Id,
					() => authorAlias.Name,
					() => shipmentAlias.Comment)
				);

			if(Filter.NotFullOrdered)
				query.WhereNot(x => x.FullOrdered);

			if(Filter.Status != null) 
				query.Where(x => x.Status == Filter.Status);

			return query
				.JoinAlias(() => shipmentAlias.CreatedbyUser, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList((list) => list
					.SelectGroup(x => x.Id)
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.StartPeriod).WithAlias(() => resultAlias.StartPeriod)
					.Select(x => x.EndPeriod).WithAlias(() => resultAlias.EndPeriod)
					.Select(x => shipmentAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(x => authorAlias.Name).WithAlias(() => resultAlias.Author)
					.Select(x => x.FullOrdered).WithAlias(() => resultAlias.FullOrdered)
					.Select(x => x.FullReceived).WithAlias(() => resultAlias.FullReceived)
					.Select(x => x.HasReceive).WithAlias(() => resultAlias.HasReceive)
					.Select(x => x.Submitted).WithAlias(() => resultAlias.Submitted)
					.Select(x => x.CreationDate).WithAlias(() => resultAlias.CreationDate)
					.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
				)
				.OrderBy(x => x.StartPeriod).Desc
				.TransformUsing(Transformers.AliasToBean<ShipmentJournalNode>());
		}
		public static string ColorsLegendText = 
			"<span color='black'>●</span> — пока всё по плану\n" +
			"<span color='green'>●</span> —  в текущем статусе всё выполнео\n" +
			"<span color='yellow'>●</span> — заказано частично\n" +
			"<span color='orange'>●</span> — поставлено частично\n" +
			"<span color='blue'>●</span> — заявка уже 3 дня как передана в закупку\n"+
			"<span color='red'>●</span> — в срок поставки не было поступлений\n";
	}

	public class ShipmentJournalNode {
		[SearchHighlight] public int Id { get; set; }
		public DateTime StartPeriod { get; set; }
		public DateTime EndPeriod { get; set; }
		public ShipmentStatus Status { get; set; }
		public string StatusText => Status.GetEnumTitle();
		public DateTime? Submitted { get; set; }
		[SearchHighlight] public string Author { get; set; }
		public DateTime CreationDate { get; set; }
		[SearchHighlight] public String Comment { get; set; }
		public bool FullReceived { get; set; }
		public bool FullOrdered { get; set; }
		public bool HasReceive { get; set; }
		public string RowColor {
			get { //Не забываем править ColorsLegendText
				if(EndPeriod < DateTime.Now && !HasReceive)
					return "red";
				if(Status == ShipmentStatus.Ordered) {
					if(FullOrdered)
						return "green";
					return "yellow";
				}
				if(Status == ShipmentStatus.Received) {
					if(FullOrdered && FullReceived)
						return "green";
					if(FullReceived)
						return "yellow";
					return "orange";
				}
				if(Submitted != null) //Отсчёт 3 рабочих дней
					for((DateTime t, int n) d = ((DateTime)Submitted, 0); d.t <= DateTime.Now;
							d.t = d.t.AddDays(1), d.n += (d.t.DayOfWeek != DayOfWeek.Sunday && d.t.DayOfWeek != DayOfWeek.Saturday) ? 1 : 0) 
						if(d.n > 3)
							return "blue";
				
				return "black";
			}
		}
	}
}
