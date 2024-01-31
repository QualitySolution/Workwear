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
	public class ChoiceEmployeeGroupViewModel : ViewModelBase {
		
		private readonly IUnitOfWork UoW;
		
		public ChoiceEmployeeGroupViewModel(IUnitOfWork uow)
		{
			this.UoW = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		private ObservableList<SelectedChoiceEmployeeGroups> employeeGroups;
		public ObservableList<SelectedChoiceEmployeeGroups> EmployeeGroups {
			get {
				if(employeeGroups == null)
					FillEmployeeGroup();
				return employeeGroups;
			}
		}
		
		private void FillEmployeeGroup(){
			SelectedChoiceEmployeeGroups resultAlias = null;

			employeeGroups = new ObservableList<SelectedChoiceEmployeeGroups>(UoW.Session.QueryOver<EmployeeGroup>()
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(() => true).WithAlias(() => resultAlias.Select)
					.Select(() => true).WithAlias(() => resultAlias.Highlighted)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<SelectedChoiceEmployeeGroups>())
				.List<SelectedChoiceEmployeeGroups>());

			if(ShowNullValue) {
				employeeGroups.Insert(0, new SelectedChoiceEmployeeGroups() {
					Id = -1,
					Name = " Без группы",
					Select = true,
					Highlighted = true,
				});
			}

			employeeGroups.PropertyOfElementChanged += OnPropertyOfElementChanged;
		}

		private void OnPropertyOfElementChanged(object sender, PropertyChangedEventArgs e) {
			OnPropertyChanged(nameof(AllSelected));
			OnPropertyChanged(nameof(AllUnSelected));
		}

		/// <summary>
		/// Показывать в списке строку "Без группы"
		/// Результат в спец. поле NullIsSelected
		/// </summary>
		private bool showNullValue = true;
		public bool ShowNullValue {
			get => showNullValue;
			set => SetField(ref showNullValue, value);
		}
		
		/// <summary>
		///  Массив id групп 
		/// </summary>
		public int[] SelectedChoiceEmployeeGroupsIds {
			get => EmployeeGroups.Where(x => x.Select && x.Id > 0).Select(x => x.Id).Distinct().ToArray();
		}
		
		/// <summary>
		///  Выбраны все подразделение включая "Без группы"
		/// </summary>
		public bool AllSelected {
			get => EmployeeGroups.All(x => x.Select);
		}
		
		/// <summary>
		///  Не выбрано ни одно подразделение в том числе "Без группы"
		/// </summary>
		public bool AllUnSelected {
			get => EmployeeGroups.All(x => !x.Select);
		}
		
		/// <summary>
		/// В списке выбрано "Без группы"
		/// </summary>
		public bool NullIsSelected {
			get => EmployeeGroups.Any(x => x.Select && x.Id == -1);
		}
		
		public void SelectAll() {
			foreach (var s in EmployeeGroups)
				s.Select = true;
		}
		 
		public void UnSelectAll() {
			foreach (var s in EmployeeGroups)
				s.Select = false;
		}
		
		public void SelectLike(string maskLike) {
			foreach(var line in EmployeeGroups)
				line.Highlighted = line.Name.ToLower().Contains(maskLike.ToLower());
			EmployeeGroups.Sort(Comparison);
		}

		private int Comparison(SelectedChoiceEmployeeGroups x, SelectedChoiceEmployeeGroups y) {
			if(x.Highlighted == y.Highlighted)
				return x.Name.CompareTo(y.Name);
			return x.Highlighted ? -1 : 1;
		}
	}

	public class SelectedChoiceEmployeeGroups : PropertyChangedBase {
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
