
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.ReportParameters.Views
{
	public partial class ClothingServiceReportView
	{
		private global::Gtk.VBox vbox1;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonshowclosed;

		private global::Gtk.HBox hbox1;

		private global::Gamma.GtkWidgets.yLabel ylabelPeriod;

		private global::Gamma.Widgets.yDatePeriodPicker ydateperiodpicker;

		private global::Gamma.GtkWidgets.yButton buttonRun;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.ReportParameters.Views.ClothingServiceReportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.ReportParameters.Views.ClothingServiceReportView";
			// Container child Workwear.ReportParameters.Views.ClothingServiceReportView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ycheckbuttonshowclosed = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonshowclosed.CanFocus = true;
			this.ycheckbuttonshowclosed.Name = "ycheckbuttonshowclosed";
			this.ycheckbuttonshowclosed.Label = global::Mono.Unix.Catalog.GetString("Показывать закрытые");
			this.ycheckbuttonshowclosed.DrawIndicator = true;
			this.ycheckbuttonshowclosed.UseUnderline = true;
			this.vbox1.Add(this.ycheckbuttonshowclosed);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ycheckbuttonshowclosed]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.ylabelPeriod = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelPeriod.Name = "ylabelPeriod";
			this.ylabelPeriod.LabelProp = global::Mono.Unix.Catalog.GetString("Период поступления заявок:");
			this.hbox1.Add(this.ylabelPeriod);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.ylabelPeriod]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.ydateperiodpicker = new global::Gamma.Widgets.yDatePeriodPicker();
			this.ydateperiodpicker.Events = ((global::Gdk.EventMask)(256));
			this.ydateperiodpicker.Name = "ydateperiodpicker";
			this.ydateperiodpicker.StartDate = new global::System.DateTime(0);
			this.ydateperiodpicker.EndDate = new global::System.DateTime(0);
			this.hbox1.Add(this.ydateperiodpicker);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.ydateperiodpicker]));
			w3.Position = 1;
			this.vbox1.Add(this.hbox1);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox1]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.buttonRun = new global::Gamma.GtkWidgets.yButton();
			this.buttonRun.CanFocus = true;
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.UseUnderline = true;
			this.buttonRun.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vbox1.Add(this.buttonRun);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.buttonRun]));
			w5.PackType = ((global::Gtk.PackType)(1));
			w5.Position = 3;
			w5.Expand = false;
			w5.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}