using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using Workwear.Tools.Sizes;
using QS.Report;
using Gamma.Utilities;

namespace workwear.ReportParameters.ViewModels
{
	public class ListBySizeViewModel : ReportParametersViewModelBase
	{
		private readonly SizeService sizeService;

		public ListBySizeViewModel(RdlViewerViewModel rdlViewerViewModel, SizeService sizeService) : base(rdlViewerViewModel)
		{
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			Title = "Список по размерам";
		}
		protected override Dictionary<string, object> Parameters => SetParameters();
		private Dictionary<string, object> SetParameters() {
			var parameters = new Dictionary<string, object>();
			using (var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot()) {
				var sizes = sizeService.GetSizeType(unitOfWork, onlyUseInEmployee: true).Take(6).ToList();
				parameters.Add($"group_by_subdivision", GroupBySubdivision);
				for (var count = 0; count < sizes.Count; count++)
				{
					parameters.Add($"type_id_{count}", sizes[count].Id);
					parameters.Add($"type_name_{count}", sizes[count].Name);
				}
				
				var sizesData = sizeService.GetSizeType(unitOfWork, onlyUseInEmployee: false).Take(12).ToList();
				for(var count = 0; count < sizesData.Count; count++) {
					parameters.Add($"size_id_{count}", sizesData[count].Id);
					parameters.Add($"size_name_{count}", sizesData[count].Name);
				}
			}
			return parameters;
		}
		
		
		private bool groupBySubdivision = false;
		public virtual bool GroupBySubdivision {
			get => groupBySubdivision;
			set => SetField(ref groupBySubdivision, value);
		}
		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}
		public bool VisibleShowGroup => ReportType == SizeReportType.Common;
		private SizeReportType reportType;
		public virtual SizeReportType ReportType {
			get => reportType;
			set {
				SetField(ref reportType, value); 
				OnPropertyChanged(nameof(VisibleShowGroup));
			}
		}
		
		public enum SizeReportType {
			[ReportIdentifier("ListBySize")]
			[Display(Name = "Форматировано")]
			Common,
			[ReportIdentifier("SpravkaEmployeeSizes")]
			[Display(Name = "Только данные")]
			Flat
		}
	}
}
