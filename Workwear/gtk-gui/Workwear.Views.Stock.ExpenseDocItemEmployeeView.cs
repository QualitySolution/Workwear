
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Stock
{
	public partial class ExpenseDocItemEmployeeView
	{
		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox2;

		private global::Gtk.Label label1;

		private global::Gamma.GtkWidgets.yButton buttonColorsLegend;

		private global::Gtk.Button buttonFillBuhDoc;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeItems;

		private global::Gtk.HBox hbox1;

		private global::Gamma.GtkWidgets.yButton buttonAdd;

		private global::Gtk.Button buttonDel;

		private global::Gtk.Button buttonShowAllSize;

		private global::Gamma.GtkWidgets.yLabel labelSum;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Stock.ExpenseDocItemEmployeeView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Stock.ExpenseDocItemEmployeeView";
			// Container child Workwear.Views.Stock.ExpenseDocItemEmployeeView.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 0F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Выдача со склада");
			this.hbox2.Add(this.label1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.label1]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.buttonColorsLegend = new global::Gamma.GtkWidgets.yButton();
			this.buttonColorsLegend.TooltipMarkup = "Цветовая легенда";
			this.buttonColorsLegend.CanFocus = true;
			this.buttonColorsLegend.Name = "buttonColorsLegend";
			this.buttonColorsLegend.UseUnderline = true;
			global::Gtk.Image w2 = new global::Gtk.Image();
			w2.Pixbuf = global::Gdk.Pixbuf.LoadFromResource("Workwear.icon.buttons.legend.png");
			this.buttonColorsLegend.Image = w2;
			this.hbox2.Add(this.buttonColorsLegend);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.buttonColorsLegend]));
			w3.PackType = ((global::Gtk.PackType)(1));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.buttonFillBuhDoc = new global::Gtk.Button();
			this.buttonFillBuhDoc.CanFocus = true;
			this.buttonFillBuhDoc.Name = "buttonFillBuhDoc";
			this.buttonFillBuhDoc.UseUnderline = true;
			this.buttonFillBuhDoc.Label = global::Mono.Unix.Catalog.GetString("Заменить бух. документ");
			global::Gtk.Image w4 = new global::Gtk.Image();
			w4.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-copy", global::Gtk.IconSize.Menu);
			this.buttonFillBuhDoc.Image = w4;
			this.hbox2.Add(this.buttonFillBuhDoc);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.buttonFillBuhDoc]));
			w5.PackType = ((global::Gtk.PackType)(1));
			w5.Position = 2;
			w5.Expand = false;
			w5.Fill = false;
			this.vbox2.Add(this.hbox2);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox2]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
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
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.GtkScrolledWindow]));
			w8.Position = 1;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonAdd = new global::Gamma.GtkWidgets.yButton();
			this.buttonAdd.CanFocus = true;
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.UseUnderline = true;
			this.buttonAdd.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w9 = new global::Gtk.Image();
			w9.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAdd.Image = w9;
			this.hbox1.Add(this.buttonAdd);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonAdd]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonDel = new global::Gtk.Button();
			this.buttonDel.Sensitive = false;
			this.buttonDel.CanFocus = true;
			this.buttonDel.Name = "buttonDel";
			this.buttonDel.UseUnderline = true;
			this.buttonDel.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w11 = new global::Gtk.Image();
			w11.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonDel.Image = w11;
			this.hbox1.Add(this.buttonDel);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonDel]));
			w12.Position = 1;
			w12.Expand = false;
			w12.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonShowAllSize = new global::Gtk.Button();
			this.buttonShowAllSize.Sensitive = false;
			this.buttonShowAllSize.CanFocus = true;
			this.buttonShowAllSize.Name = "buttonShowAllSize";
			this.buttonShowAllSize.UseUnderline = true;
			this.buttonShowAllSize.Label = global::Mono.Unix.Catalog.GetString("Выбрать другой размер");
			this.hbox1.Add(this.buttonShowAllSize);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonShowAllSize]));
			w13.Position = 2;
			w13.Expand = false;
			w13.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.labelSum = new global::Gamma.GtkWidgets.yLabel();
			this.labelSum.Name = "labelSum";
			this.labelSum.Xalign = 1F;
			this.labelSum.LabelProp = global::Mono.Unix.Catalog.GetString("Количество:");
			this.labelSum.UseMarkup = true;
			this.hbox1.Add(this.labelSum);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.labelSum]));
			w14.Position = 3;
			this.vbox2.Add(this.hbox1);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox1]));
			w15.Position = 2;
			w15.Expand = false;
			w15.Fill = false;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonFillBuhDoc.Clicked += new global::System.EventHandler(this.OnButtonFillBuhDocClicked);
			this.buttonColorsLegend.Clicked += new global::System.EventHandler(this.OnButtonColorsLegendClicked);
			this.buttonAdd.Clicked += new global::System.EventHandler(this.OnButtonAddClicked);
			this.buttonDel.Clicked += new global::System.EventHandler(this.OnButtonDelClicked);
			this.buttonShowAllSize.Clicked += new global::System.EventHandler(this.OnButtonShowAllSizeClicked);
		}
	}
}
