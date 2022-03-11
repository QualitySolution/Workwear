
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Stock
{
	public partial class CompletionView
	{
		private global::Gamma.GtkWidgets.yVBox yvbox1;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.VSeparator vseparator1;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Table table2;

		private global::QS.Views.Control.EntityEntry entityWarehouseExpense;

		private global::QS.Views.Control.EntityEntry entityWarehouseReceipt;

		private global::Gtk.Label labelResult;

		private global::Gtk.Label labelSource;

		private global::Gtk.Table table3;

		private global::Gtk.ScrolledWindow GtkScrolledWindowComments;

		private global::Gamma.GtkWidgets.yTextView ytextComment;

		private global::Gtk.Label label26;

		private global::Gtk.Label label4;

		private global::Gtk.Label label5;

		private global::Gtk.Label label6;

		private global::QS.Widgets.GtkUI.DatePicker ydateDoc;

		private global::Gamma.GtkWidgets.yLabel ylabelCreatedBy;

		private global::Gamma.GtkWidgets.yLabel ylabelId;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox6;

		private global::Gtk.Label label1;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeExpenseItems;

		private global::Gtk.HBox hbox7;

		private global::Gamma.GtkWidgets.yButton buttonAddExpenseNomenclature;

		private global::Gamma.GtkWidgets.yButton buttonDelExpenseNomenclature;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gtk.VBox vbox3;

		private global::Gtk.HBox hbox8;

		private global::Gtk.Label label2;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gamma.GtkWidgets.yTreeView ytreeReceiptItems;

		private global::Gtk.HBox hbox9;

		private global::Gamma.GtkWidgets.yButton buttonAddReceiptNomenclature;

		private global::Gamma.GtkWidgets.yButton buttonDelReceiptNomenclature;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Views.Stock.CompletionView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Views.Stock.CompletionView";
			// Container child workwear.Views.Stock.CompletionView.Gtk.Container+ContainerChild
			this.yvbox1 = new global::Gamma.GtkWidgets.yVBox();
			this.yvbox1.Name = "yvbox1";
			this.yvbox1.Spacing = 6;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.hbox4 = new global::Gtk.HBox();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.buttonSave = new global::Gtk.Button();
			this.buttonSave.CanFocus = true;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.UseUnderline = true;
			this.buttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-save", global::Gtk.IconSize.Menu);
			this.buttonSave.Image = w1;
			this.hbox4.Add(this.buttonSave);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.buttonSave]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.buttonCancel = new global::Gtk.Button();
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-revert-to-saved", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w3;
			this.hbox4.Add(this.buttonCancel);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.buttonCancel]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.vseparator1 = new global::Gtk.VSeparator();
			this.vseparator1.Name = "vseparator1";
			this.hbox4.Add(this.vseparator1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.vseparator1]));
			w5.Position = 2;
			w5.Expand = false;
			w5.Fill = false;
			this.yvbox1.Add(this.hbox4);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.hbox4]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			this.hbox1.BorderWidth = ((uint)(3));
			// Container child hbox1.Gtk.Box+BoxChild
			this.table2 = new global::Gtk.Table(((uint)(2)), ((uint)(2)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.entityWarehouseExpense = new global::QS.Views.Control.EntityEntry();
			this.entityWarehouseExpense.Events = ((global::Gdk.EventMask)(256));
			this.entityWarehouseExpense.Name = "entityWarehouseExpense";
			this.table2.Add(this.entityWarehouseExpense);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table2[this.entityWarehouseExpense]));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.entityWarehouseReceipt = new global::QS.Views.Control.EntityEntry();
			this.entityWarehouseReceipt.Events = ((global::Gdk.EventMask)(256));
			this.entityWarehouseReceipt.Name = "entityWarehouseReceipt";
			this.table2.Add(this.entityWarehouseReceipt);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table2[this.entityWarehouseReceipt]));
			w8.TopAttach = ((uint)(1));
			w8.BottomAttach = ((uint)(2));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.labelResult = new global::Gtk.Label();
			this.labelResult.Name = "labelResult";
			this.labelResult.Xalign = 1F;
			this.labelResult.LabelProp = global::Mono.Unix.Catalog.GetString("Склад получения результа<span foreground=\"red\">*</span>:");
			this.labelResult.UseMarkup = true;
			this.table2.Add(this.labelResult);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table2[this.labelResult]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.labelSource = new global::Gtk.Label();
			this.labelSource.Name = "labelSource";
			this.labelSource.Xalign = 1F;
			this.labelSource.LabelProp = global::Mono.Unix.Catalog.GetString("Склад комплектующих<span foreground=\"red\">*</span>:");
			this.labelSource.UseMarkup = true;
			this.table2.Add(this.labelSource);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table2[this.labelSource]));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox1.Add(this.table2);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.table2]));
			w11.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.table3 = new global::Gtk.Table(((uint)(4)), ((uint)(2)), false);
			this.table3.Name = "table3";
			this.table3.RowSpacing = ((uint)(6));
			this.table3.ColumnSpacing = ((uint)(6));
			// Container child table3.Gtk.Table+TableChild
			this.GtkScrolledWindowComments = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindowComments.Name = "GtkScrolledWindowComments";
			this.GtkScrolledWindowComments.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindowComments.Gtk.Container+ContainerChild
			this.ytextComment = new global::Gamma.GtkWidgets.yTextView();
			this.ytextComment.CanFocus = true;
			this.ytextComment.Name = "ytextComment";
			this.ytextComment.AcceptsTab = false;
			this.ytextComment.WrapMode = ((global::Gtk.WrapMode)(3));
			this.GtkScrolledWindowComments.Add(this.ytextComment);
			this.table3.Add(this.GtkScrolledWindowComments);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table3[this.GtkScrolledWindowComments]));
			w13.TopAttach = ((uint)(3));
			w13.BottomAttach = ((uint)(4));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label26 = new global::Gtk.Label();
			this.label26.Name = "label26";
			this.label26.Xalign = 1F;
			this.label26.Yalign = 0F;
			this.label26.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table3.Add(this.label26);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table3[this.label26]));
			w14.TopAttach = ((uint)(3));
			w14.BottomAttach = ((uint)(4));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Номер:");
			this.table3.Add(this.label4);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table3[this.label4]));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Пользователь:");
			this.table3.Add(this.label5);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table3[this.label5]));
			w16.TopAttach = ((uint)(1));
			w16.BottomAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label6 = new global::Gtk.Label();
			this.label6.Name = "label6";
			this.label6.Xalign = 1F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString("Дата<span foreground=\"red\">*</span>:");
			this.label6.UseMarkup = true;
			this.table3.Add(this.label6);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table3[this.label6]));
			w17.TopAttach = ((uint)(2));
			w17.BottomAttach = ((uint)(3));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.ydateDoc = new global::QS.Widgets.GtkUI.DatePicker();
			this.ydateDoc.Events = ((global::Gdk.EventMask)(256));
			this.ydateDoc.Name = "ydateDoc";
			this.ydateDoc.WithTime = false;
			this.ydateDoc.HideCalendarButton = false;
			this.ydateDoc.Date = new global::System.DateTime(0);
			this.ydateDoc.IsEditable = true;
			this.ydateDoc.AutoSeparation = true;
			this.table3.Add(this.ydateDoc);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table3[this.ydateDoc]));
			w18.TopAttach = ((uint)(2));
			w18.BottomAttach = ((uint)(3));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(2));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.ylabelCreatedBy = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelCreatedBy.Name = "ylabelCreatedBy";
			this.ylabelCreatedBy.LabelProp = global::Mono.Unix.Catalog.GetString("user");
			this.table3.Add(this.ylabelCreatedBy);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table3[this.ylabelCreatedBy]));
			w19.TopAttach = ((uint)(1));
			w19.BottomAttach = ((uint)(2));
			w19.LeftAttach = ((uint)(1));
			w19.RightAttach = ((uint)(2));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.ylabelId = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelId.Name = "ylabelId";
			this.ylabelId.LabelProp = global::Mono.Unix.Catalog.GetString("id");
			this.table3.Add(this.ylabelId);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table3[this.ylabelId]));
			w20.LeftAttach = ((uint)(1));
			w20.RightAttach = ((uint)(2));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox1.Add(this.table3);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.table3]));
			w21.Position = 1;
			this.yvbox1.Add(this.hbox1);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.hbox1]));
			w22.Position = 1;
			w22.Expand = false;
			w22.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.WidthRequest = 0;
			this.yhbox1.HeightRequest = 0;
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox6 = new global::Gtk.HBox();
			this.hbox6.Name = "hbox6";
			this.hbox6.Spacing = 6;
			// Container child hbox6.Gtk.Box+BoxChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 0F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Комплектующие");
			this.hbox6.Add(this.label1);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.label1]));
			w23.Position = 0;
			w23.Expand = false;
			w23.Fill = false;
			this.vbox2.Add(this.hbox6);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox6]));
			w24.Position = 0;
			w24.Expand = false;
			w24.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeExpenseItems = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeExpenseItems.CanFocus = true;
			this.ytreeExpenseItems.Name = "ytreeExpenseItems";
			this.GtkScrolledWindow.Add(this.ytreeExpenseItems);
			this.vbox2.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.GtkScrolledWindow]));
			w26.Position = 1;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox7 = new global::Gtk.HBox();
			this.hbox7.Name = "hbox7";
			this.hbox7.Spacing = 6;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonAddExpenseNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.buttonAddExpenseNomenclature.CanFocus = true;
			this.buttonAddExpenseNomenclature.Name = "buttonAddExpenseNomenclature";
			this.buttonAddExpenseNomenclature.UseUnderline = true;
			this.buttonAddExpenseNomenclature.Label = global::Mono.Unix.Catalog.GetString("Добавить номенклатуру");
			global::Gtk.Image w27 = new global::Gtk.Image();
			w27.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddExpenseNomenclature.Image = w27;
			this.hbox7.Add(this.buttonAddExpenseNomenclature);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonAddExpenseNomenclature]));
			w28.Position = 0;
			w28.Expand = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonDelExpenseNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.buttonDelExpenseNomenclature.CanFocus = true;
			this.buttonDelExpenseNomenclature.Name = "buttonDelExpenseNomenclature";
			this.buttonDelExpenseNomenclature.UseUnderline = true;
			this.buttonDelExpenseNomenclature.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			this.hbox7.Add(this.buttonDelExpenseNomenclature);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonDelExpenseNomenclature]));
			w29.Position = 1;
			w29.Expand = false;
			w29.Fill = false;
			this.vbox2.Add(this.hbox7);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox7]));
			w30.Position = 2;
			w30.Expand = false;
			w30.Fill = false;
			this.yhbox1.Add(this.vbox2);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.vbox2]));
			w31.Position = 0;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Sensitive = false;
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString(">");
			this.yhbox1.Add(this.ylabel1);
			global::Gtk.Box.BoxChild w32 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ylabel1]));
			w32.Position = 1;
			w32.Expand = false;
			w32.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.vbox3 = new global::Gtk.VBox();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.hbox8 = new global::Gtk.HBox();
			this.hbox8.Name = "hbox8";
			this.hbox8.Spacing = 6;
			// Container child hbox8.Gtk.Box+BoxChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 0F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Результат");
			this.hbox8.Add(this.label2);
			global::Gtk.Box.BoxChild w33 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.label2]));
			w33.Position = 0;
			w33.Expand = false;
			w33.Fill = false;
			this.vbox3.Add(this.hbox8);
			global::Gtk.Box.BoxChild w34 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.hbox8]));
			w34.Position = 0;
			w34.Expand = false;
			w34.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.ytreeReceiptItems = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeReceiptItems.CanFocus = true;
			this.ytreeReceiptItems.Name = "ytreeReceiptItems";
			this.GtkScrolledWindow1.Add(this.ytreeReceiptItems);
			this.vbox3.Add(this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w36 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.GtkScrolledWindow1]));
			w36.Position = 1;
			// Container child vbox3.Gtk.Box+BoxChild
			this.hbox9 = new global::Gtk.HBox();
			this.hbox9.Name = "hbox9";
			this.hbox9.Spacing = 6;
			// Container child hbox9.Gtk.Box+BoxChild
			this.buttonAddReceiptNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.buttonAddReceiptNomenclature.CanFocus = true;
			this.buttonAddReceiptNomenclature.Name = "buttonAddReceiptNomenclature";
			this.buttonAddReceiptNomenclature.UseUnderline = true;
			this.buttonAddReceiptNomenclature.Label = global::Mono.Unix.Catalog.GetString("Добавить номенклатуру");
			global::Gtk.Image w37 = new global::Gtk.Image();
			w37.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddReceiptNomenclature.Image = w37;
			this.hbox9.Add(this.buttonAddReceiptNomenclature);
			global::Gtk.Box.BoxChild w38 = ((global::Gtk.Box.BoxChild)(this.hbox9[this.buttonAddReceiptNomenclature]));
			w38.Position = 0;
			w38.Expand = false;
			// Container child hbox9.Gtk.Box+BoxChild
			this.buttonDelReceiptNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.buttonDelReceiptNomenclature.CanFocus = true;
			this.buttonDelReceiptNomenclature.Name = "buttonDelReceiptNomenclature";
			this.buttonDelReceiptNomenclature.UseUnderline = true;
			this.buttonDelReceiptNomenclature.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			this.hbox9.Add(this.buttonDelReceiptNomenclature);
			global::Gtk.Box.BoxChild w39 = ((global::Gtk.Box.BoxChild)(this.hbox9[this.buttonDelReceiptNomenclature]));
			w39.Position = 1;
			w39.Expand = false;
			w39.Fill = false;
			this.vbox3.Add(this.hbox9);
			global::Gtk.Box.BoxChild w40 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.hbox9]));
			w40.Position = 2;
			w40.Expand = false;
			w40.Fill = false;
			this.yhbox1.Add(this.vbox3);
			global::Gtk.Box.BoxChild w41 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.vbox3]));
			w41.Position = 2;
			this.yvbox1.Add(this.yhbox1);
			global::Gtk.Box.BoxChild w42 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.yhbox1]));
			w42.Position = 2;
			this.Add(this.yvbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Show();
		}
	}
}
