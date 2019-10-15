using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using workwear.Domain.Operations;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear.Representations.Organization
{
	public class EmployeeBalanceVM : RepresentationModelWithoutEntityBase<EmployeeBalanceVMNode>
	{
		EmployeeCard employee;

		public EmployeeCard Employee {
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

			EmployeeIssueOperation expenseOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType itemtypesAlias = null;
			MeasurementUnits unitsAlias = null;
			EmployeeIssueOperation removeOperationAlias = null;
			IncomeItem incomeItemOnIncomeAlias = null;

			var query = UoW.Session.QueryOver<EmployeeIssueOperation>(() => expenseOperationAlias)
				.Where(e => e.Employee == Employee);

			var subqueryRemove = QueryOver.Of<EmployeeIssueOperation>(() => removeOperationAlias)
				.Where(() => removeOperationAlias.IssuedOperation.Id == expenseOperationAlias.Id)
				.Select (Projections.Sum<EmployeeIssueOperation> (o => o.Returned));

			var expenseList = query
				.JoinAlias (() => expenseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias (() => nomenclatureAlias.Type, () => itemtypesAlias)
				.JoinAlias (() => itemtypesAlias.Units, () => unitsAlias)
				.JoinAlias (() => expenseOperationAlias.IncomeOnStock, () => incomeItemOnIncomeAlias)
				.Where (e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > DateTime.Today)
				.SelectList (list => list
					.SelectGroup (() => expenseOperationAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.Select (() => unitsAlias.Name).WithAlias (() => resultAlias.UnitsName)
					.Select (() => nomenclatureAlias.Size).WithAlias (() => resultAlias.Size)
					.Select (() => nomenclatureAlias.WearGrowth).WithAlias (() => resultAlias.Growth)
					.Select (() => incomeItemOnIncomeAlias.Cost).WithAlias (() => resultAlias.AvgCost)
					.Select (() => expenseOperationAlias.WearPercent).WithAlias (() => resultAlias.WearPercent)
					.Select (() => expenseOperationAlias.Issued).WithAlias (() => resultAlias.Added)
					.Select (() => expenseOperationAlias.OperationTime).WithAlias (() => resultAlias.IssuedDate)
					.Select(() => expenseOperationAlias.StartOfUse).WithAlias(() => resultAlias.StartUseDate)
					.Select (() => expenseOperationAlias.ExpiryByNorm).WithAlias (() => resultAlias.ExpiryDate)
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
			.AddColumn ("Износ на сегодня").AddProgressRenderer (e => ((int)(e.Percentage * 100)).Clamp(0, 100))
			.AddSetter ((w, e) => w.Text = e.ExpiryDate.HasValue ? String.Format("до {0:d}", e.ExpiryDate.Value) : string.Empty)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelEntityBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			var op = updatedSubject as EmployeeIssueOperation;
			return op != null && op.Employee.IsSame(Employee);
		}

		#endregion

		public EmployeeBalanceVM (EmployeeBalanceFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public EmployeeBalanceVM (IUnitOfWork uow) : base (typeof(EmployeeIssueOperation))
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
		public decimal WearPercent { get; set;}

		public DateTime IssuedDate { get; set;}
		public DateTime? StartUseDate { get; set; }
		public DateTime? ExpiryDate { get; set;}

		public decimal Percentage => EmployeeIssueOperation.CalculatePercentWear(DateTime.Today, StartUseDate, ExpiryDate, WearPercent);

		public int Added { get; set;}
		public int Removed { get; set;}

		public string BalanceText => String.Format ("{0} {1}", Added - Removed, UnitsName);

		public string AvgCostText => AvgCost > 0 ? CurrencyWorks.GetShortCurrencyString (AvgCost) : String.Empty;
	}
}

