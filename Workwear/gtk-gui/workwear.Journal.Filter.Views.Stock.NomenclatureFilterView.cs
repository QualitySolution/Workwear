
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Journal.Filter.Views.Stock
{
	public partial class NomenclatureFilterView
	{
		private global::Gtk.Table table1;

		private global::QS.Views.Control.EntityEntry entityItemsType;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Journal.Filter.Views.Stock.NomenclatureFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Journal.Filter.Views.Stock.NomenclatureFilterView";
			// Container child workwear.Journal.Filter.Views.Stock.NomenclatureFilterView.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(3)), ((uint)(3)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.entityItemsType = new global::QS.Views.Control.EntityEntry();
			this.entityItemsType.Events = ((global::Gdk.EventMask)(256));
			this.entityItemsType.Name = "entityItemsType";
			this.table1.Add(this.entityItemsType);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.entityItemsType]));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Тип номенклатуры:");
			this.table1.Add(this.ylabel1);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel1]));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.table1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
