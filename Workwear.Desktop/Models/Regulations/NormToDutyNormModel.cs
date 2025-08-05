using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;

namespace Workwear.Models.Regulations {
	public class NormToDutyNormModel {
		private Dictionary<(int employeeId, int normItemId), DutyNormItem> relevantItemsIds = new Dictionary<(int, int), DutyNormItem>();
		
		public virtual void CopyDataFromNorm(int normId, int employeeId) {
			DutyNorm newDutyNorm = new DutyNorm();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Копирование данных из нормы")) {
				var norm = uow.GetById<Norm>(normId);
				var employee = uow.GetById<EmployeeCard>(employeeId);
				var itemIds = norm.Items.Select(i => i.Id).ToArray();
				
				newDutyNorm.Name = norm.Name != null ? $"{norm.Name} ({employee.ShortName})" : $"Дежурная ({employee.ShortName})";
				newDutyNorm.ResponsibleEmployee = employee;
				newDutyNorm.DateFrom = norm.DateFrom;
				newDutyNorm.DateTo = norm.DateTo;
				newDutyNorm.Comment = norm.Comment ?? "";
				newDutyNorm.Subdivision = employee.Subdivision;
				uow.Save(newDutyNorm);
				
				foreach(var item in norm.Items) {
					var employeeCardItem = uow.Session
						.Query<EmployeeCardItem>()
						.Where(x => x.EmployeeCard.Id == employeeId)
						.FirstOrDefault(x => x.ActiveNormItem.Id == item.Id);
					
					var dutyNormItem = CopyNormItem(newDutyNorm, item, employeeCardItem);
					uow.Save(dutyNormItem);
					if(!relevantItemsIds.ContainsKey((employee.Id, item.Id)))
						relevantItemsIds.Add((employee.Id, item.Id), dutyNormItem);
				}
				
				var employeeIssueOperations = GetOperationsForEmployeeWithNormItems(employeeId, itemIds, uow);
				foreach(var op in employeeIssueOperations) {
					DutyNormIssueOperation dutyNormIssueOperation = new DutyNormIssueOperation{
						DutyNorm = newDutyNorm
					};
					СopyEmployeeIssueOperation(op, dutyNormIssueOperation);
					uow.Save(dutyNormIssueOperation);
				}
				uow.Commit();
			}
		}

		public virtual DutyNormItem CopyNormItem(DutyNorm newDutyNorm, NormItem normItem, EmployeeCardItem employeeCardItem) {
			DutyNormItem newDutyNormItem = new DutyNormItem();
			
			newDutyNormItem.DutyNorm = newDutyNorm;
			newDutyNormItem.ProtectionTools = normItem.ProtectionTools;
			newDutyNormItem.Amount = normItem.Amount;
			
			switch(normItem.NormPeriod) {
				case NormPeriodType.Year:
					newDutyNormItem.NormPeriod = DutyNormPeriodType.Year; 
				break;
				case NormPeriodType.Month:
					newDutyNormItem.NormPeriod = DutyNormPeriodType.Month;
					break;
				case NormPeriodType.Wearout:
				case NormPeriodType.Shift:
				case NormPeriodType.Duty:
					newDutyNormItem.NormPeriod = DutyNormPeriodType.Wearout;
				break;
			}
			
			newDutyNormItem.PeriodCount = normItem.PeriodCount;
			newDutyNormItem.NormParagraph = normItem.NormParagraph;
			newDutyNormItem.Comment = normItem.Comment;
			newDutyNormItem.Graph = new IssueGraph();
			newDutyNormItem.NextIssue = employeeCardItem.NextIssue;
			
			return newDutyNormItem;
		}
		
		public virtual void СopyEmployeeIssueOperation (EmployeeIssueOperation issueOperation, DutyNormIssueOperation dutyNormIssueOperation) 
		{
			dutyNormIssueOperation.DutyNormItem = relevantItemsIds[(dutyNormIssueOperation.DutyNorm.ResponsibleEmployee.Id, issueOperation.NormItem.Id)];
			dutyNormIssueOperation.OperationTime = issueOperation.OperationTime;
			dutyNormIssueOperation.Nomenclature = issueOperation.Nomenclature;
			dutyNormIssueOperation.ProtectionTools = issueOperation.ProtectionTools;
			dutyNormIssueOperation.WearPercent = issueOperation.WearPercent;
			dutyNormIssueOperation.AutoWriteoffDate = issueOperation.AutoWriteoffDate;
			dutyNormIssueOperation.Issued = issueOperation.Issued;
			dutyNormIssueOperation.WarehouseOperation = issueOperation.WarehouseOperation;
		}

		public IList<EmployeeIssueOperation> GetOperationsForEmployeeWithNormItems(int employeeId, int[] normItemsIds, IUnitOfWork uow) {
			var query = uow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.NormItem.Id.IsIn(normItemsIds))
				.Where(o => o.Employee.Id == employeeId)
				.Where(o => o.Issued > 0);
			return query.List();

		}
	}
}
