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
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace workwear.ViewModel
{
	public class ObjectBalanceVM : RepresentationModelWithoutEntityBase<ObjectBalanceVMNode>
	{
		private Subdivision subdivision;

		private Subdivision Subdivision {
			get => subdivision;
			set => subdivision = value;
		}
		
		#region IRepresentationModel implementation
		public override void UpdateNodes () {
			if(Subdivision == null) {
				SetItemsSource (new List<ObjectBalanceVMNode> ());
				return;
			}

			ObjectBalanceVMNode resultAlias = null;

			SubdivisionIssueOperation issueOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType itemTypesAlias = null;
			MeasurementUnits unitsAlias = null;
			SubdivisionIssueOperation removeIssueOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;

			var expense = UoW.Session.QueryOver(() => issueOperationAlias)
				.Where(e => e.Subdivision == Subdivision);

			var subQueryRemove = QueryOver.Of(() => removeIssueOperationAlias)
				.Where(() => removeIssueOperationAlias.IssuedOperation == issueOperationAlias)
				.Select (Projections.Sum<SubdivisionIssueOperation> (o => o.Returned));

			var expenseList = expense
				.JoinAlias (() => issueOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias (() => nomenclatureAlias.Type, () => itemTypesAlias)
				.JoinAlias (() => itemTypesAlias.Units, () => unitsAlias)
				.JoinAlias (() => issueOperationAlias.WarehouseOperation, () => warehouseOperationAlias)
				.Where (e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > DateTime.Today)
				.SelectList (list => list
					.SelectGroup (() => issueOperationAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.Select (() => unitsAlias.Name).WithAlias (() => resultAlias.UnitsName)
					.Select (() => issueOperationAlias.WearPercent).WithAlias (() => resultAlias.WearPercent)
					.Select (() => issueOperationAlias.Issued).WithAlias (() => resultAlias.Added)
					.Select (() => issueOperationAlias.OperationTime).WithAlias (() => resultAlias.IssuedDate)
					.Select (() => issueOperationAlias.ExpiryOn).WithAlias (() => resultAlias.ExpiryDate)
					.SelectSubQuery (subQueryRemove).WithAlias (() => resultAlias.Removed)
				)
				.TransformUsing (Transformers.AliasToBean<ObjectBalanceVMNode> ())
				.List<ObjectBalanceVMNode> ().Where(r => r.Added - r.Removed != 0);		

			SetItemsSource (expenseList.ToList ());
		}

		public override IColumnsConfig ColumnsConfig { get; } = ColumnsConfigFactory.Create<ObjectBalanceVMNode>()
			.AddColumn ("Наименование").AddTextRenderer (e => e.NomenclatureName).WrapWidth(1000)
			.AddColumn ("Количество").AddTextRenderer (e => e.BalanceText)
			.AddColumn ("Срок службы").AddProgressRenderer (e => (int)(100 - (e.Percentage * 100)))
				.AddSetter ((w, e) => w.Text = e.ExpiryDate.HasValue ? 
				$"до {e.ExpiryDate.Value:d}" : string.Empty)
			.Finish ();

		#endregion
		#region implemented abstract members of RepresentationModelEntityBase
		protected override bool NeedUpdateFunc (object updatedSubject) => true;
		#endregion
		public ObjectBalanceVM (Subdivision facility) : 
			this(UnitOfWorkFactory.CreateWithoutRoot ()) => Subdivision = facility;

		public ObjectBalanceVM (IUnitOfWork uow) : 
			base (typeof(Expense), typeof(Income), typeof(Writeoff)) => UoW = uow;
	}

	public class ObjectBalanceVMNode {
		public int Id { get; set; }
		[UseForSearch]
		public string NomenclatureName { get; set;}
		public string UnitsName { get; set;}
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
		public string BalanceText => $"{Added - Removed} {UnitsName}";
	}
}
