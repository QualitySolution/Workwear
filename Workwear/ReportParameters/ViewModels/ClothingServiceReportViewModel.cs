using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using Workwear.Tools.Sizes;

namespace Workwear.ReportParameters.ViewModels {
	public class ClothingServiceReportViewModel: ReportParametersViewModelBase {
		private readonly SizeService sizeService;
		
		public ClothingServiceReportViewModel(RdlViewerViewModel rdlViewerViewModel, SizeService sizeService) : base(rdlViewerViewModel)
		{
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			Title = "Обслуживание одежды";
			Identifier = "ClothingServiceReport";
		}
		protected override Dictionary<string, object> Parameters => SetParameters();
		
		private Dictionary<string, object> SetParameters() {
			var parameters = new Dictionary<string, object>();
			using (var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot()) {
				var sizes = sizeService.GetSizeType(unitOfWork, onlyUseInEmployee: true).Take(6).ToList();
				parameters.Add($"show_closed", showClosed);
			}
			return parameters;
		}
		
		private bool showClosed = false;
		public virtual bool ShowClosed {
			get => showClosed;
			set => SetField(ref showClosed, value);
		}
	}
	
}
