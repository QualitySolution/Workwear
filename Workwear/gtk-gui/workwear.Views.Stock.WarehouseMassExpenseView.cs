
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Stock
{
	public partial class WarehouseMassExpenseView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox10;

		private global::Gtk.HBox hbox11;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.HBox hbox12;

		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox13;

		private global::Gtk.Label label2;

		private global::QS.Views.Control.EntityEntry entityentryWarehouseExpense;

		private global::Gtk.HBox hbox14;

		private global::Gtk.Label label3;

		private global::QS.Widgets.GtkUI.DatePicker datepicker;

		private global::Gtk.ScrolledWindow GtkScrolledWindow3;

		private global::Gamma.GtkWidgets.yLabel textMessage;

		private global::Gtk.HBox hbox8;

		private global::Gtk.Label label6;

		private global::Gtk.ScrolledWindow GtkScrolledWindow2;

		private global::Gamma.GtkWidgets.yTextView ytextComment;

		private global::Gtk.HBox hbox6;

		private global::Gtk.Label label1;

		private global::Gtk.HBox hbox7;

		private global::Gtk.Button buttonIssuanceSheetCreate;

		private global::Gtk.Button buttonIssuanceSheetOpen;

		private global::Gtk.Button buttonIssuanceSheetPrint;

		private global::Gtk.Label label4;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView tableEmployee;

		private global::Gtk.HBox hbox3;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Button buttonAddEmployee;

		private global::Gtk.Button buttonRemoveEmployee;

		private global::Gtk.Button buttonCreateEmployee;

		private global::Gtk.Label label5;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gamma.GtkWidgets.yTreeView tableNomenclature;

		private global::Gtk.HBox hbox5;

		private global::Gtk.HBox hbox9;

		private global::Gtk.Button buttonAddNomenclature;

		private global::Gtk.Button buttonRemoveNomenclature;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Views.Stock.WarehouseMassExpenseView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Views.Stock.WarehouseMassExpenseView";
			// Container child workwear.Views.Stock.WarehouseMassExpenseView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox10 = new global::Gtk.HBox();
			this.hbox10.Name = "hbox10";
			this.hbox10.Spacing = 6;
			// Container child hbox10.Gtk.Box+BoxChild
			this.hbox11 = new global::Gtk.HBox();
			this.hbox11.Name = "hbox11";
			this.hbox11.Spacing = 6;
			// Container child hbox11.Gtk.Box+BoxChild
			this.buttonSave = new global::Gtk.Button();
			this.buttonSave.CanFocus = true;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.UseUnderline = true;
			this.buttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-save", global::Gtk.IconSize.Menu);
			this.buttonSave.Image = w1;
			this.hbox11.Add(this.buttonSave);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox11[this.buttonSave]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox11.Gtk.Box+BoxChild
			this.buttonCancel = new global::Gtk.Button();
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-revert-to-saved", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w3;
			this.hbox11.Add(this.buttonCancel);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox11[this.buttonCancel]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.hbox10.Add(this.hbox11);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox10[this.hbox11]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			this.vbox1.Add(this.hbox10);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox10]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox12 = new global::Gtk.HBox();
			this.hbox12.Name = "hbox12";
			this.hbox12.Spacing = 6;
			// Container child hbox12.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox13 = new global::Gtk.HBox();
			this.hbox13.Name = "hbox13";
			this.hbox13.Spacing = 6;
			// Container child hbox13.Gtk.Box+BoxChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Склад:");
			this.hbox13.Add(this.label2);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox13[this.label2]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child hbox13.Gtk.Box+BoxChild
			this.entityentryWarehouseExpense = new global::QS.Views.Control.EntityEntry();
			this.entityentryWarehouseExpense.Events = ((global::Gdk.EventMask)(256));
			this.entityentryWarehouseExpense.Name = "entityentryWarehouseExpense";
			this.hbox13.Add(this.entityentryWarehouseExpense);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox13[this.entityentryWarehouseExpense]));
			w8.Position = 1;
			this.vbox2.Add(this.hbox13);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox13]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox14 = new global::Gtk.HBox();
			this.hbox14.Name = "hbox14";
			this.hbox14.Spacing = 6;
			// Container child hbox14.Gtk.Box+BoxChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Дата:");
			this.hbox14.Add(this.label3);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox14[this.label3]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child hbox14.Gtk.Box+BoxChild
			this.datepicker = new global::QS.Widgets.GtkUI.DatePicker();
			this.datepicker.Events = ((global::Gdk.EventMask)(256));
			this.datepicker.Name = "datepicker";
			this.datepicker.WithTime = false;
			this.datepicker.HideCalendarButton = false;
			this.datepicker.Date = new global::System.DateTime(0);
			this.datepicker.IsEditable = true;
			this.datepicker.AutoSeparation = true;
			this.hbox14.Add(this.datepicker);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hbox14[this.datepicker]));
			w11.Position = 1;
			this.vbox2.Add(this.hbox14);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox14]));
			w12.Position = 1;
			w12.Expand = false;
			w12.Fill = false;
			this.hbox12.Add(this.vbox2);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox12[this.vbox2]));
			w13.Position = 0;
			// Container child hbox12.Gtk.Box+BoxChild
			this.GtkScrolledWindow3 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow3.Name = "GtkScrolledWindow3";
			this.GtkScrolledWindow3.HscrollbarPolicy = ((global::Gtk.PolicyType)(2));
			this.GtkScrolledWindow3.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow3.Gtk.Container+ContainerChild
			global::Gtk.Viewport w14 = new global::Gtk.Viewport();
			w14.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.textMessage = new global::Gamma.GtkWidgets.yLabel();
			this.textMessage.Name = "textMessage";
			w14.Add(this.textMessage);
			this.GtkScrolledWindow3.Add(w14);
			this.hbox12.Add(this.GtkScrolledWindow3);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hbox12[this.GtkScrolledWindow3]));
			w17.Position = 1;
			this.vbox1.Add(this.hbox12);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox12]));
			w18.Position = 1;
			w18.Expand = false;
			w18.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox8 = new global::Gtk.HBox();
			this.hbox8.Name = "hbox8";
			this.hbox8.Spacing = 6;
			// Container child hbox8.Gtk.Box+BoxChild
			this.label6 = new global::Gtk.Label();
			this.label6.Name = "label6";
			this.label6.Xalign = 1F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.hbox8.Add(this.label6);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.label6]));
			w19.Position = 0;
			w19.Expand = false;
			w19.Fill = false;
			// Container child hbox8.Gtk.Box+BoxChild
			this.GtkScrolledWindow2 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow2.Name = "GtkScrolledWindow2";
			this.GtkScrolledWindow2.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow2.Gtk.Container+ContainerChild
			this.ytextComment = new global::Gamma.GtkWidgets.yTextView();
			this.ytextComment.CanFocus = true;
			this.ytextComment.Name = "ytextComment";
			this.ytextComment.AcceptsTab = false;
			this.ytextComment.WrapMode = ((global::Gtk.WrapMode)(3));
			this.GtkScrolledWindow2.Add(this.ytextComment);
			this.hbox8.Add(this.GtkScrolledWindow2);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.GtkScrolledWindow2]));
			w21.Position = 1;
			this.vbox1.Add(this.hbox8);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox8]));
			w22.Position = 2;
			w22.Expand = false;
			w22.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox6 = new global::Gtk.HBox();
			this.hbox6.Name = "hbox6";
			this.hbox6.Spacing = 6;
			// Container child hbox6.Gtk.Box+BoxChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Создать:");
			this.hbox6.Add(this.label1);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.label1]));
			w23.Position = 0;
			w23.Expand = false;
			w23.Fill = false;
			// Container child hbox6.Gtk.Box+BoxChild
			this.hbox7 = new global::Gtk.HBox();
			this.hbox7.Name = "hbox7";
			this.hbox7.Spacing = 6;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonIssuanceSheetCreate = new global::Gtk.Button();
			this.buttonIssuanceSheetCreate.CanFocus = true;
			this.buttonIssuanceSheetCreate.Name = "buttonIssuanceSheetCreate";
			this.buttonIssuanceSheetCreate.UseUnderline = true;
			this.buttonIssuanceSheetCreate.Label = global::Mono.Unix.Catalog.GetString("Создать");
			global::Gtk.Image w24 = new global::Gtk.Image();
			w24.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-new", global::Gtk.IconSize.Menu);
			this.buttonIssuanceSheetCreate.Image = w24;
			this.hbox7.Add(this.buttonIssuanceSheetCreate);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonIssuanceSheetCreate]));
			w25.Position = 0;
			w25.Expand = false;
			w25.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonIssuanceSheetOpen = new global::Gtk.Button();
			this.buttonIssuanceSheetOpen.CanFocus = true;
			this.buttonIssuanceSheetOpen.Name = "buttonIssuanceSheetOpen";
			this.buttonIssuanceSheetOpen.UseUnderline = true;
			this.buttonIssuanceSheetOpen.Label = global::Mono.Unix.Catalog.GetString("Открыть");
			global::Gtk.Image w26 = new global::Gtk.Image();
			w26.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-edit", global::Gtk.IconSize.Menu);
			this.buttonIssuanceSheetOpen.Image = w26;
			this.hbox7.Add(this.buttonIssuanceSheetOpen);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonIssuanceSheetOpen]));
			w27.Position = 1;
			w27.Expand = false;
			w27.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonIssuanceSheetPrint = new global::Gtk.Button();
			this.buttonIssuanceSheetPrint.CanFocus = true;
			this.buttonIssuanceSheetPrint.Name = "buttonIssuanceSheetPrint";
			this.buttonIssuanceSheetPrint.UseUnderline = true;
			this.buttonIssuanceSheetPrint.Label = global::Mono.Unix.Catalog.GetString("Печать");
			global::Gtk.Image w28 = new global::Gtk.Image();
			w28.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-print", global::Gtk.IconSize.Menu);
			this.buttonIssuanceSheetPrint.Image = w28;
			this.hbox7.Add(this.buttonIssuanceSheetPrint);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonIssuanceSheetPrint]));
			w29.Position = 2;
			w29.Expand = false;
			w29.Fill = false;
			this.hbox6.Add(this.hbox7);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.hbox7]));
			w30.Position = 1;
			w30.Expand = false;
			w30.Fill = false;
			this.vbox1.Add(this.hbox6);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox6]));
			w31.Position = 3;
			w31.Expand = false;
			w31.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 0F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Сотрудники");
			this.vbox1.Add(this.label4);
			global::Gtk.Box.BoxChild w32 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.label4]));
			w32.Position = 4;
			w32.Expand = false;
			w32.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.tableEmployee = new global::Gamma.GtkWidgets.yTreeView();
			this.tableEmployee.CanFocus = true;
			this.tableEmployee.Name = "tableEmployee";
			this.GtkScrolledWindow.Add(this.tableEmployee);
			this.vbox1.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w34 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow]));
			w34.Position = 5;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.hbox4 = new global::Gtk.HBox();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.buttonAddEmployee = new global::Gtk.Button();
			this.buttonAddEmployee.CanFocus = true;
			this.buttonAddEmployee.Name = "buttonAddEmployee";
			this.buttonAddEmployee.UseUnderline = true;
			this.buttonAddEmployee.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w35 = new global::Gtk.Image();
			w35.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddEmployee.Image = w35;
			this.hbox4.Add(this.buttonAddEmployee);
			global::Gtk.Box.BoxChild w36 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.buttonAddEmployee]));
			w36.Position = 0;
			w36.Expand = false;
			w36.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.buttonRemoveEmployee = new global::Gtk.Button();
			this.buttonRemoveEmployee.Sensitive = false;
			this.buttonRemoveEmployee.CanFocus = true;
			this.buttonRemoveEmployee.Name = "buttonRemoveEmployee";
			this.buttonRemoveEmployee.UseUnderline = true;
			this.buttonRemoveEmployee.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w37 = new global::Gtk.Image();
			w37.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonRemoveEmployee.Image = w37;
			this.hbox4.Add(this.buttonRemoveEmployee);
			global::Gtk.Box.BoxChild w38 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.buttonRemoveEmployee]));
			w38.Position = 1;
			w38.Expand = false;
			w38.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.buttonCreateEmployee = new global::Gtk.Button();
			this.buttonCreateEmployee.CanFocus = true;
			this.buttonCreateEmployee.Name = "buttonCreateEmployee";
			this.buttonCreateEmployee.UseUnderline = true;
			this.buttonCreateEmployee.Label = global::Mono.Unix.Catalog.GetString("Создать");
			global::Gtk.Image w39 = new global::Gtk.Image();
			w39.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-new", global::Gtk.IconSize.Menu);
			this.buttonCreateEmployee.Image = w39;
			this.hbox4.Add(this.buttonCreateEmployee);
			global::Gtk.Box.BoxChild w40 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.buttonCreateEmployee]));
			w40.Position = 2;
			w40.Expand = false;
			w40.Fill = false;
			this.hbox3.Add(this.hbox4);
			global::Gtk.Box.BoxChild w41 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.hbox4]));
			w41.Position = 0;
			w41.Expand = false;
			w41.Fill = false;
			this.vbox1.Add(this.hbox3);
			global::Gtk.Box.BoxChild w42 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox3]));
			w42.Position = 6;
			w42.Expand = false;
			w42.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 0F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Номенклатура");
			this.vbox1.Add(this.label5);
			global::Gtk.Box.BoxChild w43 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.label5]));
			w43.Position = 7;
			w43.Expand = false;
			w43.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.tableNomenclature = new global::Gamma.GtkWidgets.yTreeView();
			this.tableNomenclature.CanFocus = true;
			this.tableNomenclature.Name = "tableNomenclature";
			this.GtkScrolledWindow1.Add(this.tableNomenclature);
			this.vbox1.Add(this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w45 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow1]));
			w45.Position = 8;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox5 = new global::Gtk.HBox();
			this.hbox5.Name = "hbox5";
			this.hbox5.Spacing = 6;
			// Container child hbox5.Gtk.Box+BoxChild
			this.hbox9 = new global::Gtk.HBox();
			this.hbox9.Name = "hbox9";
			this.hbox9.Spacing = 6;
			// Container child hbox9.Gtk.Box+BoxChild
			this.buttonAddNomenclature = new global::Gtk.Button();
			this.buttonAddNomenclature.Sensitive = false;
			this.buttonAddNomenclature.CanFocus = true;
			this.buttonAddNomenclature.Name = "buttonAddNomenclature";
			this.buttonAddNomenclature.UseUnderline = true;
			this.buttonAddNomenclature.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w46 = new global::Gtk.Image();
			w46.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddNomenclature.Image = w46;
			this.hbox9.Add(this.buttonAddNomenclature);
			global::Gtk.Box.BoxChild w47 = ((global::Gtk.Box.BoxChild)(this.hbox9[this.buttonAddNomenclature]));
			w47.Position = 0;
			w47.Expand = false;
			w47.Fill = false;
			// Container child hbox9.Gtk.Box+BoxChild
			this.buttonRemoveNomenclature = new global::Gtk.Button();
			this.buttonRemoveNomenclature.Sensitive = false;
			this.buttonRemoveNomenclature.CanFocus = true;
			this.buttonRemoveNomenclature.Name = "buttonRemoveNomenclature";
			this.buttonRemoveNomenclature.UseUnderline = true;
			this.buttonRemoveNomenclature.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w48 = new global::Gtk.Image();
			w48.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonRemoveNomenclature.Image = w48;
			this.hbox9.Add(this.buttonRemoveNomenclature);
			global::Gtk.Box.BoxChild w49 = ((global::Gtk.Box.BoxChild)(this.hbox9[this.buttonRemoveNomenclature]));
			w49.Position = 1;
			w49.Expand = false;
			w49.Fill = false;
			this.hbox5.Add(this.hbox9);
			global::Gtk.Box.BoxChild w50 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.hbox9]));
			w50.Position = 0;
			w50.Expand = false;
			w50.Fill = false;
			this.vbox1.Add(this.hbox5);
			global::Gtk.Box.BoxChild w51 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox5]));
			w51.Position = 9;
			w51.Expand = false;
			w51.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonIssuanceSheetCreate.Clicked += new global::System.EventHandler(this.OnButtonIssuanceSheetCreateClicked);
			this.buttonIssuanceSheetOpen.Clicked += new global::System.EventHandler(this.OnButtonIssuanceSheetOpenClicked);
			this.buttonIssuanceSheetPrint.Clicked += new global::System.EventHandler(this.OnButtonIssuanceSheetPrintClicked);
			this.buttonAddEmployee.Clicked += new global::System.EventHandler(this.OnButtonAddEmployeeClicked);
			this.buttonRemoveEmployee.Clicked += new global::System.EventHandler(this.OnButtonRemoveEmployeeClicked);
			this.buttonCreateEmployee.Clicked += new global::System.EventHandler(this.OnButtonCreateEmployeeClicked);
			this.buttonAddNomenclature.Clicked += new global::System.EventHandler(this.OnButtonAddNomenclatureClicked);
			this.buttonRemoveNomenclature.Clicked += new global::System.EventHandler(this.OnButtonRemoveNomenclatureClicked);
		}
	}
}
