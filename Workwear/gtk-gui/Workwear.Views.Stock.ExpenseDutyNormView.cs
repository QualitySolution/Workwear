
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

		private global::Gtk.HBox yAutoNumber;

		private global::Gamma.GtkWidgets.yEntry entryId;

		private global::Gamma.GtkWidgets.yCheckButton checkAuto;

		private global::QS.Views.Control.EntityEntry yentryNorm;

		private global::QS.Views.Control.EntityEntry yentryResponsible;

		private global::QS.Views.Control.EntityEntry yentryWarehouseExpense;

		private global::Gtk.Table table3;

		private global::Gtk.ScrolledWindow GtkScrolledWindowComments;

		private global::Gamma.GtkWidgets.yTextView ytextComment;

		private global::Gtk.HBox hbox8;

		private global::Gamma.GtkWidgets.yButton buttonColorsLegend;

		private global::Gtk.Label label26;

		private global::Gtk.Label label5;

		private global::Gtk.Label label6;

		private global::QS.Widgets.GtkUI.DatePicker ydateDoc;

		private global::Gamma.GtkWidgets.yLabel ylabelCreatedBy;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeItems;

		private global::Gtk.HBox hbox7;

		private global::Gamma.GtkWidgets.yButton ybuttonAdd;

		private global::Gamma.GtkWidgets.yButton ybuttonDel;

		private global::Gamma.GtkWidgets.yButton ybuttonChoosePositions;

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
			this.yAutoNumber = new global::Gtk.HBox();
			this.yAutoNumber.Name = "yAutoNumber";
			this.yAutoNumber.Spacing = 6;
			// Container child yAutoNumber.Gtk.Box+BoxChild
			this.entryId = new global::Gamma.GtkWidgets.yEntry();
			this.entryId.Sensitive = false;
			this.entryId.CanFocus = true;
			this.entryId.Name = "entryId";
			this.entryId.Text = global::Mono.Unix.Catalog.GetString("авто");
			this.entryId.IsEditable = true;
			this.entryId.WidthChars = 10;
			this.entryId.MaxLength = 15;
			this.entryId.InvisibleChar = '●';
			this.yAutoNumber.Add(this.entryId);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.yAutoNumber[this.entryId]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child yAutoNumber.Gtk.Box+BoxChild
			this.checkAuto = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkAuto.CanFocus = true;
			this.checkAuto.Name = "checkAuto";
			this.checkAuto.Label = global::Mono.Unix.Catalog.GetString("Автоматически");
			this.checkAuto.Active = true;
			this.checkAuto.DrawIndicator = true;
			this.checkAuto.UseUnderline = true;
			this.yAutoNumber.Add(this.checkAuto);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.yAutoNumber[this.checkAuto]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			this.table2.Add(this.yAutoNumber);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table2[this.yAutoNumber]));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryNorm = new global::QS.Views.Control.EntityEntry();
			this.yentryNorm.Events = ((global::Gdk.EventMask)(256));
			this.yentryNorm.Name = "yentryNorm";
			this.table2.Add(this.yentryNorm);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryNorm]));
			w13.TopAttach = ((uint)(2));
			w13.BottomAttach = ((uint)(3));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryResponsible = new global::QS.Views.Control.EntityEntry();
			this.yentryResponsible.Events = ((global::Gdk.EventMask)(256));
			this.yentryResponsible.Name = "yentryResponsible";
			this.table2.Add(this.yentryResponsible);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryResponsible]));
			w14.TopAttach = ((uint)(3));
			w14.BottomAttach = ((uint)(4));
			w14.LeftAttach = ((uint)(1));
			w14.RightAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryWarehouseExpense = new global::QS.Views.Control.EntityEntry();
			this.yentryWarehouseExpense.Events = ((global::Gdk.EventMask)(256));
			this.yentryWarehouseExpense.Name = "yentryWarehouseExpense";
			this.table2.Add(this.yentryWarehouseExpense);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryWarehouseExpense]));
			w15.TopAttach = ((uint)(1));
			w15.BottomAttach = ((uint)(2));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox1.Add(this.table2);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.table2]));
			w16.Position = 0;
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
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table3[this.GtkScrolledWindowComments]));
			w18.TopAttach = ((uint)(2));
			w18.BottomAttach = ((uint)(3));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(2));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.hbox8 = new global::Gtk.HBox();
			this.hbox8.Name = "hbox8";
			this.hbox8.Spacing = 6;
			// Container child hbox8.Gtk.Box+BoxChild
			this.buttonColorsLegend = new global::Gamma.GtkWidgets.yButton();
			this.buttonColorsLegend.TooltipMarkup = "Цветовая легенда";
			this.buttonColorsLegend.CanFocus = true;
			this.buttonColorsLegend.Name = "buttonColorsLegend";
			this.buttonColorsLegend.UseUnderline = true;
			global::Gtk.Image w19 = new global::Gtk.Image();
			w19.Pixbuf = global::Gdk.Pixbuf.LoadFromResource("Workwear.icon.buttons.legend.png");
			this.buttonColorsLegend.Image = w19;
			this.hbox8.Add(this.buttonColorsLegend);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.buttonColorsLegend]));
			w20.PackType = ((global::Gtk.PackType)(1));
			w20.Position = 1;
			w20.Expand = false;
			w20.Fill = false;
			this.table3.Add(this.hbox8);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table3[this.hbox8]));
			w21.TopAttach = ((uint)(3));
			w21.BottomAttach = ((uint)(4));
			w21.LeftAttach = ((uint)(1));
			w21.RightAttach = ((uint)(2));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label26 = new global::Gtk.Label();
			this.label26.Name = "label26";
			this.label26.Xalign = 1F;
			this.label26.Yalign = 0F;
			this.label26.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table3.Add(this.label26);
			global::Gtk.Table.TableChild w22 = ((global::Gtk.Table.TableChild)(this.table3[this.label26]));
			w22.TopAttach = ((uint)(2));
			w22.BottomAttach = ((uint)(3));
			w22.XOptions = ((global::Gtk.AttachOptions)(4));
			w22.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Пользователь:");
			this.table3.Add(this.label5);
			global::Gtk.Table.TableChild w23 = ((global::Gtk.Table.TableChild)(this.table3[this.label5]));
			w23.XOptions = ((global::Gtk.AttachOptions)(4));
			w23.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label6 = new global::Gtk.Label();
			this.label6.Name = "label6";
			this.label6.Xalign = 1F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString("Дата<span foreground=\"red\">*</span>:");
			this.label6.UseMarkup = true;
			this.table3.Add(this.label6);
			global::Gtk.Table.TableChild w24 = ((global::Gtk.Table.TableChild)(this.table3[this.label6]));
			w24.TopAttach = ((uint)(1));
			w24.BottomAttach = ((uint)(2));
			w24.XOptions = ((global::Gtk.AttachOptions)(4));
			w24.YOptions = ((global::Gtk.AttachOptions)(4));
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
			global::Gtk.Table.TableChild w25 = ((global::Gtk.Table.TableChild)(this.table3[this.ydateDoc]));
			w25.TopAttach = ((uint)(1));
			w25.BottomAttach = ((uint)(2));
			w25.LeftAttach = ((uint)(1));
			w25.RightAttach = ((uint)(2));
			w25.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.ylabelCreatedBy = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelCreatedBy.Name = "ylabelCreatedBy";
			this.ylabelCreatedBy.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel1");
			this.table3.Add(this.ylabelCreatedBy);
			global::Gtk.Table.TableChild w26 = ((global::Gtk.Table.TableChild)(this.table3[this.ylabelCreatedBy]));
			w26.LeftAttach = ((uint)(1));
			w26.RightAttach = ((uint)(2));
			w26.XOptions = ((global::Gtk.AttachOptions)(4));
			w26.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox1.Add(this.table3);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.table3]));
			w27.Position = 1;
			this.dialog1_VBox.Add(this.hbox1);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox1]));
			w28.Position = 1;
			w28.Expand = false;
			w28.Fill = false;
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
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.GtkScrolledWindow]));
			w30.Position = 2;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox7 = new global::Gtk.HBox();
			this.hbox7.Name = "hbox7";
			this.hbox7.Spacing = 6;
			// Container child hbox7.Gtk.Box+BoxChild
			this.ybuttonAdd = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonAdd.CanFocus = true;
			this.ybuttonAdd.Name = "ybuttonAdd";
			this.ybuttonAdd.UseUnderline = true;
			this.ybuttonAdd.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w31 = new global::Gtk.Image();
			w31.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.ybuttonAdd.Image = w31;
			this.hbox7.Add(this.ybuttonAdd);
			global::Gtk.Box.BoxChild w32 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.ybuttonAdd]));
			w32.Position = 0;
			w32.Expand = false;
			w32.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.ybuttonDel = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonDel.Sensitive = false;
			this.ybuttonDel.CanFocus = true;
			this.ybuttonDel.Name = "ybuttonDel";
			this.ybuttonDel.UseUnderline = true;
			this.ybuttonDel.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w33 = new global::Gtk.Image();
			w33.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.ybuttonDel.Image = w33;
			this.hbox7.Add(this.ybuttonDel);
			global::Gtk.Box.BoxChild w34 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.ybuttonDel]));
			w34.Position = 1;
			w34.Expand = false;
			w34.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.ybuttonChoosePositions = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonChoosePositions.Sensitive = false;
			this.ybuttonChoosePositions.CanFocus = true;
			this.ybuttonChoosePositions.Name = "ybuttonChoosePositions";
			this.ybuttonChoosePositions.UseUnderline = true;
			this.ybuttonChoosePositions.Label = global::Mono.Unix.Catalog.GetString("Выбрать варианты");
			global::Gtk.Image w35 = new global::Gtk.Image();
			w35.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-convert", global::Gtk.IconSize.Menu);
			this.ybuttonChoosePositions.Image = w35;
			this.hbox7.Add(this.ybuttonChoosePositions);
			global::Gtk.Box.BoxChild w36 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.ybuttonChoosePositions]));
			w36.Position = 2;
			w36.Expand = false;
			w36.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.labelSum = new global::Gamma.GtkWidgets.yLabel();
			this.labelSum.Name = "labelSum";
			this.labelSum.Xalign = 1F;
			this.labelSum.LabelProp = global::Mono.Unix.Catalog.GetString("Количество:");
			this.labelSum.UseMarkup = true;
			this.hbox7.Add(this.labelSum);
			global::Gtk.Box.BoxChild w37 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.labelSum]));
			w37.Position = 3;
			this.dialog1_VBox.Add(this.hbox7);
			global::Gtk.Box.BoxChild w38 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox7]));
			w38.Position = 3;
			w38.Expand = false;
			w38.Fill = false;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonColorsLegend.Clicked += new global::System.EventHandler(this.OnButtonColorsLegendClicked);
			this.ybuttonAdd.Clicked += new global::System.EventHandler(this.OnYbuttonAddClicked);
			this.ybuttonDel.Clicked += new global::System.EventHandler(this.OnYbuttonDelClicked);
			this.ybuttonChoosePositions.Clicked += new global::System.EventHandler(this.OnYbuttonChoosePositionsClicked);
		}
	}
}
