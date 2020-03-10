using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear.ViewModel
{
	public class ObjectBalanceVM : RepresentationModelWithoutEntityBase<ObjectBalanceVMNode>
	{
		Subdivision subdivision;

		Subdivision Subdivision {
			get {
				if (Filter != null)
					return Filter.RestrictObject;
				else
					return subdivision;
			}
			set {
				subdivision = value;
			}
		}

		public ObjectBalanceFilter Filter {
			get {
				return RepresentationFilter as ObjectBalanceFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			if(Subdivision == null)
			{
				SetItemsSource (new List<ObjectBalanceVMNode> ());
				return;
			}

			ObjectBalanceVMNode resultAlias = null;

			SubdivisionIssueOperation issueOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType itemtypesAlias = null;
			MeasurementUnits unitsAlias = null;
			SubdivisionIssueOperation removeIssueOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;

			var expense = UoW.Session.QueryOver<SubdivisionIssueOperation>(() => issueOperationAlias)
				.Where(e => e.Subdivision == Subdivision);

			var subqueryRemove = QueryOver.Of<SubdivisionIssueOperation>(() => removeIssueOperationAlias)
				.Where(() => removeIssueOperationAlias.IssuedOperation == issueOperationAlias)
				.Select (Projections.Sum<SubdivisionIssueOperation> (o => o.Returned));

			var expenseList = expense
				.JoinAlias (() => issueOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias (() => nomenclatureAlias.Type, () => itemtypesAlias)
				.JoinAlias (() => itemtypesAlias.Units, () => unitsAlias)
				.JoinAlias (() => issueOperationAlias.WarehouseOperation, () => warehouseOperationAlias)
				.Where (e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > DateTime.Today)
				.SelectList (list => list
					.SelectGroup (() => issueOperationAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.Select (() => unitsAlias.Name).WithAlias (() => resultAlias.UnitsName)
					.Select (() => issueOperationAlias.Size).WithAlias (() => resultAlias.Size)
					.Select (() => issueOperationAlias.WearGrowth).WithAlias (() => resultAlias.Growth)
					.Select (() => issueOperationAlias.WearPercent).WithAlias (() => resultAlias.WearPercent)
					.Select (() => issueOperationAlias.Issued).WithAlias (() => resultAlias.Added)
					.Select (() => issueOperationAlias.OperationTime).WithAlias (() => resultAlias.IssuedDate)
					.Select (() => issueOperationAlias.ExpiryOn).WithAlias (() => resultAlias.ExpiryDate)
					.SelectSubQuery (subqueryRemove).WithAlias (() => resultAlias.Removed)
				)
				.TransformUsing (Transformers.AliasToBean<ObjectBalanceVMNode> ())
				.List<ObjectBalanceVMNode> ().Where(r => r.Added - r.Removed != 0);		

			SetItemsSource (expenseList.ToList ());
		}

		IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<ObjectBalanceVMNode> ()
			.AddColumn ("Наименование").AddTextRenderer (e => e.NomenclatureName)
			.AddColumn ("Количество").AddTextRenderer (e => e.BalanceText)
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

		public ObjectBalanceVM (ObjectBalanceFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public ObjectBalanceVM (Subdivision facility) : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			Subdivision = facility;
		}

		public ObjectBalanceVM (IUnitOfWork uow) : base (typeof(Expense), typeof(Income), typeof(Writeoff))
		{
			this.UoW = uow;
		}
	}

	public class ObjectBalanceVMNode
	{
		public int Id { get; set; }

		[UseForSearch]
		public string NomenclatureName { get; set;}
		public string UnitsName { get; set;}
		public string Size { get; set;}
		public string Growth { get; set;}
		public decimal WearPercent { get; set;}

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

		public string AvgLifeText {get { 
				return WearPercent.ToString ("P");
			}}
	}
}

