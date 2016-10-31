
// This file has been generated by the GUI designer. Do not modify.
namespace workwear
{
	public partial class NomenclatureDlg
	{
		private global::Gtk.Table table2;
		
		private global::Gtk.HBox hbox2;
		
		private global::Gamma.Widgets.yEnumComboBox ycomboWearStd;
		
		private global::Gamma.GtkWidgets.yComboBox ycomboWearSize;
		
		private global::Gtk.Label label7;
		
		private global::Gtk.Label label8;
		
		private global::Gtk.Label label9;
		
		private global::Gtk.Label labelGrowth;
		
		private global::Gtk.Label labelSize;
		
		private global::Gamma.Widgets.yEnumComboBox ycomboClothesSex;
		
		private global::Gamma.GtkWidgets.yComboBox ycomboWearGrowth;
		
		private global::Gamma.Widgets.yEntryReference yentryItemsType;
		
		private global::Gamma.GtkWidgets.yEntry yentryName;
		
		private global::Gamma.GtkWidgets.yLabel ylabelClothesSex;
		
		private global::Gamma.GtkWidgets.yLabel ylabelId;
		
		private global::Gtk.Button buttonCancel;
		
		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget workwear.NomenclatureDlg
			this.Name = "workwear.NomenclatureDlg";
			this.Title = global::Mono.Unix.Catalog.GetString ("Новая номенклатура");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child workwear.NomenclatureDlg.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.table2 = new global::Gtk.Table (((uint)(6)), ((uint)(2)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.ycomboWearStd = new global::Gamma.Widgets.yEnumComboBox ();
			this.ycomboWearStd.Name = "ycomboWearStd";
			this.ycomboWearStd.ShowSpecialStateAll = false;
			this.ycomboWearStd.ShowSpecialStateNot = false;
			this.ycomboWearStd.UseShortTitle = true;
			this.ycomboWearStd.DefaultFirst = true;
			this.hbox2.Add (this.ycomboWearStd);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.ycomboWearStd]));
			w2.Position = 0;
			// Container child hbox2.Gtk.Box+BoxChild
			this.ycomboWearSize = new global::Gamma.GtkWidgets.yComboBox ();
			this.ycomboWearSize.Name = "ycomboWearSize";
			this.hbox2.Add (this.ycomboWearSize);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.ycomboWearSize]));
			w3.Position = 1;
			this.table2.Add (this.hbox2);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table2 [this.hbox2]));
			w4.TopAttach = ((uint)(4));
			w4.BottomAttach = ((uint)(5));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label ();
			this.label7.Name = "label7";
			this.label7.Xalign = 1F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString ("Код:");
			this.table2.Add (this.label7);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table2 [this.label7]));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label8 = new global::Gtk.Label ();
			this.label8.Name = "label8";
			this.label8.Xalign = 1F;
			this.label8.LabelProp = global::Mono.Unix.Catalog.GetString ("Наименование<span foreground=\"red\">*</span>:");
			this.label8.UseMarkup = true;
			this.table2.Add (this.label8);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table2 [this.label8]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label9 = new global::Gtk.Label ();
			this.label9.Name = "label9";
			this.label9.Xalign = 1F;
			this.label9.LabelProp = global::Mono.Unix.Catalog.GetString ("Номенклатурная группа<span foreground=\"red\">*</span>:");
			this.label9.UseMarkup = true;
			this.table2.Add (this.label9);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table2 [this.label9]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.labelGrowth = new global::Gtk.Label ();
			this.labelGrowth.Name = "labelGrowth";
			this.labelGrowth.Xalign = 1F;
			this.labelGrowth.LabelProp = global::Mono.Unix.Catalog.GetString ("Рост:");
			this.table2.Add (this.labelGrowth);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table2 [this.labelGrowth]));
			w8.TopAttach = ((uint)(5));
			w8.BottomAttach = ((uint)(6));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.labelSize = new global::Gtk.Label ();
			this.labelSize.Name = "labelSize";
			this.labelSize.Xalign = 1F;
			this.labelSize.LabelProp = global::Mono.Unix.Catalog.GetString ("Размер:");
			this.table2.Add (this.labelSize);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table2 [this.labelSize]));
			w9.TopAttach = ((uint)(4));
			w9.BottomAttach = ((uint)(5));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ycomboClothesSex = new global::Gamma.Widgets.yEnumComboBox ();
			this.ycomboClothesSex.Name = "ycomboClothesSex";
			this.ycomboClothesSex.ShowSpecialStateAll = false;
			this.ycomboClothesSex.ShowSpecialStateNot = false;
			this.ycomboClothesSex.UseShortTitle = false;
			this.ycomboClothesSex.DefaultFirst = false;
			this.table2.Add (this.ycomboClothesSex);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table2 [this.ycomboClothesSex]));
			w10.TopAttach = ((uint)(3));
			w10.BottomAttach = ((uint)(4));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ycomboWearGrowth = new global::Gamma.GtkWidgets.yComboBox ();
			this.ycomboWearGrowth.Name = "ycomboWearGrowth";
			this.table2.Add (this.ycomboWearGrowth);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table2 [this.ycomboWearGrowth]));
			w11.TopAttach = ((uint)(5));
			w11.BottomAttach = ((uint)(6));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryItemsType = new global::Gamma.Widgets.yEntryReference ();
			this.yentryItemsType.Events = ((global::Gdk.EventMask)(256));
			this.yentryItemsType.Name = "yentryItemsType";
			this.table2.Add (this.yentryItemsType);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table2 [this.yentryItemsType]));
			w12.TopAttach = ((uint)(2));
			w12.BottomAttach = ((uint)(3));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryName = new global::Gamma.GtkWidgets.yEntry ();
			this.yentryName.CanFocus = true;
			this.yentryName.Name = "yentryName";
			this.yentryName.IsEditable = true;
			this.yentryName.MaxLength = 240;
			this.yentryName.InvisibleChar = '●';
			this.table2.Add (this.yentryName);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table2 [this.yentryName]));
			w13.TopAttach = ((uint)(1));
			w13.BottomAttach = ((uint)(2));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(2));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ylabelClothesSex = new global::Gamma.GtkWidgets.yLabel ();
			this.ylabelClothesSex.Name = "ylabelClothesSex";
			this.ylabelClothesSex.Xalign = 1F;
			this.ylabelClothesSex.LabelProp = global::Mono.Unix.Catalog.GetString ("ylabel1");
			this.table2.Add (this.ylabelClothesSex);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table2 [this.ylabelClothesSex]));
			w14.TopAttach = ((uint)(3));
			w14.BottomAttach = ((uint)(4));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ylabelId = new global::Gamma.GtkWidgets.yLabel ();
			this.ylabelId.Name = "ylabelId";
			this.ylabelId.LabelProp = global::Mono.Unix.Catalog.GetString ("ylabel1");
			this.table2.Add (this.ylabelId);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table2 [this.ylabelId]));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			w1.Add (this.table2);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(w1 [this.table2]));
			w16.Position = 0;
			w16.Expand = false;
			w16.Fill = false;
			// Internal child workwear.NomenclatureDlg.ActionArea
			global::Gtk.HButtonBox w17 = this.ActionArea;
			w17.Name = "dialog1_ActionArea";
			w17.Spacing = 10;
			w17.BorderWidth = ((uint)(5));
			w17.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString ("О_тменить");
			global::Gtk.Image w18 = new global::Gtk.Image ();
			w18.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-cancel", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w18;
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w19 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w17 [this.buttonCancel]));
			w19.Expand = false;
			w19.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = global::Mono.Unix.Catalog.GetString ("_OK");
			global::Gtk.Image w20 = new global::Gtk.Image ();
			w20.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-ok", global::Gtk.IconSize.Menu);
			this.buttonOk.Image = w20;
			w17.Add (this.buttonOk);
			global::Gtk.ButtonBox.ButtonBoxChild w21 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w17 [this.buttonOk]));
			w21.Position = 1;
			w21.Expand = false;
			w21.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 425;
			this.DefaultHeight = 269;
			this.ycomboClothesSex.Hide ();
			this.ylabelClothesSex.Hide ();
			this.Show ();
			this.yentryItemsType.Changed += new global::System.EventHandler (this.OnYentryItemsTypeChanged);
			this.ycomboClothesSex.Changed += new global::System.EventHandler (this.OnYcomboClothesSexChanged);
			this.ycomboWearStd.Changed += new global::System.EventHandler (this.OnYcomboWearStdChanged);
			this.buttonOk.Clicked += new global::System.EventHandler (this.OnButtonOkClicked);
		}
	}
}
