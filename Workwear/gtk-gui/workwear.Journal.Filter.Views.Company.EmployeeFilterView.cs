
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Journal.Filter.Views.Company
{
	public partial class EmployeeFilterView
	{
		private global::Gtk.Table table1;

		private global::Gamma.GtkWidgets.yCheckButton checkShowOnlyWithoutNorms;

		private global::Gamma.GtkWidgets.yCheckButton checkShowOnlyWork;

		private global::QS.Views.Control.EntityEntry entityDepartment;

		private global::QS.Views.Control.EntityEntry entitySubdivision;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Journal.Filter.Views.Company.EmployeeFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Journal.Filter.Views.Company.EmployeeFilterView";
			// Container child workwear.Journal.Filter.Views.Company.EmployeeFilterView.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(2)), ((uint)(3)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.checkShowOnlyWithoutNorms = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkShowOnlyWithoutNorms.CanFocus = true;
			this.checkShowOnlyWithoutNorms.Name = "checkShowOnlyWithoutNorms";
			this.checkShowOnlyWithoutNorms.Label = global::Mono.Unix.Catalog.GetString("Только сотрудники без норм");
			this.checkShowOnlyWithoutNorms.DrawIndicator = true;
			this.checkShowOnlyWithoutNorms.UseUnderline = true;
			this.table1.Add(this.checkShowOnlyWithoutNorms);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.checkShowOnlyWithoutNorms]));
			w1.TopAttach = ((uint)(1));
			w1.BottomAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.checkShowOnlyWork = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkShowOnlyWork.CanFocus = true;
			this.checkShowOnlyWork.Name = "checkShowOnlyWork";
			this.checkShowOnlyWork.Label = global::Mono.Unix.Catalog.GetString("Только работающие");
			this.checkShowOnlyWork.Active = true;
			this.checkShowOnlyWork.DrawIndicator = true;
			this.checkShowOnlyWork.UseUnderline = true;
			this.table1.Add(this.checkShowOnlyWork);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.checkShowOnlyWork]));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entityDepartment = new global::QS.Views.Control.EntityEntry();
			this.entityDepartment.Events = ((global::Gdk.EventMask)(256));
			this.entityDepartment.Name = "entityDepartment";
			this.table1.Add(this.entityDepartment);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.entityDepartment]));
			w3.TopAttach = ((uint)(1));
			w3.BottomAttach = ((uint)(2));
			w3.LeftAttach = ((uint)(2));
			w3.RightAttach = ((uint)(3));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entitySubdivision = new global::QS.Views.Control.EntityEntry();
			this.entitySubdivision.Events = ((global::Gdk.EventMask)(256));
			this.entitySubdivision.Name = "entitySubdivision";
			this.table1.Add(this.entitySubdivision);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.entitySubdivision]));
			w4.LeftAttach = ((uint)(2));
			w4.RightAttach = ((uint)(3));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Отдел:");
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.table1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
