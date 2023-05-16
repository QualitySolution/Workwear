
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Company.EmployeeChildren
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

		private global::Gtk.Button buttonInspection;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Company.EmployeeChildren.EmployeeListedItemsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Company.EmployeeChildren.EmployeeListedItemsView";
			// Container child Workwear.Views.Company.EmployeeChildren.EmployeeListedItemsView.Gtk.Container+ContainerChild
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
			// Container child hbox9.Gtk.Box+BoxChild
			this.buttonInspection = new global::Gtk.Button();
			this.buttonInspection.Sensitive = false;
			this.buttonInspection.CanFocus = true;
			this.buttonInspection.Name = "buttonInspection";
			this.buttonInspection.UseUnderline = true;
			this.buttonInspection.Label = global::Mono.Unix.Catalog.GetString("Переоценка");
			global::Gtk.Image w9 = new global::Gtk.Image();
			w9.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-find", global::Gtk.IconSize.Menu);
			this.buttonInspection.Image = w9;
			this.hbox9.Add(this.buttonInspection);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox9[this.buttonInspection]));
			w10.Position = 3;
			w10.Expand = false;
			w10.Fill = false;
			this.vbox1.Add(this.hbox9);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox9]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonGiveWear.Clicked += new global::System.EventHandler(this.OnButtonGiveWearClicked);
			this.buttonReturnWear.Clicked += new global::System.EventHandler(this.OnButtonReturnWearClicked);
			this.buttonWriteOffWear.Clicked += new global::System.EventHandler(this.OnButtonWriteOffWearClicked);
			this.buttonInspection.Clicked += new global::System.EventHandler(this.OnButtonInspectionClicked);
		}
	}
}
