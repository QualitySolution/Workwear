using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report.ViewModels;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Tools.Features;
using Workwear.ReportParameters.ViewModels;
using System.ComponentModel;

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
			
			ChoiceEmployeeGroupViewModel = new ChoiceEmployeeGroupViewModel(UoW);
			ChoiceEmployeeGroupViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"subdivision_id", SubdivisionEntry.Entity?.Id ?? -1 },
					{"issue_type", IssueType?.ToString() },
					{"show_sex", ShowSex },
					{"summary", Summary},
					{"show_size", ShowSize},
					{"without_groups", ChoiceEmployeeGroupViewModel.NullIsSelected },	
					{"employee_groups_ids", ChoiceEmployeeGroupViewModel.SelectedIdsMod},
					{"printPromo", featuresService.Available(WorkwearFeature.PrintPromo)},
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
		public bool VisibleChoiceEmployeeGroup => featuresService.Available(WorkwearFeature.EmployeeGroups);
		#endregion
		#region Свойства
		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		public bool SensetiveLoad => !ChoiceEmployeeGroupViewModel.AllUnSelected;
		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(ChoiceEmployeeGroupViewModel.AllUnSelected)==e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
		}
		
		#endregion

		#region ViewModels
		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
		public ChoiceEmployeeGroupViewModel ChoiceEmployeeGroupViewModel;
		#endregion

		public void Dispose()
		{
			UoW.Dispose();
		}
	}
}
