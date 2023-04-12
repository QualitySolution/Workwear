using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using Gamma.Widgets;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Repository.Company;
using Workwear.Tools.Features;

namespace workwear.ReportParameters.ViewModels {
	public class AmountIssuedWearViewModel : ReportParametersUowViewModelBase
	{
		public readonly FeaturesService FeaturesService;
		public AmountIssuedWearViewModel(
			RdlViewerViewModel rdlViewerViewModel, 
			IUnitOfWorkFactory uowFactory, 
			SubdivisionRepository subdivisionRepository,
			FeaturesService featuresService) : base(rdlViewerViewModel, uowFactory) 
		{
			FeaturesService = featuresService;
			Title = "Справка о выданной спецодежде";
			using(var uow = uowFactory.CreateWithoutRoot()) {
				SelectedSubdivison resultAlias = null;
				Subdivisions = subdivisionRepository.ActiveQuery(uow) 
					.SelectList(list => list
					   .Select(x => x.Id).WithAlias(() => resultAlias.Id)
					   .Select(x => x.Name).WithAlias(() => resultAlias.Name)
				)
				.TransformUsing(Transformers.AliasToBean<SelectedSubdivison>())
				.List<SelectedSubdivison>();
			}
			Subdivisions.Insert(0, 
				new SelectedSubdivison {
					Id = -1,
					Name = "Без подразделения" 
			});
			foreach(var item in Subdivisions) {
				item.PropertyChanged += (sender, e) => OnPropertyChanged(nameof(SensitiveLoad));
			}

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
					{"withoutsub", Subdivisions.First(x =>x.Id == -1).Select },
					{"subdivisions", SelectSubdivisons() },
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
		[PropertyChangedAlso(nameof(SensitiveLoad))]
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}

		private DateTime? endDate;
		[PropertyChangedAlso(nameof(SensitiveLoad))]
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

		private bool selectAll = true;
		public virtual bool SelectAll {
			get => selectAll;
			set {
				SetField(ref selectAll, value);
				{
					foreach(var item in Subdivisions) {
						item.Select = value;
					}
				}
			}
		}

		public IList<SelectedSubdivison> Subdivisions;

		private IList<Owner> owners;
		public IList<Owner> Owners {
			get => owners;
			set => SetField(ref owners, value);
		}

		public bool SensitiveLoad => StartDate != null && EndDate != null && Subdivisions.Any(x => x.Select);

		#region Visible
		public bool VisibleOwners;
		public bool VisibleCostCenter;
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

		private int[] SelectSubdivisons() {
			var selectedId = Subdivisions
				.Where(x => x.Select).Select(x => x.Id).ToList();
			
			if (!AddChildSubdivisions) return selectedId.ToArray();

			var parents = UoW.Session.QueryOver<Subdivision>()
				.WhereRestrictionOn(c => c.Id).IsIn(selectedId)
				.List();

			return parents
				.SelectMany(x => x.AllGenerationsSubdivisions).Take(500)
				.Select(x => x.Id)
				.Distinct()
				.ToArray();
		}
	}

	public class SelectedSubdivison : PropertyChangedBase
	{
		private bool select;
		public virtual bool Select {
			get => select;
			set => SetField(ref select, value);
		}
		 
		public int Id { get; set; }
		public string Name { get; set; }
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
