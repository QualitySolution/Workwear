
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Journal.Filter.Views.Company
{
	public partial class EmployeeFilterView
	{
		private global::Gtk.Table table1;

		private global::Gamma.GtkWidgets.yCheckButton checkShowOnlyWork;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Journal.Filter.Views.Company.EmployeeFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Journal.Filter.Views.Company.EmployeeFilterView";
			// Container child workwear.Journal.Filter.Views.Company.EmployeeFilterView.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(1)), ((uint)(3)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.checkShowOnlyWork = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkShowOnlyWork.CanFocus = true;
			this.checkShowOnlyWork.Name = "checkShowOnlyWork";
			this.checkShowOnlyWork.Label = global::Mono.Unix.Catalog.GetString("Только работающие");
			this.checkShowOnlyWork.Active = true;
			this.checkShowOnlyWork.DrawIndicator = true;
			this.checkShowOnlyWork.UseUnderline = true;
			this.table1.Add(this.checkShowOnlyWork);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.checkShowOnlyWork]));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.table1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
