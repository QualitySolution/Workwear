using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using Workwear.Measurements;

namespace workwear.ReportParameters.ViewModels
{
	public class ListBySizeViewModel : ReportParametersViewModelBase
	{
		public ListBySizeViewModel(RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel)
		{
			Title = "Список по размерам";
			Identifier = "ListBySize";
		}
		protected override Dictionary<string, object> Parameters => SetParameters();
		private Dictionary<string, object> SetParameters() {
			var parameters = new Dictionary<string, object>();
			using var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();
			var sizes = new SizeService().GetSizeType(unitOfWork, onlyUseInEmployee: true).Take(6).ToList();
			for (var count = 0; count < sizes.Count; count++) {
				parameters.Add($"type_id_{count}", sizes[count].Id);
				parameters.Add($"type_name_{count}", sizes[count].Name);
			}
			return parameters;
		}
	}
}
