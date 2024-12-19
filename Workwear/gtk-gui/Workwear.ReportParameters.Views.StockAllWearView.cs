
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.ReportParameters.Views
{
	public partial class StockAllWearView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.Table table1;

		private global::Gamma.Widgets.yEnumComboBox comboReportType;

		private global::Gamma.Widgets.yDatePicker ydateReport;

		private global::Gamma.Widgets.yEnumComboBox yenumcomboboxWarehouse;

		private global::Gamma.GtkWidgets.yLabel ylabel_date;

		private global::Gamma.GtkWidgets.yLabel ylabel_warehouse;

		private global::Gamma.GtkWidgets.yLabel ylabel6;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonShowSumm;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonShowSex;

		private global::Gamma.GtkWidgets.yButton buttonRun;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.ReportParameters.Views.StockAllWearView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.ReportParameters.Views.StockAllWearView";
			// Container child Workwear.ReportParameters.Views.StockAllWearView.Gtk.Container+ContainerChild
			this.dialog1_VBox = new global::Gtk.VBox();
			this.dialog1_VBox.Name = "dialog1_VBox";
			this.dialog1_VBox.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(4)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.comboReportType = new global::Gamma.Widgets.yEnumComboBox();
			this.comboReportType.Name = "comboReportType";
			this.comboReportType.ShowSpecialStateAll = false;
			this.comboReportType.ShowSpecialStateNot = false;
			this.comboReportType.UseShortTitle = false;
			this.comboReportType.DefaultFirst = true;
			this.table1.Add(this.comboReportType);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.comboReportType]));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ydateReport = new global::Gamma.Widgets.yDatePicker();
			this.ydateReport.Events = ((global::Gdk.EventMask)(256));
			this.ydateReport.Name = "ydateReport";
			this.ydateReport.WithTime = false;
			this.ydateReport.Date = new global::System.DateTime(0);
			this.ydateReport.IsEditable = true;
			this.ydateReport.AutoSeparation = true;
			this.table1.Add(this.ydateReport);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.ydateReport]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yenumcomboboxWarehouse = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumcomboboxWarehouse.Name = "yenumcomboboxWarehouse";
			this.yenumcomboboxWarehouse.ShowSpecialStateAll = false;
			this.yenumcomboboxWarehouse.ShowSpecialStateNot = false;
			this.yenumcomboboxWarehouse.UseShortTitle = false;
			this.yenumcomboboxWarehouse.DefaultFirst = true;
			this.table1.Add(this.yenumcomboboxWarehouse);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.yenumcomboboxWarehouse]));
			w3.TopAttach = ((uint)(2));
			w3.BottomAttach = ((uint)(3));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel_date = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel_date.Name = "ylabel_date";
			this.ylabel_date.Xalign = 1F;
			this.ylabel_date.LabelProp = global::Mono.Unix.Catalog.GetString("Дата отчета:");
			this.table1.Add(this.ylabel_date);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel_date]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel_warehouse = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel_warehouse.Name = "ylabel_warehouse";
			this.ylabel_warehouse.Xalign = 1F;
			this.ylabel_warehouse.LabelProp = global::Mono.Unix.Catalog.GetString("Склад:");
			this.table1.Add(this.ylabel_warehouse);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel_warehouse]));
			w5.TopAttach = ((uint)(2));
			w5.BottomAttach = ((uint)(3));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel6 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel6.Name = "ylabel6";
			this.ylabel6.Xalign = 1F;
			this.ylabel6.LabelProp = global::Mono.Unix.Catalog.GetString("Вид отчета:");
			this.table1.Add(this.ylabel6);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel6]));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			this.dialog1_VBox.Add(this.table1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.table1]));
			w7.Position = 0;
			w7.Expand = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.ycheckbuttonShowSumm = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonShowSumm.CanFocus = true;
			this.ycheckbuttonShowSumm.Name = "ycheckbuttonShowSumm";
			this.ycheckbuttonShowSumm.Label = global::Mono.Unix.Catalog.GetString("Показать стоимость");
			this.ycheckbuttonShowSumm.DrawIndicator = true;
			this.ycheckbuttonShowSumm.UseUnderline = true;
			this.dialog1_VBox.Add(this.ycheckbuttonShowSumm);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.ycheckbuttonShowSumm]));
			w8.Position = 1;
			w8.Expand = false;
			w8.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.ycheckbuttonShowSex = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonShowSex.CanFocus = true;
			this.ycheckbuttonShowSex.Name = "ycheckbuttonShowSex";
			this.ycheckbuttonShowSex.Label = global::Mono.Unix.Catalog.GetString("Показать пол");
			this.ycheckbuttonShowSex.DrawIndicator = true;
			this.ycheckbuttonShowSex.UseUnderline = true;
			this.dialog1_VBox.Add(this.ycheckbuttonShowSex);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.ycheckbuttonShowSex]));
			w9.Position = 2;
			w9.Expand = false;
			w9.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.buttonRun = new global::Gamma.GtkWidgets.yButton();
			this.buttonRun.CanFocus = true;
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.UseUnderline = true;
			this.buttonRun.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.dialog1_VBox.Add(this.buttonRun);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.buttonRun]));
			w10.PackType = ((global::Gtk.PackType)(1));
			w10.Position = 3;
			w10.Expand = false;
			w10.Fill = false;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonRun.Clicked += new global::System.EventHandler(this.OnButtonRunClicked);
		}
	}
}
