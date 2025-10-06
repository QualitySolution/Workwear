using System;
using System.Collections.Generic;
using System.Linq;
using QS.ViewModels;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;
using Workwear.Models.Operations;
using Workwear.Tools;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Visits {
	public class IssuanceRequestEmployeeCardItemsViewModel: ViewModelBase {
		private readonly EmployeeIssueModel employeeIssueModel;
		private readonly StockBalanceModel stockBalanceModel;
		private readonly BaseParameters baseParameters;
		private readonly IssuanceRequestViewModel parent;
		public IssuanceRequestEmployeeCardItemsViewModel(
			EmployeeIssueModel employeeIssueModel, 
			StockBalanceModel stockBalanceModel, 
			BaseParameters baseParameters,
			IssuanceRequestViewModel parent) 
		{
			this.employeeIssueModel = employeeIssueModel ?? throw new ArgumentNullException(nameof(employeeIssueModel));
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
		}
		public IssuanceRequest IssuanceRequest => parent.Entity;
		public IList<EmployeeCardItemsVmNode> GroupedEmployeeCardItems { get; set; }
		
		#region События
		public void OnShow() {
			if(GroupedEmployeeCardItems == null)
				UpdateNodes();
		}
		#endregion

		public void UpdateNodes() {
			IList<EmployeeCardItemsVmNode> employeeCardItemsNodeList = new List<EmployeeCardItemsVmNode>();
			CollectiveExpense collectiveExpenseAlias = null;

			stockBalanceModel.OnDate = IssuanceRequest.ReceiptDate;
			var employees = employeeIssueModel.PreloadEmployeeInfo(IssuanceRequest.Employees.Select(x => x.Id).ToArray());
			employeeIssueModel.PreloadWearItems(employees.Select(x => x.Id).ToArray());
			var employeeCardItems = employees
				.SelectMany(x => x.WorkwearItems)
				.Where(x => x.ProtectionTools.Type.IssueType == IssueType.Collective)
				.ToArray();
			
			var collectiveExpenseItems = parent.UoW.Session.QueryOver<CollectiveExpenseItem>()
				.JoinAlias(x => x.Document, () => collectiveExpenseAlias)
				.Where(() => collectiveExpenseAlias.IssuanceRequest.Id == IssuanceRequest.Id)
				.List();
			
			employeeIssueModel.FillWearInStockInfo(employeeCardItems, stockBalanceModel);
			employeeIssueModel.FillWearReceivedInfo(employeeCardItems);

			var alreadyIssuedOperationsIds = new HashSet<int>(collectiveExpenseItems.Select(x => x.EmployeeIssueOperation.Id));
			
			foreach(var item in employeeCardItems) {
				var wearSize = item.EmployeeCard.Sizes
					.Where(x => x.SizeType == item.ProtectionTools.Type.SizeType)
					.Where(x => x.SizeType.CategorySizeType == CategorySizeType.Size)
					.Select(x => x.Size)
					.FirstOrDefault();
				var height = item.EmployeeCard.Sizes
					.Where(x => x.SizeType == item.ProtectionTools.Type.SizeType)
					.Where(x => x.SizeType.CategorySizeType == CategorySizeType.Height)
					.Select(x => x.Size)
					.FirstOrDefault();
				var issuedByCollectiveExpense = collectiveExpenseItems
					.Where(x => x.ProtectionTools.Id == item.ProtectionTools.Id)
					.Where(x => x.Employee.Id == item.EmployeeCard.Id)
					.Sum(x => x.Amount);
				var need = item.CalculateRequiredIssue(baseParameters, IssuanceRequest.ReceiptDate.AddSeconds(-1), excludeOperationIds: alreadyIssuedOperationsIds);
				EmployeeCardItemsVmNode employeeCardItemsNode = new EmployeeCardItemsVmNode() {
					Id = item.Id,
					EmployeeCardId = item.EmployeeCard.Id,
					ProtectionToolsId = item.ProtectionTools.Id,
					ProtectionToolsName = item.ProtectionTools.Name,
					WearSize = wearSize,
					Height = height,
					Units = item.ProtectionTools.Type?.Units?.Name,
					Need = need,
					Issued = Math.Min(issuedByCollectiveExpense, need),
					InStock = item.InStock.Sum(x => x.Amount),
				};
				employeeCardItemsNodeList.Add(employeeCardItemsNode);
			}
			
			GroupedEmployeeCardItems = employeeCardItemsNodeList
				.GroupBy(x => (x.ProtectionToolsId, x.WearSize, x.Height))
				.Select(node => new EmployeeCardItemsVmNode {
					ProtectionToolsId = node.Key.ProtectionToolsId,
					ProtectionToolsName = node.First().ProtectionToolsName,
					WearSize = node.Key.WearSize,
					Height = node.Key.Height,
					Units = node.First().Units,
					Need = node.Sum(x => x.Need),
					Issued = node.Sum(x => x.Issued),
					InStock = node.First().InStock
				})
				.Where(node => node.Need != 0)
				.ToList();
		}
	}
	public class EmployeeCardItemsVmNode {
		public int Id { get; set; }
		public int EmployeeCardId { get; set; }
		public int ProtectionToolsId { get; set; }
		public string ProtectionToolsName { get; set; }
		public Size WearSize { get; set; }
		public Size Height { get; set; }
		public string Sizes => SizeService.SizeTitle(WearSize, Height);
		public string Units { get; set; } 
		public int Need { get; set; }
		public string NeedText => $"{Need} {Units}";
		public int Issued { get; set; }
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
