
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Journal.Filter.Views.Statements
{
	public partial class IssuanceSheetFilterView
	{
		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::QS.Widgets.GtkUI.DateRangePicker datePeriodDocs;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Journal.Filter.Views.Statements.IssuanceSheetFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Journal.Filter.Views.Statements.IssuanceSheetFilterView";
			// Container child workwear.Journal.Filter.Views.Statements.IssuanceSheetFilterView.Gtk.Container+ContainerChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(3));
			this.ytable1.NColumns = ((uint)(2));
			this.ytable1.RowSpacing = ((uint)(6));
			this.ytable1.ColumnSpacing = ((uint)(6));
			// Container child ytable1.Gtk.Table+TableChild
			this.datePeriodDocs = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.datePeriodDocs.Events = ((global::Gdk.EventMask)(256));
			this.datePeriodDocs.Name = "datePeriodDocs";
			this.datePeriodDocs.StartDate = new global::System.DateTime(0);
			this.datePeriodDocs.EndDate = new global::System.DateTime(0);
			this.ytable1.Add(this.datePeriodDocs);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.ytable1[this.datePeriodDocs]));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("За период:");
			this.ytable1.Add(this.ylabel1);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel1]));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.ytable1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
