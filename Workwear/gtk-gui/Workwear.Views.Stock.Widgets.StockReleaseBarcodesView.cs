
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Stock.Widgets
{
	public partial class StockReleaseBarcodesView
	{
		private global::Gamma.GtkWidgets.yVBox yvbox1;

		private global::Gamma.GtkWidgets.yLabel labelDescription;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gamma.GtkWidgets.ySpinButton amountSpin;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yEntry entryBarcodesLabel;

		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::Gamma.GtkWidgets.yLabel labelAllNomenclature;

		private global::Gamma.GtkWidgets.yLabel labelBarcodesInStock;

		private global::Gamma.GtkWidgets.yLabel labelWithBarcodesAmount;

		private global::Gamma.GtkWidgets.yLabel labelWithoutBarcodes;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

		private global::Gamma.GtkWidgets.yLabel ylabel4;

		private global::Gamma.GtkWidgets.yLabel ylabel5;

		private global::Gamma.GtkWidgets.yLabel ylabel8;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yButton createBarcodesButton;

		private global::Gamma.GtkWidgets.yButton ybuttonCancel;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Stock.Widgets.StockReleaseBarcodesView
			global::Stetic.BinContainer.Attach(this);
			this.WidthRequest = 0;
			this.HeightRequest = 0;
			this.Name = "Workwear.Views.Stock.Widgets.StockReleaseBarcodesView";
			// Container child Workwear.Views.Stock.Widgets.StockReleaseBarcodesView.Gtk.Container+ContainerChild
			this.yvbox1 = new global::Gamma.GtkWidgets.yVBox();
			this.yvbox1.WidthRequest = 0;
			this.yvbox1.HeightRequest = 0;
			this.yvbox1.Name = "yvbox1";
			this.yvbox1.Spacing = 6;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.labelDescription = new global::Gamma.GtkWidgets.yLabel();
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Wrap = true;
			this.yvbox1.Add(this.labelDescription);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.labelDescription]));
			w1.Position = 0;
			w1.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Количество штрихкодов");
			this.yvbox1.Add(this.ylabel1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.ylabel1]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.amountSpin = new global::Gamma.GtkWidgets.ySpinButton(0D, 100D, 1D);
			this.amountSpin.CanFocus = true;
			this.amountSpin.Name = "amountSpin";
			this.amountSpin.Adjustment.PageIncrement = 10D;
			this.amountSpin.ClimbRate = 1D;
			this.amountSpin.Numeric = true;
			this.amountSpin.ValueAsDecimal = 0m;
			this.amountSpin.ValueAsInt = 0;
			this.yvbox1.Add(this.amountSpin);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.amountSpin]));
			w3.Position = 2;
			w3.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Название");
			this.yvbox1.Add(this.ylabel2);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.ylabel2]));
			w4.Position = 3;
			w4.Expand = false;
			w4.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.entryBarcodesLabel = new global::Gamma.GtkWidgets.yEntry();
			this.entryBarcodesLabel.CanFocus = true;
			this.entryBarcodesLabel.Name = "entryBarcodesLabel";
			this.entryBarcodesLabel.IsEditable = true;
			this.entryBarcodesLabel.InvisibleChar = '•';
			this.yvbox1.Add(this.entryBarcodesLabel);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.entryBarcodesLabel]));
			w5.Position = 4;
			w5.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(4));
			this.ytable1.NColumns = ((uint)(2));
			this.ytable1.Homogeneous = true;
			this.ytable1.RowSpacing = ((uint)(6));
			this.ytable1.ColumnSpacing = ((uint)(10));
			// Container child ytable1.Gtk.Table+TableChild
			this.labelAllNomenclature = new global::Gamma.GtkWidgets.yLabel();
			this.labelAllNomenclature.Name = "labelAllNomenclature";
			this.labelAllNomenclature.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel6");
			this.ytable1.Add(this.labelAllNomenclature);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytable1[this.labelAllNomenclature]));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.labelBarcodesInStock = new global::Gamma.GtkWidgets.yLabel();
			this.labelBarcodesInStock.Name = "labelBarcodesInStock";
			this.labelBarcodesInStock.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel7");
			this.ytable1.Add(this.labelBarcodesInStock);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.ytable1[this.labelBarcodesInStock]));
			w7.TopAttach = ((uint)(3));
			w7.BottomAttach = ((uint)(4));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.labelWithBarcodesAmount = new global::Gamma.GtkWidgets.yLabel();
			this.labelWithBarcodesAmount.Name = "labelWithBarcodesAmount";
			this.labelWithBarcodesAmount.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel9");
			this.ytable1.Add(this.labelWithBarcodesAmount);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytable1[this.labelWithBarcodesAmount]));
			w8.TopAttach = ((uint)(2));
			w8.BottomAttach = ((uint)(3));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.labelWithoutBarcodes = new global::Gamma.GtkWidgets.yLabel();
			this.labelWithoutBarcodes.Name = "labelWithoutBarcodes";
			this.labelWithoutBarcodes.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel6");
			this.ytable1.Add(this.labelWithoutBarcodes);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.ytable1[this.labelWithoutBarcodes]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(2));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 0F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Без штрихкода:");
			this.ytable1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel3]));
			w10.TopAttach = ((uint)(1));
			w10.BottomAttach = ((uint)(2));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel4 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel4.Name = "ylabel4";
			this.ylabel4.Xalign = 0F;
			this.ylabel4.LabelProp = global::Mono.Unix.Catalog.GetString("Штрихкодов на складе:");
			this.ytable1.Add(this.ylabel4);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel4]));
			w11.TopAttach = ((uint)(3));
			w11.BottomAttach = ((uint)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel5 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel5.Name = "ylabel5";
			this.ylabel5.Xalign = 0F;
			this.ylabel5.LabelProp = global::Mono.Unix.Catalog.GetString("Общее количество:");
			this.ytable1.Add(this.ylabel5);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel5]));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel8 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel8.Name = "ylabel8";
			this.ylabel8.Xalign = 0F;
			this.ylabel8.LabelProp = global::Mono.Unix.Catalog.GetString("Со штрихкодом:");
			this.ytable1.Add(this.ylabel8);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel8]));
			w13.TopAttach = ((uint)(2));
			w13.BottomAttach = ((uint)(3));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			this.yvbox1.Add(this.ytable1);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.ytable1]));
			w14.Position = 5;
			w14.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.createBarcodesButton = new global::Gamma.GtkWidgets.yButton();
			this.createBarcodesButton.WidthRequest = 0;
			this.createBarcodesButton.HeightRequest = 0;
			this.createBarcodesButton.CanFocus = true;
			this.createBarcodesButton.Name = "createBarcodesButton";
			this.createBarcodesButton.UseUnderline = true;
			this.createBarcodesButton.FocusOnClick = false;
			this.createBarcodesButton.Xalign = 0F;
			this.createBarcodesButton.Yalign = 0F;
			this.createBarcodesButton.Label = global::Mono.Unix.Catalog.GetString("Создать");
			global::Gtk.Image w15 = new global::Gtk.Image();
			w15.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-apply", global::Gtk.IconSize.Menu);
			this.createBarcodesButton.Image = w15;
			this.yhbox1.Add(this.createBarcodesButton);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.createBarcodesButton]));
			w16.PackType = ((global::Gtk.PackType)(1));
			w16.Position = 0;
			w16.Expand = false;
			w16.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ybuttonCancel = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCancel.CanFocus = true;
			this.ybuttonCancel.Name = "ybuttonCancel";
			this.ybuttonCancel.UseUnderline = true;
			this.ybuttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			global::Gtk.Image w17 = new global::Gtk.Image();
			w17.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-cancel", global::Gtk.IconSize.Menu);
			this.ybuttonCancel.Image = w17;
			this.yhbox1.Add(this.ybuttonCancel);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ybuttonCancel]));
			w18.PackType = ((global::Gtk.PackType)(1));
			w18.Position = 1;
			w18.Expand = false;
			w18.Fill = false;
			this.yvbox1.Add(this.yhbox1);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.yhbox1]));
			w19.Position = 6;
			w19.Fill = false;
			this.Add(this.yvbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Show();
			this.ybuttonCancel.Clicked += new global::System.EventHandler(this.OnButtonCancel);
			this.createBarcodesButton.Clicked += new global::System.EventHandler(this.OnCreateBarcodesButtonClicked);
		}
	}
}
