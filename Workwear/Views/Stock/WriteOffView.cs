using System;
using System.Linq;
using Autofac;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Validation;
using QS.Views.Dialog;
using QSOrmProject;
using QSWidgetLib;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using Workwear.Measurements;
using workwear.Repository;
using workwear.Repository.Stock;
using workwear.Tools.Features;
using workwear.ViewModels.Stock;
using IdToStringConverter = Gamma.Binding.Converters.IdToStringConverter;

namespace workwear.Views.Stock
{
	public partial class WriteOffView : EntityDialogViewBase<WriteOffViewModel, Writeoff>
	{
		private enum ColumnTags { BuhDoc }
		
		public WriteOffView(WriteOffViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			ConfigureItems();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding
					.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter())
					.InitializeFromSource ();
				ylabelCreatedBy.Binding
					.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp)
					.InitializeFromSource ();
				ydateDoc.Binding
					.AddBinding(Entity, e => e.Date, w => w.Date)
					.InitializeFromSource();
				ytextComment.Binding
					.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
					.InitializeFromSource();
				labelSum.Binding
					.AddBinding(ViewModel, vm => vm.Total, w => w.LabelProp)
					.InitializeFromSource();
				buttonDel.Binding
					.AddBinding(ViewModel, vm => vm.DelSensitive, w => w.Sensitive)
					.InitializeFromSource();
		}

		private void ConfigureItems()
		{
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<WriteoffItem> ()
					.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
					.AddColumn("Размер").MinWidth(60)
						.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
						.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature?.Type?.SizeType, onlyUseInNomenclature:true).ToList())
						.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
					.AddColumn("Рост").MinWidth(70)
						.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
						.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature?.Type?.HeightType, onlyUseInNomenclature:true).ToList())
						.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
					.AddColumn ("Процент износа").AddNumericRenderer(e => e.WearPercent, new MultiplierToPercentConverter())
						.Editing(new Adjustment(0, 0, 999, 1, 10, 0)).WidthChars(6).Digits(0)
						.AddTextRenderer(e => "%", expand: false)
					.AddColumn ("Списано из").AddTextRenderer (e => e.LastOwnText)
					.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(7)
						.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
					.AddColumn("Бухгалтерский документ").Tag(ColumnTags.BuhDoc).AddTextRenderer(e => e.BuhDocument)
						.AddSetter((c, e) => c.Editable = e.WriteoffFrom == WriteoffFrom.Employee)
					.Finish ();
			
			ytreeItems.Binding
				.AddBinding(Entity, vm => vm.ObservableItems, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;
		}

		#region Methods
		private void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
			if (args.Event.Button != 3) return;
			var menu = new Menu();
			var selected = ytreeItems.GetSelectedObject<WriteoffItem>();
			var item = new MenuItemId<WriteoffItem>("Открыть номенклатуру");
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
			var item = ((MenuItemId<WriteoffItem>) sender).ID;
			ViewModel.NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(ViewModel, EntityUoWBuilder.ForOpen(item.Nomenclature.Id));
		}
		private void YtreeItems_Selection_Changed(object sender, EventArgs e) {
			ViewModel.DelSensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}
		#endregion

		//public WriteOffDocDlg() {
		// Build();
		// autofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
		// UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Writeoff> ();
		// Entity.Date = DateTime.Today;
		// Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
		// //ItemsTable.CurWarehouse = 
		// //	new StockRepository()
		// //		.GetDefaultWarehouse(UoW, autofacScope
		// //			.Resolve<FeaturesService>(), Entity.CreatedbyUser.Id);
		// //ItemsTable.SizeService = autofacScope.Resolve<SizeService>();
		// ConfigureDlg ();
		//}
		// public WriteOffDocDlg (EmployeeCard employee) : this () { 
		// 	//ItemsTable.CurWorker = employee;
		// }
		//
		// public WriteOffDocDlg (Subdivision facility) : this () {
		// 	//ItemsTable.CurObject = facility;
		// }
		//
		// public WriteOffDocDlg (Writeoff item) : this (item.Id) {}
		//
		// public WriteOffDocDlg (int id) {
		// 	Build ();
		// 	autofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
		// 	UoWGeneric = UnitOfWorkFactory.CreateForRoot<Writeoff> (id);
		// 	//ItemsTable.SizeService = autofacScope.Resolve<SizeService>();
		// 	ConfigureDlg ();
		// }

		// private void ConfigureDlg() {
		// 	ylabelId.Binding
		// 		.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter())
		// 		.InitializeFromSource ();
		// 	ylabelCreatedBy.Binding
		// 		.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp)
		// 		.InitializeFromSource ();
		// 	ydateDoc.Binding
		// 		.AddBinding(Entity, e => e.Date, w => w.Date)
		// 		.InitializeFromSource();
		// 	ytextComment.Binding
		// 		.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
		// 		.InitializeFromSource();
		// 	//ItemsTable.WriteOffDoc = Entity;
		// }			

		// public override bool Save() {
		// 	Logger.Info ("Запись документа...");
		// 	var valid = new QSValidator<Writeoff> (UoWGeneric.Root);
		// 	if (valid.RunDlgIfNotValid((Gtk.Window)Toplevel))
		// 		return false;
		// 	
		// 	Entity.UpdateOperations(UoW);
		// 	if (Entity.Id == 0)
		// 		Entity.CreationDate = DateTime.Now;
		// 	
		// 	UoWGeneric.Save ();
		// 	if(Entity.Items.Any(w => w.WriteoffFrom == WriteoffFrom.Employee)) {
		// 		Logger.Debug ("Обновляем записи о выданной одежде в карточке сотрудника...");
		// 		foreach(var employeeGroup in 
		// 			Entity.Items.Where(w => w.WriteoffFrom == WriteoffFrom.Employee)
		// 				.GroupBy(w => w.EmployeeWriteoffOperation.Employee))
		// 		{
		// 			var employee = employeeGroup.Key;
		// 			foreach(var itemsGroup in 
		// 				employeeGroup.GroupBy(i => i.Nomenclature.Type.Id))
		// 			{
		// 				var wearItem = 
		// 					employee.WorkwearItems.FirstOrDefault(i => i.ProtectionTools.Id == itemsGroup.Key);
		// 				if(wearItem == null) {
		// 					Logger.Debug ("Позиции <{0}> не требуется к выдаче, пропускаем...", 
		// 						itemsGroup.First ().Nomenclature.Type.Name);
		// 					continue;
		// 				}
		// 				wearItem.UpdateNextIssue (UoW);
		// 			}
		// 		}
		// 		UoWGeneric.Commit ();
		// 	}
		// 	Logger.Info ("Ok");
		// 	return true;
		// }
		// public override void Destroy() {
		// 	base.Destroy();
		// 	autofacScope.Dispose();
		// }

		#region Items

		// private enum ColumnTags { BuhDoc }
		// private Writeoff writeOffDoc;
		// public SizeService SizeService { get; set; }
		// public Writeoff WriteOffDoc {
		// 	get => writeOffDoc;
		// 	set { if (writeOffDoc == value)
		// 			return;
		// 		writeOffDoc = value;
		// 		ytreeItems.ItemsDataSource = writeOffDoc.ObservableItems;
		// 		writeOffDoc.ObservableItems.ListContentChanged += WriteoffDoc_ObservableItems_ListContentChanged;
		// 		CalculateTotal();
		// 		WriteOffDoc.Items.ToList().ForEach(item => item.PropertyChanged += Item_PropertyChanged);
		// 	}
		// }
		//
		// private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) {
		// 	if(e.PropertyName == nameof(WriteoffItem.BuhDocument))
		// 		((EntityDialogBase<Writeoff>) MyEntityDialog).HasChanges = true;
		// }
		//
		// private void WriteoffDoc_ObservableItems_ListContentChanged (object sender, EventArgs e) {
		// 	CalculateTotal();
		// }
		//
		// public EmployeeCard CurWorker { get; set;}
		//
		// public Subdivision CurObject { get; set;}
		//
		// public Warehouse CurWarehouse { get; set; }
		//
		// public WriteOffItemsView()
		// {
		// 	Build();
		//
		// 	ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<WriteoffItem> ()
		// 		.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
		// 		.AddColumn("Размер").MinWidth(60)
		// 			.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
		// 			.DynamicFillListFunc(x => SizeService.GetSize(UoW, x.Nomenclature?.Type?.SizeType, onlyUseInNomenclature:true).ToList())
		// 			.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
		// 		.AddColumn("Рост").MinWidth(70)
		// 			.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
		// 			.DynamicFillListFunc(x => SizeService.GetSize(UoW, x.Nomenclature?.Type?.HeightType, onlyUseInNomenclature:true).ToList())
		// 			.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
		// 		.AddColumn ("Процент износа").AddNumericRenderer(e => e.WearPercent, new MultiplierToPercentConverter())
		// 			.Editing(new Adjustment(0, 0, 999, 1, 10, 0)).WidthChars(6).Digits(0)
		// 			.AddTextRenderer(e => "%", expand: false)
		// 		.AddColumn ("Списано из").AddTextRenderer (e => e.LastOwnText)
		// 		.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(7)
		// 			.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
		// 		.AddColumn("Бухгалтерский документ").Tag(ColumnTags.BuhDoc).AddTextRenderer(e => e.BuhDocument)
		// 			.AddSetter((c, e) => c.Editable = e.WriteoffFrom == WriteoffFrom.Employee)
		// 		.Finish ();
		// 	ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
		// 	ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;
		// }
		// #region PopupMenu
		//
		//
		// private void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
		// 	if (args.Event.Button != 3) return;
		// 	var menu = new Menu();
		// 	var selected = ytreeItems.GetSelectedObject<WriteoffItem>();
		// 	var item = new MenuItemId<WriteoffItem>("Открыть номенклатуру");
		// 	item.ID = selected;
		// 	if(selected == null)
		// 		item.Sensitive = false;
		// 	else
		// 		item.Activated += Item_Activated;
		// 	menu.Add(item);
		// 	menu.ShowAll();
		// 	menu.Popup();
		// }
		//
		// private void Item_Activated(object sender, EventArgs e) {
		// 	var item = (sender as MenuItemId<WriteoffItem>).ID;
		// 	MainClass.MainWin.NavigationManager.
		// 		OpenViewModelOnTdi<NomenclatureViewModel, IEntityUoWBuilder>(MyTdiDialog, EntityUoWBuilder.ForOpen(item.Nomenclature.Id));
		// }
		//
		// #endregion
		// private void YtreeItems_Selection_Changed (object sender, EventArgs e) {
		// 	buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		// }
		//
		// protected void OnButtonAddStoreClicked (object sender, EventArgs e) {
		// 	var selectJournal = 
		// 		MainClass.MainWin.NavigationManager
		// 			.OpenViewModelOnTdi<StockBalanceJournalViewModel>(MyTdiDialog, QS.Navigation.OpenPageOptions.AsSlave);
		//
		// 	if(CurWarehouse != null) {
		// 		selectJournal.ViewModel.Filter.Warehouse = CurWarehouse;
		// 	}
		//
		// 	selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
		// 	selectJournal.ViewModel.OnSelectResult += SelectFromStock_OnSelectResult;
		// }
		//
		// private void SelectFromStock_OnSelectResult(object sender, JournalSelectedEventArgs e) {
		// 	var selectVM = sender as StockBalanceJournalViewModel;
		// 	foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
		// 		WriteOffDoc.AddItem(node.GetStockPosition(UoW), selectVM.Filter.Warehouse, node.Amount);
		// 	}
		// 	CalculateTotal();
		// }
		//
		// private void OnButtonDelClicked (object sender, EventArgs e) {
		// 	WriteOffDoc.RemoveItem (ytreeItems.GetSelectedObject<WriteoffItem> ());
		// 	CalculateTotal();
		// }
		//
		// private void OnButtonAddWorkerClicked(object sender, EventArgs e) {
		// 	var filter = new EmployeeBalanceFilter (MyOrmDialog.UoW);
		// 	var selectFromEmployeeDlg = 
		// 		new ReferenceRepresentation (new EmployeeBalanceVM (filter), "Выданное сотрудникам");
		// 	filter.parrentTab = selectFromEmployeeDlg;
		//
		// 	if(CurWorker != null)
		// 		filter.RestrictEmployee = CurWorker;
		//
		// 	selectFromEmployeeDlg.ShowFilter = CurWorker == null;
		// 	selectFromEmployeeDlg.Mode = OrmReferenceMode.MultiSelect;
		// 	selectFromEmployeeDlg.ObjectSelected += SelectFromEmployeeDlg_ObjectSelected;
		// 	OpenSlaveTab(selectFromEmployeeDlg);
		// }
		//
		// private void OnButtonAddObjectClicked(object sender, EventArgs e) {
		// 	var filter = new ObjectBalanceFilter (MyOrmDialog.UoW);
		// 	if (CurObject != null)
		// 		filter.RestrictObject = CurObject;
		//
		// 	var selectFromObjectDlg = new ReferenceRepresentation (new ViewModel.ObjectBalanceVM (filter),
		// 	                                                      "Выданное на подразделения");
		// 	selectFromObjectDlg.ShowFilter = CurObject == null;
		// 	selectFromObjectDlg.Mode = OrmReferenceMode.MultiSelect;
		// 	selectFromObjectDlg.ObjectSelected += SelectFromObjectDlg_ObjectSelected;;
		// 	OpenSlaveTab(selectFromObjectDlg);
		// }
		//
		// private void SelectFromObjectDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e) {
		// 	foreach(var node in e.GetNodes<ViewModel.ObjectBalanceVMNode> ()) {
		// 		WriteOffDoc.AddItem(MyOrmDialog.UoW.GetById<SubdivisionIssueOperation> (node.Id), node.Added - node.Removed);
		// 	}
		// 	CalculateTotal();
		// }
		//
		// private void SelectFromEmployeeDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e) {
		// 	foreach(var node in e.GetNodes<EmployeeBalanceVMNode> ()) {
		// 		WriteOffDoc.AddItem(MyOrmDialog.UoW.GetById<EmployeeIssueOperation> (node.Id), node.Added - node.Removed);
		// 	}
		// 	CalculateTotal();
		// }
		//
		// private void CalculateTotal() {
		// 	labelSum.Markup = String.Format ("Позиций в документе: <u>{0}</u>  Количество единиц: <u>{1}</u>",
		// 		WriteOffDoc.Items.Count,
		// 		WriteOffDoc.Items.Sum(x => x.Amount)
		// 	);
		// 	buttonFillBuhDoc.Sensitive = WriteOffDoc.Items.Count > 0;
		// }
		//
		// private void OnButtonFillBuhDocClicked(object sender, EventArgs e) {
		// 	using (var dlg = new Dialog("Введите бухгалтерский документ", MainClass.MainWin, DialogFlags.Modal)) {
		// 		var docEntry = new Entry(80);
		// 		if (writeOffDoc.Items.Any(x => x.WriteoffFrom == WriteoffFrom.Employee))
		// 			docEntry.Text = writeOffDoc.Items.First(x => x.WriteoffFrom == WriteoffFrom.Employee).BuhDocument;
		// 		docEntry.TooltipText = "Бухгалтерский документ по которому было произведено списание. Отобразится вместо подписи сотрудника в карточке.";
		// 		docEntry.ActivatesDefault = true;
		// 		dlg.VBox.Add(docEntry);
		// 		dlg.AddButton("Заменить", ResponseType.Ok);
		// 		dlg.AddButton("Отмена", ResponseType.Cancel);
		// 		dlg.DefaultResponse = ResponseType.Ok;
		// 		dlg.ShowAll();
		// 		if (dlg.Run() == (int)ResponseType.Ok) {
		// 			writeOffDoc.ObservableItems
		// 				.Where(x => x.WriteoffFrom == WriteoffFrom.Employee)
		// 				.ToList().ForEach(x => x.BuhDocument = docEntry.Text);
		// 		}
		// 		dlg.Destroy();
		// 	}
		// }

		#endregion
	}
}