
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Stock
{
	public partial class NomenclatureView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::QS.Widgets.MenuButton menuInternal;

		private global::Gtk.Table table2;

		private global::Gamma.GtkWidgets.yCheckButton checkWashable;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTextView ytextComment;

		private global::Gtk.Label label1;

		private global::Gtk.Label label7;

		private global::Gtk.Label label8;

		private global::Gtk.Label label9;

		private global::Gtk.Label labelSaleCost;

		private global::Gamma.GtkWidgets.yCheckButton ycheckArchival;

		private global::Gamma.GtkWidgets.yCheckButton ycheckBarcode;

		private global::Gamma.Widgets.yEnumComboBox ycomboClothesSex;

		private global::QS.Views.Control.EntityEntry yentryItemsType;

		private global::Gamma.GtkWidgets.yEntry yentryName;

		private global::Gamma.GtkWidgets.yEntry yentryNumber;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yLabel ylabelAvgRating;

		private global::Gamma.GtkWidgets.yButton ybuttonratingDetails;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gamma.GtkWidgets.yLabel ylabelClothesSex;

		private global::Gamma.GtkWidgets.ySpinButton yspinbuttonSaleCost;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Stock.NomenclatureView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Stock.NomenclatureView";
			// Container child Workwear.Views.Stock.NomenclatureView.Gtk.Container+ContainerChild
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
			w5.Pixbuf = global::Gdk.Pixbuf.LoadFromResource("Workwear.icon.buttons.menu.png");
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
			this.table2 = new global::Gtk.Table(((uint)(10)), ((uint)(2)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.checkWashable = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkWashable.WidthRequest = 0;
			this.checkWashable.HeightRequest = 0;
			this.checkWashable.CanFocus = true;
			this.checkWashable.Name = "checkWashable";
			this.checkWashable.Label = global::Mono.Unix.Catalog.GetString("Можно сдать в стирку");
			this.checkWashable.DrawIndicator = true;
			this.checkWashable.UseUnderline = true;
			this.checkWashable.FocusOnClick = false;
			this.checkWashable.Xalign = 0F;
			this.checkWashable.Yalign = 0F;
			this.table2.Add(this.checkWashable);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table2[this.checkWashable]));
			w8.TopAttach = ((uint)(7));
			w8.BottomAttach = ((uint)(8));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
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
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table2[this.GtkScrolledWindow]));
			w10.TopAttach = ((uint)(9));
			w10.BottomAttach = ((uint)(10));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.Yalign = 0F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.table2.Add(this.label1);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table2[this.label1]));
			w11.TopAttach = ((uint)(9));
			w11.BottomAttach = ((uint)(10));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label();
			this.label7.Name = "label7";
			this.label7.Xalign = 1F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString("Номенклатурный номер:");
			this.table2.Add(this.label7);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table2[this.label7]));
			w12.TopAttach = ((uint)(2));
			w12.BottomAttach = ((uint)(3));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label8 = new global::Gtk.Label();
			this.label8.Name = "label8";
			this.label8.Xalign = 1F;
			this.label8.LabelProp = global::Mono.Unix.Catalog.GetString("Наименование<span foreground=\"red\">*</span>:");
			this.label8.UseMarkup = true;
			this.table2.Add(this.label8);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table2[this.label8]));
			w13.TopAttach = ((uint)(1));
			w13.BottomAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label9 = new global::Gtk.Label();
			this.label9.Name = "label9";
			this.label9.Xalign = 1F;
			this.label9.LabelProp = global::Mono.Unix.Catalog.GetString("Тип номенклатуры<span foreground=\"red\">*</span>:");
			this.label9.UseMarkup = true;
			this.table2.Add(this.label9);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table2[this.label9]));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.labelSaleCost = new global::Gtk.Label();
			this.labelSaleCost.Name = "labelSaleCost";
			this.labelSaleCost.Xalign = 1F;
			this.labelSaleCost.LabelProp = global::Mono.Unix.Catalog.GetString("Стоимость продажи:");
			this.table2.Add(this.labelSaleCost);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table2[this.labelSaleCost]));
			w15.TopAttach = ((uint)(3));
			w15.BottomAttach = ((uint)(4));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ycheckArchival = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckArchival.CanFocus = true;
			this.ycheckArchival.Name = "ycheckArchival";
			this.ycheckArchival.Label = global::Mono.Unix.Catalog.GetString("Архивная");
			this.ycheckArchival.DrawIndicator = true;
			this.ycheckArchival.UseUnderline = true;
			this.table2.Add(this.ycheckArchival);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table2[this.ycheckArchival]));
			w16.TopAttach = ((uint)(5));
			w16.BottomAttach = ((uint)(6));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ycheckBarcode = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckBarcode.CanFocus = true;
			this.ycheckBarcode.Name = "ycheckBarcode";
			this.ycheckBarcode.Label = global::Mono.Unix.Catalog.GetString("Использовать штрихкод");
			this.ycheckBarcode.DrawIndicator = true;
			this.ycheckBarcode.UseUnderline = true;
			this.table2.Add(this.ycheckBarcode);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table2[this.ycheckBarcode]));
			w17.TopAttach = ((uint)(6));
			w17.BottomAttach = ((uint)(7));
			w17.LeftAttach = ((uint)(1));
			w17.RightAttach = ((uint)(2));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ycomboClothesSex = new global::Gamma.Widgets.yEnumComboBox();
			this.ycomboClothesSex.Name = "ycomboClothesSex";
			this.ycomboClothesSex.ShowSpecialStateAll = false;
			this.ycomboClothesSex.ShowSpecialStateNot = false;
			this.ycomboClothesSex.UseShortTitle = false;
			this.ycomboClothesSex.DefaultFirst = false;
			this.table2.Add(this.ycomboClothesSex);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table2[this.ycomboClothesSex]));
			w18.TopAttach = ((uint)(4));
			w18.BottomAttach = ((uint)(5));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(2));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryItemsType = new global::QS.Views.Control.EntityEntry();
			this.yentryItemsType.Events = ((global::Gdk.EventMask)(256));
			this.yentryItemsType.Name = "yentryItemsType";
			this.table2.Add(this.yentryItemsType);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryItemsType]));
			w19.LeftAttach = ((uint)(1));
			w19.RightAttach = ((uint)(2));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryName = new global::Gamma.GtkWidgets.yEntry();
			this.yentryName.CanFocus = true;
			this.yentryName.Name = "yentryName";
			this.yentryName.IsEditable = true;
			this.yentryName.MaxLength = 240;
			this.yentryName.InvisibleChar = '●';
			this.table2.Add(this.yentryName);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryName]));
			w20.TopAttach = ((uint)(1));
			w20.BottomAttach = ((uint)(2));
			w20.LeftAttach = ((uint)(1));
			w20.RightAttach = ((uint)(2));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryNumber = new global::Gamma.GtkWidgets.yEntry();
			this.yentryNumber.CanFocus = true;
			this.yentryNumber.Name = "yentryNumber";
			this.yentryNumber.IsEditable = true;
			this.yentryNumber.MaxLength = 20;
			this.yentryNumber.InvisibleChar = '●';
			this.table2.Add(this.yentryNumber);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryNumber]));
			w21.TopAttach = ((uint)(2));
			w21.BottomAttach = ((uint)(3));
			w21.LeftAttach = ((uint)(1));
			w21.RightAttach = ((uint)(2));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ylabelAvgRating = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelAvgRating.Name = "ylabelAvgRating";
			this.ylabelAvgRating.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel2");
			this.yhbox1.Add(this.ylabelAvgRating);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ylabelAvgRating]));
			w22.Position = 0;
			w22.Expand = false;
			w22.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ybuttonratingDetails = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonratingDetails.CanFocus = true;
			this.ybuttonratingDetails.Name = "ybuttonratingDetails";
			this.ybuttonratingDetails.UseUnderline = true;
			this.ybuttonratingDetails.Label = global::Mono.Unix.Catalog.GetString("Подробнее");
			this.yhbox1.Add(this.ybuttonratingDetails);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ybuttonratingDetails]));
			w23.Position = 1;
			w23.Expand = false;
			w23.Fill = false;
			this.table2.Add(this.yhbox1);
			global::Gtk.Table.TableChild w24 = ((global::Gtk.Table.TableChild)(this.table2[this.yhbox1]));
			w24.TopAttach = ((uint)(8));
			w24.BottomAttach = ((uint)(9));
			w24.LeftAttach = ((uint)(1));
			w24.RightAttach = ((uint)(2));
			w24.XOptions = ((global::Gtk.AttachOptions)(4));
			w24.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.Xalign = 1F;
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Средняя оценка:");
			this.table2.Add(this.ylabel1);
			global::Gtk.Table.TableChild w25 = ((global::Gtk.Table.TableChild)(this.table2[this.ylabel1]));
			w25.TopAttach = ((uint)(8));
			w25.BottomAttach = ((uint)(9));
			w25.XOptions = ((global::Gtk.AttachOptions)(4));
			w25.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ylabelClothesSex = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelClothesSex.Name = "ylabelClothesSex";
			this.ylabelClothesSex.Xalign = 1F;
			this.ylabelClothesSex.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel1");
			this.table2.Add(this.ylabelClothesSex);
			global::Gtk.Table.TableChild w26 = ((global::Gtk.Table.TableChild)(this.table2[this.ylabelClothesSex]));
			w26.TopAttach = ((uint)(4));
			w26.BottomAttach = ((uint)(5));
			w26.XOptions = ((global::Gtk.AttachOptions)(4));
			w26.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yspinbuttonSaleCost = new global::Gamma.GtkWidgets.ySpinButton(0D, 99999999D, 1D);
			this.yspinbuttonSaleCost.CanFocus = true;
			this.yspinbuttonSaleCost.Name = "yspinbuttonSaleCost";
			this.yspinbuttonSaleCost.Adjustment.PageIncrement = 10D;
			this.yspinbuttonSaleCost.ClimbRate = 1D;
			this.yspinbuttonSaleCost.Digits = ((uint)(2));
			this.yspinbuttonSaleCost.Numeric = true;
			this.yspinbuttonSaleCost.ValueAsDecimal = 0m;
			this.yspinbuttonSaleCost.ValueAsInt = 0;
			this.table2.Add(this.yspinbuttonSaleCost);
			global::Gtk.Table.TableChild w27 = ((global::Gtk.Table.TableChild)(this.table2[this.yspinbuttonSaleCost]));
			w27.TopAttach = ((uint)(3));
			w27.BottomAttach = ((uint)(4));
			w27.LeftAttach = ((uint)(1));
			w27.RightAttach = ((uint)(2));
			w27.XOptions = ((global::Gtk.AttachOptions)(4));
			w27.YOptions = ((global::Gtk.AttachOptions)(4));
			this.dialog1_VBox.Add(this.table2);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.table2]));
			w28.Position = 1;
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
