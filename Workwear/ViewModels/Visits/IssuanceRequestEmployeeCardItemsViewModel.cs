using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.Extensions.Observable.Collections.List;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
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
		public IObservableList<EmployeeCardItemsVmNode> GroupedList = new ObservableList<EmployeeCardItemsVmNode>();
		private IList<EmployeeCardItemsVmNode> groupedEmployeeCardItems;
		public virtual IList<EmployeeCardItemsVmNode> GroupedEmployeeCardItems {
			get => groupedEmployeeCardItems;
			set => SetField(ref groupedEmployeeCardItems, value);
		}
		private IssuanceRequest IssuanceRequest => parent.Entity;
		private IList<EmployeeCard> Employees => parent.Employees;
		private IList<CollectiveExpense> CollectiveExpenses => parent.CollectiveExpenses;
		private EmployeeCardItem[] EmployeeCardItems { get; set; }
		private CollectiveExpenseItem[] CollectiveExpenseItems { get; set; }
		
		#region События

		#region Предзагрузка
		private EmployeeCardItem[] LoadEmployeeCardItems(int[] employeesIds) {
			ProtectionTools protectionToolsAlias = null;
			ItemsType itemsTypeAlias = null;
			return parent.UoW.Session.QueryOver<EmployeeCardItem>()
				.Where(x => x.EmployeeCard.Id.IsIn(employeesIds))
				.JoinAlias(x => x.ProtectionTools, () => protectionToolsAlias)
				.JoinAlias(() => protectionToolsAlias.Type, () => itemsTypeAlias)
				.Where(() => itemsTypeAlias.IssueType == IssueType.Collective)
				.List()
				.ToArray();
		}
		private CollectiveExpenseItem[] LoadCollectiveExpenseItems(int[] employeesIds, int[] collectiveExpensesIds) {
			ProtectionTools protectionToolsAlias = null;
			ItemsType itemsTypeAlias = null;
			return parent.UoW.Session.QueryOver<CollectiveExpenseItem>()
				.Where(x => x.Document.Id.IsIn(collectiveExpensesIds))
				.Where(x => x.Employee.Id.IsIn(employeesIds))
				.JoinAlias(x => x.ProtectionTools, () => protectionToolsAlias)
				.JoinAlias(() => protectionToolsAlias.Type, () => itemsTypeAlias)
				/*Подгружаем только коллективные выдачи, исключая индивидуальные выдачи сотруднику.
				Чтобы не забыть, когда будем считать и обычные выдачи (!)*/
				.Where(() => itemsTypeAlias.IssueType == IssueType.Collective)
				.List()
				.ToArray();
		}
		public void ReloadData() {
			var employeesIds = Employees.Select(e => e.Id).ToArray();
			var collectiveExpensesIds = CollectiveExpenses.Select(c => c.Id).ToArray();
			stockBalanceModel.OnDate = IssuanceRequest.ReceiptDate;
			employeeIssueModel.PreloadEmployeeInfo(employeesIds);
			employeeIssueModel.PreloadWearItems(employeesIds);
			EmployeeCardItems = LoadEmployeeCardItems(employeesIds);
			CollectiveExpenseItems = LoadCollectiveExpenseItems(employeesIds, collectiveExpensesIds);
			employeeIssueModel.FillWearInStockInfo(EmployeeCardItems, stockBalanceModel);
			employeeIssueModel.FillWearReceivedInfo(EmployeeCardItems);
			UpdateNodes();
		}
		#endregion
		public void OnShow() {
			if(GroupedEmployeeCardItems == null)
				ReloadData();
		}

		private void UpdateNodes() {
			IList<EmployeeCardItemsVmNode> employeeCardItemsNodeList = new List<EmployeeCardItemsVmNode>();
			GroupedList.Clear();
			var alreadyIssuedOperationsIds = new HashSet<int>(CollectiveExpenseItems.Select(x => x.EmployeeIssueOperation.Id));
			
			foreach(var item in EmployeeCardItems) {
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
				var issuedByCollectiveExpense = CollectiveExpenseItems
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

			foreach(var item in GroupedEmployeeCardItems)
				GroupedList.Add(item);
		}
		#endregion
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
