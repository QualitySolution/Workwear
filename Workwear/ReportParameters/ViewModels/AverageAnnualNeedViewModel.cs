using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report.ViewModels;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;

namespace workwear.ReportParameters.ViewModels
{
	public class AverageAnnualNeedViewModel : ReportParametersViewModelBase, IDisposable
	{
		IUnitOfWork UoW;
		private readonly FeaturesService featuresService;

		public AverageAnnualNeedViewModel(RdlViewerViewModel rdlViewerViewModel, IUnitOfWorkFactory uowFactory, INavigationManager navigation, ILifetimeScope autofacScope, FeaturesService featuresService) : base(rdlViewerViewModel)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));

			Title = "Среднегодовая потребность";
			Identifier = "AverageAnnualNeed";

			UoW = uowFactory.CreateWithoutRoot();

			var builder = new CommonEEVMBuilderFactory(rdlViewerViewModel, UoW, navigation, autofacScope);
			SubdivisionEntry = builder.ForEntity<Subdivision>().MakeByType().Finish();

			ShowSex = false;
			ShowSize = true;
			Summary = true;
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"subdivision_id", SubdivisionEntry.Entity?.Id ?? -1 },
					{"issue_type", IssueType?.ToString() },
					{"show_sex", ShowSex },
					{"summary", Summary},
					{"show_size", ShowSize}
				 };

		#region Параметры
		private IssueType? issueType;
		public virtual IssueType? IssueType {
			get => issueType;
			set => SetField(ref issueType, value);
		}

		public bool ShowSex { get; set; }
		public bool ShowSize { get; set; }
		public bool Summary { get; set; }
		
		#endregion
		#region Свойства
		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		public bool SensetiveLoad => true;
		#endregion

		#region ViewModels
		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
		#endregion

		public void Dispose()
		{
			UoW.Dispose();
		}
	}
}
