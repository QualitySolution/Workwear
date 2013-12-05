
// This file has been generated by the GUI designer. Do not modify.
namespace workwear
{
	public partial class IncomeTable
	{
		private global::Gtk.VBox vbox2;
		private global::Gtk.Label label1;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.TreeView treeviewItems;
		private global::Gtk.HBox hbox1;
		private global::Gtk.Button buttonAdd;
		private global::Gtk.Button buttonDel;
		private global::Gtk.Label labelSum;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget workwear.IncomeTable
			global::Stetic.BinContainer.Attach (this);
			this.Name = "workwear.IncomeTable";
			// Container child workwear.IncomeTable.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.Xalign = 0F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Поступления на склад");
			this.vbox2.Add (this.label1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.label1]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.treeviewItems = new global::Gtk.TreeView ();
			this.treeviewItems.HeightRequest = 120;
			this.treeviewItems.CanFocus = true;
			this.treeviewItems.Name = "treeviewItems";
			this.GtkScrolledWindow.Add (this.treeviewItems);
			this.vbox2.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.GtkScrolledWindow]));
			w3.Position = 1;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonAdd = new global::Gtk.Button ();
			this.buttonAdd.TooltipMarkup = "Добавить услугу из начисления";
			this.buttonAdd.Sensitive = false;
			this.buttonAdd.CanFocus = true;
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.UseUnderline = true;
			this.buttonAdd.Label = global::Mono.Unix.Catalog.GetString ("Добавить");
			global::Gtk.Image w4 = new global::Gtk.Image ();
			w4.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAdd.Image = w4;
			this.hbox1.Add (this.buttonAdd);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.buttonAdd]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonDel = new global::Gtk.Button ();
			this.buttonDel.CanFocus = true;
			this.buttonDel.Name = "buttonDel";
			this.buttonDel.UseUnderline = true;
			this.buttonDel.Label = global::Mono.Unix.Catalog.GetString ("Удалить");
			global::Gtk.Image w6 = new global::Gtk.Image ();
			w6.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonDel.Image = w6;
			this.hbox1.Add (this.buttonDel);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.buttonDel]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.labelSum = new global::Gtk.Label ();
			this.labelSum.Name = "labelSum";
			this.labelSum.Xalign = 1F;
			this.labelSum.LabelProp = global::Mono.Unix.Catalog.GetString ("Количество:");
			this.hbox1.Add (this.labelSum);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.labelSum]));
			w8.Position = 2;
			this.vbox2.Add (this.hbox1);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox1]));
			w9.Position = 2;
			w9.Expand = false;
			w9.Fill = false;
			this.Add (this.vbox2);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
			this.treeviewItems.CursorChanged += new global::System.EventHandler (this.OnTreeviewItemsCursorChanged);
			this.buttonAdd.Clicked += new global::System.EventHandler (this.OnButtonAddClicked);
			this.buttonDel.Clicked += new global::System.EventHandler (this.OnButtonDelClicked);
		}
	}
}
