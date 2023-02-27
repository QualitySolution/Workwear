using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		
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

		/// <summary>
		/// Выполняет пересчет всех даты следующих выдач для всех сотрудников.
		/// </summary>
		/// <param name="progress">Можно предать начатый прогресс, количество шагов прогресса равно количеству сотрудников + 1</param>
		public void UpdateNextIssueAll(EmployeeCard[] employees, IProgressBarDisplayable progress = null, CancellationToken? cancellation = null, uint commitBatchSize = 1000, Action<EmployeeCard, string[]> changeLog = null) {
			bool needClose = false;
			IUnitOfWork uow = unitOfWorkProvider.UoW;
			if(progress != null && !progress.IsStarted) {
				progress.Start(employees.Length + 1);
				needClose = true;
			}
			progress.Add(text: "Получаем информацию о прошлых выдачах");
			CheckAndPrepareGraphs(employees);

			int step = 0;
			foreach(var employee in employees) {
				if(cancellation?.IsCancellationRequested ?? false) {
					break;
				}
				progress.Add(text: $"Обработка {employee.ShortName}");
				step++;
				var oldDates = employee.WorkwearItems.Select(x => x.NextIssue).ToArray();
				
				foreach(var wearItem in employee.WorkwearItems) {
					wearItem.Graph = GetPreparedOrEmptyGraph(employee, wearItem.ProtectionTools);
					wearItem.UpdateNextIssue(uow);
				}
				
				var changes = employee.WorkwearItems.Select((x, i) => x.NextIssue?.Date != oldDates[i]?.Date ? $"Изменена дата следующей выдачи с {oldDates[i]:d} на {x.NextIssue:d} для потребности [{x.Title}]" : null)
					.Where(x => x != null).ToArray();
				changeLog?.Invoke(employee, changes);
				if(changes.Any())
					uow.Save(employee);
				if(step % commitBatchSize == 0)
					uow.Commit();
			}
			progress.Add(text: "Готово");
			if(needClose)
				progress.Close();
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

		private IssueGraph GetPreparedOrEmptyGraph(EmployeeCard employee, ProtectionTools protectionTools) {
			var key = GetKey(employee, protectionTools);
			if(graphs.TryGetValue(key, out IssueGraph graph))
				return graph;
			return new IssueGraph(new List<EmployeeIssueOperation>());
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
