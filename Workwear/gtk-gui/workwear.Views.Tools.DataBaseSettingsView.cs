
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Tools
{
	public partial class DataBaseSettingsView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox2;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gtk.Table table1;

		private global::Gamma.GtkWidgets.yCheckButton checkCheckBalances;

		private global::Gamma.GtkWidgets.yCheckButton checkEmployeeSizeRanges;

		private global::Gamma.Widgets.yEnumComboBox ComboExtendPeriod;

		private global::Gamma.Widgets.yEnumComboBox ComboShirtExpluatacion;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gtk.Label label4;

		private global::Gamma.GtkWidgets.yCheckButton ycheckAutoWriteoff;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.ySpinButton spbutAheadOfShedule;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

		private global::Gamma.GtkWidgets.yLabel ylabel4;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Views.Tools.DataBaseSettingsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Views.Tools.DataBaseSettingsView";
			// Container child workwear.Views.Tools.DataBaseSettingsView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.buttonSave = new global::Gtk.Button();
			this.buttonSave.CanFocus = true;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.UseUnderline = true;
			this.buttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-save", global::Gtk.IconSize.Menu);
			this.buttonSave.Image = w1;
			this.hbox2.Add(this.buttonSave);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.buttonSave]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.buttonCancel = new global::Gtk.Button();
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-revert-to-saved", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w3;
			this.hbox2.Add(this.buttonCancel);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.buttonCancel]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.vbox1.Add(this.hbox2);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox2]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			global::Gtk.Viewport w6 = new global::Gtk.Viewport();
			w6.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(7)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.checkCheckBalances = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkCheckBalances.CanFocus = true;
			this.checkCheckBalances.Name = "checkCheckBalances";
			this.checkCheckBalances.Label = global::Mono.Unix.Catalog.GetString("Включено");
			this.checkCheckBalances.DrawIndicator = true;
			this.checkCheckBalances.UseUnderline = true;
			this.table1.Add(this.checkCheckBalances);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.checkCheckBalances]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.checkEmployeeSizeRanges = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkEmployeeSizeRanges.TooltipMarkup = "Политика позволяет указывать в сотруднике не конкретный размер, диапазон размеров" +
				", для выдачи в подборе одежды только указанного дипазона.";
			this.checkEmployeeSizeRanges.CanFocus = true;
			this.checkEmployeeSizeRanges.Name = "checkEmployeeSizeRanges";
			this.checkEmployeeSizeRanges.Label = global::Mono.Unix.Catalog.GetString("Включено");
			this.checkEmployeeSizeRanges.DrawIndicator = true;
			this.checkEmployeeSizeRanges.UseUnderline = true;
			this.table1.Add(this.checkEmployeeSizeRanges);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.checkEmployeeSizeRanges]));
			w8.TopAttach = ((uint)(3));
			w8.BottomAttach = ((uint)(4));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ComboExtendPeriod = new global::Gamma.Widgets.yEnumComboBox();
			this.ComboExtendPeriod.Name = "ComboExtendPeriod";
			this.ComboExtendPeriod.ShowSpecialStateAll = false;
			this.ComboExtendPeriod.ShowSpecialStateNot = false;
			this.ComboExtendPeriod.UseShortTitle = false;
			this.ComboExtendPeriod.DefaultFirst = false;
			this.table1.Add(this.ComboExtendPeriod);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.ComboExtendPeriod]));
			w9.TopAttach = ((uint)(6));
			w9.BottomAttach = ((uint)(7));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ComboShirtExpluatacion = new global::Gamma.Widgets.yEnumComboBox();
			this.ComboShirtExpluatacion.Name = "ComboShirtExpluatacion";
			this.ComboShirtExpluatacion.ShowSpecialStateAll = false;
			this.ComboShirtExpluatacion.ShowSpecialStateNot = false;
			this.ComboShirtExpluatacion.UseShortTitle = false;
			this.ComboShirtExpluatacion.DefaultFirst = false;
			this.table1.Add(this.ComboShirtExpluatacion);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.ComboShirtExpluatacion]));
			w10.TopAttach = ((uint)(5));
			w10.BottomAttach = ((uint)(6));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Авто списание с сотрудника:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Это значение будет применено только к новым выдачам. На старые выдачи это не повл" +
					"ияет. Для старых выдач автосписание можно отключить вручную для каждой позиции.");
			this.label2.Wrap = true;
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w12.TopAttach = ((uint)(1));
			w12.BottomAttach = ((uint)(2));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Диапазоны в размерах сотрудника:");
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w13.TopAttach = ((uint)(3));
			w13.BottomAttach = ((uint)(4));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Разрешить выдачу раньше срока на:");
			this.table1.Add(this.label4);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.label4]));
			w14.TopAttach = ((uint)(4));
			w14.BottomAttach = ((uint)(5));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckAutoWriteoff = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckAutoWriteoff.CanFocus = true;
			this.ycheckAutoWriteoff.Name = "ycheckAutoWriteoff";
			this.ycheckAutoWriteoff.Label = global::Mono.Unix.Catalog.GetString("Включено");
			this.ycheckAutoWriteoff.DrawIndicator = true;
			this.ycheckAutoWriteoff.UseUnderline = true;
			this.table1.Add(this.ycheckAutoWriteoff);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckAutoWriteoff]));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.spbutAheadOfShedule = new global::Gamma.GtkWidgets.ySpinButton(0D, 356D, 1D);
			this.spbutAheadOfShedule.CanFocus = true;
			this.spbutAheadOfShedule.Name = "spbutAheadOfShedule";
			this.spbutAheadOfShedule.Adjustment.PageIncrement = 10D;
			this.spbutAheadOfShedule.ClimbRate = 1D;
			this.spbutAheadOfShedule.Numeric = true;
			this.spbutAheadOfShedule.ValueAsDecimal = 0m;
			this.spbutAheadOfShedule.ValueAsInt = 0;
			this.yhbox1.Add(this.spbutAheadOfShedule);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.spbutAheadOfShedule]));
			w16.Position = 0;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("дней");
			this.yhbox1.Add(this.ylabel2);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ylabel2]));
			w17.Position = 1;
			w17.Expand = false;
			w17.Fill = false;
			this.table1.Add(this.yhbox1);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table1[this.yhbox1]));
			w18.TopAttach = ((uint)(4));
			w18.BottomAttach = ((uint)(5));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(2));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.Xalign = 1F;
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Проверять остатки при расходе со склада:");
			this.table1.Add(this.ylabel1);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel1]));
			w19.TopAttach = ((uint)(2));
			w19.BottomAttach = ((uint)(3));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 1F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Переносить дату начала эксплуатации:");
			this.table1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel3]));
			w20.TopAttach = ((uint)(5));
			w20.BottomAttach = ((uint)(6));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel4 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel4.Name = "ylabel4";
			this.ylabel4.Xalign = 1F;
			this.ylabel4.LabelProp = global::Mono.Unix.Catalog.GetString("Увеличить период эксплуатации пропорционально кол-ву:");
			this.table1.Add(this.ylabel4);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel4]));
			w21.TopAttach = ((uint)(6));
			w21.BottomAttach = ((uint)(7));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			w6.Add(this.table1);
			this.GtkScrolledWindow.Add(w6);
			this.vbox1.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow]));
			w24.Position = 1;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
