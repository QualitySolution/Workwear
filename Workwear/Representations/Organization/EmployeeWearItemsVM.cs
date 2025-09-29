using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;
using Workwear.Models.Operations;
using Workwear.Tools;

namespace workwear.Representations.Organization {
	public class EmployeeWearItemsVM: RepresentationModelWithoutEntityBase<EmployeeWearItemsVMNode> {
		private readonly StockBalanceModel stockBalanceModel;
		private readonly BaseParameters baseParameters;
		private readonly EmployeeIssueModel issueModel;
		public EmployeeWearItemsVM(
			StockBalanceModel stockBalanceModel,
			EmployeeIssueModel issueModel,
			BaseParameters baseParameters,
			IUnitOfWork uow): base(typeof(EmployeeCardItem)) {
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
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
			EmployeeCard employeeCardAlias = null;
			EmployeeCardItem employeeCardItemAlias = null;
			ProtectionTools protectionToolsAlias = null;
			ItemsType itemsTypeAlias = null;
			MeasurementUnits measurementUnitsAlias = null;
			EmployeeSize employeeSizeAlias = null;
			EmployeeSize employeeHeightAlias = null;
			SizeType sizeTypeAlias = null;
			SizeType heightTypeAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			CollectiveExpenseItem collectiveExpenseItemAlias = null;
			CollectiveExpense collectiveExpenseAlias = null;
			NormItem normItemAlias = null;
			
			var collectiveExpenseItems = UoW.Session.QueryOver<CollectiveExpenseItem>(() => collectiveExpenseItemAlias)
				.JoinAlias(() => collectiveExpenseItemAlias.Document, () => collectiveExpenseAlias)
				.Where(() => collectiveExpenseAlias.IssuanceRequest == IssuanceRequest)
				.List();
			
			var queryEmployeeCardItems = UoW.Session.QueryOver<EmployeeCardItem>(() => employeeCardItemAlias)
				.JoinAlias(() => employeeCardItemAlias.EmployeeCard, () => employeeCardAlias)
				.Where(() => employeeCardAlias.Id.IsIn(IssuanceRequest.Employees.Select(x => x.Id).ToList()));

			var employeeCardItems = queryEmployeeCardItems.List();
			
			var queryAllData = queryEmployeeCardItems
				.JoinAlias(() => employeeCardItemAlias.ActiveNormItem, () => normItemAlias)
				.JoinAlias(() => normItemAlias.ProtectionTools, () => protectionToolsAlias)
				.JoinAlias(() => protectionToolsAlias.Type, () => itemsTypeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => itemsTypeAlias.Units, () => measurementUnitsAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => itemsTypeAlias.SizeType, () => sizeTypeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => itemsTypeAlias.HeightType, () => heightTypeAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => employeeSizeAlias, 
					() => employeeSizeAlias.Employee.Id == employeeCardAlias.Id
					&& employeeSizeAlias.SizeType.Id == sizeTypeAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeSizeAlias.Size, () => sizeAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => employeeHeightAlias,
					() => employeeHeightAlias.Employee.Id == employeeCardAlias.Id
					&& employeeHeightAlias.SizeType.Id == heightTypeAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeHeightAlias.Size, () => heightAlias, JoinType.LeftOuterJoin);
			
			var employeeWearItemsNodeList = queryAllData
				.SelectList(list => list
					.Select(() => employeeCardItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => employeeCardAlias.Id).WithAlias(() => resultAlias.EmployeeCardId)
					.Select(() => protectionToolsAlias.Id).WithAlias(() => resultAlias.ProtectionToolsId)
					.Select(() => protectionToolsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsName)
					.Select(() => sizeAlias.Id).WithAlias(() => resultAlias.WearSizeId)
					.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSize)
					.Select(() => heightAlias.Id).WithAlias(() => resultAlias.HeightId)
					.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
					.Select(() => measurementUnitsAlias.Name).WithAlias(() => resultAlias.Units)
				)
				.TransformUsing(Transformers.AliasToBean<EmployeeWearItemsVMNode>())
				.List<EmployeeWearItemsVMNode>();
			
