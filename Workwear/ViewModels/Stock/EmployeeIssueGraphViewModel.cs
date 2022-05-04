using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Regulations;

namespace workwear.ViewModels.Stock
{
	public class EmployeeIssueGraphViewModel : WindowDialogViewModelBase
    {
        public IList<GraphInterval> Intervals { get; }
        public EmployeeIssueGraphViewModel(
            INavigationManager navigation, 
            IUnitOfWorkFactory factory, 
            EmployeeCard employee, 
            ProtectionTools protectionTools) : base(navigation)
        {
            var unitOfWork = factory.CreateWithoutRoot();
            Intervals = IssueGraph.MakeIssueGraph(unitOfWork, employee, protectionTools).Intervals;
			Title = $"Хронология {employee.ShortName} - {protectionTools.Name}";

		}
    }
}