
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Journal.Filter.Views.Stock
{
	public partial class NomenclatureFilterView
	{
		private global::Gtk.Table table1;

		private global::QS.Views.Control.EntityEntry entityItemsType;

		private global::QS.Views.Control.EntityEntry entityProtectionTools;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonOnlyWithRating;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yCheckButton yShowArchival;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Journal.Filter.Views.Stock.NomenclatureFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Journal.Filter.Views.Stock.NomenclatureFilterView";
			// Container child workwear.Journal.Filter.Views.Stock.NomenclatureFilterView.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(4)), ((uint)(3)), false);
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
			w1.RightAttach = ((uint)(3));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entityProtectionTools = new global::QS.Views.Control.EntityEntry();
			this.entityProtectionTools.Events = ((global::Gdk.EventMask)(256));
			this.entityProtectionTools.Name = "entityProtectionTools";
			this.table1.Add(this.entityProtectionTools);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.entityProtectionTools]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(3));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckbuttonOnlyWithRating = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonOnlyWithRating.CanFocus = true;
			this.ycheckbuttonOnlyWithRating.Name = "ycheckbuttonOnlyWithRating";
			this.ycheckbuttonOnlyWithRating.Label = global::Mono.Unix.Catalog.GetString("Только с оценками");
			this.ycheckbuttonOnlyWithRating.DrawIndicator = true;
			this.ycheckbuttonOnlyWithRating.UseUnderline = true;
			this.ycheckbuttonOnlyWithRating.Xalign = 1F;
			this.table1.Add(this.ycheckbuttonOnlyWithRating);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckbuttonOnlyWithRating]));
			w3.TopAttach = ((uint)(2));
			w3.BottomAttach = ((uint)(3));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			this.table1.Add(this.yhbox1);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.yhbox1]));
			w4.TopAttach = ((uint)(2));
			w4.BottomAttach = ((uint)(3));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.Xalign = 1F;
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Тип номенклатуры:");
			this.ylabel1.Justify = ((global::Gtk.Justification)(1));
			this.table1.Add(this.ylabel1);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel1]));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Номенклатура нормы:");
			this.table1.Add(this.ylabel2);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel2]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yShowArchival = new global::Gamma.GtkWidgets.yCheckButton();
			this.yShowArchival.CanFocus = true;
			this.yShowArchival.Name = "yShowArchival";
			this.yShowArchival.Label = global::Mono.Unix.Catalog.GetString("Показать архивные");
			this.yShowArchival.DrawIndicator = true;
			this.yShowArchival.UseUnderline = true;
			this.table1.Add(this.yShowArchival);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.yShowArchival]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.LeftAttach = ((uint)(2));
			w7.RightAttach = ((uint)(3));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.table1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
