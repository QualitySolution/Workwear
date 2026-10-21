using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Tools;
using Workwear.Tools.Features;

namespace Workwear.ReportParameters.ViewModels {
	public class WearStatementViewModel : ReportParametersUowViewModelBase, IDialogDocumentation {
		private readonly FeaturesService featuresService;

		public WearStatementViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			FeaturesService featuresService
			) : base(rdlViewerViewModel, uowFactory)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			Title = "Сводная ведомость";
			Identifier = "WearStatement";

			var builder = new CommonEEVMBuilderFactory(rdlViewerViewModel, UoW, navigation, autofacScope);
			SubdivisionEntry = builder.ForEntity<Subdivision>().MakeByType().Finish();
			SubdivisionEntry.Changed += (s, e) => OnPropertyChanged(nameof(SensitiveLoad));
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("reports.html#wear-statement");
		public string ButtonTooltip => DocHelper.GetReportDocTooltip(Title);
		#endregion

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"id", SubdivisionEntry.Entity?.Id ?? -1 },
			{"printPromo", featuresService.Available(WorkwearFeature.PrintPromo)},
		};

		public EntityEntryViewModel<Subdivision> SubdivisionEntry;

		public bool SensitiveLoad => SubdivisionEntry.Entity != null;
	}
}
