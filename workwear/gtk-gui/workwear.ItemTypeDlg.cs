
// This file has been generated by the GUI designer. Do not modify.
namespace workwear
{
	public partial class ItemTypeDlg
	{
		private global::Gtk.Table table1;
		
		private global::Gtk.HBox hboxLife;
		
		private global::Gamma.GtkWidgets.yCheckButton ycheckLife;
		
		private global::Gamma.GtkWidgets.ySpinButton yspinMonths;
		
		private global::Gamma.GtkWidgets.yLabel ylabel1;
		
		private global::Gtk.Label label;
		
		private global::Gtk.Label label1;
		
		private global::Gtk.Label label10;
		
		private global::Gtk.Label label2;
		
		private global::Gtk.Label label3;
		
		private global::Gtk.Label labelLife;
		
		private global::Gamma.Widgets.yEnumComboBox ycomboCategory;
		
		private global::Gamma.Widgets.ySpecComboBox ycomboUnits;
		
		private global::Gamma.Widgets.yEnumComboBox ycomboWearCategory;
		
		private global::Gamma.GtkWidgets.yEntry yentryName;
		
		private global::Gamma.GtkWidgets.yLabel ylabelId;
		
		private global::Gtk.Button buttonCancel;
		
		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget workwear.ItemTypeDlg
			this.Name = "workwear.ItemTypeDlg";
			this.Title = global::Mono.Unix.Catalog.GetString ("Новый тип номенклатуры");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child workwear.ItemTypeDlg.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table (((uint)(6)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			this.table1.BorderWidth = ((uint)(9));
			// Container child table1.Gtk.Table+TableChild
			this.hboxLife = new global::Gtk.HBox ();
			this.hboxLife.Name = "hboxLife";
			this.hboxLife.Spacing = 6;
			// Container child hboxLife.Gtk.Box+BoxChild
			this.ycheckLife = new global::Gamma.GtkWidgets.yCheckButton ();
			this.ycheckLife.CanFocus = true;
			this.ycheckLife.Name = "ycheckLife";
			this.ycheckLife.Label = "";
			this.ycheckLife.DrawIndicator = true;
			this.ycheckLife.UseUnderline = true;
			this.hboxLife.Add (this.ycheckLife);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hboxLife [this.ycheckLife]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hboxLife.Gtk.Box+BoxChild
			this.yspinMonths = new global::Gamma.GtkWidgets.ySpinButton (0, 100, 1);
			this.yspinMonths.Sensitive = false;
			this.yspinMonths.CanFocus = true;
			this.yspinMonths.Name = "yspinMonths";
			this.yspinMonths.Adjustment.PageIncrement = 10;
			this.yspinMonths.ClimbRate = 1;
			this.yspinMonths.Numeric = true;
			this.yspinMonths.ValueAsDecimal = 0m;
			this.yspinMonths.ValueAsInt = 0;
			this.hboxLife.Add (this.yspinMonths);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hboxLife [this.yspinMonths]));
			w3.Position = 1;
			// Container child hboxLife.Gtk.Box+BoxChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel ();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString ("месяцев");
			this.hboxLife.Add (this.ylabel1);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hboxLife [this.ylabel1]));
			w4.Position = 2;
			w4.Expand = false;
			w4.Fill = false;
			this.table1.Add (this.hboxLife);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1 [this.hboxLife]));
			w5.TopAttach = ((uint)(5));
			w5.BottomAttach = ((uint)(6));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label = new global::Gtk.Label ();
			this.label.Name = "label";
			this.label.Xalign = 1F;
			this.label.LabelProp = global::Mono.Unix.Catalog.GetString ("Код:");
			this.table1.Add (this.label);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1 [this.label]));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Категория<span foreground=\"red\">*</span>:");
			this.label1.UseMarkup = true;
			this.table1.Add (this.label1);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1 [this.label1]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label10 = new global::Gtk.Label ();
			this.label10.Name = "label10";
			this.label10.Xalign = 1F;
			this.label10.LabelProp = global::Mono.Unix.Catalog.GetString ("Единицы измерения:");
			this.table1.Add (this.label10);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1 [this.label10]));
			w8.TopAttach = ((uint)(4));
			w8.BottomAttach = ((uint)(5));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("Название группы<span foreground=\"red\">*</span>:");
			this.label2.UseMarkup = true;
			this.table1.Add (this.label2);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1 [this.label2]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label ();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString ("Вид спецодежды:");
			this.table1.Add (this.label3);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1 [this.label3]));
			w10.TopAttach = ((uint)(3));
			w10.BottomAttach = ((uint)(4));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelLife = new global::Gtk.Label ();
			this.labelLife.Name = "labelLife";
			this.labelLife.Xalign = 1F;
			this.labelLife.LabelProp = global::Mono.Unix.Catalog.GetString ("Срок службы:");
			this.table1.Add (this.labelLife);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelLife]));
			w11.TopAttach = ((uint)(5));
			w11.BottomAttach = ((uint)(6));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycomboCategory = new global::Gamma.Widgets.yEnumComboBox ();
			this.ycomboCategory.Name = "ycomboCategory";
			this.ycomboCategory.ShowSpecialStateAll = false;
			this.ycomboCategory.ShowSpecialStateNot = false;
			this.ycomboCategory.UseShortTitle = false;
			this.ycomboCategory.DefaultFirst = false;
			this.table1.Add (this.ycomboCategory);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1 [this.ycomboCategory]));
			w12.TopAttach = ((uint)(2));
			w12.BottomAttach = ((uint)(3));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycomboUnits = new global::Gamma.Widgets.ySpecComboBox ();
			this.ycomboUnits.Name = "ycomboUnits";
			this.ycomboUnits.AddIfNotExist = false;
			this.ycomboUnits.ShowSpecialStateAll = false;
			this.ycomboUnits.ShowSpecialStateNot = false;
			this.table1.Add (this.ycomboUnits);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1 [this.ycomboUnits]));
			w13.TopAttach = ((uint)(4));
			w13.BottomAttach = ((uint)(5));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycomboWearCategory = new global::Gamma.Widgets.yEnumComboBox ();
			this.ycomboWearCategory.Name = "ycomboWearCategory";
			this.ycomboWearCategory.ShowSpecialStateAll = false;
			this.ycomboWearCategory.ShowSpecialStateNot = true;
			this.ycomboWearCategory.UseShortTitle = false;
			this.ycomboWearCategory.DefaultFirst = false;
			this.table1.Add (this.ycomboWearCategory);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1 [this.ycomboWearCategory]));
			w14.TopAttach = ((uint)(3));
			w14.BottomAttach = ((uint)(4));
			w14.LeftAttach = ((uint)(1));
			w14.RightAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryName = new global::Gamma.GtkWidgets.yEntry ();
			this.yentryName.CanFocus = true;
			this.yentryName.Name = "yentryName";
			this.yentryName.IsEditable = true;
			this.yentryName.MaxLength = 180;
			this.yentryName.InvisibleChar = '●';
			this.table1.Add (this.yentryName);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1 [this.yentryName]));
			w15.TopAttach = ((uint)(1));
			w15.BottomAttach = ((uint)(2));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelId = new global::Gamma.GtkWidgets.yLabel ();
			this.ylabelId.Name = "ylabelId";
			this.ylabelId.LabelProp = global::Mono.Unix.Catalog.GetString ("ylabel1");
			this.table1.Add (this.ylabelId);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1 [this.ylabelId]));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			w1.Add (this.table1);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(w1 [this.table1]));
			w17.Position = 0;
			w17.Expand = false;
			w17.Fill = false;
			// Internal child workwear.ItemTypeDlg.ActionArea
			global::Gtk.HButtonBox w18 = this.ActionArea;
			w18.Name = "dialog1_ActionArea";
			w18.Spacing = 10;
			w18.BorderWidth = ((uint)(5));
			w18.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString ("О_тменить");
			global::Gtk.Image w19 = new global::Gtk.Image ();
			w19.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-cancel", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w19;
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w20 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w18 [this.buttonCancel]));
			w20.Expand = false;
			w20.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = global::Mono.Unix.Catalog.GetString ("_OK");
			global::Gtk.Image w21 = new global::Gtk.Image ();
			w21.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-ok", global::Gtk.IconSize.Menu);
			this.buttonOk.Image = w21;
			w18.Add (this.buttonOk);
			global::Gtk.ButtonBox.ButtonBoxChild w22 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w18 [this.buttonOk]));
			w22.Position = 1;
			w22.Expand = false;
			w22.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 454;
			this.DefaultHeight = 287;
			this.Show ();
			this.ycomboCategory.Changed += new global::System.EventHandler (this.OnYcomboCategoryChanged);
			this.ycheckLife.Toggled += new global::System.EventHandler (this.OnYcheckLifeToggled);
			this.buttonOk.Clicked += new global::System.EventHandler (this.OnButtonOkClicked);
		}
	}
}
