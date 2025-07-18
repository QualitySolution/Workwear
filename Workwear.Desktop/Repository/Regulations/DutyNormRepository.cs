using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.Repository.Regulations {
	public class DutyNormRepository {
		private UnitOfWorkProvider unitOfWorkProvider;
		private IUnitOfWork RepoUoW => unitOfWorkProvider?.UoW;
		
		public DutyNormRepository(UnitOfWorkProvider unitOfWorkProvider) {
			this.unitOfWorkProvider = unitOfWorkProvider;
		}
		public Dictionary<int, int> CalculateWrittenOff(DutyNormIssueOperation[] operations, IUnitOfWork uow, DateTime? onDate = null) {
			var wo = uow.Session.QueryOver<DutyNormIssueOperation>()
				.Where(o => o.IssuedOperation.Id
					.IsIn(operations.Select(x => x.Id).ToArray()));
			if(onDate != null)
				wo.Where(o => o.OperationTime <= onDate);
			
			return wo.List()
				?.GroupBy(o => o.Id)
				.ToDictionary(g => g.Key, g => g.Sum(o => o.Returned));
		}
		
		public void LoadFullInfo(int[] dNormsId,  IUnitOfWork uow = null) {
			
			var itemsTypes = (uow ?? RepoUoW).Session.QueryOver<ItemsType>()
				.Future();
			var protectionTools = (uow ?? RepoUoW).Session.QueryOver<ProtectionTools>()
				.Future();
			
			var query = (uow ?? RepoUoW).Session.QueryOver<DutyNorm>()
				.Where(x => x.Id.IsIn(dNormsId))
				.Fetch(SelectMode.Fetch, x => x.Subdivision)
				.Fetch(SelectMode.Fetch, x => x.ResponsibleEmployee)
				.Fetch(SelectMode.Fetch, x => x.ResponsibleLeader);
			
			DutyNormItem dutyNormItemAlias = null;
			ProtectionTools protectionToolsAlias = null;
			ItemsType itemsTypeAlias = null;
			query
				.JoinAlias(x => x.Items, () => dutyNormItemAlias)	
				.JoinAlias(() => dutyNormItemAlias.ProtectionTools, () => protectionToolsAlias)
				.JoinAlias(() => protectionToolsAlias.Type, () => itemsTypeAlias);
		
			query.List();
		}

		/// <summary>
		/// Получаем все выдачи для списка строк норм.
		/// </summary>
		public IList<DutyNormIssueOperation> AllIssueOperationForItems(DutyNormItem[] items, IUnitOfWork uow = null) {
			var itemsId = items?
				.Select(i => i?.Id)      // Допускаем null
				.Where(id => id != null && id != 0)  // Игнорируем null и 0
				.Select(id => id.Value)  // Берем значение (т.к. null уже отфильтрован)
				.ToArray() ?? Array.Empty<int>();
			
			return (uow ?? RepoUoW).Session.QueryOver<DutyNormIssueOperation>()
				.Fetch(SelectMode.Fetch, x => x.ProtectionTools)
				.Fetch(SelectMode.Fetch, x => x.ProtectionTools.Type)
				.Where(x => x.DutyNormItem.IsIn(itemsId))
				.List();
		}
		
		/// <summary>
		/// Получаем все строки норм по фильтрам
		/// </summary>
		public virtual IList<DutyNormItem> AllItemsFor(
			int[] dutyNormsIds = null,
			int[] responsibleemployeesIds = null,
			int[] protectionToolsIds = null,
			IUnitOfWork uow = null)
		{
			DutyNorm dutyNormAlias = null;
			EmployeeCard responsibleEmployeeAlias = null;
			
			var query = (uow ?? RepoUoW).Session.QueryOver<DutyNormItem>();
			if(dutyNormsIds != null && dutyNormsIds.Any())
				query.Where(o => o.DutyNorm.Id.IsIn(dutyNormsIds));
			if(protectionToolsIds != null && protectionToolsIds.Any())
				query.Where(o => o.ProtectionTools.Id.IsIn(protectionToolsIds));
			if(responsibleemployeesIds != null && responsibleemployeesIds.Any()) { 
				query.JoinAlias(x => x.DutyNorm, () => dutyNormAlias)
				.JoinAlias(() => dutyNormAlias.ResponsibleEmployee, () => responsibleEmployeeAlias)
				.Where(() => responsibleEmployeeAlias.Id.IsIn(responsibleemployeesIds));
			}

			var res = query.List();
			return res;
		}
	}
}
