using System;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using Gamma.Widgets;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Utilities;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Tools.Features;

namespace workwear.Journal.ViewModels.Stock
{
	/// <summary>
	/// Stock balance journal view model. Для подробного отображения баланса склада
	/// </summary>
	public class StockBalanceJournalViewModel : JournalViewModelBase
	{
		public bool ShowSummary;
		public readonly FeaturesService FeaturesService;

		public StockBalanceFilterViewModel Filter { get; private set; }

		public StockBalanceJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			FeaturesService featuresService) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			JournalFilter = Filter = autofacScope.Resolve<StockBalanceFilterViewModel>(
				new TypedParameter(typeof(JournalViewModelBase), this));

			var dataLoader = new ThreadDataLoader<StockBalanceJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			CreateNodeActions();

			UpdateOnChanges(typeof(WarehouseOperation), typeof(Nomenclature));
			TabName = "Остатки по складу " + 
			          (featuresService.Available(WorkwearFeature.Warehouses) ? Filter.Warehouse?.Name : "");

			Filter.PropertyChanged += (sender, e) => 
				TabName = "Остатки по складу " + 
				          (featuresService.Available(WorkwearFeature.Warehouses) ? Filter.Warehouse?.Name : "");
			this.FeaturesService = featuresService;
		}

		protected IQueryOver<WarehouseOperation> ItemsQuery(IUnitOfWork uow)
		{
			StockBalanceJournalNode resultAlias = null;

			WarehouseOperation warehouseExpenseOperationAlias = null;
			WarehouseOperation warehouseExpenseYearOperationAlias = null;
			WarehouseOperation warehouseIncomeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;

			Nomenclature nomenclatureAlias = null;
			ItemsType itemTypesAlias = null;
			MeasurementUnits unitsAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			Owner ownerAlias = null;

			// null == null => null              null <=> null => true
			var expenseQuery = QueryOver.Of(() => warehouseExpenseOperationAlias)
				.Where(() => warehouseExpenseOperationAlias.Nomenclature.Id == warehouseOperationAlias.Nomenclature.Id
				             && (warehouseExpenseOperationAlias.WearSize.Id == warehouseOperationAlias.WearSize.Id
				                 || warehouseExpenseOperationAlias.WearSize == null && warehouseOperationAlias.WearSize == null)
				             && (warehouseExpenseOperationAlias.Height.Id == warehouseOperationAlias.Height.Id
				                 || warehouseExpenseOperationAlias.Height == null && warehouseOperationAlias.Height == null)
				             && (warehouseExpenseOperationAlias.Owner.Id == warehouseOperationAlias.Owner.Id
				                 || warehouseExpenseOperationAlias.Owner == null && warehouseOperationAlias.Owner == null)
				             && warehouseExpenseOperationAlias.WearPercent == warehouseOperationAlias.WearPercent)
				.Where(e => e.OperationTime < Filter.Date.AddDays(1));

			if(Filter.Warehouse == null)
				expenseQuery.Where(x => x.ExpenseWarehouse != null);
			else
				expenseQuery.Where(x => x.ExpenseWarehouse == Filter.Warehouse);

			expenseQuery.Select(Projections
								.Sum(Projections
									.Property(() => warehouseExpenseOperationAlias.Amount)));
			
			if(Filter.SelectOwner != null)
				switch(Filter.SelectOwner) {
					case (SpecialComboState.All): break; //все
					case (SpecialComboState.Not): expenseQuery.Where(x => x.Owner == null); break; //без собственника 
					default: expenseQuery.Where(x => x.Owner.Id == DomainHelper.GetId(Filter.SelectOwner)); break;
				}

			var incomeSubQuery = QueryOver.Of(() => warehouseIncomeOperationAlias)
				.Where(() => warehouseIncomeOperationAlias.Nomenclature.Id == warehouseOperationAlias.Nomenclature.Id 
				             && (warehouseIncomeOperationAlias.WearSize.Id == warehouseOperationAlias.WearSize.Id
				                 || warehouseIncomeOperationAlias.WearSize == null && warehouseOperationAlias.WearSize == null)
				             && (warehouseIncomeOperationAlias.Height.Id == warehouseOperationAlias.Height.Id
				                 || warehouseIncomeOperationAlias.Height == null && warehouseOperationAlias.Height == null)
				             && (warehouseIncomeOperationAlias.Owner.Id == warehouseOperationAlias.Owner.Id
				                 || warehouseIncomeOperationAlias.Owner == null && warehouseOperationAlias.Owner == null)
				             && warehouseIncomeOperationAlias.WearPercent == warehouseOperationAlias.WearPercent)
				.Where(e => e.OperationTime < Filter.Date.AddDays(1));
			if(Filter.Warehouse == null)
				incomeSubQuery.Where(x => x.ReceiptWarehouse != null);
			else
				incomeSubQuery.Where(x => x.ReceiptWarehouse == Filter.Warehouse);

			incomeSubQuery.Select(Projections
								.Sum(Projections
									.Property(() => warehouseIncomeOperationAlias.Amount)));

			if(Filter.SelectOwner != null)
				switch(Filter.SelectOwner) {
					case (SpecialComboState.All): break;  //все
					case (SpecialComboState.Not): incomeSubQuery.Where(x => x.Owner == null); break; //без собственника
					default: incomeSubQuery.Where(x => x.Owner.Id == DomainHelper.GetId(Filter.SelectOwner)); break; 
				}
			
			var projection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.SubQuery(incomeSubQuery),
				Projections.SubQuery(expenseQuery)
			);

			var queryStock = uow.Session.QueryOver(() => warehouseOperationAlias);

			queryStock.Where(Filter.ShowNegativeBalance
				? Restrictions.Not(Restrictions.Eq(projection, 0))
				: Restrictions.Gt(projection, 0));

			//Если у нас выключена способность показывать общие по всем складам остатки.
			//Но не указан склад мы должны показывать пустую таблицу.
			//Это заведомо ложное условие.
			if(ShowSummary == false && Filter.Warehouse == null)
				queryStock.Where(x => x.Id == -1);

			if (Filter.ProtectionTools != null) {
				queryStock.Where(x 
					=> x.Nomenclature.IsIn(Filter.ProtectionTools.Nomenclatures.ToArray()));
			}
			
			// Рассчет среднего расхода за день
			var expenseYearQuery = QueryOver.Of(() => warehouseExpenseYearOperationAlias)
				.Where(() => warehouseExpenseYearOperationAlias.Nomenclature.Id == warehouseOperationAlias.Nomenclature.Id
				             && (warehouseExpenseYearOperationAlias.WearSize.Id == warehouseOperationAlias.WearSize.Id
				                 || warehouseExpenseYearOperationAlias.WearSize == null && warehouseOperationAlias.WearSize == null)
				             && (warehouseExpenseYearOperationAlias.Height.Id == warehouseOperationAlias.Height.Id
				                 || warehouseExpenseYearOperationAlias.Height == null && warehouseOperationAlias.Height == null)
				             && (warehouseExpenseYearOperationAlias.Owner.Id == warehouseOperationAlias.Owner.Id
				                 || warehouseExpenseYearOperationAlias.Owner == null && warehouseOperationAlias.Owner == null)
				             && warehouseExpenseYearOperationAlias.WearPercent == warehouseOperationAlias.WearPercent)
				.Where(e => e.OperationTime < Filter.Date.AddDays(1) && e.OperationTime >= Filter.Date.AddYears(-1));

			if(Filter.Warehouse == null)
				expenseYearQuery.Where(x => x.ExpenseWarehouse != null);
			else
				expenseYearQuery.Where(x => x.ExpenseWarehouse == Filter.Warehouse);

			expenseYearQuery.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.Double, "SUM(?1)/DATEDIFF(NOW(), MIN(?2))"),
					NHibernateUtil.Double,
					Projections.Property(() => warehouseExpenseYearOperationAlias.Amount),
					Projections.Property(() => warehouseExpenseYearOperationAlias.OperationTime))
				);

			queryStock
				.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Type, () => itemTypesAlias)
				.JoinAlias(() => itemTypesAlias.Units, () => unitsAlias)
				.JoinAlias(() => warehouseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => warehouseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => warehouseOperationAlias.Owner, () => ownerAlias, JoinType.LeftOuterJoin)
				.Where(GetSearchCriterion(
					() => nomenclatureAlias.Id,
					() => nomenclatureAlias.Number,
					() => nomenclatureAlias.Name,
					() => sizeAlias.Name,
					() => heightAlias.Name))
				.SelectList(list => list
					.SelectGroup(() => warehouseOperationAlias.Nomenclature.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomeclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => nomenclatureAlias.Number).WithAlias(() => resultAlias.NomenclatureNumber)
					.Select(() => nomenclatureAlias.Sex).WithAlias(() => resultAlias.Sex)
					.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
					.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.SizeName)
					.Select(() => heightAlias.Name).WithAlias(() => resultAlias.HeightName)
					.Select(() => ownerAlias.Name).WithAlias(() => resultAlias.OwnerName)
					.Select(() => nomenclatureAlias.SaleCost).WithAlias(() => resultAlias.SaleCost)
					.SelectGroup(() => sizeAlias.Id).WithAlias(() => resultAlias.SizeId)
					.SelectGroup(() => heightAlias.Id).WithAlias(() => resultAlias.HeightId)
					.SelectGroup(() => ownerAlias.Id).WithAlias(() => resultAlias.OwnerId)
					.SelectGroup(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
					.Select(projection).WithAlias(() => resultAlias.Amount)
					.SelectSubQuery(expenseYearQuery).WithAlias(() => resultAlias.DailyConsumption)
				);

