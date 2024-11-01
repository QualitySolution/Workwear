
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.ReportParameters.Views
{
	public partial class WarehouseTransferReportView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox1;

		private global::Gamma.GtkWidgets.yLabel ylabelDuration;

		private global::Gamma.Widgets.yDatePeriodPicker ydateperiodpicker;

		private global::Gtk.HBox hbox3;

		private global::Gamma.GtkWidgets.yLabel ylabelExpenseWarehouse;

		private global::Gamma.Widgets.ySpecComboBox yspeccomboboxWarehouseExpense;

		private global::Gtk.HBox hbox4;

		private global::Gamma.GtkWidgets.yLabel ylabelReceiptWarehouse;

		private global::Gamma.Widgets.ySpecComboBox yspeccomboboxWarehouseReceipt;

		private global::Gtk.HBox hbox5;

		private global::Gamma.GtkWidgets.yLabel ylabelOwner;

		private global::Gamma.Widgets.ySpecComboBox yspeccomboboxOwner;

		private global::Gamma.GtkWidgets.yButton buttonRun;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.ReportParameters.Views.WarehouseTransferReportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.ReportParameters.Views.WarehouseTransferReportView";
			// Container child Workwear.ReportParameters.Views.WarehouseTransferReportView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.ylabelDuration = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelDuration.Name = "ylabelDuration";
			this.ylabelDuration.LabelProp = global::Mono.Unix.Catalog.GetString("Период перемещения:");
			this.hbox1.Add(this.ylabelDuration);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.ylabelDuration]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.ydateperiodpicker = new global::Gamma.Widgets.yDatePeriodPicker();
			this.ydateperiodpicker.Events = ((global::Gdk.EventMask)(256));
			this.ydateperiodpicker.Name = "ydateperiodpicker";
			this.ydateperiodpicker.StartDate = new global::System.DateTime(0);
			this.ydateperiodpicker.EndDate = new global::System.DateTime(0);
			this.hbox1.Add(this.ydateperiodpicker);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.ydateperiodpicker]));
			w2.Position = 1;
			this.vbox1.Add(this.hbox1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox1]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.ylabelExpenseWarehouse = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelExpenseWarehouse.Name = "ylabelExpenseWarehouse";
			this.ylabelExpenseWarehouse.LabelProp = global::Mono.Unix.Catalog.GetString("Склад списания:");
			this.hbox3.Add(this.ylabelExpenseWarehouse);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.ylabelExpenseWarehouse]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox3.Gtk.Box+BoxChild
			this.yspeccomboboxWarehouseExpense = new global::Gamma.Widgets.ySpecComboBox();
			this.yspeccomboboxWarehouseExpense.Name = "yspeccomboboxWarehouseExpense";
			this.yspeccomboboxWarehouseExpense.AddIfNotExist = false;
			this.yspeccomboboxWarehouseExpense.DefaultFirst = false;
			this.yspeccomboboxWarehouseExpense.ShowSpecialStateAll = false;
			this.yspeccomboboxWarehouseExpense.ShowSpecialStateNot = false;
			this.yspeccomboboxWarehouseExpense.NameForSpecialStateNot = "";
			this.hbox3.Add(this.yspeccomboboxWarehouseExpense);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.yspeccomboboxWarehouseExpense]));
			w5.Position = 1;
			this.vbox1.Add(this.hbox3);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox3]));
			w6.Position = 1;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox4 = new global::Gtk.HBox();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.ylabelReceiptWarehouse = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelReceiptWarehouse.Name = "ylabelReceiptWarehouse";
			this.ylabelReceiptWarehouse.LabelProp = global::Mono.Unix.Catalog.GetString("Склад получения:");
			this.hbox4.Add(this.ylabelReceiptWarehouse);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.ylabelReceiptWarehouse]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.yspeccomboboxWarehouseReceipt = new global::Gamma.Widgets.ySpecComboBox();
			this.yspeccomboboxWarehouseReceipt.Name = "yspeccomboboxWarehouseReceipt";
			this.yspeccomboboxWarehouseReceipt.AddIfNotExist = false;
			this.yspeccomboboxWarehouseReceipt.DefaultFirst = false;
			this.yspeccomboboxWarehouseReceipt.ShowSpecialStateAll = false;
			this.yspeccomboboxWarehouseReceipt.ShowSpecialStateNot = false;
			this.yspeccomboboxWarehouseReceipt.NameForSpecialStateNot = "";
			this.hbox4.Add(this.yspeccomboboxWarehouseReceipt);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.yspeccomboboxWarehouseReceipt]));
			w8.Position = 1;
			this.vbox1.Add(this.hbox4);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox4]));
			w9.Position = 2;
			w9.Expand = false;
			w9.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox5 = new global::Gtk.HBox();
			this.hbox5.Name = "hbox5";
			this.hbox5.Spacing = 6;
			// Container child hbox5.Gtk.Box+BoxChild
			this.ylabelOwner = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelOwner.Name = "ylabelOwner";
			this.ylabelOwner.LabelProp = global::Mono.Unix.Catalog.GetString("Собственник:");
			this.hbox5.Add(this.ylabelOwner);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.ylabelOwner]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child hbox5.Gtk.Box+BoxChild
			this.yspeccomboboxOwner = new global::Gamma.Widgets.ySpecComboBox();
			this.yspeccomboboxOwner.Name = "yspeccomboboxOwner";
			this.yspeccomboboxOwner.AddIfNotExist = false;
			this.yspeccomboboxOwner.DefaultFirst = false;
			this.yspeccomboboxOwner.ShowSpecialStateAll = false;
			this.yspeccomboboxOwner.ShowSpecialStateNot = false;
			this.yspeccomboboxOwner.NameForSpecialStateNot = "";
			this.hbox5.Add(this.yspeccomboboxOwner);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.yspeccomboboxOwner]));
			w11.Position = 1;
			this.vbox1.Add(this.hbox5);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox5]));
			w12.Position = 3;
			w12.Expand = false;
			w12.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.buttonRun = new global::Gamma.GtkWidgets.yButton();
			this.buttonRun.CanFocus = true;
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.UseUnderline = true;
			this.buttonRun.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vbox1.Add(this.buttonRun);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.buttonRun]));
			w13.PackType = ((global::Gtk.PackType)(1));
			w13.Position = 4;
			w13.Expand = false;
			w13.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
