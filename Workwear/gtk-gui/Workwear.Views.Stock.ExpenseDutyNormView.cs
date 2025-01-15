
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Stock
{
	public partial class ExpenseDutyNormView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Table table2;

		private global::Gtk.Label label3;

		private global::Gtk.Label label4;

		private global::Gtk.Label label7;

		private global::Gtk.Label labelNorm;

		private global::QS.Views.Control.EntityEntry yentryNorm;

		private global::QS.Views.Control.EntityEntry yentryResponsible;

		private global::QS.Views.Control.EntityEntry yentryWarehouseExpense;

		private global::Gamma.GtkWidgets.yLabel ylabelId;

		private global::Gtk.Table table3;

		private global::Gtk.ScrolledWindow GtkScrolledWindowComments;

		private global::Gamma.GtkWidgets.yTextView ytextComment;

		private global::Gtk.Label label26;

		private global::Gtk.Label label5;

		private global::Gtk.Label label6;

		private global::QS.Widgets.GtkUI.DatePicker ydateDoc;

		private global::Gamma.GtkWidgets.yLabel ylabelCreatedBy;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeItems;

		private global::Gtk.HBox hbox6;

		private global::Gamma.GtkWidgets.yButton buttonAdd;

		private global::Gtk.Button buttonDel;

		private global::Gamma.GtkWidgets.yLabel labelSum;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Stock.ExpenseDutyNormView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Stock.ExpenseDutyNormView";
			// Container child Workwear.Views.Stock.ExpenseDutyNormView.Gtk.Container+ContainerChild
			this.dialog1_VBox = new global::Gtk.VBox();
			this.dialog1_VBox.Name = "dialog1_VBox";
			this.dialog1_VBox.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
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
			this.dialog1_VBox.Add(this.hbox4);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox4]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			this.hbox1.BorderWidth = ((uint)(3));
			// Container child hbox1.Gtk.Box+BoxChild
			this.table2 = new global::Gtk.Table(((uint)(4)), ((uint)(2)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Склад<span foreground=\"red\">*</span>:");
			this.label3.UseMarkup = true;
			this.table2.Add(this.label3);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table2[this.label3]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Номер:");
			this.table2.Add(this.label4);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table2[this.label4]));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label();
			this.label7.Name = "label7";
			this.label7.Xalign = 1F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString("Ответственный:");
			this.label7.UseMarkup = true;
			this.table2.Add(this.label7);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table2[this.label7]));
			w8.TopAttach = ((uint)(3));
			w8.BottomAttach = ((uint)(4));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.labelNorm = new global::Gtk.Label();
			this.labelNorm.Name = "labelNorm";
			this.labelNorm.Xalign = 1F;
			this.labelNorm.LabelProp = global::Mono.Unix.Catalog.GetString("Дежурная норма<span foreground=\"red\">*</span>:");
			this.labelNorm.UseMarkup = true;
			this.table2.Add(this.labelNorm);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table2[this.labelNorm]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryNorm = new global::QS.Views.Control.EntityEntry();
			this.yentryNorm.Events = ((global::Gdk.EventMask)(256));
			this.yentryNorm.Name = "yentryNorm";
			this.table2.Add(this.yentryNorm);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryNorm]));
			w10.TopAttach = ((uint)(2));
			w10.BottomAttach = ((uint)(3));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryResponsible = new global::QS.Views.Control.EntityEntry();
			this.yentryResponsible.Events = ((global::Gdk.EventMask)(256));
			this.yentryResponsible.Name = "yentryResponsible";
			this.table2.Add(this.yentryResponsible);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryResponsible]));
			w11.TopAttach = ((uint)(3));
			w11.BottomAttach = ((uint)(4));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryWarehouseExpense = new global::QS.Views.Control.EntityEntry();
			this.yentryWarehouseExpense.Events = ((global::Gdk.EventMask)(256));
			this.yentryWarehouseExpense.Name = "yentryWarehouseExpense";
			this.table2.Add(this.yentryWarehouseExpense);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryWarehouseExpense]));
			w12.TopAttach = ((uint)(1));
			w12.BottomAttach = ((uint)(2));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ylabelId = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelId.Name = "ylabelId";
			this.ylabelId.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel3");
			this.table2.Add(this.ylabelId);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table2[this.ylabelId]));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox1.Add(this.table2);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.table2]));
			w14.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.table3 = new global::Gtk.Table(((uint)(3)), ((uint)(2)), false);
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
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table3[this.GtkScrolledWindowComments]));
			w16.TopAttach = ((uint)(2));
			w16.BottomAttach = ((uint)(3));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label26 = new global::Gtk.Label();
			this.label26.Name = "label26";
			this.label26.Xalign = 1F;
			this.label26.Yalign = 0F;
			this.label26.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table3.Add(this.label26);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table3[this.label26]));
			w17.TopAttach = ((uint)(2));
			w17.BottomAttach = ((uint)(3));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Пользователь:");
			this.table3.Add(this.label5);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table3[this.label5]));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label6 = new global::Gtk.Label();
			this.label6.Name = "label6";
			this.label6.Xalign = 1F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString("Дата<span foreground=\"red\">*</span>:");
			this.label6.UseMarkup = true;
			this.table3.Add(this.label6);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table3[this.label6]));
			w19.TopAttach = ((uint)(1));
			w19.BottomAttach = ((uint)(2));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.ydateDoc = new global::QS.Widgets.GtkUI.DatePicker();
			this.ydateDoc.Events = ((global::Gdk.EventMask)(256));
			this.ydateDoc.Name = "ydateDoc";
			this.ydateDoc.WithTime = false;
			this.ydateDoc.HideCalendarButton = false;
			this.ydateDoc.Date = new global::System.DateTime(0);
			this.ydateDoc.IsEditable = true;
			this.ydateDoc.AutoSeparation = true;
			this.ydateDoc.HideButtonClearDate = false;
			this.table3.Add(this.ydateDoc);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table3[this.ydateDoc]));
			w20.TopAttach = ((uint)(1));
			w20.BottomAttach = ((uint)(2));
			w20.LeftAttach = ((uint)(1));
			w20.RightAttach = ((uint)(2));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.ylabelCreatedBy = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelCreatedBy.Name = "ylabelCreatedBy";
			this.ylabelCreatedBy.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel1");
			this.table3.Add(this.ylabelCreatedBy);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table3[this.ylabelCreatedBy]));
			w21.LeftAttach = ((uint)(1));
			w21.RightAttach = ((uint)(2));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox1.Add(this.table3);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.table3]));
			w22.Position = 1;
			this.dialog1_VBox.Add(this.hbox1);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox1]));
			w23.Position = 1;
			w23.Expand = false;
			w23.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeItems = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeItems.CanFocus = true;
			this.ytreeItems.Name = "ytreeItems";
			this.GtkScrolledWindow.Add(this.ytreeItems);
			this.dialog1_VBox.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.GtkScrolledWindow]));
			w25.Position = 2;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox6 = new global::Gtk.HBox();
			this.hbox6.Name = "hbox6";
			this.hbox6.Spacing = 6;
			// Container child hbox6.Gtk.Box+BoxChild
			this.buttonAdd = new global::Gamma.GtkWidgets.yButton();
			this.buttonAdd.CanFocus = true;
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.UseUnderline = true;
			this.buttonAdd.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w26 = new global::Gtk.Image();
			w26.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAdd.Image = w26;
			this.hbox6.Add(this.buttonAdd);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.buttonAdd]));
			w27.Position = 0;
			w27.Expand = false;
			w27.Fill = false;
			// Container child hbox6.Gtk.Box+BoxChild
			this.buttonDel = new global::Gtk.Button();
			this.buttonDel.Sensitive = false;
			this.buttonDel.CanFocus = true;
			this.buttonDel.Name = "buttonDel";
			this.buttonDel.UseUnderline = true;
			this.buttonDel.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w28 = new global::Gtk.Image();
			w28.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonDel.Image = w28;
			this.hbox6.Add(this.buttonDel);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.buttonDel]));
			w29.Position = 1;
			w29.Expand = false;
			w29.Fill = false;
			// Container child hbox6.Gtk.Box+BoxChild
			this.labelSum = new global::Gamma.GtkWidgets.yLabel();
			this.labelSum.Name = "labelSum";
			this.labelSum.Xalign = 1F;
			this.labelSum.LabelProp = global::Mono.Unix.Catalog.GetString("Количество:");
			this.labelSum.UseMarkup = true;
			this.hbox6.Add(this.labelSum);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.labelSum]));
			w30.Position = 2;
			this.dialog1_VBox.Add(this.hbox6);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox6]));
			w31.Position = 3;
			w31.Expand = false;
			w31.Fill = false;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonAdd.Clicked += new global::System.EventHandler(this.OnButtonAddClicked);
			this.buttonDel.Clicked += new global::System.EventHandler(this.OnButtonDelClicked);
		}
	}
}
