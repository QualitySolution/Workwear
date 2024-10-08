
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Regulations
{
	public partial class ProtectionToolsView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.HBox hbox5;

		private global::Gtk.Table table1;

		private global::QS.Views.Control.EntityEntry entryCategories;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTextView ytextComment;

		private global::Gtk.Label label;

		private global::Gtk.Label label10;

		private global::Gtk.Label label2;

		private global::Gtk.Label label4;

		private global::Gtk.Label label9;

		private global::Gtk.Label labelCategories;

		private global::QS.Views.Control.EntityEntry yentryItemsType;

		private global::Gamma.GtkWidgets.yEntry yentryName;

		private global::Gamma.GtkWidgets.yLabel ylabelId;

		private global::Gamma.GtkWidgets.yLabel ylabelSupply;

		private global::Gamma.GtkWidgets.ySpinButton yspinAssessedCost;

		private global::Gamma.GtkWidgets.yTable ytableSupply;

		private global::Gamma.GtkWidgets.yButton ybutton_remFemale;

		private global::Gamma.GtkWidgets.yButton ybutton_remMale;

		private global::Gamma.GtkWidgets.yButton ybutton_remUni;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yButton ybuttonSupplyUni;

		private global::Gamma.GtkWidgets.yButton ybuttonSupplyTwoSex;

		private global::Gamma.GtkWidgets.yLabel ylabelSupplyFemale;

		private global::Gamma.GtkWidgets.yLabel ylabelSupplyMale;

		private global::Gamma.GtkWidgets.yLabel ylabelSupplyUni;

		private global::Gamma.Widgets.yListComboBox ylistcomboboxSupplyFemale;

		private global::Gamma.Widgets.yListComboBox ylistcomboboxSupplyMale;

		private global::Gamma.Widgets.yListComboBox ylistcomboboxSupplyUni;

		private global::Gtk.HBox hbox7;

		private global::Gtk.Button buttonAddNomenclature;

		private global::Gtk.Button buttonRemoveNomeclature;

		private global::Gamma.GtkWidgets.yButton buttonCreateNomenclature;

		private global::Gtk.HBox hbox3;

		private global::Gtk.ScrolledWindow GtkScrolledWindow2;

		private global::Gamma.GtkWidgets.yTreeView ytreeItems;

		private global::Gtk.Label label5;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Regulations.ProtectionToolsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Regulations.ProtectionToolsView";
			// Container child Workwear.Views.Regulations.ProtectionToolsView.Gtk.Container+ContainerChild
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
			this.hbox5 = new global::Gtk.HBox();
			this.hbox5.Name = "hbox5";
			this.hbox5.Spacing = 6;
			// Container child hbox5.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(7)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			this.table1.BorderWidth = ((uint)(9));
			// Container child table1.Gtk.Table+TableChild
			this.entryCategories = new global::QS.Views.Control.EntityEntry();
			this.entryCategories.Events = ((global::Gdk.EventMask)(256));
			this.entryCategories.Name = "entryCategories";
			this.table1.Add(this.entryCategories);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.entryCategories]));
			w6.TopAttach = ((uint)(4));
			w6.BottomAttach = ((uint)(5));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytextComment = new global::Gamma.GtkWidgets.yTextView();
			this.ytextComment.CanFocus = true;
			this.ytextComment.Name = "ytextComment";
			this.ytextComment.WrapMode = ((global::Gtk.WrapMode)(3));
			this.GtkScrolledWindow.Add(this.ytextComment);
			this.table1.Add(this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.GtkScrolledWindow]));
			w8.TopAttach = ((uint)(6));
			w8.BottomAttach = ((uint)(7));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label = new global::Gtk.Label();
			this.label.Name = "label";
			this.label.Xalign = 1F;
			this.label.LabelProp = global::Mono.Unix.Catalog.GetString("Код:");
			this.table1.Add(this.label);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.label]));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label10 = new global::Gtk.Label();
			this.label10.Name = "label10";
			this.label10.Xalign = 1F;
			this.label10.LabelProp = global::Mono.Unix.Catalog.GetString("Оценочная стоимость:");
			this.label10.UseMarkup = true;
			this.table1.Add(this.label10);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.label10]));
			w10.TopAttach = ((uint)(3));
			w10.BottomAttach = ((uint)(4));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Наименование<span foreground=\"red\">*</span>:");
			this.label2.UseMarkup = true;
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w11.TopAttach = ((uint)(1));
			w11.BottomAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.Yalign = 0F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table1.Add(this.label4);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.label4]));
			w12.TopAttach = ((uint)(6));
			w12.BottomAttach = ((uint)(7));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label9 = new global::Gtk.Label();
			this.label9.Name = "label9";
			this.label9.Xalign = 1F;
			this.label9.LabelProp = global::Mono.Unix.Catalog.GetString("Тип номенклатуры<span foreground=\"red\">*</span>:");
			this.label9.UseMarkup = true;
			this.table1.Add(this.label9);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.label9]));
			w13.TopAttach = ((uint)(2));
			w13.BottomAttach = ((uint)(3));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelCategories = new global::Gtk.Label();
			this.labelCategories.Name = "labelCategories";
			this.labelCategories.Xalign = 1F;
			this.labelCategories.LabelProp = global::Mono.Unix.Catalog.GetString("Категория аналитики:");
			this.labelCategories.UseMarkup = true;
			this.table1.Add(this.labelCategories);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.labelCategories]));
			w14.TopAttach = ((uint)(4));
			w14.BottomAttach = ((uint)(5));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryItemsType = new global::QS.Views.Control.EntityEntry();
			this.yentryItemsType.Events = ((global::Gdk.EventMask)(256));
			this.yentryItemsType.Name = "yentryItemsType";
			this.table1.Add(this.yentryItemsType);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1[this.yentryItemsType]));
			w15.TopAttach = ((uint)(2));
			w15.BottomAttach = ((uint)(3));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryName = new global::Gamma.GtkWidgets.yEntry();
			this.yentryName.CanFocus = true;
			this.yentryName.Name = "yentryName";
			this.yentryName.IsEditable = true;
			this.yentryName.MaxLength = 800;
			this.yentryName.InvisibleChar = '●';
			this.table1.Add(this.yentryName);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1[this.yentryName]));
			w16.TopAttach = ((uint)(1));
			w16.BottomAttach = ((uint)(2));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelId = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelId.Name = "ylabelId";
			this.ylabelId.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel1");
			this.table1.Add(this.ylabelId);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelId]));
			w17.LeftAttach = ((uint)(1));
			w17.RightAttach = ((uint)(2));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelSupply = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelSupply.Name = "ylabelSupply";
			this.ylabelSupply.Xalign = 1F;
			this.ylabelSupply.Yalign = 0F;
			this.ylabelSupply.LabelProp = global::Mono.Unix.Catalog.GetString("Закупать:");
			this.table1.Add(this.ylabelSupply);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelSupply]));
			w18.TopAttach = ((uint)(5));
			w18.BottomAttach = ((uint)(6));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yspinAssessedCost = new global::Gamma.GtkWidgets.ySpinButton(0D, 99999999D, 10D);
			this.yspinAssessedCost.CanFocus = true;
			this.yspinAssessedCost.Name = "yspinAssessedCost";
			this.yspinAssessedCost.Adjustment.PageIncrement = 100D;
			this.yspinAssessedCost.ClimbRate = 1D;
			this.yspinAssessedCost.Digits = ((uint)(2));
			this.yspinAssessedCost.Numeric = true;
			this.yspinAssessedCost.ValueAsDecimal = 0m;
			this.yspinAssessedCost.ValueAsInt = 0;
			this.table1.Add(this.yspinAssessedCost);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table1[this.yspinAssessedCost]));
			w19.TopAttach = ((uint)(3));
			w19.BottomAttach = ((uint)(4));
			w19.LeftAttach = ((uint)(1));
			w19.RightAttach = ((uint)(2));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ytableSupply = new global::Gamma.GtkWidgets.yTable();
			this.ytableSupply.Name = "ytableSupply";
			this.ytableSupply.NRows = ((uint)(4));
			this.ytableSupply.NColumns = ((uint)(3));
			this.ytableSupply.RowSpacing = ((uint)(6));
			this.ytableSupply.ColumnSpacing = ((uint)(6));
			// Container child ytableSupply.Gtk.Table+TableChild
			this.ybutton_remFemale = new global::Gamma.GtkWidgets.yButton();
			this.ybutton_remFemale.CanFocus = true;
			this.ybutton_remFemale.Name = "ybutton_remFemale";
			this.ybutton_remFemale.UseUnderline = true;
			global::Gtk.Image w20 = new global::Gtk.Image();
			w20.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-delete", global::Gtk.IconSize.Menu);
			this.ybutton_remFemale.Image = w20;
			this.ytableSupply.Add(this.ybutton_remFemale);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.ytableSupply[this.ybutton_remFemale]));
			w21.TopAttach = ((uint)(3));
			w21.BottomAttach = ((uint)(4));
			w21.LeftAttach = ((uint)(2));
			w21.RightAttach = ((uint)(3));
			w21.XOptions = ((global::Gtk.AttachOptions)(0));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableSupply.Gtk.Table+TableChild
			this.ybutton_remMale = new global::Gamma.GtkWidgets.yButton();
			this.ybutton_remMale.CanFocus = true;
			this.ybutton_remMale.Name = "ybutton_remMale";
			this.ybutton_remMale.UseUnderline = true;
			global::Gtk.Image w22 = new global::Gtk.Image();
			w22.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-delete", global::Gtk.IconSize.Menu);
			this.ybutton_remMale.Image = w22;
			this.ytableSupply.Add(this.ybutton_remMale);
			global::Gtk.Table.TableChild w23 = ((global::Gtk.Table.TableChild)(this.ytableSupply[this.ybutton_remMale]));
			w23.TopAttach = ((uint)(2));
			w23.BottomAttach = ((uint)(3));
			w23.LeftAttach = ((uint)(2));
			w23.RightAttach = ((uint)(3));
			w23.XOptions = ((global::Gtk.AttachOptions)(0));
			w23.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableSupply.Gtk.Table+TableChild
			this.ybutton_remUni = new global::Gamma.GtkWidgets.yButton();
			this.ybutton_remUni.CanFocus = true;
			this.ybutton_remUni.Name = "ybutton_remUni";
			this.ybutton_remUni.UseUnderline = true;
			global::Gtk.Image w24 = new global::Gtk.Image();
			w24.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-delete", global::Gtk.IconSize.Menu);
			this.ybutton_remUni.Image = w24;
			this.ytableSupply.Add(this.ybutton_remUni);
			global::Gtk.Table.TableChild w25 = ((global::Gtk.Table.TableChild)(this.ytableSupply[this.ybutton_remUni]));
			w25.TopAttach = ((uint)(1));
			w25.BottomAttach = ((uint)(2));
			w25.LeftAttach = ((uint)(2));
			w25.RightAttach = ((uint)(3));
			w25.XOptions = ((global::Gtk.AttachOptions)(0));
			w25.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableSupply.Gtk.Table+TableChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ybuttonSupplyUni = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSupplyUni.CanFocus = true;
			this.ybuttonSupplyUni.Name = "ybuttonSupplyUni";
			this.ybuttonSupplyUni.UseUnderline = true;
			this.ybuttonSupplyUni.Label = global::Mono.Unix.Catalog.GetString("Универсально");
			this.yhbox1.Add(this.ybuttonSupplyUni);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ybuttonSupplyUni]));
			w26.Position = 0;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ybuttonSupplyTwoSex = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSupplyTwoSex.CanFocus = true;
			this.ybuttonSupplyTwoSex.Name = "ybuttonSupplyTwoSex";
			this.ybuttonSupplyTwoSex.UseUnderline = true;
			this.ybuttonSupplyTwoSex.Label = global::Mono.Unix.Catalog.GetString("Мужской/Женский");
			this.yhbox1.Add(this.ybuttonSupplyTwoSex);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ybuttonSupplyTwoSex]));
			w27.Position = 1;
			this.ytableSupply.Add(this.yhbox1);
			global::Gtk.Table.TableChild w28 = ((global::Gtk.Table.TableChild)(this.ytableSupply[this.yhbox1]));
			w28.RightAttach = ((uint)(3));
			// Container child ytableSupply.Gtk.Table+TableChild
			this.ylabelSupplyFemale = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelSupplyFemale.Name = "ylabelSupplyFemale";
			this.ylabelSupplyFemale.Xalign = 1F;
			this.ylabelSupplyFemale.Yalign = 0F;
			this.ylabelSupplyFemale.LabelProp = global::Mono.Unix.Catalog.GetString("Жен.");
			this.ytableSupply.Add(this.ylabelSupplyFemale);
			global::Gtk.Table.TableChild w29 = ((global::Gtk.Table.TableChild)(this.ytableSupply[this.ylabelSupplyFemale]));
			w29.TopAttach = ((uint)(3));
			w29.BottomAttach = ((uint)(4));
			w29.XOptions = ((global::Gtk.AttachOptions)(4));
			w29.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableSupply.Gtk.Table+TableChild
			this.ylabelSupplyMale = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelSupplyMale.Name = "ylabelSupplyMale";
			this.ylabelSupplyMale.Xalign = 1F;
			this.ylabelSupplyMale.Yalign = 0F;
			this.ylabelSupplyMale.LabelProp = global::Mono.Unix.Catalog.GetString("Муж.");
			this.ytableSupply.Add(this.ylabelSupplyMale);
			global::Gtk.Table.TableChild w30 = ((global::Gtk.Table.TableChild)(this.ytableSupply[this.ylabelSupplyMale]));
			w30.TopAttach = ((uint)(2));
			w30.BottomAttach = ((uint)(3));
			w30.XOptions = ((global::Gtk.AttachOptions)(4));
			w30.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableSupply.Gtk.Table+TableChild
			this.ylabelSupplyUni = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelSupplyUni.Name = "ylabelSupplyUni";
			this.ylabelSupplyUni.Xalign = 1F;
			this.ylabelSupplyUni.Yalign = 0F;
			this.ylabelSupplyUni.LabelProp = global::Mono.Unix.Catalog.GetString("Уни.");
			this.ytableSupply.Add(this.ylabelSupplyUni);
			global::Gtk.Table.TableChild w31 = ((global::Gtk.Table.TableChild)(this.ytableSupply[this.ylabelSupplyUni]));
			w31.TopAttach = ((uint)(1));
			w31.BottomAttach = ((uint)(2));
			w31.XOptions = ((global::Gtk.AttachOptions)(4));
			w31.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableSupply.Gtk.Table+TableChild
			this.ylistcomboboxSupplyFemale = new global::Gamma.Widgets.yListComboBox();
			this.ylistcomboboxSupplyFemale.Name = "ylistcomboboxSupplyFemale";
			this.ylistcomboboxSupplyFemale.AddIfNotExist = false;
			this.ylistcomboboxSupplyFemale.DefaultFirst = false;
			this.ytableSupply.Add(this.ylistcomboboxSupplyFemale);
			global::Gtk.Table.TableChild w32 = ((global::Gtk.Table.TableChild)(this.ytableSupply[this.ylistcomboboxSupplyFemale]));
			w32.TopAttach = ((uint)(3));
			w32.BottomAttach = ((uint)(4));
			w32.LeftAttach = ((uint)(1));
			w32.RightAttach = ((uint)(2));
			w32.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableSupply.Gtk.Table+TableChild
			this.ylistcomboboxSupplyMale = new global::Gamma.Widgets.yListComboBox();
			this.ylistcomboboxSupplyMale.Name = "ylistcomboboxSupplyMale";
			this.ylistcomboboxSupplyMale.AddIfNotExist = false;
			this.ylistcomboboxSupplyMale.DefaultFirst = false;
			this.ytableSupply.Add(this.ylistcomboboxSupplyMale);
			global::Gtk.Table.TableChild w33 = ((global::Gtk.Table.TableChild)(this.ytableSupply[this.ylistcomboboxSupplyMale]));
			w33.TopAttach = ((uint)(2));
			w33.BottomAttach = ((uint)(3));
			w33.LeftAttach = ((uint)(1));
			w33.RightAttach = ((uint)(2));
			w33.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableSupply.Gtk.Table+TableChild
			this.ylistcomboboxSupplyUni = new global::Gamma.Widgets.yListComboBox();
			this.ylistcomboboxSupplyUni.Name = "ylistcomboboxSupplyUni";
			this.ylistcomboboxSupplyUni.AddIfNotExist = false;
			this.ylistcomboboxSupplyUni.DefaultFirst = false;
			this.ytableSupply.Add(this.ylistcomboboxSupplyUni);
			global::Gtk.Table.TableChild w34 = ((global::Gtk.Table.TableChild)(this.ytableSupply[this.ylistcomboboxSupplyUni]));
			w34.TopAttach = ((uint)(1));
			w34.BottomAttach = ((uint)(2));
			w34.LeftAttach = ((uint)(1));
			w34.RightAttach = ((uint)(2));
			w34.YOptions = ((global::Gtk.AttachOptions)(4));
			this.table1.Add(this.ytableSupply);
			global::Gtk.Table.TableChild w35 = ((global::Gtk.Table.TableChild)(this.table1[this.ytableSupply]));
			w35.TopAttach = ((uint)(5));
			w35.BottomAttach = ((uint)(6));
			w35.LeftAttach = ((uint)(1));
			w35.RightAttach = ((uint)(2));
			w35.XOptions = ((global::Gtk.AttachOptions)(4));
			w35.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox5.Add(this.table1);
			global::Gtk.Box.BoxChild w36 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.table1]));
			w36.Position = 0;
			this.dialog1_VBox.Add(this.hbox5);
			global::Gtk.Box.BoxChild w37 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox5]));
			w37.Position = 1;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox7 = new global::Gtk.HBox();
			this.hbox7.Name = "hbox7";
			this.hbox7.Spacing = 6;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonAddNomenclature = new global::Gtk.Button();
			this.buttonAddNomenclature.CanFocus = true;
			this.buttonAddNomenclature.Name = "buttonAddNomenclature";
			this.buttonAddNomenclature.UseUnderline = true;
			this.buttonAddNomenclature.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w38 = new global::Gtk.Image();
			w38.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddNomenclature.Image = w38;
			this.hbox7.Add(this.buttonAddNomenclature);
			global::Gtk.Box.BoxChild w39 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonAddNomenclature]));
			w39.Position = 0;
			w39.Expand = false;
			w39.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonRemoveNomeclature = new global::Gtk.Button();
			this.buttonRemoveNomeclature.Sensitive = false;
			this.buttonRemoveNomeclature.CanFocus = true;
			this.buttonRemoveNomeclature.Name = "buttonRemoveNomeclature";
			this.buttonRemoveNomeclature.UseUnderline = true;
			this.buttonRemoveNomeclature.Label = global::Mono.Unix.Catalog.GetString("Убрать");
			global::Gtk.Image w40 = new global::Gtk.Image();
			w40.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonRemoveNomeclature.Image = w40;
			this.hbox7.Add(this.buttonRemoveNomeclature);
			global::Gtk.Box.BoxChild w41 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonRemoveNomeclature]));
			w41.Position = 1;
			w41.Expand = false;
			w41.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonCreateNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.buttonCreateNomenclature.TooltipMarkup = "Создать складскую номенклатуру с тем же названием.";
			this.buttonCreateNomenclature.CanFocus = true;
			this.buttonCreateNomenclature.Name = "buttonCreateNomenclature";
			this.buttonCreateNomenclature.UseUnderline = true;
			this.buttonCreateNomenclature.Label = global::Mono.Unix.Catalog.GetString("Создать идентичную номенклатуру");
			this.hbox7.Add(this.buttonCreateNomenclature);
			global::Gtk.Box.BoxChild w42 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonCreateNomenclature]));
			w42.PackType = ((global::Gtk.PackType)(1));
			w42.Position = 2;
			w42.Expand = false;
			w42.Fill = false;
			this.dialog1_VBox.Add(this.hbox7);
			global::Gtk.Box.BoxChild w43 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox7]));
			w43.PackType = ((global::Gtk.PackType)(1));
			w43.Position = 2;
			w43.Expand = false;
			w43.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.GtkScrolledWindow2 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow2.Name = "GtkScrolledWindow2";
			this.GtkScrolledWindow2.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow2.Gtk.Container+ContainerChild
			this.ytreeItems = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeItems.CanFocus = true;
			this.ytreeItems.Name = "ytreeItems";
			this.GtkScrolledWindow2.Add(this.ytreeItems);
			this.hbox3.Add(this.GtkScrolledWindow2);
			global::Gtk.Box.BoxChild w45 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.GtkScrolledWindow2]));
			w45.Position = 0;
			this.dialog1_VBox.Add(this.hbox3);
			global::Gtk.Box.BoxChild w46 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox3]));
			w46.PackType = ((global::Gtk.PackType)(1));
			w46.Position = 3;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 0F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Номенклатура</b>");
			this.label5.UseMarkup = true;
			this.dialog1_VBox.Add(this.label5);
			global::Gtk.Box.BoxChild w47 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.label5]));
			w47.PackType = ((global::Gtk.PackType)(1));
			w47.Position = 4;
			w47.Expand = false;
			w47.Fill = false;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.ybuttonSupplyUni.Clicked += new global::System.EventHandler(this.OnYbuttonSupplyUniClicked);
			this.ybuttonSupplyTwoSex.Clicked += new global::System.EventHandler(this.OnYbuttonSupplyTwoSexClicked);
			this.ybutton_remUni.Clicked += new global::System.EventHandler(this.OnYbuttonRemUniClicked);
			this.ybutton_remMale.Clicked += new global::System.EventHandler(this.OnYbuttonRemMaleClicked);
			this.ybutton_remFemale.Clicked += new global::System.EventHandler(this.OnYbuttonRemFemaleClicked);
			this.buttonAddNomenclature.Clicked += new global::System.EventHandler(this.OnButtonAddNomenclatureClicked);
			this.buttonRemoveNomeclature.Clicked += new global::System.EventHandler(this.OnButtonRemoveNomeclatureClicked);
			this.buttonCreateNomenclature.Clicked += new global::System.EventHandler(this.OnButtonCreateNomenclatureClicked);
		}
	}
}
