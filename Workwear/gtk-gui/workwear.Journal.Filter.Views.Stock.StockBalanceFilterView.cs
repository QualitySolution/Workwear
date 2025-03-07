
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Journal.Filter.Views.Stock
{
	public partial class StockBalanceFilterView
	{
		private global::Gtk.Table table1;

		private global::Gtk.HBox hbox3;

		private global::QS.Views.Control.EntityEntry entityWarehouse;

		private global::Gamma.GtkWidgets.yLabel ylabelOwner;

		private global::Gamma.Widgets.ySpecComboBox yspeccomboboxOwners;

		private global::Gtk.HBox hbox4;

		private global::Gamma.Widgets.yEnumComboBox yenumcomboboxAmount;

		private global::Gamma.GtkWidgets.yHBox yhboxDuty;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gamma.Widgets.yDatePicker ydateDate;

		private global::Gamma.GtkWidgets.yCheckButton chShowNegative;

		private global::Gamma.GtkWidgets.yLabel labelAmount;

		private global::Gamma.GtkWidgets.yLabel labelWarehouse;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Journal.Filter.Views.Stock.StockBalanceFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Journal.Filter.Views.Stock.StockBalanceFilterView";
			// Container child workwear.Journal.Filter.Views.Stock.StockBalanceFilterView.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(2)), ((uint)(2)), false);
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.entityWarehouse = new global::QS.Views.Control.EntityEntry();
			this.entityWarehouse.Events = ((global::Gdk.EventMask)(256));
			this.entityWarehouse.Name = "entityWarehouse";
			this.hbox3.Add(this.entityWarehouse);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.entityWarehouse]));
			w1.Position = 0;
			// Container child hbox3.Gtk.Box+BoxChild
			this.ylabelOwner = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelOwner.Name = "ylabelOwner";
			this.ylabelOwner.Xalign = 1F;
			this.ylabelOwner.LabelProp = global::Mono.Unix.Catalog.GetString("Собственник:");
			this.hbox3.Add(this.ylabelOwner);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.ylabelOwner]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox3.Gtk.Box+BoxChild
			this.yspeccomboboxOwners = new global::Gamma.Widgets.ySpecComboBox();
			this.yspeccomboboxOwners.Name = "yspeccomboboxOwners";
			this.yspeccomboboxOwners.AddIfNotExist = false;
			this.yspeccomboboxOwners.DefaultFirst = false;
			this.yspeccomboboxOwners.ShowSpecialStateAll = true;
			this.yspeccomboboxOwners.ShowSpecialStateNot = true;
			this.yspeccomboboxOwners.NameForSpecialStateNot = "Без собственника";
			this.hbox3.Add(this.yspeccomboboxOwners);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.yspeccomboboxOwners]));
			w3.Position = 2;
			w3.Expand = false;
			w3.Fill = false;
			this.table1.Add(this.hbox3);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox3]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hbox4 = new global::Gtk.HBox();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.yenumcomboboxAmount = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumcomboboxAmount.Name = "yenumcomboboxAmount";
			this.yenumcomboboxAmount.ShowSpecialStateAll = false;
			this.yenumcomboboxAmount.ShowSpecialStateNot = false;
			this.yenumcomboboxAmount.UseShortTitle = false;
			this.yenumcomboboxAmount.DefaultFirst = false;
			this.hbox4.Add(this.yenumcomboboxAmount);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.yenumcomboboxAmount]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.yhboxDuty = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxDuty.Name = "yhboxDuty";
			this.yhboxDuty.Spacing = 6;
			// Container child yhboxDuty.Gtk.Box+BoxChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.Xalign = 1F;
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Дата:");
			this.yhbox1.Add(this.ylabel1);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ylabel1]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ydateDate = new global::Gamma.Widgets.yDatePicker();
			this.ydateDate.Events = ((global::Gdk.EventMask)(256));
			this.ydateDate.Name = "ydateDate";
			this.ydateDate.WithTime = false;
			this.ydateDate.Date = new global::System.DateTime(0);
			this.ydateDate.IsEditable = true;
			this.ydateDate.AutoSeparation = true;
			this.yhbox1.Add(this.ydateDate);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ydateDate]));
			w7.Position = 1;
			this.yhboxDuty.Add(this.yhbox1);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.yhboxDuty[this.yhbox1]));
			w8.Position = 0;
			// Container child yhboxDuty.Gtk.Box+BoxChild
			this.chShowNegative = new global::Gamma.GtkWidgets.yCheckButton();
			this.chShowNegative.CanFocus = true;
			this.chShowNegative.Name = "chShowNegative";
			this.chShowNegative.Label = global::Mono.Unix.Catalog.GetString("Показать отрицательный баланс");
			this.chShowNegative.DrawIndicator = true;
			this.chShowNegative.UseUnderline = true;
			this.chShowNegative.Xalign = 1F;
			this.yhboxDuty.Add(this.chShowNegative);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.yhboxDuty[this.chShowNegative]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			this.hbox4.Add(this.yhboxDuty);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.yhboxDuty]));
			w10.Position = 1;
			this.table1.Add(this.hbox4);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox4]));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelAmount = new global::Gamma.GtkWidgets.yLabel();
			this.labelAmount.Name = "labelAmount";
			this.labelAmount.Xalign = 1F;
			this.labelAmount.LabelProp = global::Mono.Unix.Catalog.GetString("В количестве:");
			this.table1.Add(this.labelAmount);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.labelAmount]));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelWarehouse = new global::Gamma.GtkWidgets.yLabel();
			this.labelWarehouse.Name = "labelWarehouse";
			this.labelWarehouse.Xalign = 1F;
			this.labelWarehouse.LabelProp = global::Mono.Unix.Catalog.GetString("Склад:");
			this.table1.Add(this.labelWarehouse);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.labelWarehouse]));
			w13.TopAttach = ((uint)(1));
			w13.BottomAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.table1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
