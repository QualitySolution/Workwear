using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using workwear.Domain.Operations;
using workwear.Domain.Company;
using workwear.Domain.Sizes;
using workwear.Domain.Stock;

namespace workwear.Representations.Organization
{
	public class EmployeeBalanceVM : RepresentationModelWithoutEntityBase<EmployeeBalanceVMNode>
	{
		private EmployeeCard employee;

		public EmployeeCard Employee {
			get => Filter != null ? Filter.RestrictEmployee : employee;
			set => employee = value;
		}

		public EmployeeBalanceFilter Filter {
			get => RepresentationFilter as EmployeeBalanceFilter;
			set => RepresentationFilter = value;
		}
		#region IRepresentationModel implementation
		public override void UpdateNodes () {
			if(Employee == null) {
				SetItemsSource (new List<EmployeeBalanceVMNode> ());
				return;
			}

			EmployeeBalanceVMNode resultAlias = null;
			EmployeeIssueOperation expenseOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType itemTypesAlias = null;
			MeasurementUnits unitsAlias = null;
			EmployeeIssueOperation removeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;

			var query = UoW.Session.QueryOver(() => expenseOperationAlias)
				.Where(e => e.Employee == Employee);

			var subQueryRemove = QueryOver.Of(() => removeOperationAlias)
				.Where(() => removeOperationAlias.IssuedOperation.Id == expenseOperationAlias.Id)
				.Select (Projections.Sum<EmployeeIssueOperation> (o => o.Returned));

			var expenseList = query
				.JoinAlias (() => expenseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(()=> expenseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
				.JoinAlias (() => nomenclatureAlias.Type, () => itemTypesAlias)
				.JoinAlias (() => itemTypesAlias.Units, () => unitsAlias)
				.JoinAlias (() => expenseOperationAlias.WarehouseOperation, () => warehouseOperationAlias, JoinType.LeftOuterJoin)
				.Where (e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > DateTime.Today)
				.SelectList (list => list
					.SelectGroup (() => expenseOperationAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.Select (() => unitsAlias.Name).WithAlias (() => resultAlias.UnitsName)
					.Select (() => sizeAlias.Name).WithAlias (() => resultAlias.WearSize)
					.Select (() => heightAlias.Name).WithAlias (() => resultAlias.Height)
					.Select (() => warehouseOperationAlias.Cost).WithAlias (() => resultAlias.AvgCost)
					.Select (() => expenseOperationAlias.WearPercent).WithAlias (() => resultAlias.WearPercent)
					.Select (() => expenseOperationAlias.Issued).WithAlias (() => resultAlias.Added)
					.Select (() => expenseOperationAlias.OperationTime).WithAlias (() => resultAlias.IssuedDate)
					.Select(() => expenseOperationAlias.StartOfUse).WithAlias(() => resultAlias.StartUseDate)
					.Select (() => expenseOperationAlias.ExpiryByNorm).WithAlias (() => resultAlias.ExpiryDate)
					.SelectSubQuery (subQueryRemove).WithAlias (() => resultAlias.Removed)
				)
				.TransformUsing (Transformers.AliasToBean<EmployeeBalanceVMNode> ())
				.List<EmployeeBalanceVMNode> ().Where(r => r.Added - r.Removed != 0);		

			SetItemsSource (expenseList.ToList ());
		}
		private IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<EmployeeBalanceVMNode> ()
			.AddColumn ("Наименование").AddTextRenderer (e => e.NomenclatureName)
			.AddColumn ("Размер").AddTextRenderer (e => e.WearSize)
			.AddColumn ("Рост").AddTextRenderer (e => e.Height)
			.AddColumn ("Количество").AddTextRenderer (e => e.BalanceText)
			.AddColumn ("Cтоимость").AddTextRenderer (e => e.AvgCostText)
			.AddColumn ("Износ на сегодня").AddProgressRenderer (e => ((int)(e.Percentage * 100)).Clamp(0, 100))
			.AddSetter ((w, e) => w.Text = e.ExpiryDate.HasValue ? $"до {e.ExpiryDate.Value:d}" : string.Empty)
			.Finish ();
		public override IColumnsConfig ColumnsConfig => treeViewConfig;
		#endregion
		#region implemented abstract members of RepresentationModelEntityBase
		protected override bool NeedUpdateFunc (object updatedSubject) {
			return updatedSubject is EmployeeIssueOperation op && op.Employee.IsSame(Employee);
		}

		#endregion
		public EmployeeBalanceVM (EmployeeBalanceFilter filter) : this(filter.UoW) => Filter = filter;
		public EmployeeBalanceVM (IUnitOfWork uow) : base (typeof(EmployeeIssueOperation)) => 
			UoW = uow;
	}

	public class EmployeeBalanceVMNode {
		public int Id { get; set; }
		[UseForSearch]
		public string NomenclatureName { get; set;}
		public string UnitsName { get; set;}
		public string WearSize { get; set; }
		public string Height { get; set; }
		public decimal AvgCost { get; set;}
		public decimal WearPercent { get; set;}
		public DateTime IssuedDate { get; set;}
		public DateTime? StartUseDate { get; set; }
		public DateTime? ExpiryDate { get; set;}
		public decimal Percentage => 
			EmployeeIssueOperation.CalculatePercentWear(DateTime.Today, StartUseDate, ExpiryDate, WearPercent);
		public int Added { get; set;}
		public int Removed { get; set;}
		public string BalanceText => $"{Added - Removed} {UnitsName}";
		public string AvgCostText => AvgCost > 0 ? CurrencyWorks.GetShortCurrencyString (AvgCost) : String.Empty;
	}
}

