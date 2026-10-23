using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.Extensions.Observable.Collections.List;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Visits;
using Workwear.Models.Operations;
using Workwear.Models.Visits;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Visits {
	public class IssuanceRequestEmployeeCardItemsViewModel: ViewModelBase {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly EmployeeIssueModel employeeIssueModel;
		private readonly StockBalanceModel stockBalanceModel;
		private readonly IssuanceRequestModel issuanceRequestModel;
		private readonly IssuanceRequestViewModel parent;
		private readonly ModalProgressCreator modalProgress;
		public IssuanceRequestEmployeeCardItemsViewModel(
			EmployeeIssueModel employeeIssueModel,
			StockBalanceModel stockBalanceModel,
			IssuanceRequestModel issuanceRequestModel,
			IssuanceRequestViewModel parent,
			ModalProgressCreator modalProgress)
		{
			this.employeeIssueModel = employeeIssueModel ?? throw new ArgumentNullException(nameof(employeeIssueModel));
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.issuanceRequestModel = issuanceRequestModel ?? throw new ArgumentNullException(nameof(issuanceRequestModel));
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
			this.modalProgress = modalProgress ?? throw new ArgumentNullException(nameof(modalProgress));
		}
		public IObservableList<EmployeeCardItemsVmNode> GroupedList = new ObservableList<EmployeeCardItemsVmNode>();
		/// <summary>
		/// Потребности по строкам карточек сотрудников, по ним считается статус заявки.
		/// </summary>
		public IList<IssuanceRequestNeed> Needs { get; private set; } = new List<IssuanceRequestNeed>();
		public IList<EmployeeCardItemsVmNode> EmployeeCardItemsNodeList = new List<EmployeeCardItemsVmNode>();
		private IList<EmployeeCardItem> employeeCardItems = new List<EmployeeCardItem>();
		private IList<EmployeeCardItemsVmNode> groupedEmployeeCardItems;
		public virtual IList<EmployeeCardItemsVmNode> GroupedEmployeeCardItems {
			get => groupedEmployeeCardItems;
			set => SetField(ref groupedEmployeeCardItems, value);
		}
		private IssuanceRequest IssuanceRequest => parent.Entity;
		
		#region События
		public void OnShow() {
			if(GroupedEmployeeCardItems == null)
				UpdateNodes(parent.SelectWarehouseStock);
		}

		public void UpdateInStock(Warehouse warehouse) {
			stockBalanceModel.OnDate = IssuanceRequest.ReceiptDate;
			stockBalanceModel.Warehouse = warehouse;
			employeeIssueModel.FillWearInStockInfo(employeeCardItems, stockBalanceModel);
			foreach(var item in EmployeeCardItemsNodeList) {
				item.InStock = employeeCardItems.FirstOrDefault(x => x.Id == item.Id).InStock.Sum(x => x.Amount);
			}

			foreach(var grItem in GroupedList) {
				grItem.InStock = EmployeeCardItemsNodeList
					.Where(x => x.ProtectionToolsId == grItem.ProtectionToolsId)
					.Where(x => x.WearSize?.Id == grItem.WearSize?.Id)
					.Where(x => x.Height?.Id == grItem.Height?.Id)
					.Select(x => x.InStock).First();
			}
		}
		
		public void UpdateNodes(Warehouse warehouse, bool needPerformance = true) {
			employeeCardItems.Clear();
			EmployeeCardItemsNodeList.Clear();
			GroupedList.Clear();
			ProgressPerformanceHelper performance = null;
			if(needPerformance)
				performance =  new ProgressPerformanceHelper(modalProgress, 10,"Старт" ,logger);

			Needs = issuanceRequestModel.CalculateNeeds(IssuanceRequest, step => performance?.CheckPoint(step));
			employeeCardItems = Needs.Select(x => x.EmployeeCardItem).ToList();
			foreach(var need in Needs) {
				var item = need.EmployeeCardItem;
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
				EmployeeCardItemsVmNode employeeCardItemsNode = new EmployeeCardItemsVmNode() {
					Id = item.Id,
					EmployeeCardId = item.EmployeeCard.Id,
					ProtectionToolsId = item.ProtectionTools.Id,
					ProtectionToolsName = item.ProtectionTools.Name,
					WearSize = wearSize,
					Height = height,
					Units = item.ProtectionTools.Type?.Units?.Name,
					Need = need.Need,
					Issued = need.Issued,
				};
				EmployeeCardItemsNodeList.Add(employeeCardItemsNode);
			}
			
			performance?.CheckPoint("Группирровка потребностей");
			GroupedEmployeeCardItems = EmployeeCardItemsNodeList
				.GroupBy(x => (x.ProtectionToolsId, x.WearSize, x.Height))
				.Select(node => new EmployeeCardItemsVmNode {
					ProtectionToolsId = node.Key.ProtectionToolsId,
					ProtectionToolsName = node.First().ProtectionToolsName,
					WearSize = node.Key.WearSize,
					Height = node.Key.Height,
					Units = node.First().Units,
					Need = node.Sum(x => x.Need),
					Issued = node.Sum(x => x.Issued),
				})
				.Where(node => node.Need != 0)
				.ToList();
			performance?.CheckPoint("Добавление элементов в Observable-коллекцию");
			foreach(var item in GroupedEmployeeCardItems)
				GroupedList.Add(item);
			
			performance?.CheckPoint(nameof(employeeIssueModel.FillWearInStockInfo));
			UpdateInStock(warehouse);
			
			performance?.End();
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
		public string InStockText => $"{(InStock > 0 ? InStock : 0)} {Units}";
		
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
