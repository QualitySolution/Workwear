
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Stock.Widgets
{
	public partial class SizeWidgetView
	{
		private global::Gtk.Table table1;

		private global::Gamma.GtkWidgets.yButton AddButton;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gtk.Table CheckBoxPlace;

		private global::Gtk.HBox hbox2;

		private global::Gtk.Button selectAllButton;

		private global::Gtk.HBox GrowthInfoBox;

		private global::Gtk.Label GrowthInfoLabel;

		private global::Gamma.Widgets.yListComboBox GrowthBox;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Views.Stock.Widgets.SizeWidgetView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Views.Stock.Widgets.SizeWidgetView";
			// Container child workwear.Views.Stock.Widgets.SizeWidgetView.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(3)), ((uint)(3)), false);
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.AddButton = new global::Gamma.GtkWidgets.yButton();
			this.AddButton.CanFocus = true;
			this.AddButton.Name = "AddButton";
			this.AddButton.UseUnderline = true;
			this.AddButton.Label = global::Mono.Unix.Catalog.GetString("Добавить в документ");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.AddButton.Image = w1;
			this.table1.Add(this.AddButton);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.AddButton]));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(0));
			// Container child table1.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.HeightRequest = 600;
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.HscrollbarPolicy = ((global::Gtk.PolicyType)(2));
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			global::Gtk.Viewport w3 = new global::Gtk.Viewport();
			w3.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.CheckBoxPlace = new global::Gtk.Table(((uint)(1)), ((uint)(4)), false);
			this.CheckBoxPlace.Name = "CheckBoxPlace";
			this.CheckBoxPlace.RowSpacing = ((uint)(6));
			this.CheckBoxPlace.ColumnSpacing = ((uint)(6));
			w3.Add(this.CheckBoxPlace);
			this.GtkScrolledWindow.Add(w3);
			this.table1.Add(this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.GtkScrolledWindow]));
			w6.TopAttach = ((uint)(2));
			w6.BottomAttach = ((uint)(3));
			w6.RightAttach = ((uint)(3));
			// Container child table1.Gtk.Table+TableChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.selectAllButton = new global::Gtk.Button();
			this.selectAllButton.CanFocus = true;
			this.selectAllButton.Name = "selectAllButton";
			this.selectAllButton.UseUnderline = true;
			this.selectAllButton.Label = global::Mono.Unix.Catalog.GetString("Выбрать все");
			this.hbox2.Add(this.selectAllButton);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.selectAllButton]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.GrowthInfoBox = new global::Gtk.HBox();
			this.GrowthInfoBox.Name = "GrowthInfoBox";
			this.GrowthInfoBox.Spacing = 6;
			// Container child GrowthInfoBox.Gtk.Box+BoxChild
			this.GrowthInfoLabel = new global::Gtk.Label();
			this.GrowthInfoLabel.Name = "GrowthInfoLabel";
			this.GrowthInfoLabel.Xalign = 1F;
			this.GrowthInfoLabel.LabelProp = global::Mono.Unix.Catalog.GetString("Рост:");
			this.GrowthInfoBox.Add(this.GrowthInfoLabel);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.GrowthInfoBox[this.GrowthInfoLabel]));
			w8.Position = 0;
			// Container child GrowthInfoBox.Gtk.Box+BoxChild
			this.GrowthBox = new global::Gamma.Widgets.yListComboBox();
			this.GrowthBox.Name = "GrowthBox";
			this.GrowthBox.AddIfNotExist = false;
			this.GrowthBox.DefaultFirst = false;
			this.GrowthInfoBox.Add(this.GrowthBox);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.GrowthInfoBox[this.GrowthBox]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			this.hbox2.Add(this.GrowthInfoBox);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.GrowthInfoBox]));
			w10.Position = 1;
			this.table1.Add(this.hbox2);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox2]));
			w11.TopAttach = ((uint)(1));
			w11.BottomAttach = ((uint)(2));
			w11.RightAttach = ((uint)(3));
			w11.YOptions = ((global::Gtk.AttachOptions)(6));
			this.Add(this.table1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.selectAllButton.Clicked += new global::System.EventHandler(this.selectAllButton_Clicked);
			this.AddButton.Clicked += new global::System.EventHandler(this.OnAddButtonClicked);
		}
	}
}
