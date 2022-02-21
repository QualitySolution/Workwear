
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.ReportParameters.Views
{
	public partial class RequestSheetView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.Table table1;

		private global::Gamma.Widgets.ySpecComboBox comboQuarter;

		private global::QS.Views.Control.EntityEntry entitySubdivision;

		private global::Gtk.HBox hbox1;

		private global::Gtk.RadioButton radioMonth;

		private global::Gtk.RadioButton radioQuarter;

		private global::Gtk.RadioButton radioYear;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		private global::Gamma.GtkWidgets.yLabel labelPeriodType;

		private global::Gamma.Widgets.yEnumComboBox yenumIssueType;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yButton ycheckbuttonAllNomenclature;

		private global::Gtk.Button buttonRun;

		private global::Gtk.ScrolledWindow scrolledwindow1;

		private global::Gamma.GtkWidgets.yTreeView ytreeNomenclature;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.ReportParameters.Views.RequestSheetView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.ReportParameters.Views.RequestSheetView";
			// Container child workwear.ReportParameters.Views.RequestSheetView.Gtk.Container+ContainerChild
			this.dialog1_VBox = new global::Gtk.VBox();
			this.dialog1_VBox.Name = "dialog1_VBox";
			this.dialog1_VBox.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(4)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.comboQuarter = new global::Gamma.Widgets.ySpecComboBox();
			this.comboQuarter.Name = "comboQuarter";
			this.comboQuarter.AddIfNotExist = false;
			this.comboQuarter.DefaultFirst = false;
			this.comboQuarter.ShowSpecialStateAll = false;
			this.comboQuarter.ShowSpecialStateNot = false;
			this.table1.Add(this.comboQuarter);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.comboQuarter]));
			w1.TopAttach = ((uint)(2));
			w1.BottomAttach = ((uint)(3));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entitySubdivision = new global::QS.Views.Control.EntityEntry();
			this.entitySubdivision.Events = ((global::Gdk.EventMask)(256));
			this.entitySubdivision.Name = "entitySubdivision";
			this.table1.Add(this.entitySubdivision);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.entitySubdivision]));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.radioMonth = new global::Gtk.RadioButton(global::Mono.Unix.Catalog.GetString("Месяц"));
			this.radioMonth.CanFocus = true;
			this.radioMonth.Name = "radioMonth";
			this.radioMonth.DrawIndicator = true;
			this.radioMonth.UseUnderline = true;
			this.radioMonth.Group = new global::GLib.SList(global::System.IntPtr.Zero);
			this.hbox1.Add(this.radioMonth);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.radioMonth]));
			w3.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.radioQuarter = new global::Gtk.RadioButton(global::Mono.Unix.Catalog.GetString("Квартал"));
			this.radioQuarter.CanFocus = true;
			this.radioQuarter.Name = "radioQuarter";
			this.radioQuarter.DrawIndicator = true;
			this.radioQuarter.UseUnderline = true;
			this.radioQuarter.Group = this.radioMonth.Group;
			this.hbox1.Add(this.radioQuarter);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.radioQuarter]));
			w4.Position = 1;
			// Container child hbox1.Gtk.Box+BoxChild
			this.radioYear = new global::Gtk.RadioButton(global::Mono.Unix.Catalog.GetString("Год"));
			this.radioYear.CanFocus = true;
			this.radioYear.Name = "radioYear";
			this.radioYear.DrawIndicator = true;
			this.radioYear.UseUnderline = true;
			this.radioYear.Group = this.radioMonth.Group;
			this.hbox1.Add(this.radioYear);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.radioYear]));
			w5.Position = 2;
			this.table1.Add(this.hbox1);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox1]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Период:");
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w8.TopAttach = ((uint)(1));
			w8.BottomAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelPeriodType = new global::Gamma.GtkWidgets.yLabel();
			this.labelPeriodType.Name = "labelPeriodType";
			this.labelPeriodType.Xalign = 1F;
			this.labelPeriodType.LabelProp = global::Mono.Unix.Catalog.GetString("Квартал:");
			this.table1.Add(this.labelPeriodType);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.labelPeriodType]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yenumIssueType = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumIssueType.Name = "yenumIssueType";
			this.yenumIssueType.ShowSpecialStateAll = true;
			this.yenumIssueType.ShowSpecialStateNot = false;
			this.yenumIssueType.UseShortTitle = false;
			this.yenumIssueType.DefaultFirst = true;
			this.table1.Add(this.yenumIssueType);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.yenumIssueType]));
			w10.TopAttach = ((uint)(3));
			w10.BottomAttach = ((uint)(4));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.Xalign = 1F;
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Тип выдачи:");
			this.table1.Add(this.ylabel1);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel1]));
			w11.TopAttach = ((uint)(3));
			w11.BottomAttach = ((uint)(4));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			this.dialog1_VBox.Add(this.table1);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.table1]));
			w12.Position = 0;
			w12.Expand = false;
			w12.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Номенклатура нормы");
			this.dialog1_VBox.Add(this.ylabel2);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.ylabel2]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.ycheckbuttonAllNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.ycheckbuttonAllNomenclature.CanFocus = true;
			this.ycheckbuttonAllNomenclature.Name = "ycheckbuttonAllNomenclature";
			this.ycheckbuttonAllNomenclature.UseUnderline = true;
			this.ycheckbuttonAllNomenclature.Label = global::Mono.Unix.Catalog.GetString("Выделить/снять выделение");
			this.dialog1_VBox.Add(this.ycheckbuttonAllNomenclature);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.ycheckbuttonAllNomenclature]));
			w14.Position = 2;
			w14.Expand = false;
			w14.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.buttonRun = new global::Gtk.Button();
			this.buttonRun.CanFocus = true;
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.UseUnderline = true;
			this.buttonRun.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.dialog1_VBox.Add(this.buttonRun);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.buttonRun]));
			w15.PackType = ((global::Gtk.PackType)(1));
			w15.Position = 3;
			w15.Expand = false;
			w15.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow();
			this.scrolledwindow1.CanFocus = true;
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow1.Gtk.Container+ContainerChild
			this.ytreeNomenclature = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeNomenclature.CanFocus = true;
			this.ytreeNomenclature.Name = "ytreeNomenclature";
			this.ytreeNomenclature.HeadersVisible = false;
			this.scrolledwindow1.Add(this.ytreeNomenclature);
			this.dialog1_VBox.Add(this.scrolledwindow1);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.scrolledwindow1]));
			w17.PackType = ((global::Gtk.PackType)(1));
			w17.Position = 4;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Show();
			this.radioMonth.Toggled += new global::System.EventHandler(this.OnRadioMonthToggled);
			this.radioQuarter.Toggled += new global::System.EventHandler(this.OnRadioQuarterToggled);
			this.radioYear.Toggled += new global::System.EventHandler(this.OnRadioYearToggled);
			this.ycheckbuttonAllNomenclature.Clicked += new global::System.EventHandler(this.SelectAll);
			this.buttonRun.Clicked += new global::System.EventHandler(this.OnButtonRunClicked);
		}
	}
}
