using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.Repository.Stock {
	public class BarcodeRepository {
		private readonly UnitOfWorkProvider unitOfWorkProvider;

		public BarcodeRepository(UnitOfWorkProvider unitOfWorkProvider) {
			this.unitOfWorkProvider = unitOfWorkProvider;
		}
		
		public Barcode GetBarcodeByString(string barcode) {
			return unitOfWorkProvider.UoW.Session.QueryOver<Barcode>()
				.Where(x => x.Title == barcode)
				.Take(1)
				.SingleOrDefault();
		}

		public EmployeeCard GetLastEmployeeFor(Barcode barcode) {
			var result = unitOfWorkProvider.UoW.Session.Query<BarcodeOperation>()
				.Where(x => x.Barcode.Id == barcode.Id)
				.Select(x => new {
					EmployeeIssueEmployee = x.EmployeeIssueOperation.Employee,
					OverNormEmployee = x.OverNormOperation.Employee,
					DutyNormEmployee = x.DutyNormIssueOperation.DutyNorm.ResponsibleEmployee,
					Date = (DateTime?)x.EmployeeIssueOperation.OperationTime
					       ?? (DateTime?)x.OverNormOperation.OperationTime
					       ?? (DateTime?)x.DutyNormIssueOperation.OperationTime
				})
				.OrderByDescending(x => x.Date)
				.FirstOrDefault();

			return result?.EmployeeIssueEmployee ?? result?.OverNormEmployee ?? result?.DutyNormEmployee;
		}

		
		public ServiceClaim GetActiveServiceClaimFor(Barcode barcode) {
			return unitOfWorkProvider.UoW.Session.QueryOver<ServiceClaim>()
				.Where(x => x.Barcode.Id == barcode.Id)
				.Where(x => x.IsClosed == false)
				.Take(1)
				.SingleOrDefault<ServiceClaim>();
		}

		public IList<BarcodeOperation> GetBarcodeOperationsByEmployeeIssueOperations(int[] employeeIssueOperationsIds, IUnitOfWork uow) {
			var barcodeOperations = uow.Session.Query<BarcodeOperation>()
				.Where(x => employeeIssueOperationsIds.Contains(x.EmployeeIssueOperation.Id))
				.ToList();
			return barcodeOperations;
		}
	}
}
