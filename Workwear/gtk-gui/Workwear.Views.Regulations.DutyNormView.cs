
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Regulations
{
	public partial class DutyNormView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.HBox hbox4;

		private global::Gamma.GtkWidgets.yButton buttonSave;

		private global::Gamma.GtkWidgets.yButton buttonCancel;

		private global::Gamma.GtkWidgets.yNotebook tabs;

		private global::Gtk.Table table1;

		private global::QS.Widgets.GtkUI.DatePicker datefrom;

		private global::QS.Widgets.GtkUI.DatePicker dateto;

		private global::Gtk.ScrolledWindow GtkScrolledWindowComments;

		private global::Gamma.GtkWidgets.yTextView ytextComment;

		private global::Gtk.Label label1;

		private global::Gtk.Label label26;

		private global::Gtk.Label label3;

		private global::Gtk.Label label4;

		private global::Gtk.Label label5;

		private global::Gtk.Label label6;

		private global::Gtk.Label label7;

		private global::Gtk.Label label8;

		private global::Gamma.Widgets.yListComboBox ycomboAnnex;

		private global::Gamma.GtkWidgets.yEntry yentryName;

		private global::QS.Widgets.GtkUI.SpecialListComboBox yentryRegulationDoc;

		private global::Gamma.GtkWidgets.yEntry yentryTonParagraph;

		private global::Gamma.GtkWidgets.yLabel ylabelId;

		private global::Gtk.Label label9;

		private global::Gtk.VBox vbox1;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gamma.GtkWidgets.yTreeView ytreeItems;

		private global::Gtk.HBox hbox6;

		private global::Gtk.Button buttonAddItem;

		private global::Gtk.Button buttonRemoveItem;

		private global::Gtk.Label label10;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeviewHistory;

		private global::Gtk.Label label2;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Regulations.DutyNormView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Regulations.DutyNormView";
			// Container child Workwear.Views.Regulations.DutyNormView.Gtk.Container+ContainerChild
			this.dialog1_VBox = new global::Gtk.VBox();
			this.dialog1_VBox.Name = "dialog1_VBox";
			this.dialog1_VBox.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox4 = new global::Gtk.HBox();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.buttonSave = new global::Gamma.GtkWidgets.yButton();
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
			this.buttonCancel = new global::Gamma.GtkWidgets.yButton();
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
			this.tabs = new global::Gamma.GtkWidgets.yNotebook();
			this.tabs.CanFocus = true;
			this.tabs.Name = "tabs";
			this.tabs.CurrentPage = 2;
			// Container child tabs.Gtk.Notebook+NotebookChild
			this.table1 = new global::Gtk.Table(((uint)(8)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.datefrom = new global::QS.Widgets.GtkUI.DatePicker();
			this.datefrom.Events = ((global::Gdk.EventMask)(256));
			this.datefrom.Name = "datefrom";
			this.datefrom.WithTime = false;
			this.datefrom.HideCalendarButton = false;
			this.datefrom.Date = new global::System.DateTime(0);
			this.datefrom.IsEditable = true;
			this.datefrom.AutoSeparation = true;
			this.datefrom.HideButtonClearDate = false;
			this.table1.Add(this.datefrom);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.datefrom]));
			w6.TopAttach = ((uint)(5));
			w6.BottomAttach = ((uint)(6));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.dateto = new global::QS.Widgets.GtkUI.DatePicker();
			this.dateto.Events = ((global::Gdk.EventMask)(256));
			this.dateto.Name = "dateto";
			this.dateto.WithTime = false;
			this.dateto.HideCalendarButton = false;
			this.dateto.Date = new global::System.DateTime(0);
			this.dateto.IsEditable = true;
			this.dateto.AutoSeparation = true;
			this.dateto.HideButtonClearDate = false;
			this.table1.Add(this.dateto);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.dateto]));
			w7.TopAttach = ((uint)(6));
			w7.BottomAttach = ((uint)(7));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
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
			this.table1.Add(this.GtkScrolledWindowComments);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.GtkScrolledWindowComments]));
			w9.TopAttach = ((uint)(7));
			w9.BottomAttach = ((uint)(8));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Код:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label26 = new global::Gtk.Label();
			this.label26.Name = "label26";
			this.label26.Xalign = 1F;
			this.label26.Yalign = 0F;
			this.label26.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table1.Add(this.label26);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.label26]));
			w11.TopAttach = ((uint)(7));
			w11.BottomAttach = ((uint)(8));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Приложение:");
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w12.TopAttach = ((uint)(2));
			w12.BottomAttach = ((uint)(3));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Пункт приложения:");
			this.table1.Add(this.label4);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.label4]));
			w13.TopAttach = ((uint)(3));
			w13.BottomAttach = ((uint)(4));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Нормативный документ:");
			this.table1.Add(this.label5);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.label5]));
			w14.TopAttach = ((uint)(1));
			w14.BottomAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label6 = new global::Gtk.Label();
			this.label6.Name = "label6";
			this.label6.Xalign = 1F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString("Дата начала действия:");
			this.table1.Add(this.label6);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1[this.label6]));
			w15.TopAttach = ((uint)(5));
			w15.BottomAttach = ((uint)(6));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label();
			this.label7.Name = "label7";
			this.label7.Xalign = 1F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString("Дата окончания действия:");
			this.table1.Add(this.label7);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1[this.label7]));
			w16.TopAttach = ((uint)(6));
			w16.BottomAttach = ((uint)(7));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label8 = new global::Gtk.Label();
			this.label8.Name = "label8";
			this.label8.Xalign = 1F;
			this.label8.LabelProp = global::Mono.Unix.Catalog.GetString("Название нормы:");
			this.table1.Add(this.label8);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table1[this.label8]));
			w17.TopAttach = ((uint)(4));
			w17.BottomAttach = ((uint)(5));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycomboAnnex = new global::Gamma.Widgets.yListComboBox();
			this.ycomboAnnex.Name = "ycomboAnnex";
			this.ycomboAnnex.AddIfNotExist = false;
			this.ycomboAnnex.DefaultFirst = false;
			this.table1.Add(this.ycomboAnnex);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table1[this.ycomboAnnex]));
			w18.TopAttach = ((uint)(2));
			w18.BottomAttach = ((uint)(3));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(2));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryName = new global::Gamma.GtkWidgets.yEntry();
			this.yentryName.CanFocus = true;
			this.yentryName.Name = "yentryName";
			this.yentryName.IsEditable = true;
			this.yentryName.MaxLength = 200;
			this.yentryName.InvisibleChar = '●';
			this.table1.Add(this.yentryName);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table1[this.yentryName]));
			w19.TopAttach = ((uint)(4));
			w19.BottomAttach = ((uint)(5));
			w19.LeftAttach = ((uint)(1));
			w19.RightAttach = ((uint)(2));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryRegulationDoc = new global::QS.Widgets.GtkUI.SpecialListComboBox();
			this.yentryRegulationDoc.Name = "yentryRegulationDoc";
			this.yentryRegulationDoc.AddIfNotExist = false;
			this.yentryRegulationDoc.DefaultFirst = false;
			this.yentryRegulationDoc.ShowSpecialStateAll = false;
			this.yentryRegulationDoc.ShowSpecialStateNot = true;
			this.table1.Add(this.yentryRegulationDoc);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table1[this.yentryRegulationDoc]));
			w20.TopAttach = ((uint)(1));
			w20.BottomAttach = ((uint)(2));
			w20.LeftAttach = ((uint)(1));
			w20.RightAttach = ((uint)(2));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryTonParagraph = new global::Gamma.GtkWidgets.yEntry();
			this.yentryTonParagraph.CanFocus = true;
			this.yentryTonParagraph.Name = "yentryTonParagraph";
			this.yentryTonParagraph.IsEditable = true;
			this.yentryTonParagraph.MaxLength = 15;
			this.yentryTonParagraph.InvisibleChar = '●';
			this.table1.Add(this.yentryTonParagraph);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table1[this.yentryTonParagraph]));
			w21.TopAttach = ((uint)(3));
			w21.BottomAttach = ((uint)(4));
			w21.LeftAttach = ((uint)(1));
			w21.RightAttach = ((uint)(2));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelId = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelId.Name = "ylabelId";
			this.ylabelId.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel1");
			this.table1.Add(this.ylabelId);
			global::Gtk.Table.TableChild w22 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelId]));
			w22.LeftAttach = ((uint)(1));
			w22.RightAttach = ((uint)(2));
			w22.XOptions = ((global::Gtk.AttachOptions)(4));
			w22.YOptions = ((global::Gtk.AttachOptions)(4));
			this.tabs.Add(this.table1);
			// Notebook tab
			this.label9 = new global::Gtk.Label();
			this.label9.Name = "label9";
			this.label9.LabelProp = global::Mono.Unix.Catalog.GetString("Основное");
			this.tabs.SetTabLabel(this.table1, this.label9);
			this.label9.ShowAll();
			// Container child tabs.Gtk.Notebook+NotebookChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.ytreeItems = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeItems.CanFocus = true;
			this.ytreeItems.Name = "ytreeItems";
			this.GtkScrolledWindow1.Add(this.ytreeItems);
			this.vbox1.Add(this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow1]));
			w25.Position = 0;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox6 = new global::Gtk.HBox();
			this.hbox6.Name = "hbox6";
			this.hbox6.Spacing = 6;
			// Container child hbox6.Gtk.Box+BoxChild
			this.buttonAddItem = new global::Gtk.Button();
			this.buttonAddItem.CanFocus = true;
			this.buttonAddItem.Name = "buttonAddItem";
			this.buttonAddItem.UseUnderline = true;
			this.buttonAddItem.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w26 = new global::Gtk.Image();
			w26.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddItem.Image = w26;
			this.hbox6.Add(this.buttonAddItem);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.buttonAddItem]));
			w27.Position = 0;
			w27.Expand = false;
			w27.Fill = false;
			// Container child hbox6.Gtk.Box+BoxChild
			this.buttonRemoveItem = new global::Gtk.Button();
			this.buttonRemoveItem.Sensitive = false;
			this.buttonRemoveItem.CanFocus = true;
			this.buttonRemoveItem.Name = "buttonRemoveItem";
			this.buttonRemoveItem.UseUnderline = true;
			this.buttonRemoveItem.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w28 = new global::Gtk.Image();
			w28.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonRemoveItem.Image = w28;
			this.hbox6.Add(this.buttonRemoveItem);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.buttonRemoveItem]));
			w29.Position = 1;
			w29.Expand = false;
			w29.Fill = false;
			this.vbox1.Add(this.hbox6);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox6]));
			w30.Position = 1;
			w30.Expand = false;
			w30.Fill = false;
			this.tabs.Add(this.vbox1);
			global::Gtk.Notebook.NotebookChild w31 = ((global::Gtk.Notebook.NotebookChild)(this.tabs[this.vbox1]));
			w31.Position = 1;
			// Notebook tab
			this.label10 = new global::Gtk.Label();
			this.label10.Name = "label10";
			this.label10.LabelProp = global::Mono.Unix.Catalog.GetString("Норма");
			this.tabs.SetTabLabel(this.vbox1, this.label10);
			this.label10.ShowAll();
			// Container child tabs.Gtk.Notebook+NotebookChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeviewHistory = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeviewHistory.CanFocus = true;
			this.ytreeviewHistory.Name = "ytreeviewHistory";
			this.GtkScrolledWindow.Add(this.ytreeviewHistory);
			this.tabs.Add(this.GtkScrolledWindow);
			global::Gtk.Notebook.NotebookChild w33 = ((global::Gtk.Notebook.NotebookChild)(this.tabs[this.GtkScrolledWindow]));
			w33.Position = 2;
			// Notebook tab
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("История выдач");
			this.tabs.SetTabLabel(this.GtkScrolledWindow, this.label2);
			this.label2.ShowAll();
			this.dialog1_VBox.Add(this.tabs);
			global::Gtk.Box.BoxChild w34 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.tabs]));
			w34.Position = 1;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonAddItem.Clicked += new global::System.EventHandler(this.OnButtonAddItemClicked);
			this.buttonRemoveItem.Clicked += new global::System.EventHandler(this.OnButtonRemoveItemClicked);
		}
	}
}