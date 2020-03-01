﻿using System;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock
{
	public class StockBalanceJournalViewModel : JournalViewModelBase
	{
		public bool ShowSummary;

		public StockBalanceFilterViewModel Filter { get; private set; }

		public StockBalanceJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation, ILifetimeScope autofacScope) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			AutofacScope = autofacScope;
			JournalFilter = Filter = AutofacScope.Resolve<StockBalanceFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			var dataLoader = new ThreadDataLoader<StockBalanceJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			CreateNodeActions();

			UpdateOnChanges(typeof(WarehouseOperation), typeof(Nomenclature));

			Filter.PropertyChanged += (sender, e) => TabName = "Остатки по складу " + Filter.Warehouse?.Name;
		}

		protected IQueryOver<WarehouseOperation> ItemsQuery(IUnitOfWork uow)
		{
			StockBalanceJournalNode resultAlias = null;

			WarehouseOperation warehouseExpenseOperationAlias = null;
			WarehouseOperation warehouseIncomeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;

			Nomenclature nomenclatureAlias = null;
			ItemsType itemtypesAlias = null;
			MeasurementUnits unitsAlias = null;


			var expensequery = QueryOver.Of<WarehouseOperation>(() => warehouseExpenseOperationAlias)
				.Where(() => warehouseExpenseOperationAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(e => e.OperationTime < DateTime.Now);
			if(ShowSummary)
				expensequery.Where(x => x.ExpenseWarehouse != null);
			else
				expensequery.Where(x => x.ExpenseWarehouse == Filter.Warehouse);

			expensequery.Select(Projections.Sum(Projections.Property(() => warehouseExpenseOperationAlias.Amount)));

			var incomeSubQuery = QueryOver.Of<WarehouseOperation>(() => warehouseIncomeOperationAlias)
				.Where(() => warehouseIncomeOperationAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(e => e.OperationTime < DateTime.Now);
			if(ShowSummary)
				incomeSubQuery.Where(x => x.ReceiptWarehouse != null);
			else
				incomeSubQuery.Where(x => x.ReceiptWarehouse == Filter.Warehouse);

			incomeSubQuery.Select(Projections.Sum(Projections.Property(() => warehouseIncomeOperationAlias.Amount)));

			IProjection projection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.SubQuery(incomeSubQuery),
				Projections.SubQuery(expensequery)
			);

			var queryStock = uow.Session.QueryOver<WarehouseOperation>(() => warehouseOperationAlias);

			if(Filter.ShowNegativeBalance) {
				queryStock.Where(Restrictions.Not(Restrictions.Eq(projection, 0)));
			}
			else {
				queryStock.Where(Restrictions.Gt(projection, 0));
			}

			if(Filter.ItemTypeCategory != null)
				queryStock.Where(() => itemtypesAlias.Category == Filter.ItemTypeCategory);

			return queryStock
				.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Type, () => itemtypesAlias)
				.JoinAlias(() => itemtypesAlias.Units, () => unitsAlias)
				.Where(GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => warehouseOperationAlias.Size,
					() => warehouseOperationAlias.Growth))

				.SelectList(list => list
			   .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
			   .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
			   .Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
			   .SelectGroup(() => warehouseOperationAlias.Size).WithAlias(() => resultAlias.Size)
			   .SelectGroup(() => warehouseOperationAlias.Growth).WithAlias(() => resultAlias.Growth)
			   .SelectGroup(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
			   .Select(projection).WithAlias(() => resultAlias.Amount)
				)
				.OrderBy(() => nomenclatureAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<StockBalanceJournalNode>());
		}
	}

	public class StockBalanceJournalNode
	{
		public int Id { get; set; }

		public string NomenclatureName { get; set; }
		public string UnitsName { get; set; }
		public string Size { get; set; }
		public string Growth { get; set; }
		public decimal WearPercent { get; set; }
		public int Amount { get; set; }

		public string BalanceText => Amount > 0 ? String.Format("{0} {1}", Amount, UnitsName) : String.Format("<span foreground=\"red\">{0}</span> {1}", Amount, UnitsName);

		public string WearPercentText {
			get {
				return WearPercent.ToString("P");
			}
		}

		public StockPosition GetStockPosition(IUnitOfWork uow)
		{
			var nomenclature = uow.GetById<Nomenclature>(Id);
			return new StockPosition(nomenclature, Size, Growth, WearPercent);
		}
	}
}