			foreach(var node in employeeWearItemsNodeList) {
				var employeeCardItem = employeeCardItems.FirstOrDefault(x => x.Id == node.Id);
				node.Need = employeeCardItem.CalculateRequiredIssue(baseParameters, IssuanceRequest.ReceiptDate);
				node.IssuedByCollectiveExpense = collectiveExpenseItems
					.Where(x => x.Employee.GetId() == node.EmployeeCardId
													&& x.ProtectionTools.GetId() == node.ProtectionToolsId
													&& (x.WearSize != null ? x.WearSize.GetId() == node.WearSizeId : x.WearSize == null)
													&& (x.Height != null ? x.Height.GetId() == node.HeightId : x.Height == null))
					.Select(x => x.Amount)
					.Sum();
				node.Issued = node.IssuedByCollectiveExpense > node.Need ? node.Need : node.IssuedByCollectiveExpense;
				node.InStock = employeeCardItem.InStock.Sum(x => x.Amount);
			}

			var groupedList = employeeWearItemsNodeList
				.GroupBy(x => (x.ProtectionToolsId, x.WearSizeId, x.HeightId))
				.Select(node => new EmployeeWearItemsVMNode {
					ProtectionToolsId = node.Key.ProtectionToolsId,
					ProtectionToolsName = node.First().ProtectionToolsName,
					WearSizeId = node.Key.WearSizeId,
					WearSize = node.First().WearSize,
					HeightId = node.Key.HeightId,
					Height = node.First().Height,
					Units = node.First().Units,
					Need = node.Sum(x => x.Need),
					Issued = node.Sum(x => x.Issued),
					InStock = node.Sum(x => x.InStock)
				})
				.Where(node => node.Need != 0)
				.ToList();
			
			SetItemsSource(groupedList);
		}

		private IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<EmployeeWearItemsVMNode>()
			.AddColumn("Потребность").AddTextRenderer(node => node.ProtectionToolsName)
			.AddColumn("Размер/Рост").AddTextRenderer(node => node.Sizes)
			.AddColumn("Требуется").AddTextRenderer(node => node.NeedText)
			.AddColumn("Выдано").AddTextRenderer(node => node.IssuedText)
			.AddColumn("К выдаче").AddTextRenderer(node => node.NeedToBeIssuedText)
			.AddSetter((w, node) => w.Foreground = node.NeedToBeIssuedColor())
			.AddColumn("На складе").AddTextRenderer(node => node.InStockText)
			.RowCells().AddSetter<CellRendererText>((c, node) => c.Foreground = node.AllIssued)
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
		public int Id { get; set; }
		public int EmployeeCardId { get; set; }
		public int ProtectionToolsId { get; set; }
		public string ProtectionToolsName { get; set; }
		
		//TODO Скорее всего нужен будет учет пола
		public Sex Sex { get; set; }
		public int? WearSizeId { get; set; }
		public string WearSize { get; set; }
		public int? HeightId { get; set; }
		public string Height { get; set; }
		public string Sizes => String.Concat(WearSize, "/", Height);
		public string Units { get; set; } 
		public int Need { get; set; }
		public string NeedText => $"{Need} {Units}";
		public int IssuedByCollectiveExpense { get; set; }
		public int Issued { get; set; }
		public string IssuedText => $"{Issued} {Units}";
		public int NeedToBeIssued => Need >= Issued ? Need - Issued : 0;
		public string NeedToBeIssuedText => $"{NeedToBeIssued} {Units}";
		public int InStock { get; set; }
		public string InStockText => $"{InStock} {Units}";
		
		public string NeedToBeIssuedColor() {
			if(NeedToBeIssued == 0)
				return "gray";
			if(NeedToBeIssued != 0 && NeedToBeIssued < Need)
				return "orange";
			if(NeedToBeIssued == Need)
				return "red";
			return null;
		}

		public string AllIssued => NeedToBeIssued == 0 ? "gray" : null;
	}
}
