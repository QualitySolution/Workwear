using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear.ViewModel
{
	public class EmployeeBalanceVM : RepresentationModelWithoutEntityBase<EmployeeBalanceVMNode>
	{
		EmployeeCard employee;

		EmployeeCard Employee {
			get {
				if (Filter != null)
					return Filter.RestrictEmployee;
				else
					return employee;
			}
			set {
				employee = value;
			}
		}

		public EmployeeBalanceFilter Filter {
			get {
				return RepresentationFilter as EmployeeBalanceFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			if(Employee == null)
			{
				SetItemsSource (new List<EmployeeBalanceVMNode> ());
				return;
			}

			EmployeeBalanceVMNode resultAlias = null;

			Expense expenseAlias = null;
			ExpenseItem expenseItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType itemtypesAlias = null;
			MeasurementUnits unitsAlias = null;
			IncomeItem incomeItemOnRemoveAlias = null;
			IncomeItem incomeItemOnIncomeAlias = null;

			var expense = UoW.Session.QueryOver<Expense> (() => expenseAlias)
				.Where (e => e.EmployeeCard == Employee)
				.JoinQueryOver (e => e.Items, () => expenseItemAlias);

			var subqueryRemove = QueryOver.Of<IncomeItem>(() => incomeItemOnRemoveAlias)
				.Where(() => incomeItemOnRemoveAlias.IssuedOn.Id == expenseItemAlias.Id)
				.Select (Projections.Sum<IncomeItem> (o => o.Amount));

			var expenseList = expense
				.JoinAlias (() => expenseItemAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias (() => nomenclatureAlias.Type, () => itemtypesAlias)
				.JoinAlias (() => itemtypesAlias.Units, () => unitsAlias)
				.JoinAlias (() => expenseItemAlias.IncomeOn, () => incomeItemOnIncomeAlias)
				.Where (e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > DateTime.Today)
				.SelectList (list => list
					.SelectGroup (() => expenseItemAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.Select (() => unitsAlias.Name).WithAlias (() => resultAlias.UnitsName)
					.Select (() => nomenclatureAlias.Size).WithAlias (() => resultAlias.Size)
					.Select (() => nomenclatureAlias.WearGrowth).WithAlias (() => resultAlias.Growth)
					.Select (() => incomeItemOnIncomeAlias.Cost).WithAlias (() => resultAlias.AvgCost)
					.Select (() => incomeItemOnIncomeAlias.LifePercent).WithAlias (() => resultAlias.AvgLife)
					.Select (() => expenseItemAlias.Amount).WithAlias (() => resultAlias.Added)
					.Select (() => expenseAlias.Date).WithAlias (() => resultAlias.IssuedDate)
					.Select (() => expenseItemAlias.AutoWriteoffDate).WithAlias (() => resultAlias.ExpiryDate)
					.SelectSubQuery (subqueryRemove).WithAlias (() => resultAlias.Removed)
				)
				.TransformUsing (Transformers.AliasToBean<EmployeeBalanceVMNode> ())
				.List<EmployeeBalanceVMNode> ().Where(r => r.Added - r.Removed != 0);		

			SetItemsSource (expenseList.ToList ());
		}

		IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<EmployeeBalanceVMNode> ()
			.AddColumn ("Наименование").AddTextRenderer (e => e.NomenclatureName)
			.AddColumn ("Размер").AddTextRenderer (e => e.Size)
			.AddColumn ("Рост").AddTextRenderer (e => e.Growth)
			.AddColumn ("Количество").AddTextRenderer (e => e.BalanceText)
			.AddColumn ("Cтоимость").AddTextRenderer (e => e.AvgCostText)
			.AddColumn ("Срок службы").AddProgressRenderer (e => (int)(100 - (e.Percentage * 100)))
			.AddSetter ((w, e) => w.Text = e.ExpiryDate.HasValue ? String.Format("до {0:d}", e.ExpiryDate.Value) : string.Empty)
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

		public EmployeeBalanceVM (EmployeeBalanceFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public EmployeeBalanceVM (EmployeeCard employee) : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			Employee = employee;
		}

		public EmployeeBalanceVM (IUnitOfWork uow) : base (typeof(Expense), typeof(Income), typeof(Writeoff))
		{
			this.UoW = uow;
		}
	}

	public class EmployeeBalanceVMNode
	{
		public int Id { get; set; }

		[UseForSearch]
		public string NomenclatureName { get; set;}
		public string UnitsName { get; set;}
		public string Size { get; set;}
		public string Growth { get; set;}
		public decimal AvgCost { get; set;}
		public decimal AvgLife { get; set;}

		public DateTime IssuedDate { get; set;}
		public DateTime? ExpiryDate { get; set;}

		public double Percentage {
			get{
				if (ExpiryDate == null)
					return 0;
				return (ExpiryDate.Value - DateTime.Today).TotalDays / (ExpiryDate.Value - IssuedDate).TotalDays;
			}
		}

		public int Added { get; set;}
		public int Removed { get; set;}

		public string BalanceText {get{ return String.Format ("{0} {1}", Added - Removed, UnitsName);
			}}

		public string AvgCostText {get { 
				return AvgCost > 0 ? CurrencyWorks.GetShortCurrencyString (AvgCost) : String.Empty;
			}}

		public string AvgLifeText {get { 
				return AvgLife.ToString ("P");
			}}
	}
}

