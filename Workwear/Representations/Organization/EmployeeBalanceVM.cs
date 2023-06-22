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
using Workwear.Domain.Operations;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace workwear.Representations.Organization
{
	public class EmployeeBalanceVM : RepresentationModelWithoutEntityBase<EmployeeBalanceVMNode>
	{
		private EmployeeCard employee;

		public EmployeeCard Employee {
			get => employee;
			set => employee = value;
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
			ItemsType nomenclatureItemTypesAlias = null;
			MeasurementUnits nomenclatureUnitsAlias = null;
			ProtectionTools protectionToolsAlias = null;
			ItemsType protectionToolsItemTypesAlias = null;
			MeasurementUnits protectionToolsUnitsAlias  = null;
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
				.JoinAlias (() => expenseOperationAlias.Nomenclature, () => nomenclatureAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseOperationAlias.ProtectionTools, () => protectionToolsAlias, JoinType.LeftOuterJoin)
				.JoinAlias(()=> expenseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => nomenclatureAlias.Type, () => nomenclatureItemTypesAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => nomenclatureItemTypesAlias.Units, () => nomenclatureUnitsAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => protectionToolsAlias.Type, () => protectionToolsItemTypesAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => protectionToolsItemTypesAlias.Units, () => protectionToolsUnitsAlias, JoinType.LeftOuterJoin)
				.JoinAlias (() => expenseOperationAlias.WarehouseOperation, () => warehouseOperationAlias, JoinType.LeftOuterJoin)
				.Where (e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > DateTime.Today)
				.SelectList (list => list
					.SelectGroup (() => expenseOperationAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.Select (() => nomenclatureUnitsAlias.Name).WithAlias (() => resultAlias.NomenclatureUnitsName)
					.Select(() => protectionToolsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsName)
					.Select(() => protectionToolsUnitsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsUnitsName)
					.Select (() => sizeAlias.Name).WithAlias (() => resultAlias.WearSize)
					.Select (() => heightAlias.Name).WithAlias (() => resultAlias.Height)
					.Select (() => warehouseOperationAlias.Cost).WithAlias (() => resultAlias.AvgCost)
					.Select (() => expenseOperationAlias.WearPercent).WithAlias (() => resultAlias.WearPercent)
					.Select (() => expenseOperationAlias.Issued).WithAlias (() => resultAlias.Added)
					.Select (() => expenseOperationAlias.OperationTime).WithAlias (() => resultAlias.IssuedDate)
					.Select(() => expenseOperationAlias.StartOfUse).WithAlias(() => resultAlias.StartUseDate)
					.Select(() => expenseOperationAlias.ExpiryByNorm).WithAlias(() => resultAlias.ExpiryDate)
					.SelectSubQuery (subQueryRemove).WithAlias (() => resultAlias.Removed)
				)
				.TransformUsing (Transformers.AliasToBean<EmployeeBalanceVMNode> ())
				.List<EmployeeBalanceVMNode> ().Where(r => r.Added - r.Removed != 0);		

			SetItemsSource (expenseList.ToList ());
		}
		private IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<EmployeeBalanceVMNode> ()
			.AddColumn ("Наименование").AddTextRenderer (e => e.ItemName).WrapWidth(700)
			.AddSetter((w, item) => w.Foreground = item.NomenclatureName != null ? "black" : "blue")
			.AddColumn ("Размер").AddTextRenderer (e => e.WearSize)
			.AddColumn ("Рост").AddTextRenderer (e => e.Height)
			.AddColumn ("Количество").AddTextRenderer (e => e.BalanceText)
			.AddColumn ("Cтоимость").AddTextRenderer (e => e.AvgCostText)
			.AddColumn ("Износ на сегодня").AddProgressRenderer (e => ((int)(e.Percentage * 100)).Clamp(0, 100))
			.AddSetter ((w, e) => w.Text = 
				(e.ExpiryDate.HasValue ? $"до {e.ExpiryDate.Value:d}" : "до износа"))
				.Finish ();
		public override IColumnsConfig ColumnsConfig => treeViewConfig;
		#endregion
		#region implemented abstract members of RepresentationModelEntityBase
		protected override bool NeedUpdateFunc (object updatedSubject) {
			return updatedSubject is EmployeeIssueOperation op && op.Employee.IsSame(Employee);
		}

		#endregion
		public EmployeeBalanceVM (IUnitOfWork uow) : base (typeof(EmployeeIssueOperation)) => 
			UoW = uow;
	}

	public class EmployeeBalanceVMNode {
		public int Id { get; set; }
		[UseForSearch]
		public string NomenclatureName { get; set;}
		public string ProtectionToolsName { get; set; }
		public string ItemName => NomenclatureName ?? ProtectionToolsName;
		public string NomenclatureUnitsName { get; set;}
		public string ProtectionToolsUnitsName { get; set;}
		public string UnitsName => NomenclatureUnitsName ?? ProtectionToolsUnitsName;
		public string WearSize { get; set; }
		public string Height { get; set; }
		public decimal AvgCost { get; set;}
		public decimal WearPercent { get; set;}
		public DateTime IssuedDate { get; set;}
		public DateTime? StartUseDate { get; set; }
		public DateTime? ExpiryDate { get; set;}
		public decimal Percentage => ExpiryDate != null ? 
				EmployeeIssueOperation.CalculatePercentWear(DateTime.Today, StartUseDate, ExpiryDate, WearPercent) : 0;
		public int Added { get; set;}
		public int Removed { get; set;}
		public string BalanceText => $"{Added - Removed} {UnitsName}";
		public string AvgCostText => AvgCost > 0 ? CurrencyWorks.GetShortCurrencyString(AvgCost) : String.Empty;
	}
}