//711			
				if(Filter.DutyNorm != null) {
				DutyNorm dutyNormAlias = null;
				DutyNormItem dutyNormItemAlias = null;
				ProtectionTools protectionToolsAlias = null;
				Nomenclature nomenclatureAlias2 = null;
				
				var dutyList = uow.Session.QueryOver<DutyNorm>(() => dutyNormAlias)
					.JoinAlias(() => dutyNormAlias.Items, () => dutyNormItemAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => dutyNormItemAlias.ProtectionTools, () => protectionToolsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => protectionToolsAlias.Nomenclatures, () => nomenclatureAlias2, JoinType.LeftOuterJoin)
					.Where(x => x.Id == Filter.DutyNorm.Id)
					.List();
				var nomenclatureIds = dutyList
					.Select(d => d.Items.Select(i => i.ProtectionTools)
						.Select(pt => pt.Nomenclatures
							.Select(n => n.Id)));
			}
//*		
			return queryStock.OrderBy(() => nomenclatureAlias.Name).Asc
	            .ThenBy(Projections.SqlFunction(
	                new SQLFunctionTemplate(
	                    NHibernateUtil.String,
	                    "CAST(SUBSTRING_INDEX(?1, '-', 1) AS DECIMAL(5,1))"),
	                NHibernateUtil.String,
	                Projections.Property(() => sizeAlias.Name))).Asc
	            .ThenBy(Projections.SqlFunction(
	                new SQLFunctionTemplate(
	                    NHibernateUtil.String,
	                    "CAST(SUBSTRING_INDEX(?1, '-', 1) AS DECIMAL(5,1))"),
	                NHibernateUtil.String,
	                Projections.Property(() => heightAlias.Name))).Asc.TransformUsing(Transformers.AliasToBean<StockBalanceJournalNode>());

		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			var updateStatusAction = new JournalAction("Показать движения",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => OpenMovements(selected.Cast<StockBalanceJournalNode>().ToArray())
					);
			NodeActionsList.Add(updateStatusAction);
		}

		void OpenMovements(StockBalanceJournalNode[] nodes)
		{
			foreach(var node in nodes) {
				var journal = NavigationManager
					.OpenViewModel<StockMovmentsJournalViewModel>(this);
				journal.ViewModel.Filter.SetAndRefilterAtOnce(
					filter => filter.Warehouse = Filter.Warehouse, 
					filter => filter.StockPosition = node.GetStockPosition(journal.ViewModel.UoW));
			}
		}
		
		public override string FooterInfo {
			get => $"Суммарная стоимость: " +
			       $"{CurrencyWorks.GetShortCurrencyString(DataLoader.Items.Cast<StockBalanceJournalNode>().Sum(x => x.SumSaleCost))} " +
			       $"    Загружено:	" +
			       $"{DataLoader.Items.Count} шт.";
			set { }
		}
	}

	public class StockBalanceJournalNode
	{
		public int Id { get; set; }
		public int NomeclatureId { get; set; }
		public string NomenclatureName { get; set; }
		public string NomenclatureNumber { get; set; }
		public ClothesSex Sex { get; set; }
		public string SexText => Sex.GetEnumShortTitle();
		public string UnitsName { get; set; }
		public string SizeName { get; set; }
		public int SizeId { get; set; }
		public string HeightName { get; set; }
		public int HeightId { get; set; }
		public decimal WearPercent { get; set; }
		public int Amount { get; set; }
		public int OwnerId { get; set; }
		public string OwnerName { get; set; }
		public decimal SaleCost { get; set; }
		public double? DailyConsumption { get; set; }

		public string MonthConsumption => $"{DailyConsumption * 30:N1}";

		public int? SupplyDays => (int?)(Amount / DailyConsumption);

		public string Supply => DailyConsumption.HasValue && SupplyDays >= 0 ? NumberToTextRus.FormatCase(SupplyDays.Value, "{0} день", "{0} дня", "{0} дней") : null;

		public string SupplyColor {
			get {
				if(SupplyDays <= 7) return "red";
				if(SupplyDays <= 14) return "orange";
				if(SupplyDays <= 365/2) return "green";
				return "violet";
			}
		}
		public string SaleCostText => SaleCost > 0 ? CurrencyWorks.GetShortCurrencyString (SaleCost) : String.Empty;
		public decimal SumSaleCost => SaleCost > 0 ? SaleCost * Amount : 0;
		public string SumSaleCostText => CurrencyWorks.GetShortCurrencyString (SumSaleCost);
		public string BalanceText => Amount > 0 ? 
			$"{Amount} {UnitsName}" : $"<span foreground=\"red\">{Amount}</span> {UnitsName}";
		public string WearPercentText => WearPercent.ToString("P0");
		public StockPosition GetStockPosition(IUnitOfWork uow) => new StockPosition(
			uow.GetById<Nomenclature>(Id), 
			WearPercent, 
			uow.GetById<Size>(SizeId), 
			uow.GetById<Size>(HeightId),
			uow.GetById<Owner>(OwnerId));
	}
}
