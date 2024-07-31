using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NHibernate.Criterion;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Communications;

namespace workwear.Journal.ViewModels.Communications {
	public class SpecCoinsOperationsJournalViewModel : JournalViewModelBase {
		private readonly SpecCoinManagerService specCoinManagerService;
		private readonly EmployeeCard employee;

		public SpecCoinsOperationsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			SpecCoinManagerService specCoinManagerService,
			EmployeeCard employee
			) : base(unitOfWorkFactory, interactiveService, navigation) {
			this.specCoinManagerService = specCoinManagerService ?? throw new ArgumentNullException(nameof(specCoinManagerService));
			this.employee = employee ?? throw new ArgumentNullException(nameof(employee));
			if(String.IsNullOrEmpty(employee.PhoneNumber))
				throw new ArgumentNullException(nameof(employee.PhoneNumber));
			
			Title = $"Cпецкойны {employee.ShortName}";
			SearchEnabled = false;
			
			var loader = new AnyDataLoader<SpecCoinsOperationsJournalNode>(GetOperationNodes);
			loader.DynamicLoadingEnabled = false;
			DataLoader = loader;
			CreateNodeActions();
		}

		private IList<SpecCoinsOperationsJournalNode> GetOperationNodes(CancellationToken token) {
			var nodes = specCoinManagerService.GetCoinsOperations(employee.PhoneNumber, token)
				.Select(op => new SpecCoinsOperationsJournalNode {
					Operation = op
				}).ToList();

			Nomenclature nomenclatureAlias = null;
			Barcode barcodeAlias = null;
			
			var issue_ids = nodes.Where(x => x.Operation.Rating?.IssueOperationId > 0)
				.Select(x => x.Operation.Rating.IssueOperationId)
				.ToArray();
			if(issue_ids.Length > 0) {
				var issues = UoW.Session.QueryOver<EmployeeIssueOperation>()
					.Where(x => x.Id.IsIn(issue_ids))
					.JoinAlias(o => o.Nomenclature, () => nomenclatureAlias)
					.SelectList(list => list
						.Select(x => x.Id)
						.Select(() => nomenclatureAlias.Name)
					).List<object[]>();
				foreach(var node in nodes.Where(x => x.Operation.Rating?.IssueOperationId > 0)) {
					node.NomenclatureName = issues.FirstOrDefault(x => (int)x[0] == node.Operation.Rating.IssueOperationId)?[1].ToString();
				}
			}
			
			var washing_ids = nodes.Where(x => x.Operation.Rating?.ServiceClaimId > 0)
				.Select(x => x.Operation.Rating.ServiceClaimId)
				.ToArray();
			if(washing_ids.Length > 0) {
				var washings = UoW.Session.QueryOver<ServiceClaim>()
					.Where(x => x.Id.IsIn(washing_ids))
					.JoinAlias(o => o.Barcode, () => barcodeAlias)
					.JoinAlias(() => barcodeAlias.Nomenclature, () => nomenclatureAlias)
					.SelectList(list => list
						.Select(x => x.Id)
						.Select(() => nomenclatureAlias.Name)
					).List<object[]>();
				foreach(var node in nodes.Where(x => x.Operation.Rating?.ServiceClaimId > 0)) {
					node.NomenclatureName = washings.FirstOrDefault(x => (int)x[0] == node.Operation.Rating.ServiceClaimId)?[1].ToString();
				}
			}
			
			return nodes;
		}

		protected override void CreateNodeActions() {
			NodeActionsList.Clear();
			
			NodeActionsList.Add(new JournalAction( "Списать спецкойны",
				selected => true,
				selected => true,
				selected => OnDeductSpecCoins()));
		}
		
		private void OnDeductSpecCoins() {
			NavigationManager.OpenViewModel<DeductSpecCoinsViewModel, string, int, Action>
			(
				this,
				employee.PhoneNumber,
				Items.Cast<SpecCoinsOperationsJournalNode>().Select(x => x.Operation.Coin).Sum(),
				() => Refresh()
			);
		}
	}

	public class SpecCoinsOperationsJournalNode{
		public CoinsOperation Operation { get; set; }
		public string NomenclatureName { get; set; }
		public string CreateTime => Operation.CreateTime.ToDateTime().ToLongTimeString();
		public int Coin => Operation.Coin;
		public string OperationDescription => Operation.Description;
		public string Rating => new string('⭐', (int)(Operation.Rating?.Rating ?? 0));
		public string RatingDescription => $"{Operation.Rating?.Description}";
	}
}
