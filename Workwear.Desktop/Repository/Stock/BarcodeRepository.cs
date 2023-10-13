using QS.DomainModel.UoW;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
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
			EmployeeIssueOperation employeeIssueOperationAlias = null;

			return unitOfWorkProvider.UoW.Session.QueryOver<BarcodeOperation>()
				.Where(x => x.Barcode.Id == barcode.Id)
				.JoinAlias(x => x.EmployeeIssueOperation, () => employeeIssueOperationAlias)
				.Select(x => employeeIssueOperationAlias.Employee)
				.Take(1)
				.SingleOrDefault<EmployeeCard>();
		}
		
		public ServiceClaim GetActiveServiceClaimFor(Barcode barcode) {
			return unitOfWorkProvider.UoW.Session.QueryOver<ServiceClaim>()
				.Where(x => x.Barcode.Id == barcode.Id)
				.Where(x => x.IsClosed == false)
				.Take(1)
				.SingleOrDefault<ServiceClaim>();
		}
	}
}
