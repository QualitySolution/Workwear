using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Tools;

namespace Workwear.Models.Analytics {
	public class FutureIssueModel {
		private readonly BaseParameters baseParameters;

		public FutureIssueModel(BaseParameters baseParameters) {
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
		}

		/// <summary>
		/// Метод прогнозирует будущие выдачи по дежурным нормам
		/// </summary>
		/// <param name="startDate">Дата начала периода прогнозирования.</param>
		/// <param name="endDate">Дата окончания периода прогнозирования.</param>
		/// <param name="moveDebt">Если True перемещает все долги на начало периода прогнозирования.</param>
		/// <param name="dutyNormItems">Список строк дежурных норм для прогнозирования.</param>
		/// <param name="progress">Не обязательный аргумент, прогресс выполнения.</param>
		/// <returns></returns>
		public List<FutureIssueDutyNorm> CalculateDutyNormIssues(
			DateTime startDate,
			DateTime endDate,
			bool moveDebt,
			IList<DutyNormItem> dutyNormItems,
			IProgressBarDisplayable progress = null)
		{
			progress?.Start(dutyNormItems.Count + 2);
			int gc = 0;
			var issues = new List<FutureIssueDutyNorm>();
			endDate = endDate.AddDays(1); //Чтобы включить в расчёт последний день периода

			foreach(var item in dutyNormItems) {
				if(gc++ > 10000) {
					GC.Collect();
					GC.WaitForPendingFinalizers();
					gc = 0;
				}

				progress?.Add(text: "Планирование выдач для " + item.DutyNorm.Name + " - " + item.ProtectionTools.Name);

				DateTime? delayIssue = item.NextIssue < startDate ? item.NextIssue : null;
				var virtualOperations = new List<DutyNormIssueOperation>();

				Nomenclature nomenclature = item.ProtectionTools.GetSupplyNomenclature(null);

				item.UpdateNextIssue();
				while(item.NextIssue.HasValue && item.NextIssue < endDate) {
					var issueDate = (!moveDebt || (DateTime)item.NextIssue > startDate) ? (DateTime)item.NextIssue : startDate;
					int need = item.CalculateRequiredIssue(baseParameters, issueDate);
					if(need == 0)
						break;

					var lastIssueDate = item.Graph.GetWrittenOffOperation((DateTime)item.NextIssue)?.OperationTime;

					var op = new DutyNormIssueOperation {
						OperationTime = issueDate,
						StartOfUse = issueDate,
						Issued = need,
						Returned = 0,
						OverrideBefore = false,
						DutyNorm = item.DutyNorm,
						DutyNormItem = item,
						ProtectionTools = item.ProtectionTools,
					};
					op.ExpiryByNorm = item.CalculateExpireDate(op.OperationTime, need);
					op.AutoWriteoffDate = op.ExpiryByNorm;
					virtualOperations.Add(op);

					if(op.OperationTime >= startDate) {
						issues.Add(new FutureIssueDutyNorm {
							DutyNormItem = item,
							OperationDate = op.OperationTime,
							Nomenclature = nomenclature,
							Amount = op.Issued,
							LastIssueDate = lastIssueDate,
							DelayIssueDate = delayIssue,
							VirtualLastIssue = virtualOperations.Any(o => o.OperationTime == lastIssueDate)
						});
						delayIssue = null;
					}

					item.Graph.AddOperations(new List<IGraphIssueOperation> { op });
					item.UpdateNextIssue();
				}
			}

			progress?.Close();
			return issues;
		}

		/// <summary>
		/// Метод прогнозирует будущие выдачи сотрудникам
		/// </summary>
		/// <param name="startDate">Дата начала периода прогнозирования.</param>
		/// <param name="endDate">Дата окончания периода прогнозирования.</param>
		/// <param name="moveDebt">Если True перемещает все долги на начало периода прогнозирования, в этой ситуации все даты выдачи для должников будут сдвинуты на начало периода.</param>
		/// <param name="employeeItems">Список потребностей для прогнозирования.</param>
		/// <param name="progress">Не обязательный аргумент, прогресс выполнения.</param>
		/// <returns></returns>
		public List<FutureIssueEmployee> CalculateIssues(
			DateTime startDate,
			DateTime endDate,
			bool moveDebt,
			IList<EmployeeCardItem> employeeItems,
			IProgressBarDisplayable progress = null)
		{
			progress?.Start(employeeItems.Count() + 2);
			int gc = 0;
			var issues = new List<FutureIssueEmployee>();
			endDate = endDate.AddDays(1); //Чтобы включить в расчёт последний день периода

			foreach(var item in employeeItems) {
				if(gc++ > 10000) {
					GC.Collect();
					GC.WaitForPendingFinalizers();
					gc = 0;
				}

				progress?.Add(text: "Планирование выдач для " + item.EmployeeCard.ShortName);

				DateTime? delayIssue = item.NextIssue < startDate ? item.NextIssue : null;
				//список созданных объектов операций
				var virtualOperations = new List<EmployeeIssueOperation>();

				Nomenclature nomenclature = item.ProtectionTools.GetSupplyNomenclature(item.EmployeeCard?.Sex);

				item.UpdateNextIssue(null);
				while(item.NextIssue.HasValue && item.NextIssue < endDate) {
					//создаём следующую виртуальную выдачу
					var issueDate = (!moveDebt || (DateTime)item.NextIssue > startDate) ? (DateTime)item.NextIssue : startDate;
					int need = item.CalculateRequiredIssue(baseParameters, issueDate, ignoreNormConditionPeriod: true);
					if(need == 0)
						break;
					//Операция приведшая к возникновению потребности
					var lastIssueDate = item.Graph.GetWrittenOffOperation((DateTime)item.NextIssue)?.OperationTime;

					var op = new EmployeeIssueOperation(baseParameters) {
						OperationTime = issueDate,
						StartOfUse = issueDate,
						Issued = need,
						Returned = 0,
						OverrideBefore = false,
						Employee = item.EmployeeCard,
						NormItem = item.ActiveNormItem,
						ProtectionTools = item.ProtectionTools,
					};
					op.ExpiryByNorm = item.ActiveNormItem.CalculateExpireDate(op.OperationTime);
					op.AutoWriteoffDate = op.ExpiryByNorm; //Подстраховка
					virtualOperations.Add(op);

					//Создание строки выгрузки на эту выдачу
					if(op.OperationTime >= startDate) {
						issues.Add(new FutureIssueEmployee {
							EmployeeCardItem = item,
							OperationDate = op.OperationTime,
							Nomenclature = nomenclature,
							Amount = op.Issued,
							LastIssueDate = lastIssueDate,
							DelayIssueDate = delayIssue,
							//TODO протестировать
							VirtualLastIssue = virtualOperations.Any(o => o.OperationTime == lastIssueDate)
						});
						delayIssue = null;
					}

					var resetOperations = item.Graph.Intervals.Where(gi => gi.StartDate == item.NextIssue)
						.Select(ai => ai.ActiveItems)
						.Select(x => x.Select(y => y.IssueOperation).Where(o => o.OverrideBefore));
					if(op.Issued < op.NormItem.Amount && resetOperations.Any())
						foreach(var opR in resetOperations.SelectMany(x => x).Select(y => y))
							if(opR != null)
								opR.Issued = op.NormItem.Amount;

					item.Graph.AddOperations(new List<IGraphIssueOperation> { op });
					item.UpdateNextIssue(null);
				}
			}

			progress?.Close();
			return issues;
		}
	}
}
