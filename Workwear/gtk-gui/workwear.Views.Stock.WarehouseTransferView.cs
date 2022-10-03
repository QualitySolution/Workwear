
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Stock
{
	public partial class WarehouseTransferView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox1;

		private global::Gtk.HBox hbox3;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.Table table2;

		private global::QS.Widgets.GtkUI.DatePicker datepicker;

		private global::QS.Views.Control.EntityEntry entityentryWarehouseFrom;

		private global::QS.Views.Control.EntityEntry entityentryWarehouseTo;

		private global::Gamma.GtkWidgets.yEntry entryNumber;

		private global::Gamma.GtkWidgets.yEntry entryUser;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTextView ytextComment;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gtk.Label label5;

		private global::Gtk.Label label6;

		private global::Gtk.Label label7;

		private global::Gtk.HBox hbox7;

		private global::Gtk.Label label4;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gamma.GtkWidgets.yTreeView table;

		private global::Gtk.HBox hbox8;

		private global::Gtk.HBox hbox9;

		private global::Gtk.Button buttonAddItem;

		private global::Gtk.Button buttonRemoveItem;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Views.Stock.WarehouseTransferView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Views.Stock.WarehouseTransferView";
			// Container child workwear.Views.Stock.WarehouseTransferView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.buttonSave = new global::Gtk.Button();
			this.buttonSave.CanFocus = true;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.UseUnderline = true;
			this.buttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-save", global::Gtk.IconSize.Menu);
			this.buttonSave.Image = w1;
			this.hbox3.Add(this.buttonSave);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.buttonSave]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox3.Gtk.Box+BoxChild
			this.buttonCancel = new global::Gtk.Button();
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-revert-to-saved", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w3;
			this.hbox3.Add(this.buttonCancel);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.buttonCancel]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.hbox1.Add(this.hbox3);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.hbox3]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			this.vbox1.Add(this.hbox1);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox1]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.table2 = new global::Gtk.Table(((uint)(4)), ((uint)(4)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.datepicker = new global::QS.Widgets.GtkUI.DatePicker();
			this.datepicker.Events = ((global::Gdk.EventMask)(256));
			this.datepicker.Name = "datepicker";
			this.datepicker.WithTime = false;
			this.datepicker.HideCalendarButton = false;
			this.datepicker.Date = new global::System.DateTime(0);
			this.datepicker.IsEditable = true;
			this.datepicker.AutoSeparation = true;
			this.table2.Add(this.datepicker);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table2[this.datepicker]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.YOptions = ((global::Gtk.AttachOptions)(0));
			// Container child table2.Gtk.Table+TableChild
			this.entityentryWarehouseFrom = new global::QS.Views.Control.EntityEntry();
			this.entityentryWarehouseFrom.Events = ((global::Gdk.EventMask)(256));
			this.entityentryWarehouseFrom.Name = "entityentryWarehouseFrom";
			this.table2.Add(this.entityentryWarehouseFrom);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table2[this.entityentryWarehouseFrom]));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(1));
			// Container child table2.Gtk.Table+TableChild
			this.entityentryWarehouseTo = new global::QS.Views.Control.EntityEntry();
			this.entityentryWarehouseTo.Events = ((global::Gdk.EventMask)(256));
			this.entityentryWarehouseTo.Name = "entityentryWarehouseTo";
			this.table2.Add(this.entityentryWarehouseTo);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table2[this.entityentryWarehouseTo]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(2));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.entryNumber = new global::Gamma.GtkWidgets.yEntry();
			this.entryNumber.Sensitive = false;
			this.entryNumber.CanFocus = true;
			this.entryNumber.Name = "entryNumber";
			this.entryNumber.IsEditable = true;
			this.entryNumber.InvisibleChar = '●';
			this.table2.Add(this.entryNumber);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table2[this.entryNumber]));
			w10.LeftAttach = ((uint)(3));
			w10.RightAttach = ((uint)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.entryUser = new global::Gamma.GtkWidgets.yEntry();
			this.entryUser.Sensitive = false;
			this.entryUser.CanFocus = true;
			this.entryUser.Name = "entryUser";
			this.entryUser.IsEditable = false;
			this.entryUser.InvisibleChar = '●';
			this.table2.Add(this.entryUser);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table2[this.entryUser]));
			w11.TopAttach = ((uint)(1));
			w11.BottomAttach = ((uint)(2));
			w11.LeftAttach = ((uint)(3));
			w11.RightAttach = ((uint)(4));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytextComment = new global::Gamma.GtkWidgets.yTextView();
			this.ytextComment.CanFocus = true;
			this.ytextComment.Name = "ytextComment";
			this.ytextComment.AcceptsTab = false;
			this.ytextComment.WrapMode = ((global::Gtk.WrapMode)(3));
			this.GtkScrolledWindow.Add(this.ytextComment);
			this.table2.Add(this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table2[this.GtkScrolledWindow]));
			w13.TopAttach = ((uint)(2));
			w13.BottomAttach = ((uint)(4));
			w13.LeftAttach = ((uint)(3));
			w13.RightAttach = ((uint)(4));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Склад отправитель:");
			this.table2.Add(this.label1);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table2[this.label1]));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Склад получатель:");
			this.table2.Add(this.label2);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table2[this.label2]));
			w15.TopAttach = ((uint)(1));
			w15.BottomAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table2.Add(this.label3);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table2[this.label3]));
			w16.TopAttach = ((uint)(2));
			w16.BottomAttach = ((uint)(3));
			w16.LeftAttach = ((uint)(2));
			w16.RightAttach = ((uint)(3));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Номер:");
			this.table2.Add(this.label5);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table2[this.label5]));
			w17.LeftAttach = ((uint)(2));
			w17.RightAttach = ((uint)(3));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label6 = new global::Gtk.Label();
			this.label6.Name = "label6";
			this.label6.Xalign = 1F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString("Пользователь:");
			this.table2.Add(this.label6);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table2[this.label6]));
			w18.TopAttach = ((uint)(1));
			w18.BottomAttach = ((uint)(2));
			w18.LeftAttach = ((uint)(2));
			w18.RightAttach = ((uint)(3));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label();
			this.label7.Name = "label7";
			this.label7.Xalign = 1F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString("Дата:");
			this.table2.Add(this.label7);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table2[this.label7]));
			w19.TopAttach = ((uint)(2));
			w19.BottomAttach = ((uint)(3));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox1.Add(this.table2);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.table2]));
			w20.Position = 1;
			w20.Expand = false;
			w20.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox7 = new global::Gtk.HBox();
			this.hbox7.Name = "hbox7";
			this.hbox7.Spacing = 6;
			// Container child hbox7.Gtk.Box+BoxChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Номенклатура");
			this.hbox7.Add(this.label4);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.label4]));
			w21.Position = 0;
			w21.Expand = false;
			w21.Fill = false;
			this.vbox1.Add(this.hbox7);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox7]));
			w22.Position = 2;
			w22.Expand = false;
			w22.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.table = new global::Gamma.GtkWidgets.yTreeView();
			this.table.CanFocus = true;
			this.table.Name = "table";
			this.GtkScrolledWindow1.Add(this.table);
			this.vbox1.Add(this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow1]));
			w24.Position = 3;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox8 = new global::Gtk.HBox();
			this.hbox8.Name = "hbox8";
			this.hbox8.Spacing = 6;
			// Container child hbox8.Gtk.Box+BoxChild
			this.hbox9 = new global::Gtk.HBox();
			this.hbox9.Name = "hbox9";
			this.hbox9.Spacing = 6;
			// Container child hbox9.Gtk.Box+BoxChild
			this.buttonAddItem = new global::Gtk.Button();
			this.buttonAddItem.CanFocus = true;
			this.buttonAddItem.Name = "buttonAddItem";
			this.buttonAddItem.UseUnderline = true;
			this.buttonAddItem.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w25 = new global::Gtk.Image();
			w25.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddItem.Image = w25;
			this.hbox9.Add(this.buttonAddItem);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.hbox9[this.buttonAddItem]));
			w26.Position = 0;
			w26.Expand = false;
			w26.Fill = false;
			// Container child hbox9.Gtk.Box+BoxChild
			this.buttonRemoveItem = new global::Gtk.Button();
			this.buttonRemoveItem.Sensitive = false;
			this.buttonRemoveItem.CanFocus = true;
			this.buttonRemoveItem.Name = "buttonRemoveItem";
			this.buttonRemoveItem.UseUnderline = true;
			this.buttonRemoveItem.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w27 = new global::Gtk.Image();
			w27.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonRemoveItem.Image = w27;
			this.hbox9.Add(this.buttonRemoveItem);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.hbox9[this.buttonRemoveItem]));
			w28.Position = 1;
			w28.Expand = false;
			w28.Fill = false;
			this.hbox8.Add(this.hbox9);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.hbox9]));
			w29.Position = 0;
			w29.Expand = false;
			w29.Fill = false;
			this.vbox1.Add(this.hbox8);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox8]));
			w30.Position = 4;
			w30.Expand = false;
			w30.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonAddItem.Clicked += new global::System.EventHandler(this.OnButtonAddClicked);
			this.buttonRemoveItem.Clicked += new global::System.EventHandler(this.OnButtonDelClicked);
		}
	}
}