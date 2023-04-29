using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report.ViewModels;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Tools.Features;

namespace workwear.ReportParameters.ViewModels {
	public class RequestSheetViewModel : ReportParametersViewModelBase, IDisposable
	{
		private readonly FeaturesService featuresService;

		public RequestSheetViewModel(RdlViewerViewModel rdlViewerViewModel, IUnitOfWorkFactory uowFactory, INavigationManager navigation, ILifetimeScope AutofacScope, FeaturesService featuresService) : base(rdlViewerViewModel)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			
			Title = "Заявка на спецодежду";
			Identifier = "RequestSheet";

			uow = uowFactory.CreateWithoutRoot();
			var builder = new CommonEEVMBuilderFactory(rdlViewerViewModel, uow, navigation, AutofacScope);

			EntrySubdivisionViewModel = builder.ForEntity<Subdivision>().MakeByType().Finish();
			var defaultMonth = DateTime.Today.AddMonths(1);
			BeginMonth = EndMonth = defaultMonth.Month;
			BeginYear = EndYear = defaultMonth.Year;
		}

		private readonly IUnitOfWork uow;

		#region Entry
		public readonly EntityEntryViewModel<Subdivision> EntrySubdivisionViewModel;
		#endregion

		#region Свойства View
		private int beginMonth;
		[PropertyChangedAlso(nameof(SensetiveRunReport))]
		public virtual int BeginMonth {
			get => beginMonth;
			set => SetField(ref beginMonth, value);
		}

		private int beginYear;
		[PropertyChangedAlso(nameof(SensetiveRunReport))]
		public virtual int BeginYear {
			get => beginYear;
			set => SetField(ref beginYear, value);
		}

		private int endMonth;
		[PropertyChangedAlso(nameof(SensetiveRunReport))]
		public virtual int EndMonth {
			get => endMonth;
			set => SetField(ref endMonth, value);
		}

		private int endYear;
		[PropertyChangedAlso(nameof(SensetiveRunReport))]
		public virtual int EndYear {
			get => endYear;
			set => SetField(ref endYear, value);
		}

		private IssueType? issueTypeOptions;
		public IssueType? IssueTypeOptions {
			get => issueTypeOptions;
			set => SetField(ref issueTypeOptions, value);
		}

		private bool addChildSubdivisions;
		public bool AddChildSubdivisions {
			get => addChildSubdivisions;
			set => SetField(ref addChildSubdivisions, value);
		}

		private IList<SelectedProtectionTools> protectionTools;
		public IList<SelectedProtectionTools> ProtectionTools {
			get {
				if(protectionTools == null)
					FillProtectionTools();
				return protectionTools;
			}
		}
		
		private bool showSex;
		public bool ShowSex {
			get => showSex;
			set => SetField(ref showSex, value);
		}
		
		private bool excludeInVacation;
		public virtual bool ExcludeInVacation {
			get => excludeInVacation;
			set => SetField(ref excludeInVacation, value);
		}

		void FillProtectionTools(){
			SelectedProtectionTools resultAlias = null;

			protectionTools = uow.Session.QueryOver<ProtectionTools>()
				.SelectList(list => list
					   .Select(x => x.Id).WithAlias(() => resultAlias.Id)
					   .Select(x => x.Name).WithAlias(() => resultAlias.Name)
					   .Select(() => true).WithAlias(() => resultAlias.Select)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<SelectedProtectionTools>())
				.List<SelectedProtectionTools>();
		}

		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		public bool SensetiveRunReport => new DateTime(BeginYear, BeginMonth, 1) <= new DateTime(EndYear, EndMonth, 1);
		#endregion

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"begin_month", BeginMonth },
					{"begin_year", BeginYear},
					{"end_month", EndMonth},
					{"end_year", EndYear},
					{"subdivisions", SelectSubdivisions() },
					{"issue_type", IssueTypeOptions?.ToString() },
					{"protectionTools", SelectedProtectionTools() },
					{"headSubdivision", EntrySubdivisionViewModel.Entity?.Id ?? -1},
					{"show_sex", ShowSex },
					{"exclude_in_vacation", excludeInVacation }
					};

		public void Dispose()
		{
			uow.Dispose();
		}

		private int[] SelectedProtectionTools()
		{
			if(ProtectionTools.All(x => x.Select))
				return new int[] { -1 };
			if(ProtectionTools.All(x => !x.Select))
				return new int[] { -2 };
			return ProtectionTools.Where(x => x.Select).Select(x => x.Id).Distinct().ToArray();
		}

		private int[] SelectSubdivisions() {
			if(EntrySubdivisionViewModel.Entity is null) 
				return new[] {-1};
			if (!AddChildSubdivisions)
				return new[] {EntrySubdivisionViewModel.Entity.Id}; 
			return EntrySubdivisionViewModel.Entity
				.AllGenerationsSubdivisions.Take(500)
				.Select(x => x.Id)
				.Distinct()
				.ToArray();
		}
	}

	public class SelectedProtectionTools : PropertyChangedBase
	{
		private bool select;
		public virtual bool Select {
			get => select;
			set => SetField(ref select, value);
		}

		public int Id { get; set; }
		public string Name { get; set; }
	}
}
