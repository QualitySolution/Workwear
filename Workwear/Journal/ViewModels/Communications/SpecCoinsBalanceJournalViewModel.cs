using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Cloud.WearLk.Client;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Utilities.Text;
using Workwear.Domain.Company;
using Workwear.ViewModels.Communications;
using Workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Communications {
	public class SpecCoinsBalanceJournalViewModel : JournalViewModelBase {
		private readonly SpecCoinManagerService specCoinManagerService;

		public SpecCoinsBalanceJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			SpecCoinManagerService specCoinManagerService) : base(unitOfWorkFactory, interactiveService, navigation) {
			this.specCoinManagerService = specCoinManagerService ?? throw new ArgumentNullException(nameof(specCoinManagerService));
			Title = "Баланс спецкойнов";
			SearchEnabled = false;
			
			var loader = new AnyDataLoader<SpecCoinsBalanceJournalNode>(GetBalanceNodes);
			loader.DynamicLoadingEnabled = false;
			DataLoader = loader;
			CreateNodeActions();
		}

		private IList<SpecCoinsBalanceJournalNode> GetBalanceNodes(CancellationToken token) {
			SpecCoinsBalanceJournalNode resultAlias = null;
			var balances = specCoinManagerService.GetListBalances(token);
			var phones = balances.Select(x => (object)x.Phone).ToArray();
			var employees = UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.PhoneNumber.IsIn(phones))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.PersonnelNumber).WithAlias(() => resultAlias.PersonnelNumber)
					.Select(x => x.FirstName).WithAlias(() => resultAlias.EmployeeFirstName)
					.Select(x => x.LastName).WithAlias(() => resultAlias.EmployeeLastName)
					.Select(x => x.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
					.Select(x => x.PhoneNumber).WithAlias(() => resultAlias.EmployeePhone)
				)
				.TransformUsing(Transformers.AliasToBean<SpecCoinsBalanceJournalNode>())
				.List<SpecCoinsBalanceJournalNode>()
				.ToDictionary(x => x.EmployeePhone);
			var result = new List<SpecCoinsBalanceJournalNode>();
			foreach(var balance in balances) {
				if(employees.TryGetValue(balance.Phone, out var employee)) {
					employee.Balance = balance.Balance;
					result.Add(employee);
				}
			}
			return result;
		}

		protected override void CreateNodeActions() {
			NodeActionsList.Clear();
			
			NodeActionsList.Add(new JournalAction("Открыть сотрудника", 
				selected => selected.Any(),
				selected => true,
				selected => selected.Cast<SpecCoinsBalanceJournalNode>().ToList().ForEach(OnOpenEmployee)));
			NodeActionsList.Add(new JournalAction( "История операций",
				selected => selected.Any(),
				selected => true,
				selected => selected.Cast<SpecCoinsBalanceJournalNode>().ToList().ForEach(OnOpenOperations)));
			NodeActionsList.Add(new JournalAction( "Списать спецкойны",
				selected => selected.Cast<SpecCoinsBalanceJournalNode>().Any(x => x.Balance > 0),
				selected => true,
				selected => selected.Cast<SpecCoinsBalanceJournalNode>().Where(x => x.Balance > 0).ToList().ForEach(OnDeductSpecCoins)));
		}

		private void OnOpenEmployee(SpecCoinsBalanceJournalNode node) {
			NavigationManager.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
		}
		
		private void OnDeductSpecCoins(SpecCoinsBalanceJournalNode node) {
			NavigationManager.OpenViewModel<DeductSpecCoinsViewModel, string, int, Action>
			(
				this,
				node.EmployeePhone,
				node.Balance,
				() => Refresh()
			);
		}
		
		private void OnOpenOperations(SpecCoinsBalanceJournalNode node) {
			var employee = UoW.GetById<EmployeeCard>(node.Id);
			NavigationManager.OpenViewModel<SpecCoinsOperationsJournalViewModel, EmployeeCard>(this, employee);
		}
	}

	public class SpecCoinsBalanceJournalNode{
		public int Id { get; set; }
		public string EmployeePhone { get; set; }
		public int Balance { get; set; }
		public string EmployeeFirstName { get; set; }
		public string EmployeeLastName { get; set; }
		public string EmployeePatronymic { get; set; }
		public string PersonnelNumber { get; set; }
		public string EmployeeBalanceText => $"{Balance} \u24c8";
		public string EmployeeText => PersonHelper.PersonFullName(EmployeeLastName, EmployeeFirstName, EmployeePatronymic);
	}
}
