
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Journal.Filter.Views.Company
{
	public partial class EmployeeBalanceFilterView
	{
		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::QS.Widgets.GtkUI.DatePicker datepicker;

		private global::QS.Views.Control.EntityEntry yentryEmployee;

		private global::QS.Views.Control.EntityEntry yentrySubdivision;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Journal.Filter.Views.Company.EmployeeBalanceFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Journal.Filter.Views.Company.EmployeeBalanceFilterView";
			// Container child workwear.Journal.Filter.Views.Company.EmployeeBalanceFilterView.Gtk.Container+ContainerChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(4));
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
			this.yentryEmployee = new global::QS.Views.Control.EntityEntry();
			this.yentryEmployee.Events = ((global::Gdk.EventMask)(256));
			this.yentryEmployee.Name = "yentryEmployee";
			this.ytable1.Add(this.yentryEmployee);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yentryEmployee]));
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
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.Xalign = 1F;
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Дата");
			this.ytable1.Add(this.ylabel1);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel1]));
			w4.TopAttach = ((uint)(2));
			w4.BottomAttach = ((uint)(3));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Сотрудник");
			this.ytable1.Add(this.ylabel2);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel2]));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 1F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение");
			this.ytable1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel3]));
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
