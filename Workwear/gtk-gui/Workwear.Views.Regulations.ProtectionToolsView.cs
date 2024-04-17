
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

		private global::Gtk.Label label11;

		private global::Gtk.Label label2;

		private global::Gtk.Label label4;

		private global::Gtk.Label label9;

		private global::QS.Views.Control.EntityEntry yentryItemsType;

		private global::Gamma.GtkWidgets.yEntry yentryName;

		private global::Gamma.GtkWidgets.yLabel ylabelId;

		private global::Gamma.GtkWidgets.ySpinButton yspinAssessedCost;

		private global::Gtk.HBox hbox7;

		private global::Gtk.Button buttonAddNomenclature;

		private global::Gtk.Button buttonRemoveNomeclature;

		private global::Gamma.GtkWidgets.yButton buttonCreateNomenclature;

		private global::Gtk.HBox hbox2;

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
			this.table1 = new global::Gtk.Table(((uint)(6)), ((uint)(2)), false);
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
			w8.TopAttach = ((uint)(5));
			w8.BottomAttach = ((uint)(6));
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
			this.label11 = new global::Gtk.Label();
			this.label11.Name = "label11";
			this.label11.Xalign = 1F;
			this.label11.LabelProp = global::Mono.Unix.Catalog.GetString("Категория аналитики:");
			this.label11.UseMarkup = true;
			this.table1.Add(this.label11);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.label11]));
			w11.TopAttach = ((uint)(4));
			w11.BottomAttach = ((uint)(5));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Наименование<span foreground=\"red\">*</span>:");
			this.label2.UseMarkup = true;
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w12.TopAttach = ((uint)(1));
			w12.BottomAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.Yalign = 0F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table1.Add(this.label4);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.label4]));
			w13.TopAttach = ((uint)(5));
			w13.BottomAttach = ((uint)(6));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label9 = new global::Gtk.Label();
			this.label9.Name = "label9";
			this.label9.Xalign = 1F;
			this.label9.LabelProp = global::Mono.Unix.Catalog.GetString("Тип номенклатуры<span foreground=\"red\">*</span>:");
			this.label9.UseMarkup = true;
			this.table1.Add(this.label9);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.label9]));
			w14.TopAttach = ((uint)(2));
			w14.BottomAttach = ((uint)(3));
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
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table1[this.yspinAssessedCost]));
			w18.TopAttach = ((uint)(3));
			w18.BottomAttach = ((uint)(4));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(2));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox5.Add(this.table1);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.table1]));
			w19.Position = 0;
			this.dialog1_VBox.Add(this.hbox5);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox5]));
			w20.Position = 1;
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
			global::Gtk.Image w21 = new global::Gtk.Image();
			w21.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddNomenclature.Image = w21;
			this.hbox7.Add(this.buttonAddNomenclature);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonAddNomenclature]));
			w22.Position = 0;
			w22.Expand = false;
			w22.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonRemoveNomeclature = new global::Gtk.Button();
			this.buttonRemoveNomeclature.Sensitive = false;
			this.buttonRemoveNomeclature.CanFocus = true;
			this.buttonRemoveNomeclature.Name = "buttonRemoveNomeclature";
			this.buttonRemoveNomeclature.UseUnderline = true;
			this.buttonRemoveNomeclature.Label = global::Mono.Unix.Catalog.GetString("Убрать");
			global::Gtk.Image w23 = new global::Gtk.Image();
			w23.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonRemoveNomeclature.Image = w23;
			this.hbox7.Add(this.buttonRemoveNomeclature);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonRemoveNomeclature]));
			w24.Position = 1;
			w24.Expand = false;
			w24.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonCreateNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.buttonCreateNomenclature.TooltipMarkup = "Создать складскую номенклатуру с тем же названием.";
			this.buttonCreateNomenclature.CanFocus = true;
			this.buttonCreateNomenclature.Name = "buttonCreateNomenclature";
			this.buttonCreateNomenclature.UseUnderline = true;
			this.buttonCreateNomenclature.Label = global::Mono.Unix.Catalog.GetString("Создать идентичную номенклатуру");
			this.hbox7.Add(this.buttonCreateNomenclature);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonCreateNomenclature]));
			w25.PackType = ((global::Gtk.PackType)(1));
			w25.Position = 2;
			w25.Expand = false;
			w25.Fill = false;
			this.dialog1_VBox.Add(this.hbox7);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox7]));
			w26.PackType = ((global::Gtk.PackType)(1));
			w26.Position = 2;
			w26.Expand = false;
			w26.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow2 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow2.Name = "GtkScrolledWindow2";
			this.GtkScrolledWindow2.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow2.Gtk.Container+ContainerChild
			this.ytreeItems = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeItems.CanFocus = true;
			this.ytreeItems.Name = "ytreeItems";
			this.GtkScrolledWindow2.Add(this.ytreeItems);
			this.hbox2.Add(this.GtkScrolledWindow2);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.GtkScrolledWindow2]));
			w28.Position = 0;
			this.dialog1_VBox.Add(this.hbox2);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox2]));
			w29.PackType = ((global::Gtk.PackType)(1));
			w29.Position = 3;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 0F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Номенклатура</b>");
			this.label5.UseMarkup = true;
			this.dialog1_VBox.Add(this.label5);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.label5]));
			w30.PackType = ((global::Gtk.PackType)(1));
			w30.Position = 4;
			w30.Expand = false;
			w30.Fill = false;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonAddNomenclature.Clicked += new global::System.EventHandler(this.OnButtonAddNomenclatureClicked);
			this.buttonRemoveNomeclature.Clicked += new global::System.EventHandler(this.OnButtonRemoveNomeclatureClicked);
			this.buttonCreateNomenclature.Clicked += new global::System.EventHandler(this.OnButtonCreateNomenclatureClicked);
		}
	}
}
