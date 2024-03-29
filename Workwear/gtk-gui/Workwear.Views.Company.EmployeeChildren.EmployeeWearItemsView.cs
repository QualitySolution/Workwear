
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Company.EmployeeChildren
{
	public partial class EmployeeWearItemsView
	{
		private global::Gtk.VBox vbox7;

		private global::Gtk.ScrolledWindow GtkScrolledWindow3;

		private global::Gamma.GtkWidgets.yTreeView ytreeWorkwear;

		private global::Gtk.HBox hbox11;

		private global::Gtk.Button buttonGiveWearByNorm;

		private global::Gtk.Button buttonReturnWear1;

		private global::Gtk.Button buttonWriteOffWear1;

		private global::Gtk.Button buttonRefreshWorkwearItems;

		private global::QS.Widgets.MenuButton buttonManualIssueDate;

		private global::Gtk.Button buttonTimeLine;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Company.EmployeeChildren.EmployeeWearItemsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Company.EmployeeChildren.EmployeeWearItemsView";
			// Container child Workwear.Views.Company.EmployeeChildren.EmployeeWearItemsView.Gtk.Container+ContainerChild
			this.vbox7 = new global::Gtk.VBox();
			this.vbox7.Name = "vbox7";
			this.vbox7.Spacing = 6;
			// Container child vbox7.Gtk.Box+BoxChild
			this.GtkScrolledWindow3 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow3.Name = "GtkScrolledWindow3";
			this.GtkScrolledWindow3.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow3.Gtk.Container+ContainerChild
			this.ytreeWorkwear = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeWorkwear.CanFocus = true;
			this.ytreeWorkwear.Name = "ytreeWorkwear";
			this.GtkScrolledWindow3.Add(this.ytreeWorkwear);
			this.vbox7.Add(this.GtkScrolledWindow3);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox7[this.GtkScrolledWindow3]));
			w2.Position = 0;
			// Container child vbox7.Gtk.Box+BoxChild
			this.hbox11 = new global::Gtk.HBox();
			this.hbox11.Name = "hbox11";
			this.hbox11.Spacing = 6;
			// Container child hbox11.Gtk.Box+BoxChild
			this.buttonGiveWearByNorm = new global::Gtk.Button();
			this.buttonGiveWearByNorm.CanFocus = true;
			this.buttonGiveWearByNorm.Name = "buttonGiveWearByNorm";
			this.buttonGiveWearByNorm.UseUnderline = true;
			this.buttonGiveWearByNorm.Label = global::Mono.Unix.Catalog.GetString("Выдать неполученное");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonGiveWearByNorm.Image = w3;
			this.hbox11.Add(this.buttonGiveWearByNorm);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox11[this.buttonGiveWearByNorm]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox11.Gtk.Box+BoxChild
			this.buttonReturnWear1 = new global::Gtk.Button();
			this.buttonReturnWear1.CanFocus = true;
			this.buttonReturnWear1.Name = "buttonReturnWear1";
			this.buttonReturnWear1.UseUnderline = true;
			this.buttonReturnWear1.Label = global::Mono.Unix.Catalog.GetString("Возврат на склад");
			global::Gtk.Image w5 = new global::Gtk.Image();
			w5.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-redo", global::Gtk.IconSize.Menu);
			this.buttonReturnWear1.Image = w5;
			this.hbox11.Add(this.buttonReturnWear1);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox11[this.buttonReturnWear1]));
			w6.Position = 1;
			w6.Expand = false;
			w6.Fill = false;
			// Container child hbox11.Gtk.Box+BoxChild
			this.buttonWriteOffWear1 = new global::Gtk.Button();
			this.buttonWriteOffWear1.CanFocus = true;
			this.buttonWriteOffWear1.Name = "buttonWriteOffWear1";
			this.buttonWriteOffWear1.UseUnderline = true;
			this.buttonWriteOffWear1.Label = global::Mono.Unix.Catalog.GetString("Списание");
			global::Gtk.Image w7 = new global::Gtk.Image();
			w7.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-delete", global::Gtk.IconSize.Menu);
			this.buttonWriteOffWear1.Image = w7;
			this.hbox11.Add(this.buttonWriteOffWear1);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox11[this.buttonWriteOffWear1]));
			w8.Position = 2;
			w8.Expand = false;
			w8.Fill = false;
			// Container child hbox11.Gtk.Box+BoxChild
			this.buttonRefreshWorkwearItems = new global::Gtk.Button();
			this.buttonRefreshWorkwearItems.TooltipMarkup = "Обновить список потребностей по указанным нормам";
			this.buttonRefreshWorkwearItems.CanFocus = true;
			this.buttonRefreshWorkwearItems.Name = "buttonRefreshWorkwearItems";
			this.buttonRefreshWorkwearItems.UseUnderline = true;
			this.buttonRefreshWorkwearItems.Label = global::Mono.Unix.Catalog.GetString("Обновить потребности");
			global::Gtk.Image w9 = new global::Gtk.Image();
			w9.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-refresh", global::Gtk.IconSize.Menu);
			this.buttonRefreshWorkwearItems.Image = w9;
			this.hbox11.Add(this.buttonRefreshWorkwearItems);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox11[this.buttonRefreshWorkwearItems]));
			w10.Position = 3;
			w10.Expand = false;
			w10.Fill = false;
			// Container child hbox11.Gtk.Box+BoxChild
			this.buttonManualIssueDate = new global::QS.Widgets.MenuButton();
			this.buttonManualIssueDate.CanFocus = true;
			this.buttonManualIssueDate.Name = "buttonManualIssueDate";
			this.buttonManualIssueDate.UseUnderline = true;
			this.buttonManualIssueDate.UseMarkup = false;
			this.buttonManualIssueDate.LabelXAlign = 0F;
			this.buttonManualIssueDate.Label = global::Mono.Unix.Catalog.GetString("Ручные операции выдачи");
			global::Gtk.Image w11 = new global::Gtk.Image();
			w11.Pixbuf = global::Gdk.Pixbuf.LoadFromResource("Workwear.icon.rows.нand.png");
			this.buttonManualIssueDate.Image = w11;
			this.hbox11.Add(this.buttonManualIssueDate);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox11[this.buttonManualIssueDate]));
			w12.Position = 4;
			w12.Expand = false;
			w12.Fill = false;
			// Container child hbox11.Gtk.Box+BoxChild
			this.buttonTimeLine = new global::Gtk.Button();
			this.buttonTimeLine.Sensitive = false;
			this.buttonTimeLine.CanFocus = true;
			this.buttonTimeLine.Name = "buttonTimeLine";
			this.buttonTimeLine.UseUnderline = true;
			this.buttonTimeLine.Label = global::Mono.Unix.Catalog.GetString("Хронология");
			global::Gtk.Image w13 = new global::Gtk.Image();
			w13.Pixbuf = global::Gdk.Pixbuf.LoadFromResource("Workwear.icon.buttons.timeline_marker.png");
			this.buttonTimeLine.Image = w13;
			this.hbox11.Add(this.buttonTimeLine);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox11[this.buttonTimeLine]));
			w14.PackType = ((global::Gtk.PackType)(1));
			w14.Position = 5;
			w14.Expand = false;
			w14.Fill = false;
			this.vbox7.Add(this.hbox11);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox7[this.hbox11]));
			w15.Position = 1;
			w15.Expand = false;
			w15.Fill = false;
			this.Add(this.vbox7);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonGiveWearByNorm.Clicked += new global::System.EventHandler(this.OnButtonGiveWearByNormClicked);
			this.buttonReturnWear1.Clicked += new global::System.EventHandler(this.OnButtonReturnWearClicked);
			this.buttonWriteOffWear1.Clicked += new global::System.EventHandler(this.OnButtonWriteOffWearClicked);
			this.buttonRefreshWorkwearItems.Clicked += new global::System.EventHandler(this.OnButtonRefreshWorkwearItemsClicked);
			this.buttonTimeLine.Clicked += new global::System.EventHandler(this.OnButtonTimeLineClicked);
		}
	}
}
