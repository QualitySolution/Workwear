using System;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.ViewModels;
using Workwear.Domain.Company;

namespace Workwear.ReportParameters.ViewModels {
	public class ChoiceSubdivisionViewModel : ViewModelBase {
		
		private readonly IUnitOfWork UoW;
		
		public ChoiceSubdivisionViewModel(IUnitOfWorkFactory uowFactory, IUnitOfWork uow)
		{
			this.UoW = uow ?? throw new ArgumentNullException(nameof(uow));
		}
		
		private IObservableList<SelectedChoiceSubdivision> subdivisions;
		public IObservableList<SelectedChoiceSubdivision> Subdivisions {
			get {
				if(subdivisions == null)
					FillSubdivision();
				return subdivisions;
			}
		}

		void FillSubdivision(){
			SelectedChoiceSubdivision resultAlias = null;

			subdivisions = new ObservableList<SelectedChoiceSubdivision>(UoW.Session.QueryOver<Subdivision>()
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(() => true).WithAlias(() => resultAlias.Select)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<SelectedChoiceSubdivision>())
				.List<SelectedChoiceSubdivision>());
		}

		public int[] SelectedChoiceSubdivisionIds()
		{
			if(Subdivisions.All(x => x.Select))
				return new int[] { -1 };
			if(Subdivisions.All(x => !x.Select))
				return new int[] { -2 };
			return Subdivisions.Where(x => x.Select).Select(x => x.Id).Distinct().ToArray();
		}
		
		private bool selectAllState = true; 
		public void SelectUnselectAll() {
			selectAllState = !selectAllState;
			foreach (var s in Subdivisions)
				s.Select = selectAllState;
		}
	}
	
	public class SelectedChoiceSubdivision : PropertyChangedBase
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
