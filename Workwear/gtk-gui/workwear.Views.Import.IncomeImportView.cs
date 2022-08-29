
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Import
{
	public partial class IncomeImportView
	{
		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::Gtk.ScrolledWindow scrolledwindow1;

		private global::Gamma.GtkWidgets.yTreeView ytreeview1;

		private global::Gamma.GtkWidgets.yButton ybuttonCancel;

		private global::Gamma.GtkWidgets.yButton ybuttonCreateIncome;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yButton ybuttonFileChoose;

		private global::Gamma.Widgets.yListComboBox ylistcomboboxDocuments;

		private global::Gamma.GtkWidgets.yButton ybuttonDownload;

		private global::QS.Views.Control.EntityEntry entityWarehouseIncome;

		private global::Gamma.GtkWidgets.yVBox yvbox1;

		private global::Gamma.GtkWidgets.yButton ybuttonCreateNomenclature;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Views.Import.IncomeImportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Views.Import.IncomeImportView";
			// Container child workwear.Views.Import.IncomeImportView.Gtk.Container+ContainerChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(3));
			this.ytable1.NColumns = ((uint)(2));
			this.ytable1.RowSpacing = ((uint)(6));
			this.ytable1.ColumnSpacing = ((uint)(6));
			// Container child ytable1.Gtk.Table+TableChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow();
			this.scrolledwindow1.CanFocus = true;
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow1.Gtk.Container+ContainerChild
			this.ytreeview1 = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeview1.CanFocus = true;
			this.ytreeview1.Name = "ytreeview1";
			this.scrolledwindow1.Add(this.ytreeview1);
			this.ytable1.Add(this.scrolledwindow1);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytable1[this.scrolledwindow1]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			// Container child ytable1.Gtk.Table+TableChild
			this.ybuttonCancel = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCancel.CanFocus = true;
			this.ybuttonCancel.Name = "ybuttonCancel";
			this.ybuttonCancel.UseUnderline = true;
			this.ybuttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			this.ytable1.Add(this.ybuttonCancel);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ybuttonCancel]));
			w3.TopAttach = ((uint)(2));
			w3.BottomAttach = ((uint)(3));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ybuttonCreateIncome = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCreateIncome.CanFocus = true;
			this.ybuttonCreateIncome.Name = "ybuttonCreateIncome";
			this.ybuttonCreateIncome.UseUnderline = true;
			this.ybuttonCreateIncome.Label = global::Mono.Unix.Catalog.GetString("Создать поступление");
			this.ytable1.Add(this.ybuttonCreateIncome);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ybuttonCreateIncome]));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ybuttonFileChoose = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonFileChoose.CanFocus = true;
			this.ybuttonFileChoose.Name = "ybuttonFileChoose";
			this.ybuttonFileChoose.UseUnderline = true;
			this.ybuttonFileChoose.Label = global::Mono.Unix.Catalog.GetString("Выбрать файл");
			this.yhbox1.Add(this.ybuttonFileChoose);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ybuttonFileChoose]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ylistcomboboxDocuments = new global::Gamma.Widgets.yListComboBox();
			this.ylistcomboboxDocuments.Name = "ylistcomboboxDocuments";
			this.ylistcomboboxDocuments.AddIfNotExist = false;
			this.ylistcomboboxDocuments.DefaultFirst = false;
			this.yhbox1.Add(this.ylistcomboboxDocuments);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ylistcomboboxDocuments]));
			w6.Position = 1;
			w6.Expand = false;
			w6.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ybuttonDownload = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonDownload.CanFocus = true;
			this.ybuttonDownload.Name = "ybuttonDownload";
			this.ybuttonDownload.UseUnderline = true;
			this.ybuttonDownload.Label = global::Mono.Unix.Catalog.GetString("Загрузить");
			this.yhbox1.Add(this.ybuttonDownload);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ybuttonDownload]));
			w7.Position = 2;
			w7.Expand = false;
			w7.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.entityWarehouseIncome = new global::QS.Views.Control.EntityEntry();
			this.entityWarehouseIncome.Events = ((global::Gdk.EventMask)(256));
			this.entityWarehouseIncome.Name = "entityWarehouseIncome";
			this.yhbox1.Add(this.entityWarehouseIncome);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.entityWarehouseIncome]));
			w8.Position = 3;
			this.ytable1.Add(this.yhbox1);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yhbox1]));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yvbox1 = new global::Gamma.GtkWidgets.yVBox();
			this.yvbox1.Name = "yvbox1";
			this.yvbox1.Spacing = 6;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.ybuttonCreateNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCreateNomenclature.CanFocus = true;
			this.ybuttonCreateNomenclature.Name = "ybuttonCreateNomenclature";
			this.ybuttonCreateNomenclature.UseUnderline = true;
			this.ybuttonCreateNomenclature.Label = global::Mono.Unix.Catalog.GetString("Создать номенклатуры");
			this.yvbox1.Add(this.ybuttonCreateNomenclature);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.ybuttonCreateNomenclature]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			this.ytable1.Add(this.yvbox1);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yvbox1]));
			w11.TopAttach = ((uint)(1));
			w11.BottomAttach = ((uint)(2));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.ytable1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}