﻿using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Tools;

namespace Workwear.Models.Analytics {
	public class FutureIssueModel {
		private readonly BaseParameters baseParameters;

		public FutureIssueModel(BaseParameters baseParameters) {
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
		}

		/// <summary>
		/// Метод прогнозирует будущие выдачи
		/// </summary>
		/// <param name="startDate">Дата начала периода прогнозирования.</param>
		/// <param name="endDate">Дата окончания периода прогнозирования.</param>
		/// <param name="moveDebt">Если True перемещает все долги на начала периода прогнозирования, в этой ситуации все даты выдачи для должников будут сдвинуты, на начало периода.</param>
		/// <param name="employeeItems">Список потребностей для прогнозирования.</param>
		/// <param name="progress">Не обязательный аргумент, прогресс выполнения.</param>
		/// <returns></returns>
		public List<FutureIssue> CalculateIssues(
			DateTime startDate,
			DateTime endDate,
			bool moveDebt,
			IList<EmployeeCardItem> employeeItems,
			IProgressBarDisplayable progress = null) 
		{
			progress?.Start(employeeItems.Count() + 2);
			int gc = 0;
			var issues = new List<FutureIssue>();

			foreach(var item in employeeItems) {
				if(gc++ > 10000) {
					GC.Collect();
					GC.WaitForPendingFinalizers();
					gc = 0;
				}

				progress?.Add(text: "Планирование выдач для " + item.EmployeeCard.ShortName);
				if(item.Id == 52668)
					Console.WriteLine("Debug");

				DateTime? delayIssue = item.NextIssue < startDate ? item.NextIssue : null;
				//список созданных объектов операций
				List<EmployeeIssueOperation> virtualOperations = new List<EmployeeIssueOperation>();

				Nomenclature nomenclature = item.ProtectionTools.GetSupplyNomenclature(item.EmployeeCard?.Sex);

				item.UpdateNextIssue(null);
				while(item.NextIssue.HasValue && item.NextIssue < endDate) {
					//создаём следующую виртуальную выдачу
					var issueDate = (!moveDebt || (DateTime)item.NextIssue > startDate) ? (DateTime)item.NextIssue : startDate;
					int need = item.CalculateRequiredIssue(baseParameters, issueDate);
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
						issues.Add(new FutureIssue() {
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
	
	/// <summary>
	/// Будущая выдача 
	/// </summary>
	public class FutureIssue {
		public EmployeeCardItem EmployeeCardItem { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public DateTime ? LastIssueDate { get; set; }
		public bool VirtualLastIssue { get; set; }

		public EmployeeCard Employee => EmployeeCardItem.EmployeeCard;
		public Subdivision Subdivision => Employee.Subdivision;
		public Department Department => Employee.Department;
		public Post Post => Employee.Post;	
		public ProtectionTools ProtectionTools => EmployeeCardItem.ProtectionTools;
		public NormItem NormItem => EmployeeCardItem.ActiveNormItem;
		public Norm Norm => NormItem.Norm;

		public Size Size =>
			Employee.Sizes.FirstOrDefault(s => DomainHelper.EqualDomainObjects(s.SizeType, ProtectionTools.Type.SizeType))?.Size;
		public Size Height =>
			Employee.Sizes.FirstOrDefault(s => DomainHelper.EqualDomainObjects(s.SizeType, ProtectionTools.Type.HeightType))?.Size;

		/// <summary>
		/// Дата планируемой выдачи
		/// </summary>
		public DateTime ? OperationDate { get; set; }
		public DateTime ? DelayIssueDate { get; set; }
		public int Amount { get; set; }
	}
}
