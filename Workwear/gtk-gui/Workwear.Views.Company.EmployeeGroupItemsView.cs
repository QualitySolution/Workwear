
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Company
{
	public partial class EmployeeGroupItemsView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeItems;

		private global::Gtk.HBox hbox5;

		private global::Gtk.Button buttonAdd;

		private global::Gtk.Button buttonRemove;

		private global::Gtk.Button buttonтOpen;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Company.EmployeeGroupItemsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Company.EmployeeGroupItemsView";
			// Container child Workwear.Views.Company.EmployeeGroupItemsView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeItems = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeItems.CanFocus = true;
			this.ytreeItems.Name = "ytreeItems";
			this.GtkScrolledWindow.Add(this.ytreeItems);
			this.vbox1.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow]));
			w2.Position = 0;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox5 = new global::Gtk.HBox();
			this.hbox5.Name = "hbox5";
			this.hbox5.Spacing = 6;
			// Container child hbox5.Gtk.Box+BoxChild
			this.buttonAdd = new global::Gtk.Button();
			this.buttonAdd.CanFocus = true;
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.UseUnderline = true;
			this.buttonAdd.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAdd.Image = w3;
			this.hbox5.Add(this.buttonAdd);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.buttonAdd]));
			w4.Position = 0;
			w4.Expand = false;
			// Container child hbox5.Gtk.Box+BoxChild
			this.buttonRemove = new global::Gtk.Button();
			this.buttonRemove.Sensitive = false;
			this.buttonRemove.CanFocus = true;
			this.buttonRemove.Name = "buttonRemove";
			this.buttonRemove.UseUnderline = true;
			this.buttonRemove.Label = global::Mono.Unix.Catalog.GetString("Исключить");
			global::Gtk.Image w5 = new global::Gtk.Image();
			w5.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonRemove.Image = w5;
			this.hbox5.Add(this.buttonRemove);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.buttonRemove]));
			w6.Position = 1;
			w6.Expand = false;
			// Container child hbox5.Gtk.Box+BoxChild
			this.buttonтOpen = new global::Gtk.Button();
			this.buttonтOpen.Sensitive = false;
			this.buttonтOpen.CanFocus = true;
			this.buttonтOpen.Name = "buttonтOpen";
			this.buttonтOpen.UseUnderline = true;
			this.buttonтOpen.Label = global::Mono.Unix.Catalog.GetString("Открыть сотрудников");
			global::Gtk.Image w7 = new global::Gtk.Image();
			w7.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "stock_new-bcard", global::Gtk.IconSize.Menu);
			this.buttonтOpen.Image = w7;
			this.hbox5.Add(this.buttonтOpen);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.buttonтOpen]));
			w8.Position = 2;
			w8.Expand = false;
			w8.Fill = false;
			this.vbox1.Add(this.hbox5);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox5]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}