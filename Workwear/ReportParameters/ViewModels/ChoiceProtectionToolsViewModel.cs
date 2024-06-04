using System;
using System.ComponentModel;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.ViewModels;
using Workwear.Domain.Regulations;

namespace Workwear.ReportParameters.ViewModels {
	public class ChoiceProtectionToolsViewModel : ViewModelBase {
		
		private readonly IUnitOfWork UoW;
		
		public ChoiceProtectionToolsViewModel(IUnitOfWork uow)
		{
			this.UoW = uow ?? throw new ArgumentNullException(nameof(uow));
		}
		
		private ObservableList<SelectedProtectionTools> items;
		public ObservableList<SelectedProtectionTools> Items {
			get {
				if(items == null)
					FillItems();
				return items;
			}
		}

		void FillItems(){
			SelectedProtectionTools resultAlias = null;

			items = new ObservableList<SelectedProtectionTools>(UoW.Session.QueryOver<ProtectionTools>()
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(() => true).WithAlias(() => resultAlias.Select)
					.Select(() => true).WithAlias(() => resultAlias.Highlighted)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<SelectedProtectionTools>())
				.List<SelectedProtectionTools>());
			
			items.PropertyOfElementChanged += OnPropertyOfElementChanged;
		}

		private void OnPropertyOfElementChanged(object sender, PropertyChangedEventArgs e) {
			OnPropertyChanged(nameof(AllSelected));
			OnPropertyChanged(nameof(AllUnSelected));
		}
		
		/// <summary>
		///  Массив id Номенклатур нормы 
		/// </summary>
		public int[] SelectedIds {
			get => Items.Where(x => x.Select && x.Id > 0).Select(x => x.Id).Distinct().ToArray();
		}
		
		/// <summary>
		/// Массив id со спецзначениями
		/// Выводит массив id если что-то выбрано, либо массив с одним значением
		/// -1 если выбрано всё втом числе  null элемент
		/// -2 если ничего не выбрано
		/// Никогда не возвращает пустой массив.
		/// </summary>
		public int[] SelectedIdsMod {
			get {
				if(AllSelected)
					return new[] { -1 };
				var ids = Items.Where(x => x.Select && x.Id > 0).Select(x => x.Id).Distinct().ToArray();
				if(ids.Length != 0)
					return ids;
				else return new[] { -2 };
			} 
		}
		
		/// <summary>
		///  Выбраны все Номенклатуры нормы 
		/// </summary>
		public bool AllSelected {
			get => Items.All(x => x.Select);
		}
		
		/// <summary>
		///  Не выбрано ни одна номенклатура нормы 
		/// </summary>
		public bool AllUnSelected {
			get => Items.All(x => !x.Select);
		}

		public void SelectAll() {
			foreach (var pt in Items)
				pt.Select = true;
		}
		 
		public void UnSelectAll() {
			foreach (var pt in Items)
				pt.Select = false;
		}
		
		public void SelectLike(string maskLike) {
			foreach(var line in Items)
				line.Highlighted = line.Name.ToLower().Contains(maskLike.ToLower());
			Items.Sort(Comparison);
		}

		private int Comparison(SelectedProtectionTools x, SelectedProtectionTools y) {
			if(x.Highlighted == y.Highlighted)
				return x.Name.CompareTo(y.Name);
			return x.Highlighted ? -1 : 1;
		}
	}

	public class SelectedProtectionTools : PropertyChangedBase
	{
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
