using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear.ViewModel
{
	public class StockBalanceVM : RepresentationModelWithoutEntityBase<StockBalanceVMNode>
	{
		StockBalanceVMMode mode;

		StockBalanceVMGroupBy groupBy;

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			StockBalanceVMNode resultAlias = null;

			IncomeItem incomeItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType itemtypesAlias = null;
			MeasurementUnits unitsAlias = null;
			ExpenseItem expenseItemAlias = null;
			WriteoffItem writeoffItemAlias = null;

			var incomes = UoW.Session.QueryOver<IncomeItem> (() => incomeItemAlias)
				.JoinAlias (() => incomeItemAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias (() => nomenclatureAlias.Type, () => itemtypesAlias);
			
			if(mode == StockBalanceVMMode.OnlyProperties)
			{
				incomes.Where( () => itemtypesAlias.Category == ItemTypeCategory.property);
			}

			var subqueryRemove = QueryOver.Of<ExpenseItem>(() => expenseItemAlias)
				.Where(() => expenseItemAlias.IncomeOn.Id == incomeItemAlias.Id)
				.Select (Projections.Sum<ExpenseItem> (o => o.Amount));

			var subqueryWriteOff = QueryOver.Of<WriteoffItem>(() => writeoffItemAlias)
				.Where(() => writeoffItemAlias.IncomeOn.Id == incomeItemAlias.Id)
				.Select (Projections.Sum<WriteoffItem> (o => o.Amount));

			var incomeList = incomes
				.JoinAlias (() => itemtypesAlias.Units, () => unitsAlias)
				.Where (Restrictions.Gt (
					Projections.SqlFunction(
						new VarArgsSQLFunction("(", "-", ")"),
						NHibernateUtil.Int32,
						Projections.Property (() => incomeItemAlias.Amount),
						Projections.SqlFunction("COALESCE", 
							NHibernateUtil.Int32,
							Projections.SubQuery (subqueryRemove),
							Projections.Constant (0)
						),
						Projections.SqlFunction("COALESCE", 
							NHibernateUtil.Int32,
							Projections.SubQuery (subqueryWriteOff),
							Projections.Constant (0)
						)
					), 0)
				)
				.SelectList (list => list
					.SelectGroup (() => incomeItemAlias.Id).WithAlias (() => resultAlias.Id)
				    .Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.Select (() => unitsAlias.Name).WithAlias (() => resultAlias.UnitsName)
					.Select (() => nomenclatureAlias.Size).WithAlias (() => resultAlias.Size)
					.Select (() => nomenclatureAlias.WearGrowth).WithAlias (() => resultAlias.Growth)
					.Select (() => incomeItemAlias.Cost).WithAlias (() => resultAlias.AvgCost)
					.Select (() => incomeItemAlias.LifePercent).WithAlias (() => resultAlias.AvgLife)
					.Select (() => incomeItemAlias.Amount).WithAlias (() => resultAlias.Income)
					.SelectSubQuery (subqueryRemove).WithAlias (() => resultAlias.Expense)
					.SelectSubQuery (subqueryWriteOff).WithAlias (() => resultAlias.Writeoff)
				)
				.TransformUsing (Transformers.AliasToBean<StockBalanceVMNode> ())
				.List<StockBalanceVMNode> ();

			if(groupBy == StockBalanceVMGroupBy.NomenclatureId)
			{
				var groupedList = new List<StockBalanceVMNode>();
				foreach(var group in incomeList.GroupBy(x => x.NomenclatureId))
				{
					var item = group.First();
					if (group.Count() > 1)
					{
						var totalCost = group.Sum(x => x.AvgCost * x.Balance);
						var totalLife = group.Sum(x => x.AvgLife * x.Balance);
						item.Income = group.Sum(x => x.Income);
						item.Expense = group.Sum(x => x.Expense);
						item.Writeoff = group.Sum(x => x.Writeoff);
						var totalAmount = item.Balance;
						item.AvgCost = totalCost / totalAmount;
						item.AvgLife = totalLife / totalAmount;
					}
					groupedList.Add(item);
				}

				incomeList = groupedList;
			}

			SetItemsSource (incomeList.OrderBy(x => x.NomenclatureName)
				                .ThenBy(x => x.Size)
				                .ThenBy(x => x.Growth)
				                .ToList ());
		}

		IColumnsConfig treeViewConfig;

		public override IColumnsConfig ColumnsConfig {
			get { if(treeViewConfig == null)
				{
					var tempConfig = ColumnsConfigFactory.Create<StockBalanceVMNode>()
						.AddColumn("Наименование").AddTextRenderer(e => e.NomenclatureName);
					if(mode != StockBalanceVMMode.OnlyProperties)
					{
						tempConfig
							.AddColumn("Размер").AddTextRenderer(e => e.Size)
							.AddColumn("Рост").AddTextRenderer(e => e.Growth);
					}
					tempConfig
						.AddColumn("Количество").AddTextRenderer(e => e.BalanceText)
						.AddColumn("Средняя стоимость").AddTextRenderer(e => e.AvgCostText)
						.AddColumn("Среднее состояние").AddTextRenderer(e => e.AvgLifeText);
					treeViewConfig = tempConfig.Finish();
				}

				return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelEntityBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return true;
		}

		#endregion

		public StockBalanceVM (StockBalanceVMMode mode, StockBalanceVMGroupBy groupBy) : this(UnitOfWorkFactory.CreateWithoutRoot (), mode, groupBy)
		{
		}

		public StockBalanceVM(IUnitOfWork uow, StockBalanceVMMode mode, StockBalanceVMGroupBy groupBy) : this(uow)
		{
			this.mode = mode;
			this.groupBy = groupBy;
		}

		private StockBalanceVM (IUnitOfWork uow) : base (typeof(Expense), typeof(Income), typeof(Writeoff))
		{
			this.UoW = uow;
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
		public decimal AvgCost { get; set;}
		public decimal AvgLife { get; set;}

		public int Income { get; set;}
		public int Expense { get; set;}
		public int Writeoff { get; set;}

		public int Balance { get { return Income - Expense - Writeoff; } }

		public string BalanceText {get{ return String.Format ("{0} {1}", Balance, UnitsName);
			}}

		public string AvgCostText {get { 
				return AvgCost > 0 ? CurrencyWorks.GetShortCurrencyString (AvgCost) : String.Empty;
			}}

		public string AvgLifeText {get { 
				return AvgLife.ToString ("P");
			}}
	}

	public enum StockBalanceVMMode
	{
		DisplayAll,
		OnlyProperties
	}

	public enum StockBalanceVMGroupBy
	{
		IncomeItem,
		NomenclatureId
	}
}

