
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Regulations
{
	public partial class NormView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.HBox hbox4;

		private global::Gamma.GtkWidgets.yButton buttonSave;

		private global::Gamma.GtkWidgets.yButton buttonCancel;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Table table1;

		private global::QS.Widgets.GtkUI.DatePicker datefrom;

		private global::QS.Widgets.GtkUI.DatePicker dateto;

		private global::Gtk.ScrolledWindow GtkScrolledWindowComments;

		private global::Gamma.GtkWidgets.yTextView ytextComment;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		private global::Gtk.Label label26;

		private global::Gtk.Label label3;

		private global::Gtk.Label label4;

		private global::Gtk.Label label6;

		private global::Gtk.Label label7;

		private global::Gtk.Label label8;

		private global::Gamma.Widgets.yListComboBox ycomboAnnex;

		private global::Gamma.GtkWidgets.yEntry yentryName;

		private global::Gamma.Widgets.yEntryReference yentryRegulationDoc;

		private global::Gamma.GtkWidgets.yEntry yentryTonParagraph;

		private global::Gamma.GtkWidgets.yLabel ylabelId;

		private global::Gtk.VBox vbox2;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeProfessions;

		private global::Gtk.HBox hbox5;

		private global::Gtk.Button buttonNewProfession;

		private global::Gtk.Button buttonAddProfession;

		private global::Gtk.Button buttonRemoveProfession;

		private global::Gtk.Label label5;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gamma.GtkWidgets.yTreeView ytreeItems;

		private global::Gtk.HBox hbox6;

		private global::Gtk.Button buttonAddItem;

		private global::Gtk.Button buttonRemoveItem;

		private global::Gamma.GtkWidgets.yButton buttonReplaceNomeclature;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Views.Regulations.NormView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Views.Regulations.NormView";
			// Container child workwear.Views.Regulations.NormView.Gtk.Container+ContainerChild
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
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
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
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
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
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Нормативный документ:");
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w11.TopAttach = ((uint)(1));
			w11.BottomAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label26 = new global::Gtk.Label();
			this.label26.Name = "label26";
			this.label26.Xalign = 1F;
			this.label26.Yalign = 0F;
			this.label26.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table1.Add(this.label26);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.label26]));
			w12.TopAttach = ((uint)(7));
			w12.BottomAttach = ((uint)(8));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Приложение:");
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w13.TopAttach = ((uint)(2));
			w13.BottomAttach = ((uint)(3));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Пункт приложения:");
			this.table1.Add(this.label4);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.label4]));
			w14.TopAttach = ((uint)(3));
			w14.BottomAttach = ((uint)(4));
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
			this.yentryRegulationDoc = new global::Gamma.Widgets.yEntryReference();
			this.yentryRegulationDoc.Events = ((global::Gdk.EventMask)(256));
			this.yentryRegulationDoc.Name = "yentryRegulationDoc";
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
			this.hbox1.Add(this.table1);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.table1]));
			w23.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeProfessions = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeProfessions.CanFocus = true;
			this.ytreeProfessions.Name = "ytreeProfessions";
			this.GtkScrolledWindow.Add(this.ytreeProfessions);
			this.vbox2.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.GtkScrolledWindow]));
			w25.Position = 0;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox5 = new global::Gtk.HBox();
			this.hbox5.Name = "hbox5";
			this.hbox5.Spacing = 6;
			// Container child hbox5.Gtk.Box+BoxChild
			this.buttonNewProfession = new global::Gtk.Button();
			this.buttonNewProfession.CanFocus = true;
			this.buttonNewProfession.Name = "buttonNewProfession";
			this.buttonNewProfession.UseUnderline = true;
			this.buttonNewProfession.Label = global::Mono.Unix.Catalog.GetString("Новая");
			global::Gtk.Image w26 = new global::Gtk.Image();
			w26.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-new", global::Gtk.IconSize.Menu);
			this.buttonNewProfession.Image = w26;
			this.hbox5.Add(this.buttonNewProfession);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.buttonNewProfession]));
			w27.Position = 0;
			// Container child hbox5.Gtk.Box+BoxChild
			this.buttonAddProfession = new global::Gtk.Button();
			this.buttonAddProfession.CanFocus = true;
			this.buttonAddProfession.Name = "buttonAddProfession";
			this.buttonAddProfession.UseUnderline = true;
			this.buttonAddProfession.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w28 = new global::Gtk.Image();
			w28.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddProfession.Image = w28;
			this.hbox5.Add(this.buttonAddProfession);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.buttonAddProfession]));
			w29.Position = 1;
			// Container child hbox5.Gtk.Box+BoxChild
			this.buttonRemoveProfession = new global::Gtk.Button();
			this.buttonRemoveProfession.Sensitive = false;
			this.buttonRemoveProfession.CanFocus = true;
			this.buttonRemoveProfession.Name = "buttonRemoveProfession";
			this.buttonRemoveProfession.UseUnderline = true;
			this.buttonRemoveProfession.Label = global::Mono.Unix.Catalog.GetString("Убрать");
			global::Gtk.Image w30 = new global::Gtk.Image();
			w30.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonRemoveProfession.Image = w30;
			this.hbox5.Add(this.buttonRemoveProfession);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.buttonRemoveProfession]));
			w31.Position = 2;
			this.vbox2.Add(this.hbox5);
			global::Gtk.Box.BoxChild w32 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox5]));
			w32.Position = 1;
			w32.Expand = false;
			w32.Fill = false;
			this.hbox1.Add(this.vbox2);
			global::Gtk.Box.BoxChild w33 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.vbox2]));
			w33.Position = 1;
			this.dialog1_VBox.Add(this.hbox1);
			global::Gtk.Box.BoxChild w34 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox1]));
			w34.Position = 1;
			w34.Expand = false;
			w34.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 0F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Выдаваемая по норме номенклатура</b>");
			this.label5.UseMarkup = true;
			this.dialog1_VBox.Add(this.label5);
			global::Gtk.Box.BoxChild w35 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.label5]));
			w35.Position = 2;
			w35.Expand = false;
			w35.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.ytreeItems = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeItems.CanFocus = true;
			this.ytreeItems.Name = "ytreeItems";
			this.GtkScrolledWindow1.Add(this.ytreeItems);
			this.dialog1_VBox.Add(this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w37 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.GtkScrolledWindow1]));
			w37.Position = 3;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox6 = new global::Gtk.HBox();
			this.hbox6.Name = "hbox6";
			this.hbox6.Spacing = 6;
			// Container child hbox6.Gtk.Box+BoxChild
			this.buttonAddItem = new global::Gtk.Button();
			this.buttonAddItem.CanFocus = true;
			this.buttonAddItem.Name = "buttonAddItem";
			this.buttonAddItem.UseUnderline = true;
			this.buttonAddItem.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w38 = new global::Gtk.Image();
			w38.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddItem.Image = w38;
			this.hbox6.Add(this.buttonAddItem);
			global::Gtk.Box.BoxChild w39 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.buttonAddItem]));
			w39.Position = 0;
			w39.Expand = false;
			w39.Fill = false;
			// Container child hbox6.Gtk.Box+BoxChild
			this.buttonRemoveItem = new global::Gtk.Button();
			this.buttonRemoveItem.Sensitive = false;
			this.buttonRemoveItem.CanFocus = true;
			this.buttonRemoveItem.Name = "buttonRemoveItem";
			this.buttonRemoveItem.UseUnderline = true;
			this.buttonRemoveItem.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w40 = new global::Gtk.Image();
			w40.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonRemoveItem.Image = w40;
			this.hbox6.Add(this.buttonRemoveItem);
			global::Gtk.Box.BoxChild w41 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.buttonRemoveItem]));
			w41.Position = 1;
			w41.Expand = false;
			w41.Fill = false;
			// Container child hbox6.Gtk.Box+BoxChild
			this.buttonReplaceNomeclature = new global::Gamma.GtkWidgets.yButton();
			this.buttonReplaceNomeclature.TooltipMarkup = "Заменяет одну номенклатуру в норме на другую, при этом так же заменяя номенклатур" +
				"у в выдачах и потребностях сотрудников в которых использовалась данная строка но" +
				"рмы.";
			this.buttonReplaceNomeclature.Sensitive = false;
			this.buttonReplaceNomeclature.CanFocus = true;
			this.buttonReplaceNomeclature.Name = "buttonReplaceNomeclature";
			this.buttonReplaceNomeclature.UseUnderline = true;
			this.buttonReplaceNomeclature.Label = global::Mono.Unix.Catalog.GetString("Заменить номенклатуру");
			global::Gtk.Image w42 = new global::Gtk.Image();
			w42.Pixbuf = global::Gdk.Pixbuf.LoadFromResource("workwear.icon.buttons.arrows.png");
			this.buttonReplaceNomeclature.Image = w42;
			this.hbox6.Add(this.buttonReplaceNomeclature);
			global::Gtk.Box.BoxChild w43 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.buttonReplaceNomeclature]));
			w43.Position = 2;
			w43.Expand = false;
			w43.Fill = false;
			this.dialog1_VBox.Add(this.hbox6);
			global::Gtk.Box.BoxChild w44 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox6]));
			w44.Position = 4;
			w44.Expand = false;
			w44.Fill = false;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Show();
			this.yentryRegulationDoc.Changed += new global::System.EventHandler(this.OnYentryRegulationDocChanged);
			this.buttonNewProfession.Clicked += new global::System.EventHandler(this.OnButtonNewProfessionClicked);
			this.buttonAddProfession.Clicked += new global::System.EventHandler(this.OnButtonAddProfessionClicked);
			this.buttonRemoveProfession.Clicked += new global::System.EventHandler(this.OnButtonRemoveProfessionClicked);
			this.buttonAddItem.Clicked += new global::System.EventHandler(this.OnButtonAddItemClicked);
			this.buttonRemoveItem.Clicked += new global::System.EventHandler(this.OnButtonRemoveItemClicked);
			this.buttonReplaceNomeclature.Clicked += new global::System.EventHandler(this.OnButtonReplaceNomeclatureClicked);
		}
	}
}
