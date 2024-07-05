
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Stock
{
	public partial class OverNormView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Table table2;

		private global::QS.Views.Control.EntityEntry entityWarehouseExpense;

		private global::Gamma.Widgets.yEnumComboBox enumTypesComboBox;

		private global::Gtk.Label label4;

		private global::Gtk.HBox yAutoNumber;

		private global::Gamma.GtkWidgets.yEntry entryId;

		private global::Gamma.GtkWidgets.yCheckButton checkAuto;

		private global::Gamma.GtkWidgets.yLabel ylabelType;

		private global::Gamma.GtkWidgets.yLabel ylabelWarehouseExpense;

		private global::Gtk.Table table3;

		private global::Gtk.ScrolledWindow GtkScrolledWindowComments;

		private global::Gamma.GtkWidgets.yTextView ytextComment;

		private global::Gtk.Label label26;

		private global::Gtk.Label label5;

		private global::Gtk.Label label6;

		private global::QS.Widgets.GtkUI.DatePicker ydateDoc;

		private global::Gamma.GtkWidgets.yLabel ylabelCreatedBy;

		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox7;

		private global::Gtk.Label label1;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeItems;

		private global::Gtk.HBox hbox8;

		private global::Gamma.GtkWidgets.yButton buttonAddEmployee;

		private global::Gamma.GtkWidgets.yButton buttonAddEmployeeIssue;

		private global::Gamma.GtkWidgets.yButton buttonAddNomenclature;

		private global::Gamma.GtkWidgets.yButton buttonDel;

		private global::QS.Widgets.MenuButton buttonDelBarcodes;

		private global::Gamma.GtkWidgets.yLabel labelSum;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Stock.OverNormView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Stock.OverNormView";
			// Container child Workwear.Views.Stock.OverNormView.Gtk.Container+ContainerChild
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
			this.entityWarehouseExpense = new global::QS.Views.Control.EntityEntry();
			this.entityWarehouseExpense.Events = ((global::Gdk.EventMask)(256));
			this.entityWarehouseExpense.Name = "entityWarehouseExpense";
			this.table2.Add(this.entityWarehouseExpense);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table2[this.entityWarehouseExpense]));
			w6.TopAttach = ((uint)(2));
			w6.BottomAttach = ((uint)(3));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.enumTypesComboBox = new Gamma.Widgets.yEnumComboBox();
			this.enumTypesComboBox.Name = "enumTypesComboBox";
			this.enumTypesComboBox.ShowSpecialStateAll = false;
			this.enumTypesComboBox.ShowSpecialStateNot = false;
			this.enumTypesComboBox.UseShortTitle = false;
			this.enumTypesComboBox.DefaultFirst = false;
			this.table2.Add(this.enumTypesComboBox);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table2[this.enumTypesComboBox]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Номер:");
			this.table2.Add(this.label4);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table2[this.label4]));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
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
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.yAutoNumber[this.entryId]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			// Container child yAutoNumber.Gtk.Box+BoxChild
			this.checkAuto = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkAuto.CanFocus = true;
			this.checkAuto.Name = "checkAuto";
			this.checkAuto.Label = global::Mono.Unix.Catalog.GetString("Автоматически");
			this.checkAuto.Active = true;
			this.checkAuto.DrawIndicator = true;
			this.checkAuto.UseUnderline = true;
			this.yAutoNumber.Add(this.checkAuto);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.yAutoNumber[this.checkAuto]));
			w10.Position = 1;
			w10.Expand = false;
			w10.Fill = false;
			this.table2.Add(this.yAutoNumber);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table2[this.yAutoNumber]));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ylabelType = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelType.Name = "ylabelType";
			this.ylabelType.Xalign = 1F;
			this.ylabelType.LabelProp = global::Mono.Unix.Catalog.GetString("Тип:");
			this.table2.Add(this.ylabelType);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table2[this.ylabelType]));
			w12.TopAttach = ((uint)(1));
			w12.BottomAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ylabelWarehouseExpense = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelWarehouseExpense.Name = "ylabelWarehouseExpense";
			this.ylabelWarehouseExpense.Xalign = 1F;
			this.ylabelWarehouseExpense.LabelProp = global::Mono.Unix.Catalog.GetString("Склад списания<span foreground=\"red\">*</span>:");
			this.ylabelWarehouseExpense.UseMarkup = true;
			this.table2.Add(this.ylabelWarehouseExpense);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table2[this.ylabelWarehouseExpense]));
			w13.TopAttach = ((uint)(2));
			w13.BottomAttach = ((uint)(3));
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
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
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
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox7 = new global::Gtk.HBox();
			this.hbox7.Name = "hbox7";
			this.hbox7.Spacing = 6;
			// Container child hbox7.Gtk.Box+BoxChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 0F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Выдача из подменного фонда");
			this.hbox7.Add(this.label1);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.label1]));
			w24.Position = 0;
			w24.Expand = false;
			w24.Fill = false;
			this.vbox2.Add(this.hbox7);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox7]));
			w25.Position = 0;
			w25.Expand = false;
			w25.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeItems = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeItems.CanFocus = true;
			this.ytreeItems.Name = "ytreeItems";
			this.GtkScrolledWindow.Add(this.ytreeItems);
			this.vbox2.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.GtkScrolledWindow]));
			w27.Position = 1;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox8 = new global::Gtk.HBox();
			this.hbox8.Name = "hbox8";
			this.hbox8.Spacing = 6;
			// Container child hbox8.Gtk.Box+BoxChild
			this.buttonAddEmployee = new global::Gamma.GtkWidgets.yButton();
			this.buttonAddEmployee.CanFocus = true;
			this.buttonAddEmployee.Name = "buttonAddEmployee";
			this.buttonAddEmployee.UseUnderline = true;
			this.buttonAddEmployee.Label = global::Mono.Unix.Catalog.GetString("Выбрать сотрудника");
			global::Gtk.Image w28 = new global::Gtk.Image();
			w28.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddEmployee.Image = w28;
			this.hbox8.Add(this.buttonAddEmployee);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.buttonAddEmployee]));
			w29.Position = 0;
			w29.Expand = false;
			w29.Fill = false;
			// Container child hbox8.Gtk.Box+BoxChild
			this.buttonAddEmployeeIssue = new global::Gamma.GtkWidgets.yButton();
			this.buttonAddEmployeeIssue.CanFocus = true;
			this.buttonAddEmployeeIssue.Name = "buttonAddEmployeeIssue";
			this.buttonAddEmployeeIssue.UseUnderline = true;
			this.buttonAddEmployeeIssue.Label = global::Mono.Unix.Catalog.GetString("Заменить у сотрудника");
			global::Gtk.Image w30 = new global::Gtk.Image();
			w30.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddEmployeeIssue.Image = w30;
			this.hbox8.Add(this.buttonAddEmployeeIssue);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.buttonAddEmployeeIssue]));
			w31.Position = 1;
			w31.Expand = false;
			w31.Fill = false;
			// Container child hbox8.Gtk.Box+BoxChild
			this.buttonAddNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.buttonAddNomenclature.Sensitive = false;
			this.buttonAddNomenclature.CanFocus = true;
			this.buttonAddNomenclature.Name = "buttonAddNomenclature";
			this.buttonAddNomenclature.UseUnderline = true;
			this.buttonAddNomenclature.Label = global::Mono.Unix.Catalog.GetString("Выбрать варианты");
			global::Gtk.Image w32 = new global::Gtk.Image();
			w32.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-refresh", global::Gtk.IconSize.Menu);
			this.buttonAddNomenclature.Image = w32;
			this.hbox8.Add(this.buttonAddNomenclature);
			global::Gtk.Box.BoxChild w33 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.buttonAddNomenclature]));
			w33.Position = 2;
			w33.Expand = false;
			w33.Fill = false;
			// Container child hbox8.Gtk.Box+BoxChild
			this.buttonDel = new global::Gamma.GtkWidgets.yButton();
			this.buttonDel.Sensitive = false;
			this.buttonDel.CanFocus = true;
			this.buttonDel.Name = "buttonDel";
			this.buttonDel.UseStock = true;
			this.buttonDel.UseUnderline = true;
			this.buttonDel.Label = "gtk-remove";
			this.hbox8.Add(this.buttonDel);
			global::Gtk.Box.BoxChild w34 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.buttonDel]));
			w34.Position = 3;
			w34.Expand = false;
			w34.Fill = false;
			// Container child hbox8.Gtk.Box+BoxChild
			this.buttonDelBarcodes = new global::QS.Widgets.MenuButton();
			this.buttonDelBarcodes.Sensitive = false;
			this.buttonDelBarcodes.CanFocus = true;
			this.buttonDelBarcodes.Name = "buttonDelBarcodes";
			this.buttonDelBarcodes.UseUnderline = true;
			this.buttonDelBarcodes.UseMarkup = false;
			this.buttonDelBarcodes.LabelXAlign = 0F;
			this.buttonDelBarcodes.Label = global::Mono.Unix.Catalog.GetString("Удалить штрихкод");
			global::Gtk.Image w35 = new global::Gtk.Image();
			w35.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonDelBarcodes.Image = w35;
			this.hbox8.Add(this.buttonDelBarcodes);
			global::Gtk.Box.BoxChild w36 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.buttonDelBarcodes]));
			w36.Position = 4;
			w36.Expand = false;
			w36.Fill = false;
			// Container child hbox8.Gtk.Box+BoxChild
			this.labelSum = new global::Gamma.GtkWidgets.yLabel();
			this.labelSum.Name = "labelSum";
			this.labelSum.Xalign = 1F;
			this.labelSum.LabelProp = global::Mono.Unix.Catalog.GetString("Количество:");
			this.hbox8.Add(this.labelSum);
			global::Gtk.Box.BoxChild w37 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.labelSum]));
			w37.Position = 5;
			this.vbox2.Add(this.hbox8);
			global::Gtk.Box.BoxChild w38 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox8]));
			w38.Position = 2;
			w38.Expand = false;
			w38.Fill = false;
			this.dialog1_VBox.Add(this.vbox2);
			global::Gtk.Box.BoxChild w39 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.vbox2]));
			w39.Position = 2;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
