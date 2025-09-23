using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;
using Workwear.Models.Operations;

namespace workwear.Representations.Organization {
	public class EmployeeWearItemsVM: RepresentationModelWithoutEntityBase<EmployeeWearItemsVMNode> {
		private readonly StockBalanceModel stockBalanceModel;
		public EmployeeWearItemsVM(StockBalanceModel stockBalanceModel, IUnitOfWork uow): base(typeof(EmployeeCardItem)) {
			this.stockBalanceModel = stockBalanceModel;
			UoW = uow;
		}

		#region Свойства

		private IssuanceRequest issuanceRequest;
		public IssuanceRequest IssuanceRequest {
			get => issuanceRequest;
			set => SetField(ref issuanceRequest, value);
		}
		#endregion
		#region IRepresentationModel implementation

		public override void UpdateNodes() {
			if(IssuanceRequest.Employees == null) {
				SetItemsSource(new List<EmployeeWearItemsVMNode>());
				return;
			}

			EmployeeWearItemsVMNode resultAlias = null;
			CollectiveExpense collectiveExpenseAlias = null;
			CollectiveExpenseItem collectiveExpenseItemAlias = null;
			ProtectionTools protectionToolsAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			NormItem normItemAlias = null;
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			EmployeeCard employeeCardAlias = null;
			EmployeeCardItem employeeCardItemAlias = null;
			EmployeeCardItem employeeCardItemForNormAlias = null;
			
			
			var query = UoW.Session.QueryOver<CollectiveExpenseItem>(() => collectiveExpenseItemAlias)
				.JoinAlias(() => collectiveExpenseItemAlias.Document, () => collectiveExpenseAlias)
				.JoinAlias(() => collectiveExpenseItemAlias.Employee, () => employeeCardAlias)
				.Where(() => collectiveExpenseAlias.IssuanceRequest == IssuanceRequest)
				.Where(() => employeeCardAlias.Id.IsIn(IssuanceRequest.Employees.Select(x => x.Id).ToList()));

			var wearItemsList = query
				.JoinAlias(() => collectiveExpenseItemAlias.EmployeeIssueOperation, () => employeeIssueOperationAlias)
				.JoinAlias(() => employeeCardAlias.WorkwearItems, () => employeeCardItemAlias)
				.JoinEntityAlias(() => employeeCardItemForNormAlias, 
					() => employeeCardItemAlias.Id == employeeCardItemForNormAlias.Id &&
					      employeeCardItemForNormAlias.ActiveNormItem.Id == employeeIssueOperationAlias.NormItem.Id)
				.JoinAlias(() => employeeCardItemForNormAlias.ActiveNormItem, () => normItemAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => normItemAlias.ProtectionTools, () => protectionToolsAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeIssueOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeIssueOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
				.SelectList(list => list
					.SelectGroup(() => employeeIssueOperationAlias.ProtectionTools.Id).WithAlias(() => resultAlias.ProtectionToolsId)
					.SelectGroup(() => employeeIssueOperationAlias.WearSize.Id).WithAlias(() => resultAlias.WearSizeId)
					.SelectGroup(() => employeeIssueOperationAlias.Height.Id).WithAlias(() => resultAlias.HeightId)
					.Select(Projections.Sum(() => normItemAlias.Amount).WithAlias(() => resultAlias.Need))
					.Select(Projections.Sum(() => employeeIssueOperationAlias.Issued).WithAlias(() => resultAlias.Issued))
					.Select(() => protectionToolsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsName)
					.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSize)
					.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
					.Select(() => employeeCardItemForNormAlias.Id).WithAlias(() => resultAlias.EmployeeCardItemId)
					
				)
				.TransformUsing(Transformers.AliasToBean<EmployeeWearItemsVMNode>())
				.List<EmployeeWearItemsVMNode>();

			var employeeCardItemIds = wearItemsList.Select(x => x.EmployeeCardItemId).ToList();
			var employeeCardItems = UoW.Session.QueryOver<EmployeeCardItem>()
				.Where(x => x.Id.IsIn(employeeCardItemIds))
				.List<EmployeeCardItem>()
				.ToDictionary(x => x.Id);

			foreach(var node in wearItemsList) {
				var item = employeeCardItems[node.EmployeeCardItemId];
				node.InStock = stockBalanceModel.ForNomenclature(item.ProtectionTools.Nomenclatures.ToArray()).Sum(x => x.Amount);
			}

			SetItemsSource(wearItemsList.ToList());
		}

		private IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<EmployeeWearItemsVMNode>()
			.AddColumn("Потребность").AddTextRenderer(node => node.ProtectionToolsName)
			.AddColumn("Размер/Рост").AddTextRenderer(node => node.Sizes)
			.AddColumn("Требуется").AddTextRenderer(node => node.Need.ToString())
			.AddColumn("Выдано").AddTextRenderer(node => node.Issued.ToString())
			.AddColumn("К выдаче").AddTextRenderer(node => node.NeedToBeIssued.ToString())
			.AddColumn("На складе").AddTextRenderer(node => node.InStock.ToString())
			.Finish();
		
		public override IColumnsConfig ColumnsConfig => treeViewConfig;
		#endregion
		
		#region implemented abstract members of RepresentationModelEntityBase
		protected override bool NeedUpdateFunc (object updatedSubject) {
			return updatedSubject is IssuanceRequest op && op.IsSame(IssuanceRequest);
		}

		#endregion
	}

	public class EmployeeWearItemsVMNode {
		public int EmployeeCardItemId { get; set; }
		public int ProtectionToolsId { get; set; }
		public string ProtectionToolsName { get; set; }
		public int WearSizeId { get; set; }
		public string WearSize { get; set; }
		public int HeightId { get; set; }
		public string Height { get; set; }
		public string Sizes => String.Concat(WearSize, "/", Height);
		public int Need { get; set; }
		public int Issued { get; set; }
		public int NeedToBeIssued => Need >= Issued ? (Need - Issued) : 0;
		public int InStock { get; set; }
		
	}
}
