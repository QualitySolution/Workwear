using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using workwear.Domain.Stock;
using workwear.Repository.Company;

namespace workwear.ReportParameters.ViewModels
{
	public class AmountIssuedWearViewModel : ReportParametersViewModelBase
	{
		public AmountIssuedWearViewModel(RdlViewerViewModel rdlViewerViewModel, IUnitOfWorkFactory uowFactory, SubdivisionRepository subdivisionRepository) : base(rdlViewerViewModel)
		{
			Title = "Справка о выданной спецодежде";
			Identifier = "AmountIssuedWear";
			using(var uow = uowFactory.CreateWithoutRoot()) {
				SelectedSubdivison resultAlias = null;
				Subdivisons = subdivisionRepository.ActiveQuery(uow)
					.SelectList(list => list
					   .Select(x => x.Id).WithAlias(() => resultAlias.Id)
					   .Select(x => x.Name).WithAlias(() => resultAlias.Name)
				)
				.TransformUsing(Transformers.AliasToBean<SelectedSubdivison>())
				.List<SelectedSubdivison>();
			}
			Subdivisons.Insert(0, 
				new SelectedSubdivison {
					Id = -1,
					Name = "Без подразделения" 
			});
			foreach(var item in Subdivisons) {
				item.PropertyChanged += (sender, e) => OnPropertyChanged(nameof(SensetiveLoad));
			}
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"dateStart", StartDate },
					{"dateEnd", EndDate},
					{"summary", Summary},
					{"bySize", BySize},
					{"withoutsub", Summary ? false : Subdivisons.First().Select },
					{"subdivisions", Summary ? new int[] {-1} : Subdivisons.Where(x => x.Select).Select(x => x.Id).ToArray() },
					{"issue_type", IssueType?.ToString() },
				 };

		#region Параметры
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

		private bool summary = true;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		[PropertyChangedAlso(nameof(SensetiveSubdivisions))]
		public virtual bool Summary {
			get => summary;
			set => SetField(ref summary, value);
		}

		private bool bySize;
		public virtual bool BySize {
			get => bySize;
			set => SetField(ref bySize, value);
		}

		#endregion
		#region Свойства

		private bool selectAll;
		public virtual bool SelectAll {
			get => selectAll;
			set {
				SetField(ref selectAll, value);
				{
					foreach(var item in Subdivisons) {
						item.Select = value;
					}
				}
			}
		}

		public IList<SelectedSubdivison> Subdivisons;

		public bool SensetiveLoad => StartDate != null && EndDate != null && (Summary || Subdivisons.Any(x => x.Select));
		public bool SensetiveSubdivisions => !Summary;
		#endregion
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
}
