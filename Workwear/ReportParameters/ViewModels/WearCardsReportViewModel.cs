using QS.Report.ViewModels;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.Report;

namespace Workwear.ReportParameters.ViewModels {
	public class WearCardsReportViewModel: ReportParametersViewModelBase {
		public WearCardsReportViewModel(RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel) {
			Title = "Список сотрудников";
		}
		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>() {
			{"only_without_norms", OnlyWithoutNorms},
			{"only_working", OnlyWorking},
		};

		private bool onlyWithoutNorms;
		public virtual bool OnlyWithoutNorms {
			get => onlyWithoutNorms;
			set=>SetField(ref onlyWithoutNorms, value);
		}

		private bool onlyWorking;

		public virtual bool OnlyWorking {
			get => onlyWorking;
			set=>SetField(ref onlyWorking, value);
		}
		private WearCardsReportType reportType;

		public virtual WearCardsReportType ReportType {
			get => reportType;
			set {
				SetField(ref reportType, value);
        
			}
		}

		public enum WearCardsReportType {
			[ReportIdentifier("WearCardsReportFlat")]
			[Display(Name="Только данные")]
			Flat,
			[ReportIdentifier("WearCardsReport")]
			[Display(Name="Форматировано")]
			Common
		}
	}
}
