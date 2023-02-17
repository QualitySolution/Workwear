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
		private readonly UnitOfWorkProvider unitOfWorkProvider;
		private Dictionary<string, IssueGraph> graphs = new Dictionary<string, IssueGraph>();
		
		public EmployeeIssueModel(EmployeeIssueRepository employeeIssueRepository, UnitOfWorkProvider unitOfWorkProvider = null) {
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			this.unitOfWorkProvider = unitOfWorkProvider;
		}

		#region public
		public void RecalculateDateOfIssue(IList<EmployeeIssueOperation> operations, BaseParameters baseParameters, IInteractiveQuestion interactive, IUnitOfWork uow = null, IProgressBarDisplayable progress = null) {
			uow = uow ?? unitOfWorkProvider.UoW;
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
					uow.Save(operation);
					graph.Refresh();

					if(cardItem != null) {
						cardItem.Graph = graph;
						cardItem.UpdateNextIssue(uow);
						uow.Save(cardItem);
					}
					progress?.Add();
				}
			}

			progress?.Add(text: "Завершаем...");
			uow.Commit();
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

		#region Employees
		public void FillWearReceivedInfo(EmployeeCard[] employees, EmployeeIssueOperation[] notSavedOperations = null) {
			if(!employees.Any())
				return;
			var operations = employeeIssueRepository.AllOperationsFor(employees).ToList();
			if(notSavedOperations != null)
				operations.AddRange(notSavedOperations);
			foreach(var employee in employees) {
				employee.FillWearReceivedInfo(operations.Where(x => x.Employee.IsSame(employee)).ToList());
			}
		}
		#endregion
	}
}
