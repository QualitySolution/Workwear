
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Import
{
	public partial class CountersView
	{
		private global::Gamma.GtkWidgets.yTable tableCounters;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Import.CountersView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Import.CountersView";
			// Container child Workwear.Views.Import.CountersView.Gtk.Container+ContainerChild
			this.tableCounters = new global::Gamma.GtkWidgets.yTable();
			this.tableCounters.Name = "tableCounters";
			this.tableCounters.NRows = ((uint)(3));
			this.tableCounters.NColumns = ((uint)(2));
			this.tableCounters.RowSpacing = ((uint)(6));
			this.tableCounters.ColumnSpacing = ((uint)(6));
			this.Add(this.tableCounters);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
