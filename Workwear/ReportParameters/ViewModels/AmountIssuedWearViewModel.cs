using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using Gamma.Widgets;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.ReportParameters.ViewModels;
using Workwear.Tools.Features;

namespace workwear.ReportParameters.ViewModels {
	public class AmountIssuedWearViewModel : ReportParametersUowViewModelBase
	{
		public readonly FeaturesService FeaturesService;
		public ChoiceSubdivisionViewModel ChoiceSubdivisionViewModel;
		
		public AmountIssuedWearViewModel(
			RdlViewerViewModel rdlViewerViewModel, 
			IUnitOfWorkFactory uowFactory, 
			FeaturesService featuresService) : base(rdlViewerViewModel, uowFactory) 
		{
			FeaturesService = featuresService;
			Title = "Справка о выданной спецодежде";

			ChoiceSubdivisionViewModel = new ChoiceSubdivisionViewModel(UoW);
			ChoiceSubdivisionViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;

			if(FeaturesService.Available(WorkwearFeature.Owners)) {
				Owners = UoW.GetAll<Owner>().ToList();
				VisibleOwners = UoW.GetAll<Owner>().Any();
			}
				
			if(FeaturesService.Available(WorkwearFeature.CostCenter))
				VisibleCostCenter = UoW.GetAll<CostCenter>().Any();
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"dateStart", StartDate },
					{"dateEnd", EndDate},
					{"summary", !BySubdivision},
					{"bySize", BySize},
					{"withoutsub", ChoiceSubdivisionViewModel.NullIsSelected },
					{"subdivisions",  ChoiceSubdivisionViewModel.SelectedIdsMod},
					{"issue_type", IssueType?.ToString() },
					{"matchString", MatchString},
					{"noMatchString", NoMatchString},
					{"alternativeName", UseAlternativeName},
					{"showOwners", VisibleOwners && SelectOwner.Equals(SpecialComboState.All)}, //Подумал что не стоит показывать колонку если выбран конкретный собственник
					{"allOwners", SelectOwner.Equals(SpecialComboState.All)},
					{"withoutOwner", SelectOwner.Equals(SpecialComboState.Not)},
					{"ownerId", (SelectOwner as Owner)?.Id ?? -1},
					{"byEmployee", ByEmployee},
					{"showCost", ShowCost},
					{"showCostCenter", ShowCostCenter},
					{"showOnlyWithoutNorm",ShowOnlyWithoutNorm}
		};

		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}

		#region Параметры
		private AmountIssuedWearReportType reportType;
		public virtual AmountIssuedWearReportType ReportType {
			get => reportType;
			set => SetField(ref reportType, value);
		}

		private DateTime? startDate;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}

		private DateTime? endDate;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}

		private IssueType? issueType;
		public virtual IssueType? IssueType {
			get => issueType;
			set => SetField(ref issueType, value);
		}

		private bool bySubdividion = true;
		public virtual bool BySubdivision {
			get => bySubdividion;
			set => SetField(ref bySubdividion, value);
		}

		private bool byEmployee;
		public virtual bool ByEmployee {
			get => byEmployee;
			set => SetField(ref byEmployee, value);
		}

		private bool bySize;
		[PropertyChangedAlso(nameof(VisibleUseAlternative))]
		public virtual bool BySize {
			get => bySize;
			set => SetField(ref bySize, value);
		}
		
		private bool addChildSubdivisions;
		public bool AddChildSubdivisions {
			get => addChildSubdivisions;
			set => SetField(ref addChildSubdivisions, value);
		}

		private bool useAlternativeName;

		public bool UseAlternativeName {
			get => useAlternativeName;
			set => SetField(ref useAlternativeName, value);
		}
		public bool VisibleUseAlternative => BySize;

		private object selectOwner = SpecialComboState.All;
		public object SelectOwner {
			get => selectOwner;
			set => SetField(ref selectOwner, value);
		}

		private bool showCost;
		public virtual bool ShowCost {
			get => showCost;
			set => SetField(ref showCost, value);
		}

		private bool showCostCenter;
		public virtual bool ShowCostCenter {
			get => showCostCenter;
			set => SetField(ref showCostCenter, value);
		}
		
		private bool showOnlyWithoutNorm;
		public virtual bool ShowOnlyWithoutNorm {
        			get => showOnlyWithoutNorm;
        			set => SetField(ref showOnlyWithoutNorm, value);
        		}

		#endregion

		#region Свойства

		private IList<Owner> owners;
		public IList<Owner> Owners {
			get => owners;
			set => SetField(ref owners, value);
		}

		public bool SensitiveLoad => StartDate != null 
		                             && EndDate != null;

		#region Visible
		public bool VisibleOwners;
		public bool VisibleCostCenter;
		public bool VisibleIssueType => FeaturesService.Available(WorkwearFeature.CollectiveExpense);
		public bool SensetiveLoad => !ChoiceSubdivisionViewModel.AllUnSelected && startDate <= endDate;
		
		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(ChoiceSubdivisionViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
		}
		#endregion

		private string matchString;
		public string MatchString {
			get => matchString;
			set {
				SetField(ref matchString, value);
			}
		}

		private string noMatchString;
		public string NoMatchString {
			get => noMatchString;
			set {
				SetField(ref noMatchString, value);
			}
		}
		#endregion
	}

	public enum AmountIssuedWearReportType {
		[ReportIdentifier("AmountIssuedWear")]
		[Display(Name = "Для печати A4 с группировкой")]
		Common,
		[ReportIdentifier("AmountIssuedWearFlat")]
		[Display(Name = "Для экспорта (только данные)")]
		Flat
	}
}
