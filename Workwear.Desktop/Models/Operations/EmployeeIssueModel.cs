using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Repository.Operations;
using Workwear.Tools;

namespace Workwear.Models.Operations {
	public class EmployeeIssueModel {
		private readonly EmployeeIssueRepository employeeIssueRepository;
		private Dictionary<string, IssueGraph> graphs = new Dictionary<string, IssueGraph>();
		private IUnitOfWork uow;

		#region SetUp

		public EmployeeIssueModel(EmployeeIssueRepository employeeIssueRepository) {
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
		}

		public IUnitOfWork UoW {
			get => uow;
			set => uow = employeeIssueRepository.RepoUow = value;
		}

		#endregion

		#region public
		public void RecalculateDateOfIssue(IList<EmployeeIssueOperation> operations, BaseParameters baseParameters, IInteractiveQuestion interactive, IProgressBarDisplayable progress = null) {
			progress?.Start(operations.Count() + 2);
			CheckAndPrepareGraphs(operations.Select(o => o.Employee).Distinct().ToArray(), operations.Select(o => o.ProtectionTools).Distinct().ToArray());
			progress?.Add();
			foreach(var employeeGroup in operations.GroupBy(x => x.Employee)) {
				progress?.Update($"Обработка {employeeGroup.Key.ShortName}");

				foreach(var operation in employeeGroup.OrderBy(x => x.OperationTime)) {
					var graph = graphs[GetKey(employeeGroup.Key, operation.ProtectionTools)];
					var cardItem = operation.Employee.WorkwearItems
						.FirstOrDefault(x =>
							DomainHelper.EqualDomainObjects(x.ProtectionTools, operation.ProtectionTools));

					operation.RecalculateDatesOfIssueOperation(graph, baseParameters, interactive);
					UoW.Save(operation);
					graph.Refresh();

					if(cardItem != null) {
						cardItem.Graph = graph;
						cardItem.UpdateNextIssue(UoW);
						UoW.Save(cardItem);
					}
					progress?.Add();
				}
			}

			progress?.Add(text: "Завершаем...");
			UoW.Commit();
			progress?.Close();
		}
		#endregion

		#region Graph
		private void CheckAndPrepareGraphs(EmployeeCard[] employees = null, ProtectionTools[] protectionTools = null) {
			if((!employees?.Any() ?? false) && (!protectionTools?.Any() ?? false))
				throw new ArgumentNullException(nameof(employees), $"{nameof(employees)} или {nameof(protectionTools)} должны быть переданы.");

			var operations = employeeIssueRepository.AllOperationsFor(employees, protectionTools);
			foreach(var employeeGroup in operations.GroupBy(o => o.Employee)) {
				foreach(var protectionToolsGroup in employeeGroup.GroupBy(o => o.ProtectionTools)) {
					graphs[GetKey(employeeGroup.Key, protectionToolsGroup.Key)] = new IssueGraph(protectionToolsGroup.ToList());
				}
			}
		}

		private static string GetKey(EmployeeCard e, ProtectionTools p) => $"{e.Id}_{p.Id}";
		#endregion
	}
}
