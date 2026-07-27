using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.ClothingService;
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
		
		/// <summary>
		/// Заполнение связанных объектов
		/// </summary>
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
				.Fetch(SelectMode.Fetch, x => x.Nomenclature)
				.Fetch(SelectMode.Fetch, x => x.Nomenclature.Type)
				.Fetch(SelectMode.Fetch, x => x.WearSize)
				.Fetch(SelectMode.Fetch, x => x.Height)
				.Where(x => x.DutyNormItem.IsIn(itemsId))
				.List();
		}
		
		/// <summary>
		/// Ищем действующие операции выдачи, на которые ссылаются заявки на обслуживание.
		/// </summary>
		public virtual Dictionary<int, DutyNormIssueOperation> GetActualIssuedOperations(IEnumerable<ServiceClaim> claims, IUnitOfWork uow = null) {
			var claimsList = claims.Where(c => c.Barcode != null).ToList();
			var barcodeIds = claimsList.Select(c => c.Barcode.Id).Distinct().ToArray();
			if(!barcodeIds.Any())
				return new Dictionary<int, DutyNormIssueOperation>();

			DutyNormIssueOperation issueAlias = null;
			BarcodeOperation issuedBarcodeAlias = null;

			var operations = (uow ?? RepoUoW).Session.QueryOver(() => issueAlias)
				.JoinAlias(() => issueAlias.BarcodeOperations, () => issuedBarcodeAlias)
				.WhereRestrictionOn(() => issuedBarcodeAlias.Barcode.Id).IsIn(barcodeIds)
				.Where(() => issueAlias.Issued > 0)
				.Fetch(SelectMode.Fetch, x => x.DutyNorm)
				.Fetch(SelectMode.Fetch, x => x.ProtectionTools)
				.Fetch(SelectMode.Fetch, x => x.Nomenclature)
				.Fetch(SelectMode.Fetch, x => x.WearSize)
				.Fetch(SelectMode.Fetch, x => x.Height)
				.Fetch(SelectMode.Fetch, x => x.WarehouseOperation)
				.Fetch(SelectMode.Fetch, x => x.BarcodeOperations)
				.OrderBy(() => issueAlias.OperationTime).Desc
				.List()
				.Distinct()
				.ToList();

			DutyNormIssueOperation resultIssueAlias = null;
			BarcodeOperation resultBarcodeAlias = null;
			var resultPairs = new HashSet<(int OperationId, int BarcodeId)>(
				(uow ?? RepoUoW).Session.QueryOver(() => resultBarcodeAlias)
					.JoinAlias(() => resultBarcodeAlias.DutyNormIssueOperation, () => resultIssueAlias)
					.WhereRestrictionOn(() => resultBarcodeAlias.Barcode.Id).IsIn(barcodeIds)
					.Where(() => resultIssueAlias.IssuedOperation != null)
					.Fetch(SelectMode.Fetch, x => x.Barcode)
					.Fetch(SelectMode.Fetch, x => x.DutyNormIssueOperation.IssuedOperation)
					.List()
					.Select(bo => (bo.DutyNormIssueOperation.IssuedOperation.Id, bo.Barcode.Id)));

			var result = new Dictionary<int, DutyNormIssueOperation>();
			foreach(var claim in claimsList) {
				var operation = operations
					.Where(op => op.BarcodeOperations.Any(bo => bo.Barcode.Id == claim.Barcode.Id))
					.Where(op => !resultPairs.Contains((op.Id, claim.Barcode.Id)))
					.OrderByDescending(op => op.OperationTime)
					.FirstOrDefault();
				if(operation != null)
					result[claim.Id] = operation;
			}
			return result;
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
			
			var query = (uow ?? RepoUoW).Session.QueryOver<DutyNormItem>()
				.Fetch(SelectMode.Fetch, x => x.DutyNorm);
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
