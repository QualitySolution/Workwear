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

		private ObservableList<SelectedChoiceSubdivision> subdivisions;
		public ObservableList<SelectedChoiceSubdivision> Subdivisions {
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
					.Select(() => true).WithAlias(() => resultAlias.Highlighted)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<SelectedChoiceSubdivision>())
				.List<SelectedChoiceSubdivision>());

			if(ShowNullValue) {
				subdivisions.Insert(0, new SelectedChoiceSubdivision() {
					Id = -1,
					Name = " Без подразделения",
					Select = true,
					Highlighted = true,
				});
			}

			subdivisions.PropertyOfElementChanged += OnPropertyOfElementChanged;
		}

		private void OnPropertyOfElementChanged(object sender, PropertyChangedEventArgs e) {
			OnPropertyChanged(nameof(AllSelected));
			OnPropertyChanged(nameof(AllUnSelected));
		}

		/// <summary>
		/// Показывать в списке строку "Без подразделения"
		/// Результат в спец. поле NullIsSelected
		/// </summary>
		private bool showNullValue = true;
		public bool ShowNullValue {
			get => showNullValue;
			set => SetField(ref showNullValue, value);
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
		
		public void SelectLike(string maskLike) {
			foreach(var line in Subdivisions)
				line.Highlighted = line.Name.ToLower().Contains(maskLike.ToLower());
			Subdivisions.Sort(Comparison);
		}

		private int Comparison(SelectedChoiceSubdivision x, SelectedChoiceSubdivision y) {
			if(x.Highlighted == y.Highlighted)
				return x.Name.CompareTo(y.Name);
			return x.Highlighted ? -1 : 1;
		}
	}

	public class SelectedChoiceSubdivision : PropertyChangedBase {
		public int Id { get; set; }
		public string Name { get; set; }

		private bool select;

		public virtual bool Select {
			get => select;
			set => SetField(ref select, value);
		}

		private bool highlighted;

		public bool Highlighted {
			get => highlighted;
			set => SetField(ref highlighted, value);
		}
	}
}
