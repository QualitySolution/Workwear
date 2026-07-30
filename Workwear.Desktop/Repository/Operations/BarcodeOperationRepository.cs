using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;

namespace Workwear.Repository.Operations {
	public class BarcodeOperationRepository {
		private readonly UnitOfWorkProvider unitOfWorkProvider;
		private IUnitOfWork repoUow;

		[Obsolete("Лучше используйте конструктор с провайдером Uow")]
		public BarcodeOperationRepository(IUnitOfWork uow)
		{
			RepoUow = uow;
		}

		public BarcodeOperationRepository(UnitOfWorkProvider unitOfWorkProvider = null) {
			this.unitOfWorkProvider = unitOfWorkProvider;
		}

		public IUnitOfWork RepoUow {
			get => repoUow ?? unitOfWorkProvider.UoW;
			set => repoUow = value;
		}

		public virtual IList<int> GetAvailableBarcodeIdsForReturn(EmployeeIssueOperation operation, IUnitOfWork uow = null)
		{
			var issuedBarcodeIds = GetOperationBarcodeIds(operation?.BarcodeOperations);
			if(!issuedBarcodeIds.Any())
				return new List<int>();

			EmployeeIssueOperation returnOperationAlias = null;
			BarcodeOperation barcodeOperationAlias = null;
			var returnedBarcodeIds = new HashSet<int>((uow ?? RepoUow).Session.QueryOver(() => barcodeOperationAlias)
				.JoinAlias(() => barcodeOperationAlias.EmployeeIssueOperation, () => returnOperationAlias)
				.Where(() => returnOperationAlias.IssuedOperation.Id == operation.Id)
				.Select(Projections.Property(() => barcodeOperationAlias.Barcode.Id))
				.List<int>());

			return issuedBarcodeIds
				.Where(x => !returnedBarcodeIds.Contains(x))
				.ToList();
		}

		public virtual IList<int> GetAvailableBarcodeIdsForReturn(DutyNormIssueOperation operation, IUnitOfWork uow = null)
		{
			var issuedBarcodeIds = GetOperationBarcodeIds(operation?.BarcodeOperations);
			if(!issuedBarcodeIds.Any())
				return new List<int>();

			DutyNormIssueOperation returnOperationAlias = null;
			BarcodeOperation barcodeOperationAlias = null;
			var returnedBarcodeIds = new HashSet<int>((uow ?? RepoUow).Session.QueryOver(() => barcodeOperationAlias)
				.JoinAlias(() => barcodeOperationAlias.DutyNormIssueOperation, () => returnOperationAlias)
				.Where(() => returnOperationAlias.IssuedOperation.Id == operation.Id)
				.Select(Projections.Property(() => barcodeOperationAlias.Barcode.Id))
				.List<int>());

			return issuedBarcodeIds
				.Where(x => !returnedBarcodeIds.Contains(x))
				.ToList();
		}

		public virtual IList<int> GetAvailableBarcodeIdsForReturn(OverNormOperation operation, IUnitOfWork uow = null)
		{
			var issuedBarcodeIds = GetOperationBarcodeIds(operation?.BarcodeOperations);
			if(!issuedBarcodeIds.Any())
				return new List<int>();

			OverNormOperation returnOperationAlias = null;
			BarcodeOperation barcodeOperationAlias = null;
			var returnedBarcodeIds = new HashSet<int>((uow ?? RepoUow).Session.QueryOver(() => barcodeOperationAlias)
				.JoinAlias(() => barcodeOperationAlias.OverNormOperation, () => returnOperationAlias)
				.Where(() => returnOperationAlias.ReturnFromOperation.Id == operation.Id)
				.Select(Projections.Property(() => barcodeOperationAlias.Barcode.Id))
				.List<int>());

			return issuedBarcodeIds
				.Where(x => !returnedBarcodeIds.Contains(x))
				.ToList();
		}

		public virtual IList<Barcode> GetBarcodes(IEnumerable<int> barcodeIds, IUnitOfWork uow = null)
		{
			var ids = barcodeIds?.Distinct().ToArray() ?? new int[0];
			if(!ids.Any())
				return new List<Barcode>();

			return (uow ?? RepoUow).Session.QueryOver<Barcode>()
				.WhereRestrictionOn(x => x.Id).IsIn(ids)
				.List();
		}

		private static IList<int> GetOperationBarcodeIds(IEnumerable<BarcodeOperation> barcodeOperations) =>
			barcodeOperations?
				.Select(x => x.Barcode?.Id ?? 0)
				.Where(x => x > 0)
				.Distinct()
				.ToList()
			?? new List<int>();
	}
}
