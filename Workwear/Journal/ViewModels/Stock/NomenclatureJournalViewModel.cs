using System;
using Autofac;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock
{
	public class NomenclatureJournalViewModel : EntityJournalViewModelBase<Nomenclature, NomenclatureViewModel, NomenclatureJournalNode>
	{
		public FeaturesService FeaturesService { get; }
		public NomenclatureFilterViewModel Filter { get; private set; }

		public NomenclatureJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager,
			ILifetimeScope autofacScope, 
			FeaturesService featuresService,
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			UseSlider = true;

			JournalFilter = Filter = autofacScope.Resolve<NomenclatureFilterViewModel>(new TypedParameter(typeof(NomenclatureJournalViewModel), this));
			MakePopup();
		}

		protected override IQueryOver<Nomenclature> ItemsQuery(IUnitOfWork uow)
		{
			NomenclatureJournalNode resultAlias = null;
			ItemsType itemsTypeAlias = null;
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias);
			if(Filter.ItemType != null)
				query.Where(x => x.Type.Id == Filter.ItemType.Id);
			if (!Filter.ShowArchival)
				query.Where(x => !x.Archival);
			if(Filter.OnlyWithRating)
				query.Where(x => x.Rating != null);

			return query
				.Left.JoinAlias(n => n.Type, () => itemsTypeAlias)
				.Where(GetSearchCriterion(
					() => nomenclatureAlias.Id,
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.Number,
					() => itemsTypeAlias.Name,
					() => nomenclatureAlias.Archival
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Number).WithAlias(() => resultAlias.Number)
					.Select(() => itemsTypeAlias.Name).WithAlias(() => resultAlias.ItemType)
					.Select(() => nomenclatureAlias.Archival).WithAlias(() => resultAlias.Archival)
					.Select(x => x.Rating).WithAlias(() => resultAlias.Rating)
					.Select(x => x.RatingCount).WithAlias(() => resultAlias.RatingCount)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NomenclatureJournalNode>());
		}

		#region Popup
		private void MakePopup()
		{
			PopupActionsList.Add(new JournalAction("Открыть складские движения", n => true, n => true, OpenMovements));
		}

		private void OpenMovements(object[] nodes)
		{
			foreach(NomenclatureJournalNode node in nodes) {
				//Создаем заглушку для передачи id, чтобы не создавать лишнее обращение к базе, за полноценным объектом
				var nomenclature = new Nomenclature {
					Id = node.Id
				};
				NavigationManager.OpenViewModel<StockMovmentsJournalViewModel>(this, 
					addingRegistrations: builder => builder.RegisterInstance(nomenclature));
			}
		}
		#endregion
	}

	public class NomenclatureJournalNode
	{
		public int Id { get; set; }
		[SearchHighlight]
		public string Name { get; set; }
		[SearchHighlight]
		public string Number { get; set; }
		[SearchHighlight]
		public string ItemType { get; set; }
		public bool Archival { get; set; }
		
		public float? Rating { get; set; }
		public int? RatingCount { get; set; }

		public string RatingText => Rating == null ? null : $"{Rating:F1} ({RatingCount})";
	}
}
