
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Operations
{
	public partial class ManualEmployeeIssueOperationsView
	{
		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeviewOperations;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.VSeparator vseparator1;

		private global::Gamma.GtkWidgets.yHBox yhbox2;

		private global::Gamma.GtkWidgets.yButton ybuttonAdd;

		private global::Gamma.GtkWidgets.yButton ybuttonDelete;

		private global::Gamma.GtkWidgets.yTable ytable2;

		private global::Gamma.Widgets.yDatePicker ydatepicker;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.ySpinButton yspinbuttonAmmount;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Views.Operations.ManualEmployeeIssueOperationsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Views.Operations.ManualEmployeeIssueOperationsView";
			// Container child workwear.Views.Operations.ManualEmployeeIssueOperationsView.Gtk.Container+ContainerChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(3));
			this.ytable1.NColumns = ((uint)(3));
			this.ytable1.RowSpacing = ((uint)(6));
			this.ytable1.ColumnSpacing = ((uint)(6));
			// Container child ytable1.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeviewOperations = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeviewOperations.CanFocus = true;
			this.ytreeviewOperations.Name = "ytreeviewOperations";
			this.GtkScrolledWindow.Add(this.ytreeviewOperations);
			this.ytable1.Add(this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytable1[this.GtkScrolledWindow]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			// Container child ytable1.Gtk.Table+TableChild
			this.hbox4 = new global::Gtk.HBox();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.buttonSave = new global::Gtk.Button();
			this.buttonSave.CanFocus = true;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.UseUnderline = true;
			this.buttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-save", global::Gtk.IconSize.Menu);
			this.buttonSave.Image = w3;
			this.hbox4.Add(this.buttonSave);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.buttonSave]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.buttonCancel = new global::Gtk.Button();
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			global::Gtk.Image w5 = new global::Gtk.Image();
			w5.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-revert-to-saved", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w5;
			this.hbox4.Add(this.buttonCancel);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.buttonCancel]));
			w6.Position = 1;
			w6.Expand = false;
			w6.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.vseparator1 = new global::Gtk.VSeparator();
			this.vseparator1.Name = "vseparator1";
			this.hbox4.Add(this.vseparator1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.vseparator1]));
			w7.Position = 2;
			w7.Expand = false;
			w7.Fill = false;
			this.ytable1.Add(this.hbox4);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytable1[this.hbox4]));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yhbox2 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox2.Name = "yhbox2";
			this.yhbox2.Spacing = 6;
			// Container child yhbox2.Gtk.Box+BoxChild
			this.ybuttonAdd = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonAdd.CanFocus = true;
			this.ybuttonAdd.Name = "ybuttonAdd";
			this.ybuttonAdd.UseUnderline = true;
			this.ybuttonAdd.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			this.yhbox2.Add(this.ybuttonAdd);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.yhbox2[this.ybuttonAdd]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			// Container child yhbox2.Gtk.Box+BoxChild
			this.ybuttonDelete = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonDelete.CanFocus = true;
			this.ybuttonDelete.Name = "ybuttonDelete";
			this.ybuttonDelete.UseUnderline = true;
			this.ybuttonDelete.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			this.yhbox2.Add(this.ybuttonDelete);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.yhbox2[this.ybuttonDelete]));
			w10.Position = 1;
			w10.Expand = false;
			w10.Fill = false;
			this.ytable1.Add(this.yhbox2);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yhbox2]));
			w11.TopAttach = ((uint)(2));
			w11.BottomAttach = ((uint)(3));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ytable2 = new global::Gamma.GtkWidgets.yTable();
			this.ytable2.Name = "ytable2";
			this.ytable2.NRows = ((uint)(2));
			this.ytable2.NColumns = ((uint)(2));
			this.ytable2.RowSpacing = ((uint)(6));
			this.ytable2.ColumnSpacing = ((uint)(6));
			// Container child ytable2.Gtk.Table+TableChild
			this.ydatepicker = new global::Gamma.Widgets.yDatePicker();
			this.ydatepicker.Events = ((global::Gdk.EventMask)(256));
			this.ydatepicker.Name = "ydatepicker";
			this.ydatepicker.WithTime = false;
			this.ydatepicker.Date = new global::System.DateTime(0);
			this.ydatepicker.IsEditable = true;
			this.ydatepicker.AutoSeparation = true;
			this.ytable2.Add(this.ydatepicker);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.ytable2[this.ydatepicker]));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable2.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.Xalign = 1F;
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString(" Дата:");
			this.ytable2.Add(this.ylabel1);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.ytable2[this.ylabel1]));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable2.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Количество:");
			this.ytable2.Add(this.ylabel2);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.ytable2[this.ylabel2]));
			w14.TopAttach = ((uint)(1));
			w14.BottomAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable2.Gtk.Table+TableChild
			this.yspinbuttonAmmount = new global::Gamma.GtkWidgets.ySpinButton(0D, 100D, 1D);
			this.yspinbuttonAmmount.CanFocus = true;
			this.yspinbuttonAmmount.Name = "yspinbuttonAmmount";
			this.yspinbuttonAmmount.Adjustment.PageIncrement = 10D;
			this.yspinbuttonAmmount.ClimbRate = 1D;
			this.yspinbuttonAmmount.Numeric = true;
			this.yspinbuttonAmmount.ValueAsDecimal = 0m;
			this.yspinbuttonAmmount.ValueAsInt = 0;
			this.ytable2.Add(this.yspinbuttonAmmount);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.ytable2[this.yspinbuttonAmmount]));
			w15.TopAttach = ((uint)(1));
			w15.BottomAttach = ((uint)(2));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			this.ytable1.Add(this.ytable2);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ytable2]));
			w16.TopAttach = ((uint)(1));
			w16.BottomAttach = ((uint)(2));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.ytable1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
