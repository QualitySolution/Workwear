using System;
using System.ComponentModel;
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
		
		public ChoiceSubdivisionViewModel(IUnitOfWork uow)
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
		
		private void FillSubdivision(){
			SelectedChoiceSubdivision resultAlias = null;

			subdivisions = new ObservableList<SelectedChoiceSubdivision>(UoW.Session.QueryOver<Subdivision>()
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(() => true).WithAlias(() => resultAlias.Select)
				).TransformUsing(Transformers.AliasToBean<SelectedChoiceSubdivision>())
				.List<SelectedChoiceSubdivision>());
			
			
			subdivisions.Insert(0, new SelectedChoiceSubdivision() {
				Id = -1,
				Name = " Без подразделения",
				Select = true
			} );
			subdivisions.PropertyOfElementChanged += OnPropertyOfElementChanged;
		}

		private void OnPropertyOfElementChanged(object sender, PropertyChangedEventArgs e) {
			OnPropertyChanged(nameof(AllSelected));
			OnPropertyChanged(nameof(AllUnSelected));
		}

		/// <summary>
		///  Массив id подразделений 
		/// </summary>
		public int[] SelectedChoiceSubdivisionIds {
			get => Subdivisions.Where(x => x.Select && x.Id > 0).Select(x => x.Id).Distinct().ToArray();
		}
		
		/// <summary>
		///  Выбраны все подразделение включая "Без подразделения"
		/// </summary>
		public bool AllSelected {
			get => Subdivisions.All(x => x.Select);
		}
		
		/// <summary>
		///  Не выбрано ни одно подразделение в том числе "Без подразделения"
		/// </summary>
		public bool AllUnSelected {
			get => Subdivisions.All(x => !x.Select);
		}
		
		/// <summary>
		/// В списке выбрано "Без подразделенния"
		/// </summary>
		public bool NullIsSelected {
			get => Subdivisions.Any(x => x.Select && x.Id == -1);
		}
		
		public void SelectAll() {
			foreach (var s in Subdivisions)
				s.Select = true;
		}
		 
		public void UnSelectAll() {
			foreach (var s in Subdivisions)
				s.Select = false;
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
