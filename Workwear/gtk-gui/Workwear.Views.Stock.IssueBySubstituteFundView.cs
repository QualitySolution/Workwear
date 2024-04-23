
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Stock
{
	public partial class IssueBySubstituteFundView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Table table2;

		private global::QS.Views.Control.EntityEntry entityentryChairmanPerson;

		private global::QS.Views.Control.EntityEntry entityentryDirectorPerson;

		private global::Gtk.Label label4;

		private global::Gamma.GtkWidgets.yLabel ylabelChairman;

		private global::Gamma.GtkWidgets.yLabel ylabelDirector;

		private global::Gamma.GtkWidgets.yLabel ylabelId;

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

		private global::Gtk.Button buttonAdd;

		private global::Gamma.GtkWidgets.yButton buttonDel;

		private global::Gtk.Button buttonAddSubstitution;

		private global::Gamma.GtkWidgets.yLabel labelSum;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Stock.IssueBySubstituteFundView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Stock.IssueBySubstituteFundView";
			// Container child Workwear.Views.Stock.IssueBySubstituteFundView.Gtk.Container+ContainerChild
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
			this.table2 = new global::Gtk.Table(((uint)(3)), ((uint)(2)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.entityentryChairmanPerson = new global::QS.Views.Control.EntityEntry();
			this.entityentryChairmanPerson.Events = ((global::Gdk.EventMask)(256));
			this.entityentryChairmanPerson.Name = "entityentryChairmanPerson";
			this.table2.Add(this.entityentryChairmanPerson);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table2[this.entityentryChairmanPerson]));
			w6.TopAttach = ((uint)(2));
			w6.BottomAttach = ((uint)(3));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.entityentryDirectorPerson = new global::QS.Views.Control.EntityEntry();
			this.entityentryDirectorPerson.Events = ((global::Gdk.EventMask)(256));
			this.entityentryDirectorPerson.Name = "entityentryDirectorPerson";
			this.table2.Add(this.entityentryDirectorPerson);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table2[this.entityentryDirectorPerson]));
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
			this.ylabelChairman = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelChairman.Name = "ylabelChairman";
			this.ylabelChairman.Xalign = 1F;
			this.ylabelChairman.LabelProp = global::Mono.Unix.Catalog.GetString("Председатель комиссии:");
			this.table2.Add(this.ylabelChairman);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table2[this.ylabelChairman]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ylabelDirector = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelDirector.Name = "ylabelDirector";
			this.ylabelDirector.Xalign = 1F;
			this.ylabelDirector.LabelProp = global::Mono.Unix.Catalog.GetString("Постанавляющее лицо:");
			this.table2.Add(this.ylabelDirector);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table2[this.ylabelDirector]));
			w10.TopAttach = ((uint)(1));
			w10.BottomAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ylabelId = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelId.Name = "ylabelId";
			this.ylabelId.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel3");
			this.table2.Add(this.ylabelId);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table2[this.ylabelId]));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox1.Add(this.table2);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.table2]));
			w12.Position = 0;
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
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table3[this.GtkScrolledWindowComments]));
			w14.TopAttach = ((uint)(2));
			w14.BottomAttach = ((uint)(3));
			w14.LeftAttach = ((uint)(1));
			w14.RightAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label26 = new global::Gtk.Label();
			this.label26.Name = "label26";
			this.label26.Xalign = 1F;
			this.label26.Yalign = 0F;
			this.label26.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table3.Add(this.label26);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table3[this.label26]));
			w15.TopAttach = ((uint)(2));
			w15.BottomAttach = ((uint)(3));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Пользователь:");
			this.table3.Add(this.label5);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table3[this.label5]));
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
			w17.TopAttach = ((uint)(1));
			w17.BottomAttach = ((uint)(2));
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
			this.ydateDoc.HideButtonClearDate = false;
			this.table3.Add(this.ydateDoc);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table3[this.ydateDoc]));
			w18.TopAttach = ((uint)(1));
			w18.BottomAttach = ((uint)(2));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(2));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.ylabelCreatedBy = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelCreatedBy.Name = "ylabelCreatedBy";
			this.ylabelCreatedBy.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel1");
			this.table3.Add(this.ylabelCreatedBy);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table3[this.ylabelCreatedBy]));
			w19.LeftAttach = ((uint)(1));
			w19.RightAttach = ((uint)(2));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox1.Add(this.table3);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.table3]));
			w20.Position = 1;
			this.dialog1_VBox.Add(this.hbox1);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox1]));
			w21.Position = 1;
			w21.Expand = false;
			w21.Fill = false;
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
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.label1]));
			w22.Position = 0;
			w22.Expand = false;
			w22.Fill = false;
			this.vbox2.Add(this.hbox7);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox7]));
			w23.Position = 0;
			w23.Expand = false;
			w23.Fill = false;
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
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.GtkScrolledWindow]));
			w25.Position = 1;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox8 = new global::Gtk.HBox();
			this.hbox8.Name = "hbox8";
			this.hbox8.Spacing = 6;
			// Container child hbox8.Gtk.Box+BoxChild
			this.buttonAdd = new global::Gtk.Button();
			this.buttonAdd.CanFocus = true;
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.UseUnderline = true;
			this.buttonAdd.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w26 = new global::Gtk.Image();
			w26.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAdd.Image = w26;
			this.hbox8.Add(this.buttonAdd);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.buttonAdd]));
			w27.Position = 0;
			w27.Expand = false;
			w27.Fill = false;
			// Container child hbox8.Gtk.Box+BoxChild
			this.buttonDel = new global::Gamma.GtkWidgets.yButton();
			this.buttonDel.Sensitive = false;
			this.buttonDel.CanFocus = true;
			this.buttonDel.Name = "buttonDel";
			this.buttonDel.UseStock = true;
			this.buttonDel.UseUnderline = true;
			this.buttonDel.Label = "gtk-remove";
			this.hbox8.Add(this.buttonDel);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.buttonDel]));
			w28.Position = 1;
			w28.Expand = false;
			w28.Fill = false;
			// Container child hbox8.Gtk.Box+BoxChild
			this.buttonAddSubstitution = new global::Gtk.Button();
			this.buttonAddSubstitution.Sensitive = false;
			this.buttonAddSubstitution.CanFocus = true;
			this.buttonAddSubstitution.Name = "buttonAddSubstitution";
			this.buttonAddSubstitution.UseUnderline = true;
			this.buttonAddSubstitution.Label = global::Mono.Unix.Catalog.GetString("Выбрать варианты");
			global::Gtk.Image w29 = new global::Gtk.Image();
			w29.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-refresh", global::Gtk.IconSize.Menu);
			this.buttonAddSubstitution.Image = w29;
			this.hbox8.Add(this.buttonAddSubstitution);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.buttonAddSubstitution]));
			w30.Position = 2;
			w30.Expand = false;
			w30.Fill = false;
			// Container child hbox8.Gtk.Box+BoxChild
			this.labelSum = new global::Gamma.GtkWidgets.yLabel();
			this.labelSum.Name = "labelSum";
			this.labelSum.Xalign = 1F;
			this.labelSum.LabelProp = global::Mono.Unix.Catalog.GetString("Количество:");
			this.hbox8.Add(this.labelSum);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.hbox8[this.labelSum]));
			w31.Position = 3;
			this.vbox2.Add(this.hbox8);
			global::Gtk.Box.BoxChild w32 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox8]));
			w32.Position = 2;
			w32.Expand = false;
			w32.Fill = false;
			this.dialog1_VBox.Add(this.vbox2);
			global::Gtk.Box.BoxChild w33 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.vbox2]));
			w33.Position = 2;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
