using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using Gtk;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QSOrmProject;
using QSWidgetLib;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Stock;
using Workwear.Measurements;
using workwear.Representations.Organization;
using Workwear.Tools.Features;
using workwear.ViewModel;
using Workwear.ViewModels.Stock;
using Workwear.ViewModels.Stock.Widgets;

namespace workwear
{
	[ToolboxItem(true)]
	public partial class IncomeDocItemsView : WidgetOnDialogBase
	{
		private enum ColumnTags { BuhDoc }
		private Income incomeDoc;
		public SizeService SizeService { get; set; }
		public IInteractiveMessage Interactive { get; set; }
		public Income IncomeDoc {
			get => incomeDoc;
			set { if (incomeDoc == value)
					return;
				incomeDoc = value;
				ytreeItems.ItemsDataSource = incomeDoc.ObservableItems;
				incomeDoc.ObservableItems.ListContentChanged += IncomeDoc_ObservableItems_ListContentChanged;
				IncomeDoc.PropertyChanged += IncomeDoc_PropertyChanged;
				IncomeDoc_PropertyChanged(null,
					new PropertyChangedEventArgs(IncomeDoc.GetPropertyName(d => d.Operation)));
				CalculateTotal();
				IncomeDoc.Items.ToList().ForEach(item => item.PropertyChanged += Item_PropertyChanged);
				if(incomeDoc.Operation != IncomeOperations.Enter) buttonAddSizes.Visible = false;
			}
		}

		private FeaturesService featuresService = MainClass.AppDIContainer.BeginLifetimeScope().Resolve<FeaturesService>();
		public IList<Owner> Owners = UnitOfWorkFactory.CreateWithoutRoot().GetAll<Owner>().ToList();

