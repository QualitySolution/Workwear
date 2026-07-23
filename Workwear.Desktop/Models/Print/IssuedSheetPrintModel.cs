using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Gamma.Utilities;
using QS.Report;
using Workwear.Domain.Statements;
using Workwear.Tools;
using Workwear.Tools.Features;

namespace Workwear.Models.Print {
	public class IssuedSheetPrintModel {
		private readonly BaseParameters baseParameters;
		private readonly FeaturesService featuresService;

		public IssuedSheetPrintModel(BaseParameters baseParameters, FeaturesService featuresService) {
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
		}
			
		public ReportInfo GetReportInfo(IssuedSheetPrint type, IssuanceSheet sheet) {
			ReportInfo reportInfo = new ReportInfo {
				Identifier = type.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  sheet.Id },
					{"printPromo", featuresService.Available(WorkwearFeature.PrintPromo)}}
			};
			switch(type) {
				case IssuedSheetPrint.AssemblyTask :
					reportInfo.Title = $"Задание на сборку №{sheet.DocNumber ?? sheet.Id.ToString()}";
					break;
				case IssuedSheetPrint.IssuanceSheet_0504210_1 :
					reportInfo.Title = $"Ведомость выдачи материальных ценностей №{sheet.DocNumber ?? sheet.Id.ToString()}, лицевая";
					break;
				case IssuedSheetPrint.IssuanceSheet_0504210_2 :
					reportInfo.Title = $"Ведомость выдачи материальных ценностей №{sheet.DocNumber ?? sheet.Id.ToString()}, оборотная";
					int i = 1;
					foreach(var pt in sheet.Items.Select(x => x.Nomenclature).Where(x => x != null).Distinct().Take(11))
						reportInfo.Parameters.Add("pos_" + i++ +"_id", pt.Id);
					break;
				case IssuedSheetPrint.IssuanceSheet :
				case IssuedSheetPrint.IssuanceSheetVertical :
					reportInfo.Title = $"Ведомость №{sheet.DocNumber ?? sheet.Id.ToString()} (МБ-7)";
					reportInfo.Parameters = new Dictionary<string, object> {
						{ "id",  sheet.Id },
						{"printPromo", featuresService.Available(WorkwearFeature.PrintPromo)}};
					break;
				default:
					throw new NotSupportedException("type " + nameof(IssuedSheetPrint));
			}

			//Если пользователь не хочет сворачивать ФИО и табельник (настройка в базе)
			if((type == IssuedSheetPrint.IssuanceSheet || type == IssuedSheetPrint.IssuanceSheetVertical) && !baseParameters.CollapseDuplicateIssuanceSheet)
				reportInfo.Source = File.ReadAllText(reportInfo.GetPath()).Replace("<HideDuplicates>Data</HideDuplicates>", "<HideDuplicates></HideDuplicates>");
			
			return reportInfo;
		}

		
	}
	
	public enum IssuedSheetPrint
	{
		[Display(Name = "Альбомная МБ-7")]
		[ReportIdentifier("Statements.IssuanceSheet")]
		IssuanceSheet,
		[Display(Name = "Книжная МБ-7")]
		[ReportIdentifier("Statements.IssuanceSheetVertical")]
		IssuanceSheetVertical,
		[Display(Name = "Задание на сборку")]
		[ReportIdentifier("Statements.AssemblyTask")]
		AssemblyTask,
		[Display(Name = "ОКУД 0504210 ст.1")]
		[ReportIdentifier("Statements.IssuanceSheet_0504210_1")]
		IssuanceSheet_0504210_1,
		[Display(Name = "ОКУД 0504210 ст.2")]
		[ReportIdentifier("Statements.IssuanceSheet_0504210_2")]
		IssuanceSheet_0504210_2,
	}
}
