
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Company.EmployeeChilds
{
	public partial class EmployeeListedItemsView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::QSOrmProject.RepresentationTreeView treeviewListedItems;

		private global::Gtk.HBox hbox9;

		private global::Gtk.Button buttonGiveWear;

		private global::Gtk.Button buttonReturnWear;

		private global::Gtk.Button buttonWriteOffWear;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Views.Company.EmployeeChilds.EmployeeListedItemsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Views.Company.EmployeeChilds.EmployeeListedItemsView";
			// Container child workwear.Views.Company.EmployeeChilds.EmployeeListedItemsView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.treeviewListedItems = new global::QSOrmProject.RepresentationTreeView();
			this.treeviewListedItems.CanFocus = true;
			this.treeviewListedItems.Name = "treeviewListedItems";
			this.GtkScrolledWindow.Add(this.treeviewListedItems);
			this.vbox1.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow]));
			w2.Position = 0;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox9 = new global::Gtk.HBox();
			this.hbox9.Name = "hbox9";
			this.hbox9.Spacing = 6;
			// Container child hbox9.Gtk.Box+BoxChild
			this.buttonGiveWear = new global::Gtk.Button();
			this.buttonGiveWear.Sensitive = false;
			this.buttonGiveWear.CanFocus = true;
			this.buttonGiveWear.Name = "buttonGiveWear";
			this.buttonGiveWear.UseUnderline = true;
			this.buttonGiveWear.Label = global::Mono.Unix.Catalog.GetString("Выдача сотруднику");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonGiveWear.Image = w3;
			this.hbox9.Add(this.buttonGiveWear);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox9[this.buttonGiveWear]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox9.Gtk.Box+BoxChild
			this.buttonReturnWear = new global::Gtk.Button();
			this.buttonReturnWear.Sensitive = false;
			this.buttonReturnWear.CanFocus = true;
			this.buttonReturnWear.Name = "buttonReturnWear";
			this.buttonReturnWear.UseUnderline = true;
			this.buttonReturnWear.Label = global::Mono.Unix.Catalog.GetString("Возврат на склад");
			global::Gtk.Image w5 = new global::Gtk.Image();
			w5.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-redo", global::Gtk.IconSize.Menu);
			this.buttonReturnWear.Image = w5;
			this.hbox9.Add(this.buttonReturnWear);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox9[this.buttonReturnWear]));
			w6.Position = 1;
			w6.Expand = false;
			w6.Fill = false;
			// Container child hbox9.Gtk.Box+BoxChild
			this.buttonWriteOffWear = new global::Gtk.Button();
			this.buttonWriteOffWear.Sensitive = false;
			this.buttonWriteOffWear.CanFocus = true;
			this.buttonWriteOffWear.Name = "buttonWriteOffWear";
			this.buttonWriteOffWear.UseUnderline = true;
			this.buttonWriteOffWear.Label = global::Mono.Unix.Catalog.GetString("Списание");
			global::Gtk.Image w7 = new global::Gtk.Image();
			w7.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-delete", global::Gtk.IconSize.Menu);
			this.buttonWriteOffWear.Image = w7;
			this.hbox9.Add(this.buttonWriteOffWear);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox9[this.buttonWriteOffWear]));
			w8.Position = 2;
			w8.Expand = false;
			w8.Fill = false;
			this.vbox1.Add(this.hbox9);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox9]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonGiveWear.Clicked += new global::System.EventHandler(this.OnButtonGiveWearClicked);
			this.buttonReturnWear.Clicked += new global::System.EventHandler(this.OnButtonReturnWearClicked);
			this.buttonWriteOffWear.Clicked += new global::System.EventHandler(this.OnButtonWriteOffWearClicked);
		}
	}
}