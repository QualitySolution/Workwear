using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Cloud.WorkwearDictionary.Client;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Workwear.Models.Regulations;
using EtnApp = QS.Cloud.WorkwearDictionary.App;
using EtnNorm = QS.Cloud.WorkwearDictionary.Norm;
using EtnNormLevel = QS.Cloud.WorkwearDictionary.NormLevel;

namespace Workwear.ViewModels.Regulations {
	/// <summary>
	/// Выбор пунктов приложений 2 и 3 приказа 766н и предпросмотр строк, которые из них попадут в норму.
	/// Список не большой и грузится целиком, а поиск фильтрует дерево на клиенте.
	/// </summary>
	public class EtnImportViewModel : DialogViewModelBase {
		private readonly EtnDictionaryService etnDictionaryService;
		private readonly IInteractiveMessage interactive;
		private readonly EtnApp app;
		private readonly List<EtnSubjectNode> allNodes = new List<EtnSubjectNode>();
		private bool updatingSelection;

		public EtnImportViewModel(
			EtnApp app,
			EtnDictionaryService etnDictionaryService,
			INavigationManager navigation,
			IInteractiveMessage interactive) : base(navigation)
		{
			this.app = app;
			this.etnDictionaryService = etnDictionaryService ?? throw new ArgumentNullException(nameof(etnDictionaryService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));

			AppendixNumber = app == EtnApp.Dermal ? 3 : 2;
			Title = app == EtnApp.Dermal
				? "ЕТН: дерматологические СИЗ (приложение 3)"
				: "ЕТН: СИЗ по опасностям (приложение 2)";
			HeadComment = app == EtnApp.Dermal
				? "Отметьте объекты загрязнения или виды работ. Отметка на объекте берёт все типы средств под ним."
				: "Отметьте опасные события. Отметка на опасности берёт все её события сразу.";

			Plan.Rows.PropertyOfElementChanged += (s, e) => UpdateSummary();
			Plan.Rows.CollectionChanged += (s, e) => UpdateSummary();

			LoadTree();
			UpdateSummary();
		}

		public int AppendixNumber { get; }
		public string HeadComment { get; }

		public EtnImportPlan Plan { get; } = new EtnImportPlan();
		public IObservableList<EtnImportPlanRow> Rows => Plan.Rows;

		/// <summary>
		/// Корневые узлы, прошедшие фильтр поиска. Дерево строится по ним.
		/// </summary>
		public IList<EtnSubjectNode> VisibleRoots { get; private set; } = new List<EtnSubjectNode>();

		public event EventHandler TreeFilterChanged;
		public event EventHandler SubjectsSelectionChanged;

		#region Поиск

		private string searchText;
		public virtual string SearchText {
			get => searchText;
			set {
				if(SetField(ref searchText, value))
					ApplyFilter();
			}
		}

		public void ClearSearch() => SearchText = String.Empty;

		#endregion

		#region Доп. СИЗ по оценке профрисков

		/// <summary>
		/// Графы 8-9 приложения 2 - СИЗ, выдаваемые дополнительно по результатам оценки профрисков.
		/// Самая спорная часть виджета, но пока остаётся. 
		/// </summary>
		public bool VisibleIncludeAdditional => app == EtnApp.Hazards;

		private bool includeAdditional;
		public virtual bool IncludeAdditional {
			get => includeAdditional;
			set {
				if(SetField(ref includeAdditional, value))
					ReloadSelectedSources();
			}
		}

		#endregion

		#region Итог

		private string summary;
		public virtual string Summary {
			get => summary;
			private set => SetField(ref summary, value);
		}

		private bool sensitiveAdd;
		public virtual bool SensitiveAdd {
			get => sensitiveAdd;
			private set => SetField(ref sensitiveAdd, value);
		}

		private void UpdateSummary() {
			var total = Plan.Rows.Count;
			var incomplete = Plan.Rows.Count(x => !x.IsComplete);
			var sources = allNodes.Count(x => x.Selected);

			SensitiveAdd = total > incomplete;
			Summary = total == 0
				? "Ничего не выбрано"
				: incomplete == 0
					? $"Выбрано пунктов: {sources}, строк к добавлению: {total}"
					: $"Выбрано пунктов: {sources}, строк к добавлению: {total - incomplete}, " +
					  $"не заполнено количество или срок: {incomplete} (такие строки не добавятся)";
		}

		#endregion

		#region Дерево

		private void LoadTree() {
			var roots = etnDictionaryService.GetNormsList(0, 0, null, app, parentId: 0, recursive: true).Norms;

			foreach(var root in roots)
				AddNode(root, null);

			ApplyFilter();
		}

		private void AddNode(EtnNorm source, EtnSubjectNode parent) {
			var node = new EtnSubjectNode(source, parent);
			allNodes.Add(node);
			node.PropertyChanged += NodePropertyChanged;
			parent?.AllChildren.Add(node);

			foreach(var child in source.Children)
				AddNode(child, node);
		}

