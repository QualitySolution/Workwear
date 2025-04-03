using System;
using System.Linq;
using Autofac;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities;
using QS.ViewModels.Extension;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Models.Sizes;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock
{
	public class NomenclatureJournalViewModel : EntityJournalViewModelBase<Nomenclature, NomenclatureViewModel, NomenclatureJournalNode>, IDialogDocumentation
	{
		private readonly IInteractiveService interactiveService;
		private readonly SizeTypeReplaceModel sizeTypeReplaceModel;
		private readonly ModalProgressCreator progressCreator;
		public FeaturesService FeaturesService { get; }
		public NomenclatureFilterViewModel Filter { get; private set; }
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#nomenclatures");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(Nomenclature));
		#endregion
		public NomenclatureJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager,
			ILifetimeScope autofacScope, 
			FeaturesService featuresService,
			SizeTypeReplaceModel sizeTypeReplaceModel,
			ModalProgressCreator progressCreator,
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			this.sizeTypeReplaceModel = sizeTypeReplaceModel ?? throw new ArgumentNullException(nameof(sizeTypeReplaceModel));
			this.progressCreator = progressCreator ?? throw new ArgumentNullException(nameof(progressCreator));
			FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			UseSlider = true;

			JournalFilter = Filter = autofacScope.Resolve<NomenclatureFilterViewModel>(new TypedParameter(typeof(NomenclatureJournalViewModel), this));
			MakePopup();
			
			TableSelectionMode = JournalSelectionMode.Multiple;
			CreateActions();
		}

		protected override IQueryOver<Nomenclature> ItemsQuery(IUnitOfWork uow)
		{
			NomenclatureJournalNode resultAlias = null;
			ItemsType itemsTypeAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProtectionTools protectionToolsAlias = null;

			var query = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias);
			if(Filter.ItemType != null)
				query.Where(x => x.Type.Id == Filter.ItemType.Id);
			if (!Filter.ShowArchival)
				query.Where(x => !x.Archival);
			if(Filter.OnlyWithRating)
				query.Where(x => x.Rating != null);
			if(Filter.ProtectionTools != null)
				query.Left.JoinAlias(n => n.ProtectionTools, () => protectionToolsAlias)
					.Where(() => protectionToolsAlias.Id == Filter.ProtectionTools.Id);

			return query
				.Left.JoinAlias(n => n.Type, () => itemsTypeAlias)
				.Where(GetSearchCriterion(
					() => nomenclatureAlias.Id,
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.Number,
					() => itemsTypeAlias.Name
				))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Number).WithAlias(() => resultAlias.Number)
					.Select(() => itemsTypeAlias.Name).WithAlias(() => resultAlias.ItemType)
				 	.Select(() => nomenclatureAlias.Archival).WithAlias(() => resultAlias.Archival)
					.Select(() => nomenclatureAlias.Sex).WithAlias(() => resultAlias.Sex)
					.Select(x => x.UseBarcode).WithAlias(() => resultAlias.UseBarcode)
					.Select(x => x.Washable).WithAlias(() => resultAlias.Washable)
					.Select(x => x.SaleCost).WithAlias(() => resultAlias.SaleCost)
					.Select(x => x.Rating).WithAlias(() => resultAlias.Rating)
					.Select(x => x.RatingCount).WithAlias(() => resultAlias.RatingCount)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NomenclatureJournalNode>());
		}

		#region Popup
		private void MakePopup()
		{
			PopupActionsList.Add(new JournalAction("Создать копию номенклатуры", (arg) => arg.Length == 1, (arg) => true, CopyNomenclature));
			PopupActionsList.Add(new JournalAction("Открыть складские движения", n => true, n => true, OpenMovements));
		}

		private void CopyNomenclature(object[] nodes)
		{
			if(nodes.Length != 1)
				return;
			
			var page = NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
			page.ViewModel.CopyFrom(nodes[0].GetId());
		}
		private void OpenMovements(object[] nodes)
		{
			foreach(NomenclatureJournalNode node in nodes) {
				//Создаем заглушку для передачи id, чтобы не создавать лишнее обращение к базе, за полноценным объектом
				var nomenclature = new Nomenclature {
					Id = node.Id
				};
				NavigationManager.OpenViewModel<StockMovementsJournalViewModel>(this, 
					addingRegistrations: builder => builder.RegisterInstance(nomenclature));
			}
		}
		#endregion
		
		#region Actions
		private void CreateActions() {
			base.CreateNodeActions();
			var changeTypeAction = new JournalAction("Изменить тип",
				(selected) => selected.Any(),
				(selected) => SelectionMode == JournalSelectionMode.None
			);
			NodeActionsList.Add(changeTypeAction);
			
			var itemTypes = UoW.GetAll<ItemsType>().OrderBy(x => x.Name).ToList();
			foreach(ItemsType itemsType in itemTypes) {
				var updateTypeAction = new JournalAction(itemsType?.Name,
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => SetType(selected.Cast<NomenclatureJournalNode>().ToArray(), itemsType)
				);
				changeTypeAction.ChildActionsList.Add(updateTypeAction);
			}
		}
		
		private void SetType(NomenclatureJournalNode[] nodes, ItemsType itemsType) {
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var ids = nodes.Select(n => n.Id).ToArray();
				var nomenclatures = uow.Session.QueryOver<Nomenclature>()
					.Where(n =>  n.Id.IsIn(ids))
					.Fetch(SelectMode.Fetch, n => n.Type)
					.Fetch(SelectMode.Fetch)
					.List();
				if(!sizeTypeReplaceModel.TryReplaceSizes(uow, interactiveService, progressCreator, nomenclatures.ToArray(), itemsType.SizeType, itemsType.HeightType))
					return;
				uow.GetAll<Nomenclature>()
					.Where(n => ids.Contains(n.Id))
					.UpdateBuilder()
					.Set(n => n.Type, itemsType)
					.Update();
			}
			Refresh();
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
		public ClothesSex Sex { get; set; }
		public string SexText => Sex.GetEnumShortTitle();
		public bool Archival { get; set; }
		public bool UseBarcode { get; set; }
		public bool Washable { get; set; }
		public decimal SaleCost { get; set; }
		public string SaleCostText => SaleCost > 0 ? CurrencyWorks.GetShortCurrencyString (SaleCost) : String.Empty;
		public float? Rating { get; set; }
		public int? RatingCount { get; set; }

		public string RatingText => Rating == null ? null : $"{Rating:F1} ({RatingCount})";
		public string UseBarcodeText => UseBarcode ? "Используется" : String.Empty;
		public string WashableText => Washable ? "Да" : String.Empty;
	}
}
