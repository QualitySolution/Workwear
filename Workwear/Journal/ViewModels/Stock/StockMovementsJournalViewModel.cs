using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using workwear.Domain.Operations;
using workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Models.Operations;
using workwear.Models.Stock;
using workwear.Tools.Features;

namespace workwear.Journal.ViewModels.Stock
{
	public class StockMovmentsJournalViewModel : JournalViewModelBase
	{
		private readonly FeaturesService featuresService;
		private readonly OpenStockDocumentsModel openDocuments;

		public StockMovementsFilterViewModel Filter { get; private set; }

		public StockMovmentsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation, ILifetimeScope autofacScope, FeaturesService featuresService, OpenStockDocumentsModel openDocuments) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			AutofacScope = autofacScope;
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.openDocuments = openDocuments ?? throw new ArgumentNullException(nameof(openDocuments));
			JournalFilter = Filter = AutofacScope.Resolve<StockMovementsFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			var dataLoader = new ThreadDataLoader<StockMovmentsJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			CreateNodeActions();

			UpdateOnChanges(typeof(WarehouseOperation));
			TabName = "Складские движения";
		}

		protected IQueryOver<WarehouseOperation> ItemsQuery(IUnitOfWork uow)
		{
			StockMovmentsJournalNode resultAlias = null;

			WarehouseOperation warehouseOperationAlias = null;

			ExpenseItem expenseItemAlias = null;
			IncomeItem incomeItemAlias = null;
			MassExpenseOperation massExpenseOperationAlias = null; //FIXME Не реализовано
			TransferItem transferItemAlias = null;
			CollectiveExpenseItem collectiveExpenseItemAlias = null;
			WriteoffItem writeoffItemAlias = null;

			Nomenclature nomenclatureAlias = null;
			ItemsType itemtypesAlias = null;
			MeasurementUnits unitsAlias = null;

			var queryStock = uow.Session.QueryOver<WarehouseOperation>(() => warehouseOperationAlias);

			if(Filter.Warehouse != null)
				queryStock.Where(x => x.ReceiptWarehouse == Filter.Warehouse || x.ExpenseWarehouse == Filter.Warehouse);

			if (Filter.StartDate.HasValue) 
				queryStock.Where(x => x.OperationTime >= Filter.StartDate.Value);

			if(Filter.EndDate.HasValue)
				queryStock.Where(x => x.OperationTime < Filter.EndDate.Value.AddDays(1));

			if(Filter.StockPosition != null) {
				queryStock.Where(x => x.Nomenclature == Filter.StockPosition.Nomenclature);
				queryStock.Where(x => x.Size == Filter.StockPosition.Size);
				queryStock.Where(x => x.Growth == Filter.StockPosition.Growth);
				queryStock.Where(x => x.WearPercent == Filter.StockPosition.WearPercent);
			}

			if(Filter.Nomenclature != null)
				queryStock.Where(x => x.Nomenclature == Filter.Nomenclature);

			if(!String.IsNullOrWhiteSpace(Filter.Size))
				queryStock.Where(x => x.Size == Filter.Size);

			if(!String.IsNullOrWhiteSpace(Filter.Growth))
				queryStock.Where(x => x.Growth == Filter.Growth);

			IProjection receptProjection, expenseProjection;
			if(Filter.Warehouse != null) {
				receptProjection = Projections.Conditional(
						Restrictions.Eq(Projections.Property<WarehouseOperation>(x => x.ReceiptWarehouse.Id), Filter.Warehouse.Id),
						Projections.Constant(true),
						Projections.Constant(false)
					);
				expenseProjection = Projections.Conditional(
					Restrictions.Eq(Projections.Property<WarehouseOperation>(x => x.ExpenseWarehouse.Id), Filter.Warehouse.Id),
					Projections.Constant(true),
					Projections.Constant(false)
				);

				switch(Filter.Direction) {
					case DirectionOfOperation.expense:
						queryStock.Where(x => x.ExpenseWarehouse.Id == Filter.Warehouse.Id);
						break;
					case DirectionOfOperation.receipt:
						queryStock.Where(x => x.ReceiptWarehouse.Id == Filter.Warehouse.Id);
						break;
				}
			}
			else {
				receptProjection = Projections.Conditional(
						Restrictions.IsNotNull(Projections.Property<WarehouseOperation>(x => x.ReceiptWarehouse.Id)),
						Projections.Constant(true),
						Projections.Constant(false)
					);
				expenseProjection = Projections.Conditional(
					Restrictions.IsNotNull(Projections.Property<WarehouseOperation>(x => x.ExpenseWarehouse.Id)),
					Projections.Constant(true),
					Projections.Constant(false)
				);

				switch(Filter.Direction) {
					case DirectionOfOperation.expense:
						queryStock.Where(x => x.ReceiptWarehouse == null);
						break;
					case DirectionOfOperation.receipt:
						queryStock.Where(x => x.ExpenseWarehouse == null);
						break;
				}
			}

			queryStock
				.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Type, () => itemtypesAlias)
				.JoinAlias(() => itemtypesAlias.Units, () => unitsAlias)
				.JoinEntityAlias(() => expenseItemAlias, () => expenseItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => collectiveExpenseItemAlias, () => collectiveExpenseItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => incomeItemAlias, () => incomeItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => transferItemAlias, () => transferItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => writeoffItemAlias, () => writeoffItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin);

			if(Filter.CollapseOperationItems) {
				queryStock.SelectList(list => list
					.SelectGroup(() => warehouseOperationAlias.Nomenclature.Id)
					.Select(() => warehouseOperationAlias.OperationTime).WithAlias(() => resultAlias.OperationTime)
					.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
					.Select(receptProjection).WithAlias(() => resultAlias.Receipt)
					.Select(expenseProjection).WithAlias(() => resultAlias.Expense)
					.SelectSum(() => warehouseOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
			
					//Ссылки
					.SelectGroup(() => expenseItemAlias.ExpenseDoc.Id).WithAlias(() => resultAlias.ExpenceId)
					.SelectGroup(() => collectiveExpenseItemAlias.Document.Id).WithAlias(() => resultAlias.CollectiveExpenseId)
					.SelectGroup(() => incomeItemAlias.Document.Id).WithAlias(() => resultAlias.IncomeId)
					.SelectGroup(() => transferItemAlias.Document.Id).WithAlias(() => resultAlias.TransferItemId)
					.SelectGroup(() => writeoffItemAlias.Document.Id).WithAlias(() => resultAlias.WriteoffId)

					.SelectGroup(() => warehouseOperationAlias.Size).WithAlias(() => resultAlias.Size)
					.SelectGroup(() => warehouseOperationAlias.Growth).WithAlias(() => resultAlias.Growth)
					.SelectGroup(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
				);
			}
			else {
				queryStock.SelectList(list => list
					.Select(() => warehouseOperationAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => warehouseOperationAlias.OperationTime).WithAlias(() => resultAlias.OperationTime)
					.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
					.Select(receptProjection).WithAlias(() => resultAlias.Receipt)
					.Select(expenseProjection).WithAlias(() => resultAlias.Expense)
					.Select(() => warehouseOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => warehouseOperationAlias.Size).WithAlias(() => resultAlias.Size)
					.Select(() => warehouseOperationAlias.Growth).WithAlias(() => resultAlias.Growth)
					.Select(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
					//Ссылки
					.Select(() => expenseItemAlias.Id).WithAlias(() => resultAlias.ExpenceItemId)
					.Select(() => expenseItemAlias.ExpenseDoc.Id).WithAlias(() => resultAlias.ExpenceId)
					.Select(() => collectiveExpenseItemAlias.Id).WithAlias(() => resultAlias.CollectiveExpenseItemId)
					.Select(() => collectiveExpenseItemAlias.Document.Id).WithAlias(() => resultAlias.CollectiveExpenseId)
					.Select(() => incomeItemAlias.Id).WithAlias(() => resultAlias.IncomeItemId)
					.Select(() => incomeItemAlias.Document.Id).WithAlias(() => resultAlias.IncomeId)
					.Select(() => transferItemAlias.Id).WithAlias(() => resultAlias.TransferId)
					.Select(() => transferItemAlias.Document.Id).WithAlias(() => resultAlias.TransferItemId)
					.Select(() => writeoffItemAlias.Id).WithAlias(() => resultAlias.WriteoffItemId)
					.Select(() => writeoffItemAlias.Document.Id).WithAlias(() => resultAlias.WriteoffId)
				);
			}

			return queryStock
				.OrderBy(x => x.OperationTime).Desc
				.ThenBy(x => x.Id).Asc
				.TransformUsing(Transformers.AliasToBean<StockMovmentsJournalNode>());
		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();
			var updateStatusAction = new JournalAction("Открыть документ",
					(selected) => selected.Cast<StockMovmentsJournalNode>().Any(x => x.DocumentId.HasValue),
					(selected) => true,
					(selected) => OpenDocument(selected.Cast<StockMovmentsJournalNode>().ToArray())
					);
			NodeActionsList.Add(updateStatusAction);
		}

		void OpenDocument(StockMovmentsJournalNode[] nodes)
		{
			foreach(var node in nodes.Where(x => x.DocumentId.HasValue))
				openDocuments.EditDocumentDialog(this, node);
		}
	}

	public class StockMovmentsJournalNode : OperationToDocumentReference
	{
		public int Id { get; set; }
		public DateTime OperationTime { get; set; }
		public string UnitsName { get; set; }
		public bool Receipt { get; set; }
		public bool Expense { get; set; }
		public string Size { get; set; }
		public string Growth { get; set; }
		public decimal WearPercent { get; set; }
		public int Amount { get; set; }
		public string NomenclatureName { get; set; }
		public string AmountText => $"{Direction} {Amount} {UnitsName}";
		public string OperationTimeText => OperationTime.ToString("g");
		public string WearPercentText => WearPercent.ToString("P0");
		public string DocumentText => DocumentType != null ? DocumentTitle : null;
		public string Direction {
			get {
				if(Expense && Receipt)
					return "<span foreground=\"Blue\" weight=\"ultrabold\">±</span>";
				if(Receipt)
					return "<span foreground=\"Green\" weight=\"ultrabold\">+</span>";
				if(Expense)
					return "<span foreground=\"Red\" weight=\"ultrabold\">-</span>";
				return "<span foreground=\"Fuchsia\" weight=\"ultrabold\">?</span>";
			}
		}
	}
}
