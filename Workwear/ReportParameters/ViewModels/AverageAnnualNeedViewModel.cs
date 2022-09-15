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

		public AverageAnnualNeedViewModel(RdlViewerViewModel rdlViewerViewModel, IUnitOfWorkFactory uowFactory, INavigationManager navigation, ILifetimeScope autofacScope) : base(rdlViewerViewModel)
		{
			Title = "Среднегодовая потребность";
			Identifier = "AverageAnnualNeed";

			UoW = uowFactory.CreateWithoutRoot();

			var builder = new CommonEEVMBuilderFactory(rdlViewerViewModel, UoW, navigation, autofacScope);
			SubdivisionEntry = builder.ForEntity<Subdivision>().MakeByType().Finish();
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"subdivision_id", SubdivisionEntry.Entity?.Id ?? -1 },
					{"issue_type", IssueType?.ToString() },
					{"show_sex", ShowSex },
					{"summary", Summary}
				 };

		#region Параметры
		private IssueType? issueType;
		public virtual IssueType? IssueType {
			get => issueType;
			set => SetField(ref issueType, value);
		}

		public bool ShowSex { get; set; }
		
		private bool summary = true;
		public virtual bool Summary {
			get => summary;
			set => SetField(ref summary, value);
		}
		
		#endregion
		#region Свойства
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
