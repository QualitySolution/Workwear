
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.ReportParameters.Views
{
	public partial class AmountIssuedWearView
	{
		private global::Gtk.VBox vbox2;

		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::Gamma.Widgets.yEnumComboBox comboIssueType;

		private global::Gtk.Label label1;

		private global::Gamma.Widgets.yDatePeriodPicker ydateperiodpicker;

		private global::Gamma.GtkWidgets.yEntry yentryMatch;

		private global::Gamma.GtkWidgets.yEntry yentryNoMatch;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

		private global::Gamma.GtkWidgets.yCheckButton ycheckSummry;

		private global::Gamma.GtkWidgets.yCheckButton checkBySize;

		private global::Gamma.GtkWidgets.yCheckButton ycheckAll;

		private global::Gamma.GtkWidgets.yCheckButton ycheckChild;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeSubdivisions;

		private global::Gamma.GtkWidgets.yButton buttonPrintReport;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.ReportParameters.Views.AmountIssuedWearView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.ReportParameters.Views.AmountIssuedWearView";
			// Container child workwear.ReportParameters.Views.AmountIssuedWearView.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(4));
			this.ytable1.NColumns = ((uint)(2));
			this.ytable1.RowSpacing = ((uint)(6));
			this.ytable1.ColumnSpacing = ((uint)(6));
			// Container child ytable1.Gtk.Table+TableChild
			this.comboIssueType = new global::Gamma.Widgets.yEnumComboBox();
			this.comboIssueType.Name = "comboIssueType";
			this.comboIssueType.ShowSpecialStateAll = true;
			this.comboIssueType.ShowSpecialStateNot = false;
			this.comboIssueType.UseShortTitle = false;
			this.comboIssueType.DefaultFirst = false;
			this.ytable1.Add(this.comboIssueType);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.ytable1[this.comboIssueType]));
			w1.TopAttach = ((uint)(1));
			w1.BottomAttach = ((uint)(2));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Период:");
			this.ytable1.Add(this.label1);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytable1[this.label1]));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ydateperiodpicker = new global::Gamma.Widgets.yDatePeriodPicker();
			this.ydateperiodpicker.Events = ((global::Gdk.EventMask)(256));
			this.ydateperiodpicker.Name = "ydateperiodpicker";
			this.ydateperiodpicker.StartDate = new global::System.DateTime(0);
			this.ydateperiodpicker.EndDate = new global::System.DateTime(0);
			this.ytable1.Add(this.ydateperiodpicker);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ydateperiodpicker]));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yentryMatch = new global::Gamma.GtkWidgets.yEntry();
			this.yentryMatch.CanFocus = true;
			this.yentryMatch.Name = "yentryMatch";
			this.yentryMatch.IsEditable = true;
			this.yentryMatch.InvisibleChar = '•';
			this.ytable1.Add(this.yentryMatch);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yentryMatch]));
			w4.TopAttach = ((uint)(2));
			w4.BottomAttach = ((uint)(3));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yentryNoMatch = new global::Gamma.GtkWidgets.yEntry();
			this.yentryNoMatch.CanFocus = true;
			this.yentryNoMatch.Name = "yentryNoMatch";
			this.yentryNoMatch.IsEditable = true;
			this.yentryNoMatch.InvisibleChar = '•';
			this.ytable1.Add(this.yentryNoMatch);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yentryNoMatch]));
			w5.TopAttach = ((uint)(3));
			w5.BottomAttach = ((uint)(4));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.Xalign = 1F;
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Тип выдачи:");
			this.ytable1.Add(this.ylabel1);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel1]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Наименование содержит:");
			this.ytable1.Add(this.ylabel2);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel2]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 1F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Наименование не содержит:");
			this.ytable1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel3]));
			w8.TopAttach = ((uint)(3));
			w8.BottomAttach = ((uint)(4));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add(this.ytable1);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.ytable1]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.ycheckSummry = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckSummry.CanFocus = true;
			this.ycheckSummry.Name = "ycheckSummry";
			this.ycheckSummry.Label = global::Mono.Unix.Catalog.GetString("Суммарно по организации");
			this.ycheckSummry.DrawIndicator = true;
			this.ycheckSummry.UseUnderline = true;
			this.vbox2.Add(this.ycheckSummry);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.ycheckSummry]));
			w10.Position = 1;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.checkBySize = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkBySize.CanFocus = true;
			this.checkBySize.Name = "checkBySize";
			this.checkBySize.Label = global::Mono.Unix.Catalog.GetString("Детализировать по размерам");
			this.checkBySize.DrawIndicator = true;
			this.checkBySize.UseUnderline = true;
			this.vbox2.Add(this.checkBySize);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.checkBySize]));
			w11.Position = 2;
			w11.Expand = false;
			w11.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.ycheckAll = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckAll.CanFocus = true;
			this.ycheckAll.Name = "ycheckAll";
			this.ycheckAll.Label = global::Mono.Unix.Catalog.GetString("Все подразделения");
			this.ycheckAll.DrawIndicator = true;
			this.ycheckAll.UseUnderline = true;
			this.vbox2.Add(this.ycheckAll);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.ycheckAll]));
			w12.Position = 3;
			w12.Expand = false;
			w12.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.ycheckChild = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckChild.CanFocus = true;
			this.ycheckChild.Name = "ycheckChild";
			this.ycheckChild.Label = global::Mono.Unix.Catalog.GetString("Учитывать дочерние");
			this.ycheckChild.DrawIndicator = true;
			this.ycheckChild.UseUnderline = true;
			this.vbox2.Add(this.ycheckChild);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.ycheckChild]));
			w13.Position = 4;
			w13.Expand = false;
			w13.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeSubdivisions = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeSubdivisions.CanFocus = true;
			this.ytreeSubdivisions.Name = "ytreeSubdivisions";
			this.GtkScrolledWindow.Add(this.ytreeSubdivisions);
			this.vbox2.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.GtkScrolledWindow]));
			w15.Position = 5;
			// Container child vbox2.Gtk.Box+BoxChild
			this.buttonPrintReport = new global::Gamma.GtkWidgets.yButton();
			this.buttonPrintReport.Sensitive = false;
			this.buttonPrintReport.CanFocus = true;
			this.buttonPrintReport.Name = "buttonPrintReport";
			this.buttonPrintReport.UseUnderline = true;
			this.buttonPrintReport.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vbox2.Add(this.buttonPrintReport);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.buttonPrintReport]));
			w16.Position = 6;
			w16.Expand = false;
			w16.Fill = false;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonPrintReport.Clicked += new global::System.EventHandler(this.OnButtonPrintReportClicked);
		}
	}
}
