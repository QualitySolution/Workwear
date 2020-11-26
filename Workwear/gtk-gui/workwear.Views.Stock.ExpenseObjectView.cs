
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Stock
{
	public partial class ExpenseObjectView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Table table2;

		private global::QS.Views.Control.EntityEntry entitySubdivision;

		private global::QS.Views.Control.EntityEntry entityWarehouseExpense;

		private global::Gtk.HBox hboxIssuanceSheet;

		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gtk.Label labelObject;

		private global::Gamma.Widgets.yEnumComboBox ycomboOperation;

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

		private global::workwear.Views.Stock.ExpenseDocItemObjectView expensedocitemobjectview1;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Views.Stock.ExpenseObjectView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Views.Stock.ExpenseObjectView";
			// Container child workwear.Views.Stock.ExpenseObjectView.Gtk.Container+ContainerChild
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
			this.entitySubdivision = new global::QS.Views.Control.EntityEntry();
			this.entitySubdivision.Events = ((global::Gdk.EventMask)(256));
			this.entitySubdivision.Name = "entitySubdivision";
			this.table2.Add(this.entitySubdivision);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table2[this.entitySubdivision]));
			w6.TopAttach = ((uint)(2));
			w6.BottomAttach = ((uint)(3));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.entityWarehouseExpense = new global::QS.Views.Control.EntityEntry();
			this.entityWarehouseExpense.Events = ((global::Gdk.EventMask)(256));
			this.entityWarehouseExpense.Name = "entityWarehouseExpense";
			this.table2.Add(this.entityWarehouseExpense);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table2[this.entityWarehouseExpense]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.hboxIssuanceSheet = new global::Gtk.HBox();
			this.hboxIssuanceSheet.Name = "hboxIssuanceSheet";
			this.hboxIssuanceSheet.Spacing = 6;
			this.table2.Add(this.hboxIssuanceSheet);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table2[this.hboxIssuanceSheet]));
			w8.TopAttach = ((uint)(3));
			w8.BottomAttach = ((uint)(4));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Операция<span foreground=\"red\">*</span>:");
			this.label2.UseMarkup = true;
			this.table2.Add(this.label2);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table2[this.label2]));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Склад<span foreground=\"red\">*</span>:");
			this.label3.UseMarkup = true;
			this.table2.Add(this.label3);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table2[this.label3]));
			w10.TopAttach = ((uint)(1));
			w10.BottomAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.labelObject = new global::Gtk.Label();
			this.labelObject.Name = "labelObject";
			this.labelObject.Xalign = 1F;
			this.labelObject.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение<span foreground=\"red\">*</span>:");
			this.labelObject.UseMarkup = true;
			this.table2.Add(this.labelObject);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table2[this.labelObject]));
			w11.TopAttach = ((uint)(2));
			w11.BottomAttach = ((uint)(3));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ycomboOperation = new global::Gamma.Widgets.yEnumComboBox();
			this.ycomboOperation.Name = "ycomboOperation";
			this.ycomboOperation.ShowSpecialStateAll = false;
			this.ycomboOperation.ShowSpecialStateNot = false;
			this.ycomboOperation.UseShortTitle = false;
			this.ycomboOperation.DefaultFirst = false;
			this.table2.Add(this.ycomboOperation);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table2[this.ycomboOperation]));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox1.Add(this.table2);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.table2]));
			w13.Position = 0;
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
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table3[this.GtkScrolledWindowComments]));
			w15.TopAttach = ((uint)(3));
			w15.BottomAttach = ((uint)(4));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label26 = new global::Gtk.Label();
			this.label26.Name = "label26";
			this.label26.Xalign = 1F;
			this.label26.Yalign = 0F;
			this.label26.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table3.Add(this.label26);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table3[this.label26]));
			w16.TopAttach = ((uint)(3));
			w16.BottomAttach = ((uint)(4));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Номер:");
			this.table3.Add(this.label4);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table3[this.label4]));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Пользователь:");
			this.table3.Add(this.label5);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table3[this.label5]));
			w18.TopAttach = ((uint)(1));
			w18.BottomAttach = ((uint)(2));
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
			w19.TopAttach = ((uint)(2));
			w19.BottomAttach = ((uint)(3));
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
			this.table3.Add(this.ydateDoc);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table3[this.ydateDoc]));
			w20.TopAttach = ((uint)(2));
			w20.BottomAttach = ((uint)(3));
			w20.LeftAttach = ((uint)(1));
			w20.RightAttach = ((uint)(2));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.ylabelCreatedBy = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelCreatedBy.Name = "ylabelCreatedBy";
			this.ylabelCreatedBy.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel1");
			this.table3.Add(this.ylabelCreatedBy);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table3[this.ylabelCreatedBy]));
			w21.TopAttach = ((uint)(1));
			w21.BottomAttach = ((uint)(2));
			w21.LeftAttach = ((uint)(1));
			w21.RightAttach = ((uint)(2));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.ylabelId = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelId.Name = "ylabelId";
			this.ylabelId.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel3");
			this.table3.Add(this.ylabelId);
			global::Gtk.Table.TableChild w22 = ((global::Gtk.Table.TableChild)(this.table3[this.ylabelId]));
			w22.LeftAttach = ((uint)(1));
			w22.RightAttach = ((uint)(2));
			w22.XOptions = ((global::Gtk.AttachOptions)(4));
			w22.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox1.Add(this.table3);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.table3]));
			w23.Position = 1;
			this.dialog1_VBox.Add(this.hbox1);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox1]));
			w24.Position = 1;
			w24.Expand = false;
			w24.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.expensedocitemobjectview1 = new global::workwear.Views.Stock.ExpenseDocItemObjectView();
			this.expensedocitemobjectview1.Events = ((global::Gdk.EventMask)(256));
			this.expensedocitemobjectview1.Name = "expensedocitemobjectview1";
			this.dialog1_VBox.Add(this.expensedocitemobjectview1);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.expensedocitemobjectview1]));
			w25.Position = 2;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Show();
		}
	}
}
