
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Journal.Filter.Views.Regulations
{
	public partial class DutyNormBalanceFilterView
	{
		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::QS.Widgets.GtkUI.DatePicker datepicker;

		private global::QS.Views.Control.EntityEntry yentryDutyNorm;

		private global::QS.Views.Control.EntityEntry yentrySubdivision;

		private global::Gamma.GtkWidgets.yLabel ylabelDate;

		private global::Gamma.GtkWidgets.yLabel ylabelDutyNormName;

		private global::Gamma.GtkWidgets.yLabel ylabelSubdivision;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Journal.Filter.Views.Regulations.DutyNormBalanceFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Journal.Filter.Views.Regulations.DutyNormBalanceFilterView";
			// Container child Workwear.Journal.Filter.Views.Regulations.DutyNormBalanceFilterView.Gtk.Container+ContainerChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(3));
			this.ytable1.NColumns = ((uint)(3));
			this.ytable1.RowSpacing = ((uint)(6));
			this.ytable1.ColumnSpacing = ((uint)(6));
			// Container child ytable1.Gtk.Table+TableChild
			this.datepicker = new global::QS.Widgets.GtkUI.DatePicker();
			this.datepicker.Events = ((global::Gdk.EventMask)(256));
			this.datepicker.Name = "datepicker";
			this.datepicker.WithTime = false;
			this.datepicker.HideCalendarButton = false;
			this.datepicker.Date = new global::System.DateTime(0);
			this.datepicker.IsEditable = true;
			this.datepicker.AutoSeparation = false;
			this.datepicker.HideButtonClearDate = false;
			this.ytable1.Add(this.datepicker);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.ytable1[this.datepicker]));
			w1.TopAttach = ((uint)(2));
			w1.BottomAttach = ((uint)(3));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yentryDutyNorm = new global::QS.Views.Control.EntityEntry();
			this.yentryDutyNorm.Events = ((global::Gdk.EventMask)(256));
			this.yentryDutyNorm.Name = "yentryDutyNorm";
			this.ytable1.Add(this.yentryDutyNorm);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yentryDutyNorm]));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yentrySubdivision = new global::QS.Views.Control.EntityEntry();
			this.yentrySubdivision.Events = ((global::Gdk.EventMask)(256));
			this.yentrySubdivision.Name = "yentrySubdivision";
			this.ytable1.Add(this.yentrySubdivision);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yentrySubdivision]));
			w3.TopAttach = ((uint)(1));
			w3.BottomAttach = ((uint)(2));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabelDate = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelDate.Name = "ylabelDate";
			this.ylabelDate.Xalign = 1F;
			this.ylabelDate.LabelProp = global::Mono.Unix.Catalog.GetString("Дата");
			this.ytable1.Add(this.ylabelDate);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabelDate]));
			w4.TopAttach = ((uint)(2));
			w4.BottomAttach = ((uint)(3));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabelDutyNormName = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelDutyNormName.Name = "ylabelDutyNormName";
			this.ylabelDutyNormName.Xalign = 1F;
			this.ylabelDutyNormName.LabelProp = global::Mono.Unix.Catalog.GetString("Дежурная норма");
			this.ytable1.Add(this.ylabelDutyNormName);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabelDutyNormName]));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabelSubdivision = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelSubdivision.Name = "ylabelSubdivision";
			this.ylabelSubdivision.Xalign = 1F;
			this.ylabelSubdivision.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение");
			this.ytable1.Add(this.ylabelSubdivision);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabelSubdivision]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.ytable1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
