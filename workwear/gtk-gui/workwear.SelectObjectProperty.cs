
// This file has been generated by the GUI designer. Do not modify.
namespace workwear
{
	public partial class SelectObjectProperty
	{
		private global::Gtk.HBox hbox1;
		private global::Gtk.Label label1;
		private global::Gtk.ComboBox comboObject;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.TreeView treeviewCardRows;
		private global::Gtk.Button buttonCancel;
		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget workwear.SelectObjectProperty
			this.Name = "workwear.SelectObjectProperty";
			this.Title = global::Mono.Unix.Catalog.GetString ("Выданные ТМЦ");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child workwear.SelectObjectProperty.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			this.hbox1.BorderWidth = ((uint)(6));
			// Container child hbox1.Gtk.Box+BoxChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Показывать только по объекту:");
			this.hbox1.Add (this.label1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.label1]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.comboObject = new global::Gtk.ComboBox ();
			this.comboObject.Name = "comboObject";
			this.hbox1.Add (this.comboObject);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.comboObject]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			w1.Add (this.hbox1);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(w1 [this.hbox1]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			this.GtkScrolledWindow.BorderWidth = ((uint)(6));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.treeviewCardRows = new global::Gtk.TreeView ();
			this.treeviewCardRows.CanFocus = true;
			this.treeviewCardRows.Name = "treeviewCardRows";
			this.GtkScrolledWindow.Add (this.treeviewCardRows);
			w1.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(w1 [this.GtkScrolledWindow]));
			w6.Position = 1;
			// Internal child workwear.SelectObjectProperty.ActionArea
			global::Gtk.HButtonBox w7 = this.ActionArea;
			w7.Name = "dialog1_ActionArea";
			w7.Spacing = 10;
			w7.BorderWidth = ((uint)(5));
			w7.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			// Container child buttonCancel.Gtk.Container+ContainerChild
			global::Gtk.Alignment w8 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w9 = new global::Gtk.HBox ();
			w9.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w10 = new global::Gtk.Image ();
			w10.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-cancel", global::Gtk.IconSize.Menu);
			w9.Add (w10);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w12 = new global::Gtk.Label ();
			w12.LabelProp = global::Mono.Unix.Catalog.GetString ("О_тменить");
			w12.UseUnderline = true;
			w9.Add (w12);
			w8.Add (w9);
			this.buttonCancel.Add (w8);
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w16 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w7 [this.buttonCancel]));
			w16.Expand = false;
			w16.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseUnderline = true;
			// Container child buttonOk.Gtk.Container+ContainerChild
			global::Gtk.Alignment w17 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w18 = new global::Gtk.HBox ();
			w18.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w19 = new global::Gtk.Image ();
			w19.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-ok", global::Gtk.IconSize.Menu);
			w18.Add (w19);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w21 = new global::Gtk.Label ();
			w21.LabelProp = global::Mono.Unix.Catalog.GetString ("_OK");
			w21.UseUnderline = true;
			w18.Add (w21);
			w17.Add (w18);
			this.buttonOk.Add (w17);
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w25 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w7 [this.buttonOk]));
			w25.Position = 1;
			w25.Expand = false;
			w25.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 676;
			this.DefaultHeight = 304;
			this.Show ();
			this.comboObject.Changed += new global::System.EventHandler (this.OnComboWorkerChanged);
			this.treeviewCardRows.CursorChanged += new global::System.EventHandler (this.OnTreeviewCardRowsCursorChanged);
			this.treeviewCardRows.RowActivated += new global::Gtk.RowActivatedHandler (this.OnTreeviewCardRowsRowActivated);
			this.buttonOk.Clicked += new global::System.EventHandler (this.OnButtonOkClicked);
		}
	}
}
