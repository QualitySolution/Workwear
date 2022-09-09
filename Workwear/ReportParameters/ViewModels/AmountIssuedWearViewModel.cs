﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Repository.Company;

namespace workwear.ReportParameters.ViewModels
{
	public class AmountIssuedWearViewModel : ReportParametersViewModelBase
	{
		private readonly IUnitOfWork unitOfWork;
		public AmountIssuedWearViewModel(
			RdlViewerViewModel rdlViewerViewModel, 
			IUnitOfWorkFactory uowFactory, 
			SubdivisionRepository subdivisionRepository) : base(rdlViewerViewModel)
		{
			Title = "Справка о выданной спецодежде";
			Identifier = "AmountIssuedWear";
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
			unitOfWork = uowFactory.CreateWithoutRoot();
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"dateStart", StartDate },
					{"dateEnd", EndDate},
					{"summary", Summary},
					{"bySize", BySize},
					{"withoutsub", Subdivisions.First(x =>x.Id == -1).Select },
					{"subdivisions", SelectSubdivisons() },
					{"issue_type", IssueType?.ToString() },
					{"matchString", MatchString},
					{"noMatchString", NoMatchString},
					{"alternativeName", UseAlternativeName}
				 };

		#region Параметры
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

		private bool summary = true;
		public virtual bool Summary {
			get => summary;
			set => SetField(ref summary, value);
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

		public bool SensitiveLoad => StartDate != null && EndDate != null && Subdivisions.Any(x => x.Select);

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

			var parents = unitOfWork.Session.QueryOver<Subdivision>()
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
}
