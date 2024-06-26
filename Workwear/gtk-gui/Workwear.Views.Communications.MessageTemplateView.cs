
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Communications
{
	public partial class MessageTemplateView
	{
		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Button buttonSave;

		private global::Gtk.Button buttonCancel;

		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::Gamma.GtkWidgets.yEntry entityTitle;

		private global::Gamma.GtkWidgets.yEntry entryLink;

		private global::Gamma.GtkWidgets.yEntry entryLinkTitle;

		private global::Gamma.GtkWidgets.yEntry entryName;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gamma.GtkWidgets.yTextView ytextText;

		private global::Gamma.GtkWidgets.yLabel ylabel11;

		private global::Gamma.GtkWidgets.yLabel ylabel12;

		private global::Gamma.GtkWidgets.yLabel ylabel13;

		private global::Gamma.GtkWidgets.yLabel ylabel14;

		private global::Gamma.GtkWidgets.yLabel ylabel15;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Communications.MessageTemplateView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Communications.MessageTemplateView";
			// Container child Workwear.Views.Communications.MessageTemplateView.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox4 = new global::Gtk.HBox();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.buttonSave = new global::Gtk.Button();
			this.buttonSave.CanFocus = true;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.UseUnderline = true;
			this.buttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-save", global::Gtk.IconSize.Menu);
			this.buttonSave.Image = w1;
			this.hbox4.Add(this.buttonSave);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.buttonSave]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.buttonCancel = new global::Gtk.Button();
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-revert-to-saved", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w3;
			this.hbox4.Add(this.buttonCancel);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.buttonCancel]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.vbox2.Add(this.hbox4);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox4]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(5));
			this.ytable1.NColumns = ((uint)(2));
			this.ytable1.RowSpacing = ((uint)(6));
			this.ytable1.ColumnSpacing = ((uint)(6));
			// Container child ytable1.Gtk.Table+TableChild
			this.entityTitle = new global::Gamma.GtkWidgets.yEntry();
			this.entityTitle.CanFocus = true;
			this.entityTitle.Name = "entityTitle";
			this.entityTitle.IsEditable = true;
			this.entityTitle.MaxLength = 200;
			this.entityTitle.InvisibleChar = '•';
			this.ytable1.Add(this.entityTitle);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytable1[this.entityTitle]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.entryLink = new global::Gamma.GtkWidgets.yEntry();
			this.entryLink.WidthRequest = 400;
			this.entryLink.CanFocus = true;
			this.entryLink.Name = "entryLink";
			this.entryLink.IsEditable = true;
			this.entryLink.MaxLength = 100;
			this.entryLink.InvisibleChar = '•';
			this.ytable1.Add(this.entryLink);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.ytable1[this.entryLink]));
			w7.TopAttach = ((uint)(4));
			w7.BottomAttach = ((uint)(5));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.entryLinkTitle = new global::Gamma.GtkWidgets.yEntry();
			this.entryLinkTitle.WidthRequest = 400;
			this.entryLinkTitle.CanFocus = true;
			this.entryLinkTitle.Name = "entryLinkTitle";
			this.entryLinkTitle.IsEditable = true;
			this.entryLinkTitle.MaxLength = 100;
			this.entryLinkTitle.InvisibleChar = '•';
			this.ytable1.Add(this.entryLinkTitle);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytable1[this.entryLinkTitle]));
			w8.TopAttach = ((uint)(3));
			w8.BottomAttach = ((uint)(4));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.entryName = new global::Gamma.GtkWidgets.yEntry();
			this.entryName.WidthRequest = 400;
			this.entryName.CanFocus = true;
			this.entryName.Name = "entryName";
			this.entryName.IsEditable = true;
			this.entryName.MaxLength = 100;
			this.entryName.InvisibleChar = '•';
			this.ytable1.Add(this.entryName);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.ytable1[this.entryName]));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.ytextText = new global::Gamma.GtkWidgets.yTextView();
			this.ytextText.CanFocus = true;
			this.ytextText.Name = "ytextText";
			this.GtkScrolledWindow1.Add(this.ytextText);
			this.ytable1.Add(this.GtkScrolledWindow1);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.ytable1[this.GtkScrolledWindow1]));
			w11.TopAttach = ((uint)(2));
			w11.BottomAttach = ((uint)(3));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel11 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel11.Name = "ylabel11";
			this.ylabel11.Xalign = 1F;
			this.ylabel11.LabelProp = global::Mono.Unix.Catalog.GetString("Название:");
			this.ytable1.Add(this.ylabel11);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel11]));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel12 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel12.Name = "ylabel12";
			this.ylabel12.Xalign = 1F;
			this.ylabel12.LabelProp = global::Mono.Unix.Catalog.GetString("Заголовок:");
			this.ytable1.Add(this.ylabel12);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel12]));
			w13.TopAttach = ((uint)(1));
			w13.BottomAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel13 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel13.Name = "ylabel13";
			this.ylabel13.Xalign = 1F;
			this.ylabel13.Yalign = 0F;
			this.ylabel13.LabelProp = global::Mono.Unix.Catalog.GetString("Текст:");
			this.ytable1.Add(this.ylabel13);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel13]));
			w14.TopAttach = ((uint)(2));
			w14.BottomAttach = ((uint)(3));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel14 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel14.Name = "ylabel14";
			this.ylabel14.Xalign = 1F;
			this.ylabel14.Yalign = 0F;
			this.ylabel14.LabelProp = global::Mono.Unix.Catalog.GetString("Заголовок ссылки:");
			this.ylabel14.Wrap = true;
			this.ytable1.Add(this.ylabel14);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel14]));
			w15.TopAttach = ((uint)(3));
			w15.BottomAttach = ((uint)(4));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel15 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel15.Name = "ylabel15";
			this.ylabel15.Xalign = 1F;
			this.ylabel15.Yalign = 0F;
			this.ylabel15.LabelProp = global::Mono.Unix.Catalog.GetString("Ссылка:");
			this.ytable1.Add(this.ylabel15);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel15]));
			w16.TopAttach = ((uint)(4));
			w16.BottomAttach = ((uint)(5));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add(this.ytable1);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.ytable1]));
			w17.Position = 1;
			w17.Expand = false;
			w17.Fill = false;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
