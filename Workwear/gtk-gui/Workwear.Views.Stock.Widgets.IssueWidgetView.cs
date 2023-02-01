
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Stock.Widgets
{
	public partial class IssueWidgetView
	{
		private global::Gtk.Table table1;

		private global::Gtk.HBox hbox1;

		private global::Gamma.GtkWidgets.yButton AddToDocument_ybutton;

		private global::Gamma.GtkWidgets.yButton SelectAll_ybutton;

		private global::Gamma.GtkWidgets.yButton UnSelectAll_ybutton;

		private global::Gtk.ScrolledWindow scrolledwindow1;

		private global::Gtk.Table ItemListTable;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Stock.Widgets.IssueWidgetView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Stock.Widgets.IssueWidgetView";
			// Container child Workwear.Views.Stock.Widgets.IssueWidgetView.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(2)), ((uint)(1)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.AddToDocument_ybutton = new global::Gamma.GtkWidgets.yButton();
			this.AddToDocument_ybutton.CanFocus = true;
			this.AddToDocument_ybutton.Name = "AddToDocument_ybutton";
			this.AddToDocument_ybutton.UseUnderline = true;
			this.AddToDocument_ybutton.Label = global::Mono.Unix.Catalog.GetString("Добавить в документ");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.AddToDocument_ybutton.Image = w1;
			this.hbox1.Add(this.AddToDocument_ybutton);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.AddToDocument_ybutton]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.SelectAll_ybutton = new global::Gamma.GtkWidgets.yButton();
			this.SelectAll_ybutton.CanFocus = true;
			this.SelectAll_ybutton.Name = "SelectAll_ybutton";
			this.SelectAll_ybutton.UseUnderline = true;
			this.SelectAll_ybutton.Label = global::Mono.Unix.Catalog.GetString("Выделить все");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-apply", global::Gtk.IconSize.Menu);
			this.SelectAll_ybutton.Image = w3;
			this.hbox1.Add(this.SelectAll_ybutton);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.SelectAll_ybutton]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.UnSelectAll_ybutton = new global::Gamma.GtkWidgets.yButton();
			this.UnSelectAll_ybutton.CanFocus = true;
			this.UnSelectAll_ybutton.Name = "UnSelectAll_ybutton";
			this.UnSelectAll_ybutton.UseUnderline = true;
			this.UnSelectAll_ybutton.Label = global::Mono.Unix.Catalog.GetString("Снять выделение");
			global::Gtk.Image w5 = new global::Gtk.Image();
			w5.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-close", global::Gtk.IconSize.Menu);
			this.UnSelectAll_ybutton.Image = w5;
			this.hbox1.Add(this.UnSelectAll_ybutton);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.UnSelectAll_ybutton]));
			w6.Position = 2;
			w6.Expand = false;
			w6.Fill = false;
			this.table1.Add(this.hbox1);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox1]));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow();
			this.scrolledwindow1.CanFocus = true;
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow1.Gtk.Container+ContainerChild
			global::Gtk.Viewport w8 = new global::Gtk.Viewport();
			w8.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.ItemListTable = new global::Gtk.Table(((uint)(1)), ((uint)(4)), false);
			this.ItemListTable.Name = "ItemListTable";
			this.ItemListTable.RowSpacing = ((uint)(6));
			this.ItemListTable.ColumnSpacing = ((uint)(6));
			w8.Add(this.ItemListTable);
			this.scrolledwindow1.Add(w8);
			this.table1.Add(this.scrolledwindow1);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.scrolledwindow1]));
			w11.TopAttach = ((uint)(1));
			w11.BottomAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.table1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.AddToDocument_ybutton.Clicked += new global::System.EventHandler(this.OnAddToDocumentYbuttonClicked);
			this.SelectAll_ybutton.Clicked += new global::System.EventHandler(this.OnSelectAllYbuttonClicked);
		}
	}
}
