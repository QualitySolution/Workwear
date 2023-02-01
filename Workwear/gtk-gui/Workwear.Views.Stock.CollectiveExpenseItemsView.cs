
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Stock
{
	public partial class CollectiveExpenseItemsView
	{
		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox2;

		private global::Gtk.Label label1;

		private global::Gamma.GtkWidgets.yButton buttonRefreshEmployees;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeItems;

		private global::Gtk.HBox hbox1;

		private global::QS.Widgets.MenuButton buttonAdd;

		private global::Gamma.GtkWidgets.yButton buttonRefreshEmployee;

		private global::QS.Widgets.MenuButton buttonDel;

		private global::Gtk.Button buttonShowAllSize;

		private global::Gamma.GtkWidgets.yLabel labelSum;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Stock.CollectiveExpenseItemsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Stock.CollectiveExpenseItemsView";
			// Container child Workwear.Views.Stock.CollectiveExpenseItemsView.Gtk.Container+ContainerChild
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
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Строки документа");
			this.hbox2.Add(this.label1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.label1]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.buttonRefreshEmployees = new global::Gamma.GtkWidgets.yButton();
			this.buttonRefreshEmployees.CanFocus = true;
			this.buttonRefreshEmployees.Name = "buttonRefreshEmployees";
			this.buttonRefreshEmployees.UseUnderline = true;
			this.buttonRefreshEmployees.Label = global::Mono.Unix.Catalog.GetString("Перезаполнить всех");
			global::Gtk.Image w2 = new global::Gtk.Image();
			w2.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-redo", global::Gtk.IconSize.Menu);
			this.buttonRefreshEmployees.Image = w2;
			this.hbox2.Add(this.buttonRefreshEmployees);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.buttonRefreshEmployees]));
			w3.PackType = ((global::Gtk.PackType)(1));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			this.vbox2.Add(this.hbox2);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox2]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
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
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.GtkScrolledWindow]));
			w6.Position = 1;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonAdd = new global::QS.Widgets.MenuButton();
			this.buttonAdd.CanFocus = true;
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.UseUnderline = true;
			this.buttonAdd.UseMarkup = false;
			this.buttonAdd.LabelXAlign = 0F;
			this.buttonAdd.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w7 = new global::Gtk.Image();
			w7.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAdd.Image = w7;
			this.hbox1.Add(this.buttonAdd);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonAdd]));
			w8.Position = 0;
			w8.Expand = false;
			w8.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonRefreshEmployee = new global::Gamma.GtkWidgets.yButton();
			this.buttonRefreshEmployee.CanFocus = true;
			this.buttonRefreshEmployee.Name = "buttonRefreshEmployee";
			this.buttonRefreshEmployee.UseUnderline = true;
			this.buttonRefreshEmployee.Label = global::Mono.Unix.Catalog.GetString("Перезаполнить сотрудника");
			global::Gtk.Image w9 = new global::Gtk.Image();
			w9.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-redo", global::Gtk.IconSize.Menu);
			this.buttonRefreshEmployee.Image = w9;
			this.hbox1.Add(this.buttonRefreshEmployee);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonRefreshEmployee]));
			w10.Position = 1;
			w10.Expand = false;
			w10.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonDel = new global::QS.Widgets.MenuButton();
			this.buttonDel.Sensitive = false;
			this.buttonDel.CanFocus = true;
			this.buttonDel.Name = "buttonDel";
			this.buttonDel.UseUnderline = true;
			this.buttonDel.UseMarkup = false;
			this.buttonDel.LabelXAlign = 0F;
			this.buttonDel.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w11 = new global::Gtk.Image();
			w11.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonDel.Image = w11;
			this.hbox1.Add(this.buttonDel);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonDel]));
			w12.Position = 2;
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
			w13.Position = 3;
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
			w14.Position = 4;
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
			this.buttonRefreshEmployees.Clicked += new global::System.EventHandler(this.OnButtonRefreshEmployeesClicked);
			this.buttonAdd.Clicked += new global::System.EventHandler(this.OnButtonDelClicked);
			this.buttonRefreshEmployee.Clicked += new global::System.EventHandler(this.OnButtonRefreshEmployeeClicked);
			this.buttonDel.Clicked += new global::System.EventHandler(this.OnButtonDelClicked);
			this.buttonShowAllSize.Clicked += new global::System.EventHandler(this.OnButtonShowAllSizeClicked);
		}
	}
}