		private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(IncomeItem.BuhDocument)) {
				((EntityDialogBase<Income>) MyEntityDialog).HasChanges = true;
			}
		}

		private void IncomeDoc_ObservableItems_ListContentChanged (object sender, EventArgs e) {
			CalculateTotal();
		}

		private void IncomeDoc_PropertyChanged (object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == IncomeDoc.GetPropertyName (d => d.Operation) 
				|| e.PropertyName == IncomeDoc.GetPropertyName (d => d.EmployeeCard)
				|| e.PropertyName == IncomeDoc.GetPropertyName (d => d.Subdivision))
			{
				buttonAdd.Sensitive = IncomeDoc.Operation == IncomeOperations.Return && IncomeDoc.EmployeeCard != null 
					|| IncomeDoc.Operation == IncomeOperations.Object && IncomeDoc.Subdivision != null 
					|| IncomeDoc.Operation == IncomeOperations.Enter;
			}

			if(e.PropertyName == nameof(IncomeDoc.Operation))
				buttonAddSizes.Visible = IncomeDoc.Operation == IncomeOperations.Enter;

			if (e.PropertyName != IncomeDoc.GetPropertyName(x => x.Operation)) return;
			var buhDocColumn = ytreeItems.ColumnsConfig.GetColumnsByTag(ColumnTags.BuhDoc).First();
			buhDocColumn.Visible = IncomeDoc.Operation == IncomeOperations.Return;
			buttonFillBuhDoc.Visible = IncomeDoc.Operation == IncomeOperations.Return;
		}

		public IncomeDocItemsView() {
			this.Build();
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<IncomeItem> ()
				.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name).WrapWidth(700)
				.AddColumn("Сертификат").AddTextRenderer(e => e.Certificate).Editable()
				.AddColumn("Размер").MinWidth(60)
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => SizeService.GetSize(UoW, x.Nomenclature.Type.SizeType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature.Type.SizeType != null 
					                                  && incomeDoc.Operation == IncomeOperations.Enter)
				.AddColumn("Рост").MinWidth(70)
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => SizeService.GetSize(UoW, x.Nomenclature.Type.HeightType,onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature.Type.HeightType != null 
					                                  && incomeDoc.Operation == IncomeOperations.Enter)
				.AddColumn("Собственники")
					.Visible(featuresService.Available(WorkwearFeature.Owners))
					.AddComboRenderer(x => x.Owner)
					.SetDisplayFunc(x => x.Name)
					.FillItems(Owners, "Нет")
				.Editing()
				.AddColumn ("Процент износа")
					.AddNumericRenderer (e => e.WearPercent, new MultiplierToPercentConverter())
					.Editing (new Adjustment(0,0,999,1,10,0)).WidthChars(6).Digits(0)
					.AddTextRenderer (e => "%", expand: false)
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount)
					.Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(8)
					.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				.AddColumn ("Стоимость").AddNumericRenderer (e => e.Cost)
					.Editing (new Adjustment(0,0,100000000,100,1000,0)).Digits (2).WidthChars(12)
				.AddColumn("Сумма").AddNumericRenderer(x => x.Total).Digits(2)
				.AddColumn("Бухгалтерский документ").Tag(ColumnTags.BuhDoc)
					.AddTextRenderer(e => e.BuhDocument).Editable()
				.Finish ();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += YtreeItemsButtonReleaseEvent;
		}

		#region PopupMenu
		private void YtreeItemsButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
			if (args.Event.Button != 3) return;
			var menu = new Menu();
			var selected = ytreeItems.GetSelectedObject<IncomeItem>();
			var item = new MenuItemId<IncomeItem>("Открыть номенклатуру");
			item.ID = selected;
			if(selected == null)
				item.Sensitive = false;
			else
				item.Activated += Item_Activated;
			menu.Add(item);
			menu.ShowAll();
			menu.Popup();
		}

		private void Item_Activated(object sender, EventArgs e) {
			var item = ((MenuItemId<IncomeItem>) sender).ID;
			MainClass.MainWin.NavigationManager
				.OpenViewModelOnTdi<NomenclatureViewModel, IEntityUoWBuilder>
					(MyTdiDialog, EntityUoWBuilder.ForOpen(item.Nomenclature.Id));
		}
		#endregion
		private void YtreeItems_Selection_Changed(object sender, EventArgs e) {
			buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
			if(ytreeItems.GetSelectedObject<IncomeItem>() != null) {
				var obj = ytreeItems.GetSelectedObject<IncomeItem>();
				var sizeType = obj.Nomenclature?.Type?.SizeType;
				var heightType = obj.Nomenclature?.Type?.SizeType;
				if(sizeType != null)
					buttonAddSizes.Sensitive = ytreeItems.Selection.CountSelectedRows() != 1 || 
					                           !SizeService.GetSize(UoW, sizeType).Any() || 
					                           !(heightType is null) && SizeService.GetSize(UoW, heightType).Any();
				else
					buttonAddSizes.Sensitive = false;
			}
			else
				buttonAddSizes.Sensitive = false;
		}

		private void OnButtonAddClicked (object sender, EventArgs e) {
			if(IncomeDoc.Operation == IncomeOperations.Return) {
				var vm = new EmployeeBalanceVM(UoW);
				vm.Employee = IncomeDoc.EmployeeCard;
				var selectFromEmployeeDlg = new ReferenceRepresentation (vm, $"Выданное {IncomeDoc.EmployeeCard.ShortName}");
				selectFromEmployeeDlg.Mode = OrmReferenceMode.MultiSelect;
				selectFromEmployeeDlg.ObjectSelected += SelectFromEmployeeDlg_ObjectSelected;
				OpenSlaveTab(selectFromEmployeeDlg);
			}

			if(IncomeDoc.Operation == IncomeOperations.Object) {
				var selectFromObjectDlg = new ReferenceRepresentation (new ViewModel.ObjectBalanceVM (IncomeDoc.Subdivision),
				                                                       $"Выданное на {IncomeDoc.Subdivision.Name}");
				selectFromObjectDlg.Mode = OrmReferenceMode.MultiSelect;
				selectFromObjectDlg.ObjectSelected += SelectFromObjectDlg_ObjectSelected;;
				OpenSlaveTab(selectFromObjectDlg);
			}

			if (IncomeDoc.Operation == IncomeOperations.Enter) {
				var selectJournal =
					MainClass.MainWin.NavigationManager
						.OpenViewModelOnTdi<NomenclatureJournalViewModel>(MyTdiDialog, OpenPageOptions.AsSlave);
				selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
				selectJournal.ViewModel.OnSelectResult += AddNomenclature_OnSelectResult;
			}
		}

		private void AddNomenclature_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()))
				.ToList().ForEach(n => IncomeDoc.AddItem(n, Interactive));
			CalculateTotal();
		}

		private void SelectFromObjectDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e) {
			foreach(var node in e.GetNodes<ObjectBalanceVMNode> ()) {
				IncomeDoc.AddItem(MyOrmDialog.UoW.GetById<SubdivisionIssueOperation>(node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		private void SelectFromEmployeeDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e) {
			foreach(var node in e.GetNodes<EmployeeBalanceVMNode> ()) {
				IncomeDoc.AddItem(MyOrmDialog.UoW.GetById<EmployeeIssueOperation>(node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		private void OnButtonDelClicked (object sender, EventArgs e) {
			IncomeDoc.RemoveItem(ytreeItems.GetSelectedObject<IncomeItem> ());
			buttonAddSizes.Sensitive = false;
			CalculateTotal();
		}

		private void CalculateTotal() {
			labelSum.Markup =
				$"Позиций в документе: <u>{IncomeDoc.Items.Count}</u>  " +
				$"Количество единиц: <u>{IncomeDoc.Items.Sum(x => x.Amount)}</u>  " +
				$"Сумма: <u>{IncomeDoc.Items.Sum(x => x.Total):C}</u>";
			buttonFillBuhDoc.Sensitive = IncomeDoc.Items.Count > 0;
		}

		private void OnButtonFillBuhDocClicked(object sender, EventArgs e) {
			using (var dlg = new Dialog("Введите бухгалтерский документ", MainClass.MainWin, DialogFlags.Modal)) {
				var docEntry = new Entry(80);
				if (incomeDoc.Items.Count > 0)
					docEntry.Text = incomeDoc.Items.First().BuhDocument;
				docEntry.TooltipText = "Бухгалтерский документ по которому была произведена выдача. " +
				                       "Отобразится вместо подписи сотрудника в карточке.";
				docEntry.ActivatesDefault = true;
				dlg.VBox.Add(docEntry);
				dlg.AddButton("Заменить", ResponseType.Ok);
				dlg.AddButton("Отмена", ResponseType.Cancel);
				dlg.DefaultResponse = ResponseType.Ok;
				dlg.ShowAll();
				if (dlg.Run() == (int)ResponseType.Ok) {
					incomeDoc.ObservableItems.ToList().ForEach(x => x.BuhDocument = docEntry.Text);
				}
				dlg.Destroy();
			}
		}

		private void OnButtonAddSizesClicked(object sender, EventArgs e) {
			var item = ytreeItems.GetSelectedObject<IncomeItem>();
			if(item.Nomenclature == null)
				return;

			var existItems = IncomeDoc.Items.Where(i => i.Nomenclature.IsSame(item.Nomenclature) && i.Owner == item.Owner).Cast<IDocItemSizeInfo>().ToList();
			var page = MainClass.MainWin.NavigationManager.OpenViewModel<SizeWidgetViewModel, IDocItemSizeInfo, IUnitOfWork, IList<IDocItemSizeInfo>>
				(null, item, UoW, existItems);
			page.ViewModel.AddedSizes += SelectWearSize_SizeSelected;
		}
		private void SelectWearSize_SizeSelected(object sender , AddedSizesEventArgs e) {
			var item = ytreeItems.GetSelectedObject<IncomeItem>();
			foreach (var i in e.SizesWithAmount.ToList()) {
				var exist = IncomeDoc.FindItem(item.Nomenclature, i.Size, e.Height, item.Owner);
				if(exist != null)
					exist.Amount = i.Amount;
				else 
					IncomeDoc.AddItem(item.Nomenclature,  i.Size, e.Height, i.Amount, item.Certificate, item.Cost, item.Owner);
			}
			if(item.WearSize == null)
				IncomeDoc.RemoveItem(item);
			CalculateTotal();
		}
	}
}

