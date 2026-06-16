using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using Gamma.Widgets;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.ViewModels.Control;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Tools.Features;

namespace Workwear.ReportParameters.ViewModels {
	public class AmountIssuedDutyNormsViewModel : ReportParametersUowViewModelBase {
		
		public readonly FeaturesService FeaturesService;
		public ChoiceListViewModel<Subdivision> ChoiceSubdivisionViewModel;
		
		public AmountIssuedDutyNormsViewModel(
			RdlViewerViewModel rdlViewerViewModel, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			FeaturesService featuresService,
			UnitOfWorkProvider unitOfWorkProvider = null
			) : base(rdlViewerViewModel, unitOfWorkFactory, unitOfWorkProvider)
		{
			FeaturesService = featuresService;
			Title = "Справка о выдачах по дежурным нормам";

			if(FeaturesService.Available(WorkwearFeature.Owners)) {
				Owners = UoW.GetAll<Owner>().ToList();
				VisibleOwners = UoW.GetAll<Owner>().Any();
			}
			
			var subdivisionsList = UoW.GetAll<Subdivision>().ToList();
			ChoiceSubdivisionViewModel = new ChoiceListViewModel<Subdivision>(subdivisionsList);
			ChoiceSubdivisionViewModel.ShowNullValue(true, "Без подраздеения");
			ChoiceSubdivisionViewModel.PropertyChanged += (s,e) => OnPropertyChanged(nameof(SensetiveLoad));
		}
		
		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}
		
		private AmountIssuedDutyNormsReportType reportType;
		public virtual AmountIssuedDutyNormsReportType ReportType {
			get => reportType;
			set => SetField(ref reportType, value);
		}
		
		private DateTime? startDate = DateTime.Now.AddMonths(-1);
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}

		private DateTime? endDate = DateTime.Now;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}
		
		private bool useAlternativeName;
		public bool UseAlternativeName {
			get => useAlternativeName;
			set => SetField(ref useAlternativeName, value);
		}
		
		private object selectOwner = SpecialComboState.All;
		public object SelectOwner {
			get => selectOwner;
			set => SetField(ref selectOwner, value);
		}
		
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
		
		private bool showCost;
		public virtual bool ShowCost {
			get => showCost;
			set => SetField(ref showCost, value);
		}

		private IList<Owner> owners;
		public IList<Owner> Owners {
			get => owners;
			set => SetField(ref owners, value);
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"dateStart", StartDate },
			{"dateEnd", EndDate},
			{"withoutsub", ChoiceSubdivisionViewModel.NullIsSelected },
			{"subdivisions",  ChoiceSubdivisionViewModel.SelectedIdsMod},
			{"matchString", MatchString},
			{"noMatchString", NoMatchString},
			{"alternativeName", UseAlternativeName},
			{"showOwners", VisibleOwners && SelectOwner.Equals(SpecialComboState.All)},
			{"allOwners", SelectOwner.Equals(SpecialComboState.All)},
			{"withoutOwner", SelectOwner.Equals(SpecialComboState.Not)},
			{"ownerId", (SelectOwner as Owner)?.Id ?? -1},
			{"showCost", ShowCost},
		};
		
		public bool SensetiveLoad => !ChoiceSubdivisionViewModel.AllUnSelected 
		                             && StartDate != null && EndDate != null && startDate <= endDate;
		public bool VisibleShowCost => FeaturesService.Available(WorkwearFeature.Selling);
		public bool VisibleOwners;
	}

	public enum AmountIssuedDutyNormsReportType {
		[ReportIdentifier("DutyNorms.AmountIssuedDutyNorms")]
		[Display(Name = "Форматировано")]
		Common,
		[ReportIdentifier("DutyNorms.AmountIssuedDutyNormsFlat")]
		[Display(Name = "Только данные")]
		Flat
	}
}
