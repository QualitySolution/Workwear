﻿using System;
using System.Linq;
using Gtk;
using QS.Project.Domain;
using QS.Views.Dialog;
using QSOrmProject;
using QSWidgetLib;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;
using IdToStringConverter = Gamma.Binding.Converters.IdToStringConverter;

namespace Workwear.Views.Stock
{
	public partial class WriteOffView : EntityDialogViewBase<WriteOffViewModel, Writeoff>
	{
		public WriteOffView(WriteOffViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			ConfigureItems();
			CommonButtonSubscription();
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
				buttonAddObject.Sensitive = ViewModel.Employee is null;
				buttonAddWorker.Sensitive = ViewModel.Subdivision is null;
				buttonAddStore.Sensitive = ViewModel.Subdivision is null && ViewModel.Employee is null;
				buttonAddStore.Clicked += OnButtonAddStoreClicked;
				buttonAddWorker.Clicked += OnButtonAddFromEmployeeClicked;
				buttonAddObject.Clicked += OnButtonAddFromObjectClicked;
				buttonDel.Clicked += OnButtonDelClicked;
		}
		private void ConfigureItems()
		{
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<WriteoffItem> ()
					.AddColumn ("Наименование").Resizable().AddReadOnlyTextRenderer(e => e.Nomenclature?.Name ?? e.EmployeeWriteoffOperation.ProtectionTools?.Name)
						.AddSetter((w, item) => w.Foreground = item.Nomenclature != null ? "black" : "blue")
						.WrapWidth(700)
					.AddColumn("Размер").MinWidth(60)
						.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
						.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature?.Type?.SizeType, onlyUseInNomenclature:true).ToList())
						.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
					.AddColumn("Рост").MinWidth(70)
						.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
						.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature?.Type?.HeightType, onlyUseInNomenclature:true).ToList())
						.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
					.AddColumn("Собственники")
						.Visible(ViewModel.FeaturesService.Available(WorkwearFeature.Owners))
					.AddComboRenderer(x => x.Owner)
						.SetDisplayFunc(x => x.Name)
						.FillItems(ViewModel.Owners, "Нет")
						.AddSetter((c, n) => c.Editable = n.CanSetOwner)
					.AddColumn ("Процент износа").AddNumericRenderer(e => e.WearPercent, new MultiplierToPercentConverter())
						.Editing(new Adjustment(0, 0, 999, 1, 10, 0)).WidthChars(6).Digits(0)
						.AddTextRenderer(e => "%", expand: false)
					.AddColumn ("Списано из").AddTextRenderer (e => e.LastOwnText)
					.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(7)
						.AddReadOnlyTextRenderer(e => e.Nomenclature?.Type?.Units?.Name ?? e.EmployeeWriteoffOperation.ProtectionTools?.Type?.Units?.Name)
					.Finish ();
			
			ytreeItems.Binding
				.AddBinding(Entity, vm => vm.Items, w => w.ItemsDataSource)
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
			if(selected?.Nomenclature != null)
				item.Activated += Item_Activated;
			else
				item.Sensitive = false;
			menu.Add(item);
			menu.ShowAll();
			menu.Popup();
		}
		private void Item_Activated(object sender, EventArgs e) {
			var item = ((MenuItemId<WriteoffItem>) sender).ID;
			ViewModel.NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(
				ViewModel, EntityUoWBuilder.ForOpen(item.Nomenclature.Id));
		}

		private void YtreeItems_Selection_Changed(object sender, EventArgs e) => 
			ViewModel.DelSensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		protected void OnButtonAddStoreClicked(object sender, EventArgs e) => ViewModel.AddFromStock();

		private void OnButtonDelClicked(object sender, EventArgs e) => 
			ViewModel.DeleteItem(ytreeItems.GetSelectedObject<WriteoffItem>());

		private void OnButtonAddFromEmployeeClicked(object sender, EventArgs e) => ViewModel.AddFromEmployee();
		private void OnButtonAddFromObjectClicked(object sender, EventArgs e) => ViewModel.AddFromObject();
		#endregion
	}
}
