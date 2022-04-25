
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Stock
{
	public partial class NomenclatureView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::QS.Widgets.MenuButton menuInternal;

		private global::Gtk.Table table2;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTextView ytextComment;

		private global::Gtk.HBox hbox2;

		private global::Gamma.Widgets.yEnumComboBox ycomboWearStd;

		private global::Gtk.Label label1;

		private global::Gtk.Label label7;

		private global::Gtk.Label label8;

		private global::Gtk.Label label9;

		private global::Gamma.GtkWidgets.yLabel labelSize;

		private global::Gamma.Widgets.yEnumComboBox ycomboClothesSex;

		private global::QS.Views.Control.EntityEntry yentryItemsType;

		private global::Gamma.GtkWidgets.yEntry yentryName;

		private global::Gamma.GtkWidgets.yEntry yentryNumber;

		private global::Gamma.GtkWidgets.yLabel ylabelClothesSex;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Views.Stock.NomenclatureView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Views.Stock.NomenclatureView";
			// Container child workwear.Views.Stock.NomenclatureView.Gtk.Container+ContainerChild
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
			// Container child hbox4.Gtk.Box+BoxChild
			this.menuInternal = new global::QS.Widgets.MenuButton();
			this.menuInternal.CanFocus = true;
			this.menuInternal.Name = "menuInternal";
			this.menuInternal.UseUnderline = true;
			this.menuInternal.UseMarkup = false;
			this.menuInternal.LabelXAlign = 0F;
			global::Gtk.Image w5 = new global::Gtk.Image();
			w5.Pixbuf = global::Gdk.Pixbuf.LoadFromResource("workwear.icon.buttons.menu.png");
			this.menuInternal.Image = w5;
			this.hbox4.Add(this.menuInternal);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.menuInternal]));
			w6.Position = 2;
			w6.Expand = false;
			w6.Fill = false;
			this.dialog1_VBox.Add(this.hbox4);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.hbox4]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.table2 = new global::Gtk.Table(((uint)(6)), ((uint)(2)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytextComment = new global::Gamma.GtkWidgets.yTextView();
			this.ytextComment.CanFocus = true;
			this.ytextComment.Name = "ytextComment";
			this.ytextComment.WrapMode = ((global::Gtk.WrapMode)(3));
			this.GtkScrolledWindow.Add(this.ytextComment);
			this.table2.Add(this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table2[this.GtkScrolledWindow]));
			w9.TopAttach = ((uint)(5));
			w9.BottomAttach = ((uint)(6));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.ycomboWearStd = new global::Gamma.Widgets.yEnumComboBox();
			this.ycomboWearStd.Name = "ycomboWearStd";
			this.ycomboWearStd.ShowSpecialStateAll = false;
			this.ycomboWearStd.ShowSpecialStateNot = false;
			this.ycomboWearStd.UseShortTitle = true;
			this.ycomboWearStd.DefaultFirst = true;
			this.hbox2.Add(this.ycomboWearStd);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.ycomboWearStd]));
			w10.Position = 0;
			this.table2.Add(this.hbox2);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table2[this.hbox2]));
			w11.TopAttach = ((uint)(4));
			w11.BottomAttach = ((uint)(5));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.Yalign = 0F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table2.Add(this.label1);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table2[this.label1]));
			w12.TopAttach = ((uint)(5));
			w12.BottomAttach = ((uint)(6));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label();
			this.label7.Name = "label7";
			this.label7.Xalign = 1F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString("Номенклатурный номер:");
			this.table2.Add(this.label7);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table2[this.label7]));
			w13.TopAttach = ((uint)(2));
			w13.BottomAttach = ((uint)(3));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label8 = new global::Gtk.Label();
			this.label8.Name = "label8";
			this.label8.Xalign = 1F;
			this.label8.LabelProp = global::Mono.Unix.Catalog.GetString("Наименование<span foreground=\"red\">*</span>:");
			this.label8.UseMarkup = true;
			this.table2.Add(this.label8);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table2[this.label8]));
			w14.TopAttach = ((uint)(1));
			w14.BottomAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label9 = new global::Gtk.Label();
			this.label9.Name = "label9";
			this.label9.Xalign = 1F;
			this.label9.LabelProp = global::Mono.Unix.Catalog.GetString("Тип номенклатуры<span foreground=\"red\">*</span>:");
			this.label9.UseMarkup = true;
			this.table2.Add(this.label9);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table2[this.label9]));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.labelSize = new global::Gamma.GtkWidgets.yLabel();
			this.labelSize.Name = "labelSize";
			this.labelSize.Xalign = 1F;
			this.labelSize.LabelProp = global::Mono.Unix.Catalog.GetString("Стандарт размера:");
			this.table2.Add(this.labelSize);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table2[this.labelSize]));
			w16.TopAttach = ((uint)(4));
			w16.BottomAttach = ((uint)(5));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ycomboClothesSex = new global::Gamma.Widgets.yEnumComboBox();
			this.ycomboClothesSex.Name = "ycomboClothesSex";
			this.ycomboClothesSex.ShowSpecialStateAll = false;
			this.ycomboClothesSex.ShowSpecialStateNot = false;
			this.ycomboClothesSex.UseShortTitle = false;
			this.ycomboClothesSex.DefaultFirst = false;
			this.table2.Add(this.ycomboClothesSex);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table2[this.ycomboClothesSex]));
			w17.TopAttach = ((uint)(3));
			w17.BottomAttach = ((uint)(4));
			w17.LeftAttach = ((uint)(1));
			w17.RightAttach = ((uint)(2));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryItemsType = new global::QS.Views.Control.EntityEntry();
			this.yentryItemsType.Events = ((global::Gdk.EventMask)(256));
			this.yentryItemsType.Name = "yentryItemsType";
			this.table2.Add(this.yentryItemsType);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryItemsType]));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(2));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryName = new global::Gamma.GtkWidgets.yEntry();
			this.yentryName.CanFocus = true;
			this.yentryName.Name = "yentryName";
			this.yentryName.IsEditable = true;
			this.yentryName.MaxLength = 240;
			this.yentryName.InvisibleChar = '●';
			this.table2.Add(this.yentryName);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryName]));
			w19.TopAttach = ((uint)(1));
			w19.BottomAttach = ((uint)(2));
			w19.LeftAttach = ((uint)(1));
			w19.RightAttach = ((uint)(2));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryNumber = new global::Gamma.GtkWidgets.yEntry();
			this.yentryNumber.CanFocus = true;
			this.yentryNumber.Name = "yentryNumber";
			this.yentryNumber.IsEditable = true;
			this.yentryNumber.MaxLength = 240;
			this.yentryNumber.InvisibleChar = '●';
			this.table2.Add(this.yentryNumber);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryNumber]));
			w20.TopAttach = ((uint)(2));
			w20.BottomAttach = ((uint)(3));
			w20.LeftAttach = ((uint)(1));
			w20.RightAttach = ((uint)(2));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ylabelClothesSex = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelClothesSex.Name = "ylabelClothesSex";
			this.ylabelClothesSex.Xalign = 1F;
			this.ylabelClothesSex.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel1");
			this.table2.Add(this.ylabelClothesSex);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table2[this.ylabelClothesSex]));
			w21.TopAttach = ((uint)(3));
			w21.BottomAttach = ((uint)(4));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			this.dialog1_VBox.Add(this.table2);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.table2]));
			w22.Position = 1;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.ycomboClothesSex.Hide();
			this.ylabelClothesSex.Hide();
			this.Show();
		}
	}
}
