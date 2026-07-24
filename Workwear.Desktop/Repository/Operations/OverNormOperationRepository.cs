using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;

namespace Workwear.Repository.Operations {
	public class OverNormOperationRepository {
		private readonly UnitOfWorkProvider unitOfWorkProvider;
		private IUnitOfWork repoUow;

		[Obsolete("Лучше используйте конструктор с провайдером Uow")]
		public OverNormOperationRepository(IUnitOfWork uow)
		{
			RepoUow = uow;
		}

		public OverNormOperationRepository(UnitOfWorkProvider unitOfWorkProvider = null) {
			this.unitOfWorkProvider = unitOfWorkProvider;
		}

		public IUnitOfWork RepoUow {
			get => repoUow ?? unitOfWorkProvider.UoW;
			set => repoUow = value;
		}

		public virtual OverNormOperation GetIssuedOperation(int operationId, IUnitOfWork uow = null)
		{
			BarcodeOperation barcodeOperationAlias = null;
			return (uow ?? RepoUow).Session.QueryOver<OverNormOperation>()
				.Where(x => x.Id == operationId)
				.Fetch(SelectMode.Fetch, x => x.Employee)
				.Fetch(SelectMode.Fetch, x => x.Nomenclature)
				.Fetch(SelectMode.Fetch, x => x.WearSize)
				.Fetch(SelectMode.Fetch, x => x.Height)
				.Fetch(SelectMode.Fetch, x => x.WarehouseOperation)
				.Left.JoinAlias(x => x.BarcodeOperations, () => barcodeOperationAlias)
				.Fetch(SelectMode.Fetch, x => x.BarcodeOperations)
				.List()
				.Distinct()
				.SingleOrDefault();
		}

		public virtual IList<OverNormOperation> GetIssuedOperations(IEnumerable<int> operationIds, IUnitOfWork uow = null)
		{
			var ids = operationIds?.Distinct().ToArray() ?? new int[0];
			if(!ids.Any())
				return new List<OverNormOperation>();

			return (uow ?? RepoUow).Session.QueryOver<OverNormOperation>()
				.Where(x => x.Id.IsIn(ids))
				.Fetch(SelectMode.Fetch, x => x.Employee)
				.Fetch(SelectMode.Fetch, x => x.Nomenclature)
				.Fetch(SelectMode.Fetch, x => x.WearSize)
				.Fetch(SelectMode.Fetch, x => x.Height)
				.Fetch(SelectMode.Fetch, x => x.WarehouseOperation)
				.List();
		}

		public virtual IList<int> GetAvailableBarcodeIdsForReturn(OverNormOperation operation, IUnitOfWork uow = null)
		{
			var issuedBarcodeIds = operation.BarcodeOperations
				.Select(x => x.Barcode.Id)
				.Where(x => x > 0)
				.Distinct()
				.ToArray();
			if(!issuedBarcodeIds.Any())
				return new List<int>();

			OverNormOperation returnOperationAlias = null;
			BarcodeOperation barcodeOperationAlias = null;
			var returnedBarcodeIds = new HashSet<int>((uow ?? RepoUow).Session.QueryOver(() => returnOperationAlias)
				.JoinAlias(() => returnOperationAlias.BarcodeOperations, () => barcodeOperationAlias)
				.Where(() => returnOperationAlias.ReturnFromOperation.Id == operation.Id)
				.Select(Projections.Property(() => barcodeOperationAlias.Barcode.Id))
				.List<int>());

			return issuedBarcodeIds
				.Where(x => !returnedBarcodeIds.Contains(x))
				.ToList();
		}

		public virtual Dictionary<int, int> GetReturnedAmounts(
			IEnumerable<OverNormOperation> operations,
			IEnumerable<int> excludedReturnOperationIds = null,
			IUnitOfWork uow = null)
		{
			var operationIds = operations
				.Where(x => x?.Id > 0)
				.Select(x => x.Id)
				.Distinct()
				.ToArray();
			if(!operationIds.Any())
				return new Dictionary<int, int>();

			var excludedIds = excludedReturnOperationIds?.ToArray() ?? new int[0];
			OverNormOperation returnOperationAlias = null;
			var returnOperations = (uow ?? RepoUow).Session.QueryOver(() => returnOperationAlias)
				.WhereRestrictionOn(() => returnOperationAlias.ReturnFromOperation.Id)
				.IsIn(operationIds)
				.Fetch(SelectMode.Fetch, x => x.WarehouseOperation)
				.List();

			return returnOperations
				.Where(x => x.WarehouseOperation != null)
				.Where(x => !excludedIds.Contains(x.Id))
				.GroupBy(x => x.ReturnFromOperation.Id)
				.ToDictionary(x => x.Key, x => x.Sum(o => o.WarehouseOperation.Amount));
		}

		public virtual OverNormOperation GetActualIssuedOperation(ServiceClaim claim, IUnitOfWork uow = null) {
			OverNormOperation overNormOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;
			BarcodeOperation barcodeOperationAlias = null;

			var operations = (uow ?? RepoUow).Session.QueryOver(() => overNormOperationAlias)
				.JoinAlias(() => overNormOperationAlias.WarehouseOperation, () => warehouseOperationAlias)
				.JoinAlias(() => overNormOperationAlias.BarcodeOperations, () => barcodeOperationAlias)
				.Where(() => barcodeOperationAlias.Barcode.Id == claim.Barcode.Id)
				.Where(() => warehouseOperationAlias.ExpenseWarehouse != null)
				.Where(() => overNormOperationAlias.ReturnFromOperation == null)
				.Fetch(SelectMode.Fetch, x => x.Employee)
				.Fetch(SelectMode.Fetch, x => x.Nomenclature)
				.Fetch(SelectMode.Fetch, x => x.WearSize)
				.Fetch(SelectMode.Fetch, x => x.Height)
				.Fetch(SelectMode.Fetch, x => x.WarehouseOperation)
				.OrderBy(() => overNormOperationAlias.OperationTime).Desc
				.List();

			return operations.FirstOrDefault(x => !HasReturnOperation(x, uow));
		}

		public virtual bool HasReturnOperation(OverNormOperation operation, IUnitOfWork uow = null) =>
			(uow ?? RepoUow).Session.QueryOver<OverNormOperation>()
				.Where(x => x.ReturnFromOperation.Id == operation.Id)
				.RowCount() > 0;
	}
}
