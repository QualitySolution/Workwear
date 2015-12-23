using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using workwear.Domain;
using workwear.Domain.Stock;

namespace workwear.ViewModel
{
	public class StockBalanceVM : RepresentationModelWithoutEntityBase<StockBalanceVMNode>
	{
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

			var incomes = UoW.Session.QueryOver<IncomeItem> (() => incomeItemAlias);

			var subqueryRemove = QueryOver.Of<ExpenseItem>(() => expenseItemAlias)
				.Where(() => expenseItemAlias.IncomeOn.Id == incomeItemAlias.Id)
				.Select (Projections.Sum<ExpenseItem> (o => o.Amount));

			var subqueryWriteOff = QueryOver.Of<WriteoffItem>(() => writeoffItemAlias)
				.Where(() => writeoffItemAlias.IncomeOn.Id == incomeItemAlias.Id)
				.Select (Projections.Sum<WriteoffItem> (o => o.Amount));

			var incomeList = incomes
				.JoinAlias (() => incomeItemAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias (() => nomenclatureAlias.Type, () => itemtypesAlias)
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

			SetItemsSource (incomeList.ToList ());
		}

		IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<StockBalanceVMNode> ()
			.AddColumn ("Наименование").AddTextRenderer (e => e.NomenclatureName)
			.AddColumn ("Размер").AddTextRenderer (e => e.Size)
			.AddColumn ("Рост").AddTextRenderer (e => e.Growth)
			.AddColumn ("Количество").AddTextRenderer (e => e.BalanceText)
			.AddColumn ("Cтоимость").AddTextRenderer (e => e.AvgCostText)
			.AddColumn ("Cостояние").AddTextRenderer (e => e.AvgLifeText)
			.Finish ();

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

		public StockBalanceVM () : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			
		}

		public StockBalanceVM (IUnitOfWork uow) : base (typeof(Expense), typeof(Income)) //FIXME Добавить списание.
		{
			this.UoW = uow;
		}
	}

	public class StockBalanceVMNode
	{
		public int Id { get; set; }

		[UseForSearch]
		public string NomenclatureName { get; set;}
		public string UnitsName { get; set;}
		public string Size { get; set;}
		public string Growth { get; set;}
		public decimal AvgCost { get; set;}
		public decimal AvgLife { get; set;}

		public int Income { get; set;}
		public int Expense { get; set;}
		public int Writeoff { get; set;}

		public string BalanceText {get{ return String.Format ("{0} {1}", Income - Expense - Writeoff, UnitsName);
			}}

		public string AvgCostText {get { 
				return AvgCost > 0 ? CurrencyWorks.GetShortCurrencyString (AvgCost) : String.Empty;
			}}

		public string AvgLifeText {get { 
				return AvgLife.ToString ("P");
			}}
	}
}

