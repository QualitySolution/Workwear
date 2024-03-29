
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.ReportParameters.Views
{
	public partial class ChoiceProtectionToolsView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox2;

		private global::Gamma.GtkWidgets.yButton ycheckbuttonChooseAll;

		private global::Gamma.GtkWidgets.yButton ycheckbuttonUnChooseAll;

		private global::Gamma.GtkWidgets.yEntry yentrySearch;

		private global::Gamma.GtkWidgets.yImage yimage1;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeChoiseProtectionTools;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.ReportParameters.Views.ChoiceProtectionToolsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.ReportParameters.Views.ChoiceProtectionToolsView";
			// Container child Workwear.ReportParameters.Views.ChoiceProtectionToolsView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.ycheckbuttonChooseAll = new global::Gamma.GtkWidgets.yButton();
			this.ycheckbuttonChooseAll.CanFocus = true;
			this.ycheckbuttonChooseAll.Name = "ycheckbuttonChooseAll";
			this.ycheckbuttonChooseAll.UseUnderline = true;
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Gdk.Pixbuf.LoadFromResource("Workwear.icon.buttons.select-all.png");
			this.ycheckbuttonChooseAll.Image = w1;
			this.hbox2.Add(this.ycheckbuttonChooseAll);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.ycheckbuttonChooseAll]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.ycheckbuttonUnChooseAll = new global::Gamma.GtkWidgets.yButton();
			this.ycheckbuttonUnChooseAll.CanFocus = true;
			this.ycheckbuttonUnChooseAll.Name = "ycheckbuttonUnChooseAll";
			this.ycheckbuttonUnChooseAll.UseUnderline = true;
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Gdk.Pixbuf.LoadFromResource("Workwear.icon.buttons.unselect-all.png");
			this.ycheckbuttonUnChooseAll.Image = w3;
			this.hbox2.Add(this.ycheckbuttonUnChooseAll);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.ycheckbuttonUnChooseAll]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.yentrySearch = new global::Gamma.GtkWidgets.yEntry();
			this.yentrySearch.CanFocus = true;
			this.yentrySearch.Name = "yentrySearch";
			this.yentrySearch.IsEditable = true;
			this.yentrySearch.InvisibleChar = '•';
			this.hbox2.Add(this.yentrySearch);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.yentrySearch]));
			w5.Position = 2;
			// Container child hbox2.Gtk.Box+BoxChild
			this.yimage1 = new global::Gamma.GtkWidgets.yImage();
			this.yimage1.Name = "yimage1";
			this.yimage1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-find", global::Gtk.IconSize.Menu);
			this.hbox2.Add(this.yimage1);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.yimage1]));
			w6.Position = 3;
			w6.Expand = false;
			w6.Fill = false;
			this.vbox1.Add(this.hbox2);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox2]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeChoiseProtectionTools = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeChoiseProtectionTools.WidthRequest = 400;
			this.ytreeChoiseProtectionTools.CanFocus = true;
			this.ytreeChoiseProtectionTools.Name = "ytreeChoiseProtectionTools";
			this.GtkScrolledWindow.Add(this.ytreeChoiseProtectionTools);
			this.vbox1.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow]));
			w9.Position = 1;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
