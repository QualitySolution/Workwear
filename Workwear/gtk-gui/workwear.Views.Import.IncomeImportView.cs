
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Views.Import
{
	public partial class IncomeImportView
	{
		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::Gtk.ScrolledWindow scrolledwindow1;

		private global::Gamma.GtkWidgets.yTreeView ytreeview1;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yButton ybuttonLoad;

		private global::Gamma.GtkWidgets.yVBox yvboxSelectDocumentArea;

		private global::Gamma.GtkWidgets.yButton ybuttonParse;

		private global::Gamma.GtkWidgets.yHBox yhbox2;

		private global::Gamma.GtkWidgets.yButton ybuttonSave;

		private global::Gamma.GtkWidgets.yButton ybuttonCancel;

		private global::Gamma.GtkWidgets.yVBox yvbox1;

		private global::Gamma.GtkWidgets.yButton ybuttonCreateNomenclature;

		private global::Gamma.GtkWidgets.yButton ybuttonCreateSize;

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
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ybuttonLoad = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonLoad.CanFocus = true;
			this.ybuttonLoad.Name = "ybuttonLoad";
			this.ybuttonLoad.UseUnderline = true;
			this.ybuttonLoad.Label = global::Mono.Unix.Catalog.GetString("Выбрать файл");
			this.yhbox1.Add(this.ybuttonLoad);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ybuttonLoad]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.yvboxSelectDocumentArea = new global::Gamma.GtkWidgets.yVBox();
			this.yvboxSelectDocumentArea.Name = "yvboxSelectDocumentArea";
			this.yvboxSelectDocumentArea.Spacing = 6;
			this.yhbox1.Add(this.yvboxSelectDocumentArea);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.yvboxSelectDocumentArea]));
			w4.Position = 1;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ybuttonParse = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonParse.CanFocus = true;
			this.ybuttonParse.Name = "ybuttonParse";
			this.ybuttonParse.UseUnderline = true;
			this.ybuttonParse.Label = global::Mono.Unix.Catalog.GetString("Загрузить");
			this.yhbox1.Add(this.ybuttonParse);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ybuttonParse]));
			w5.Position = 2;
			w5.Expand = false;
			w5.Fill = false;
			this.ytable1.Add(this.yhbox1);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yhbox1]));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yhbox2 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox2.Name = "yhbox2";
			this.yhbox2.Spacing = 6;
			// Container child yhbox2.Gtk.Box+BoxChild
			this.ybuttonSave = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSave.CanFocus = true;
			this.ybuttonSave.Name = "ybuttonSave";
			this.ybuttonSave.UseUnderline = true;
			this.ybuttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			this.yhbox2.Add(this.ybuttonSave);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.yhbox2[this.ybuttonSave]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child yhbox2.Gtk.Box+BoxChild
			this.ybuttonCancel = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCancel.CanFocus = true;
			this.ybuttonCancel.Name = "ybuttonCancel";
			this.ybuttonCancel.UseUnderline = true;
			this.ybuttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			this.yhbox2.Add(this.ybuttonCancel);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.yhbox2[this.ybuttonCancel]));
			w8.Position = 1;
			w8.Expand = false;
			w8.Fill = false;
			this.ytable1.Add(this.yhbox2);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yhbox2]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
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
			// Container child yvbox1.Gtk.Box+BoxChild
			this.ybuttonCreateSize = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCreateSize.CanFocus = true;
			this.ybuttonCreateSize.Name = "ybuttonCreateSize";
			this.ybuttonCreateSize.UseUnderline = true;
			this.ybuttonCreateSize.Label = global::Mono.Unix.Catalog.GetString("Создать размеры");
			this.yvbox1.Add(this.ybuttonCreateSize);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.ybuttonCreateSize]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			this.ytable1.Add(this.yvbox1);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yvbox1]));
			w12.TopAttach = ((uint)(1));
			w12.BottomAttach = ((uint)(2));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.ytable1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
