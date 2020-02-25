using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.JournalFilters;

namespace workwear.ViewModel
{
	public class StockBalanceVM : RepresentationModelWithoutEntityBase<StockBalanceVMNode>
	{
		StockBalanceVMMode mode;

		Warehouse warehouse;

		public Warehouse Warehouse {
			get {
				if(Filter != null)
					return Filter.RestrictWarehouse;
				else
					return warehouse;
			}
			set {
				warehouse = value;
			}
		}


		public WarehouseFilter Filter {
			get {
				return RepresentationFilter as WarehouseFilter;
			}
			set {
				RepresentationFilter = (IRepresentationFilter)value;

			}
		}


		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			if(Warehouse == null) 
			{
				SetItemsSource(new List<StockBalanceVMNode>());
				return;
			}

			StockBalanceVMNode resultAlias = null;

			WarehouseOperation warehouseExpenseOperationAlias = null;
			WarehouseOperation warehouseIncomeOperationAlias = null;

			Nomenclature nomenclatureAlias = null;
			ItemsType itemtypesAlias = null;
			MeasurementUnits unitsAlias = null;


			var expensequery = QueryOver.Of<WarehouseOperation>(() => warehouseExpenseOperationAlias)
				.Where(() => warehouseExpenseOperationAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(x => x.ExpenseWarehouse == Filter.RestrictWarehouse)
				.Where(e => e.OperationTime < DateTime.Now)
				.Select(Projections.Sum(Projections.Property(() => warehouseExpenseOperationAlias.Amount)));


			var incomeSubQuery = QueryOver.Of<WarehouseOperation>(() => warehouseIncomeOperationAlias)
				.Where(() => warehouseIncomeOperationAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(x => x.ReceiptWarehouse == Filter.RestrictWarehouse)
				.Where(e => e.OperationTime < DateTime.Now)
				.Select(Projections.Sum(Projections.Property(() => warehouseIncomeOperationAlias.Amount)));
				
			IProjection projection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.SubQuery(incomeSubQuery),
				Projections.SubQuery(expensequery)
							
			);

			var queryStock = UoW.Session.QueryOver<WarehouseOperation>(() => warehouseExpenseOperationAlias);

			if(Filter.RestrictOnlyNull) {
				queryStock.Where(Restrictions.Not(Restrictions.Eq(projection, 0)));
			}
			else {
				queryStock.Where(Restrictions.Gt(projection, 0));

			}

			var incomeList = queryStock
				.JoinAlias(() => warehouseExpenseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Type, () => itemtypesAlias)
				.JoinAlias(() => itemtypesAlias.Units, () => unitsAlias)

			   
				.SelectList(list => list
			   .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
			   .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
			   .Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
			   .SelectGroup(() => warehouseExpenseOperationAlias.Size).WithAlias(() => resultAlias.Size)
			   .SelectGroup(() => warehouseExpenseOperationAlias.Growth).WithAlias(() => resultAlias.Growth)
			   .SelectGroup(() => warehouseExpenseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
			   .Select(projection).WithAlias(() => resultAlias.Amount)
				)
				.TransformUsing (Transformers.AliasToBean<StockBalanceVMNode> ())
				.List<StockBalanceVMNode>();

			SetItemsSource(incomeList.OrderBy(x => x.NomenclatureName)
									 .ToList());
		}


		IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<StockBalanceVMNode>()
			.AddColumn("Наименование").AddTextRenderer(e => e.NomenclatureName)
			.AddColumn("Размер").AddTextRenderer(e => e.Size)
			.AddColumn("Рост").AddTextRenderer(e => e.Growth)
			.AddColumn("Количество").AddTextRenderer(e => e.BalanceText)
			.AddColumn("Процент износа").AddTextRenderer(e => e.AvgLifeText)
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelEntityBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return true;
		}

		#endregion

		public StockBalanceVM (StockBalanceVMMode mode) : this(UnitOfWorkFactory.CreateWithoutRoot (), mode)
		{
		}

		public StockBalanceVM(IUnitOfWork uow, StockBalanceVMMode mode) : this(uow)
		{
			this.mode = mode;
		}

		private StockBalanceVM (IUnitOfWork uow) : base (typeof(Expense), typeof(Income), typeof(Writeoff))
		{
			this.UoW = uow;
		}

		public StockBalanceVM(WarehouseFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}
	}

	public class StockBalanceVMNode
	{
		public int Id { get; set; }

		public int NomenclatureId { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string NomenclatureName { get; set;}
		public string UnitsName { get; set;}
		[UseForSearch]
		[SearchHighlight]
		public string Size { get; set;}
		[UseForSearch]
		[SearchHighlight]
		public string Growth { get; set;}
		public decimal WearPercent { get; set; }
		public int Amount { get; set; }

		public string BalanceText {get{ return String.Format ("{0} {1}", Amount, UnitsName);
			}}

		public string AvgLifeText {get { 
				return WearPercent.ToString ("P");
			}}
	}

	public enum StockBalanceVMMode
	{
		DisplayAll,
		OnlyProperties
	}
}

