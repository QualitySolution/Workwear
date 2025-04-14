using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Tools;

namespace Workwear.ViewModels.Stock
{
	public class EmployeeIssueGraphViewModel : DialogViewModelBase, IDialogDocumentation
    {
        public IList<GraphInterval> Intervals { get; }
        public EmployeeIssueGraphViewModel(
            INavigationManager navigation,
            IUnitOfWorkFactory factory, 
            EmployeeCard employee, 
            ProtectionTools protectionTools) : base(navigation)
        {
            using (var unitOfWork = factory.CreateWithoutRoot())
                Intervals = IssueGraph.MakeIssueGraph(unitOfWork, employee, protectionTools).Intervals;
            Title = $"Хронология {employee.ShortName} - {protectionTools.Name}";
        }
        
        #region IDialogDocumentation
        public string DocumentationUrl => DocHelper.GetDocUrl("employees.html#issue");
        public string ButtonTooltip => DocHelper.GetDialogDocTooltip("Хронология по потребности");
        #endregion
    }
}
