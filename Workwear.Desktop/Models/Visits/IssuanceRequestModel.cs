using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;
using Workwear.Models.Operations;
using Workwear.Tools;

namespace Workwear.Models.Visits {
	/// <summary>
	/// Расчет потребностей и статуса заявки на выдачу.
	/// </summary>
	public class IssuanceRequestModel {
		private readonly EmployeeIssueModel employeeIssueModel;
		private readonly BaseParameters baseParameters;
		private readonly UnitOfWorkProvider unitOfWorkProvider;

		public IssuanceRequestModel(EmployeeIssueModel employeeIssueModel, BaseParameters baseParameters, UnitOfWorkProvider unitOfWorkProvider) {
			this.employeeIssueModel = employeeIssueModel ?? throw new ArgumentNullException(nameof(employeeIssueModel));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.unitOfWorkProvider = unitOfWorkProvider ?? throw new ArgumentNullException(nameof(unitOfWorkProvider));
		}

		#region Helpers
		private IUnitOfWork UoW => unitOfWorkProvider.UoW;
		#endregion

		/// <summary>
		/// Рассчитывает потребности сотрудников заявки по коллективным СИЗ и выданное по ним связанными документами коллективной выдачи.
		/// </summary>
		/// <param name="progressStep">Метод вызывается перед каждым шагом, передавая название шага.</param>
		public IList<IssuanceRequestNeed> CalculateNeeds(IssuanceRequest request, Action<string> progressStep = null) {
			CollectiveExpense collectiveExpenseAlias = null;

			progressStep?.Invoke(nameof(employeeIssueModel.PreloadEmployeeInfo));
			var employees = employeeIssueModel.PreloadEmployeeInfo(request.Employees.Select(x => x.Id).ToArray());

			progressStep?.Invoke(nameof(employeeIssueModel.PreloadWearItems));
			employeeIssueModel.PreloadWearItems(employees.Select(x => x.Id).ToArray());

			progressStep?.Invoke("Загрузка потребностей");
			var employeeCardItems = employees
				.SelectMany(x => x.WorkwearItems)
				.Where(x => x.ProtectionTools.Type.IssueType == IssueType.Collective)
				.ToList();

			progressStep?.Invoke("Загрузка строк док-в коллективной выдачи");
			var collectiveExpenseItems = UoW.Session.QueryOver<CollectiveExpenseItem>()
				.JoinAlias(x => x.Document, () => collectiveExpenseAlias)
				.Where(() => collectiveExpenseAlias.IssuanceRequest.Id == request.Id)
				.List();

			progressStep?.Invoke(nameof(employeeIssueModel.FillWearReceivedInfo));
			employeeIssueModel.FillWearReceivedInfo(employeeCardItems.ToArray());

			var alreadyIssuedOperationsIds = new HashSet<int>(collectiveExpenseItems.Select(x => x.EmployeeIssueOperation.Id));
			progressStep?.Invoke("Заполнение потребностей");
			var result = new List<IssuanceRequestNeed>();
			foreach(var item in employeeCardItems) {
				var issuedByCollectiveExpense = collectiveExpenseItems
					.Where(x => x.ProtectionTools.Id == item.ProtectionTools.Id)
					.Where(x => x.Employee.Id == item.EmployeeCard.Id)
					.Sum(x => x.Amount);
				var need = item.CalculateRequiredIssue(baseParameters, request.ReceiptDate.AddSeconds(-1), excludeOperationIds: alreadyIssuedOperationsIds);
				result.Add(new IssuanceRequestNeed {
					EmployeeCardItem = item,
					Need = need,
					Issued = Math.Min(issuedByCollectiveExpense, need),
				});
			}
			return result;
		}

		/// <summary>
		/// Актуальный статус заявки.
		/// </summary>
		public static IssuanceRequestStatus CalculateStatus(IssuanceRequest request, IEnumerable<IssuanceRequestNeed> needs) {
			if(request.CollectiveExpenses.Count == 0)
				return IssuanceRequestStatus.New;
			if(needs.All(x => x.Issued >= x.Need))
				return IssuanceRequestStatus.Issued;
			return IssuanceRequestStatus.PartiallyIssued;
		}

		/// <summary>
		/// Пересчитывает и сохраняет статусы заявок. (Без коммита)
		/// </summary>
		public void UpdateStatuses(IEnumerable<int> issuanceRequestIds) {
			foreach(var id in issuanceRequestIds.Distinct()) {
				var request = UoW.GetById<IssuanceRequest>(id);
				if(request == null)
					continue;
				var status = CalculateStatus(request, CalculateNeeds(request));
				if(request.Status == status)
					continue;
				request.Status = status;
				UoW.Save(request);
			}
		}
	}

	public class IssuanceRequestNeed {
		public EmployeeCardItem EmployeeCardItem { get; set; }
		public int Need { get; set; }
		public int Issued { get; set; }
	}
}