		private void ApplyFilter() {
			var search = String.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
			foreach(var root in allNodes.Where(x => x.Parent == null))
				Filter(root, search, false);

			VisibleRoots = allNodes.Where(x => x.Parent == null && x.Visible).ToList();
			TreeFilterChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Узел виден, если совпал сам или под ним есть совпадение. 
		/// </summary>
		private static bool Filter(EtnSubjectNode node, string search, bool parentMatched) {
			var matched = parentMatched || search == null || node.Contains(search);

			var anyChildVisible = false;
			foreach(var child in node.AllChildren)
				anyChildVisible |= Filter(child, search, matched);

			node.Visible = matched || anyChildVisible;
			node.RefreshVisibleChildren();
			return node.Visible;
		}

		#endregion

		#region Отметки пользователя

		private void NodePropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName != nameof(EtnSubjectNode.Selected) || updatingSelection)
				return;

			var node = (EtnSubjectNode)sender;
			var selected = node.Selected;
			var changed = new List<EtnSubjectNode> { node };
			//Переключение родителя действует на всех потомков.
			updatingSelection = true;
			try {
				foreach(var descendant in Descendants(node).Where(x => x.Selectable && x.Selected != selected)) {
					descendant.Selected = selected;
					changed.Add(descendant);
				}
			} finally {
				updatingSelection = false;
			}

			foreach(var changedNode in changed) {
				if(selected)
					LoadSource(changedNode);
				else
					Plan.RemoveSource(changedNode.NormId);
			}
			UpdateSummary();
			SubjectsSelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		private void LoadSource(EtnSubjectNode node) {
			try {
				var response = etnDictionaryService.GetNormItems(new[] { node.NormId },
					includeChildren: false, includeAdditional: IncludeAdditional);
				Plan.AddFrom(response, AppendixNumber, node.NormId);
			} catch(Exception ex) {
				interactive.ShowMessage(ImportanceLevel.Error,
					$"Не удалось получить строки пункта «{node.Name}» из справочника ЕТН.\n\n{ex.Message}",
					"Ошибка справочника ЕТН");
				updatingSelection = true;
				try { node.Selected = false; } finally { updatingSelection = false; }
			}
		}

		private void ReloadSelectedSources() {
			var selected = allNodes.Where(x => x.Selected).ToList();
			foreach(var node in selected)
				Plan.RemoveSource(node.NormId);
			foreach(var node in selected)
				LoadSource(node);

			UpdateSummary();
			SubjectsSelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		private static IEnumerable<EtnSubjectNode> Descendants(EtnSubjectNode node) {
			foreach(var child in node.AllChildren) {
				yield return child;
				foreach(var descendant in Descendants(child))
					yield return descendant;
			}
		}

		#endregion

		#region Результат

		/// <summary>
		/// Пользователь подтвердил добавление плана в норму.
		/// </summary>
		public event EventHandler<EtnImportPlan> Accepted;

		public void Accept() {
			if(!SensitiveAdd)
				return;
			Accepted?.Invoke(this, Plan);
			Close(false, CloseSource.Self);
		}

		public void Cancel() => Close(false, CloseSource.Cancel);

		#endregion
	}

	/// <summary>
	/// Узел дерева приложения ЕТН.
	/// </summary>
	public class EtnSubjectNode : PropertyChangedBase {
		private static readonly IList<EtnSubjectNode> emptyChildren = new List<EtnSubjectNode>();
		private IList<EtnSubjectNode> visibleChildren;

		public EtnSubjectNode(EtnNorm source, EtnSubjectNode parent) {
			Source = source;
			Parent = parent;
		}

		public EtnNorm Source { get; }
		public EtnSubjectNode Parent { get; }
		public IList<EtnSubjectNode> AllChildren { get; } = new List<EtnSubjectNode>();

		public IList<EtnSubjectNode> Children => visibleChildren ?? emptyChildren;

		internal bool Visible { get; set; } = true;

		internal void RefreshVisibleChildren() =>
			visibleChildren = AllChildren.Where(x => x.Visible).ToList();

		public int NormId => Source.NormId;
		public string Code => Source.NumberNorm;
		public string Name => Source.NormName;
		public EtnNormLevel Level => Source.Level;

		public bool Selectable => Level != EtnNormLevel.Section && Source.ItemsCount > 0;

		/// <summary>
		/// У некоторых событий приложения 2 вместо перечня СИЗ стоит текст приказа - показываем его
		/// вместо количества строк, иначе пункт выглядел бы пустым без объяснения.
		/// </summary>
		public string ItemsCountText =>
			Source.ItemsCount == 0 && !String.IsNullOrWhiteSpace(Source.Note)
				? Source.Note
				: Source.ItemsCount.ToString();

		private bool selected;
		public virtual bool Selected {
			get => selected;
			set => SetField(ref selected, value);
		}

		internal bool Contains(string search) =>
			(Name?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
			|| (Code?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
	}
}
