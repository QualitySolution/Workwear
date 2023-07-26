
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Communications
{
	public partial class ClaimsView
	{
		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yVBox yvbox1;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonShowClosed;

		private global::Gtk.ScrolledWindow scrolledwindow2;

		private global::Gamma.GtkWidgets.yTreeView ytreeClaims;

		private global::Gamma.GtkWidgets.yVBox yvbox2;

		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::Gamma.GtkWidgets.yButton buttonOpenProtectionTools;

		private global::Gamma.GtkWidgets.yLabel labelClaimTitle;

		private global::Gamma.GtkWidgets.yLabel labelProtectionToolsName;

		private global::Gamma.GtkWidgets.yLabel labelTitleProtectionTools;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gtk.ScrolledWindow scrolledwindow1;

		private global::Gamma.GtkWidgets.yTreeView ytreeClaimMessages;

		private global::Gamma.GtkWidgets.yVBox yvbox3;

		private global::Gamma.GtkWidgets.yHBox yhbox2;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTextView yentryMessage;

		private global::Gtk.VBox vbox2;

		private global::Gamma.GtkWidgets.yButton buttonAnswer;

		private global::Gamma.GtkWidgets.yButton buttonClose;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Communications.ClaimsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Communications.ClaimsView";
			// Container child Workwear.Views.Communications.ClaimsView.Gtk.Container+ContainerChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.yvbox1 = new global::Gamma.GtkWidgets.yVBox();
			this.yvbox1.Name = "yvbox1";
			this.yvbox1.Spacing = 6;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.ycheckbuttonShowClosed = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonShowClosed.CanFocus = true;
			this.ycheckbuttonShowClosed.Name = "ycheckbuttonShowClosed";
			this.ycheckbuttonShowClosed.Label = global::Mono.Unix.Catalog.GetString("Показывать закрытые");
			this.ycheckbuttonShowClosed.DrawIndicator = true;
			this.ycheckbuttonShowClosed.UseUnderline = true;
			this.yvbox1.Add(this.ycheckbuttonShowClosed);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.ycheckbuttonShowClosed]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.scrolledwindow2 = new global::Gtk.ScrolledWindow();
			this.scrolledwindow2.WidthRequest = 400;
			this.scrolledwindow2.CanFocus = true;
			this.scrolledwindow2.Name = "scrolledwindow2";
			this.scrolledwindow2.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow2.Gtk.Container+ContainerChild
			this.ytreeClaims = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeClaims.CanFocus = true;
			this.ytreeClaims.Name = "ytreeClaims";
			this.ytreeClaims.HeadersVisible = false;
			this.scrolledwindow2.Add(this.ytreeClaims);
			this.yvbox1.Add(this.scrolledwindow2);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.scrolledwindow2]));
			w3.Position = 1;
			this.yhbox1.Add(this.yvbox1);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.yvbox1]));
			w4.Position = 0;
			w4.Expand = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.yvbox2 = new global::Gamma.GtkWidgets.yVBox();
			this.yvbox2.Name = "yvbox2";
			this.yvbox2.Spacing = 6;
			// Container child yvbox2.Gtk.Box+BoxChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(2));
			this.ytable1.NColumns = ((uint)(3));
			this.ytable1.RowSpacing = ((uint)(6));
			this.ytable1.ColumnSpacing = ((uint)(6));
			// Container child ytable1.Gtk.Table+TableChild
			this.buttonOpenProtectionTools = new global::Gamma.GtkWidgets.yButton();
			this.buttonOpenProtectionTools.CanFocus = true;
			this.buttonOpenProtectionTools.Name = "buttonOpenProtectionTools";
			this.buttonOpenProtectionTools.UseUnderline = true;
			global::Gtk.Image w5 = new global::Gtk.Image();
			w5.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-edit", global::Gtk.IconSize.Menu);
			this.buttonOpenProtectionTools.Image = w5;
			this.ytable1.Add(this.buttonOpenProtectionTools);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytable1[this.buttonOpenProtectionTools]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.LeftAttach = ((uint)(2));
			w6.RightAttach = ((uint)(3));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.labelClaimTitle = new global::Gamma.GtkWidgets.yLabel();
			this.labelClaimTitle.Name = "labelClaimTitle";
			this.labelClaimTitle.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel3");
			this.ytable1.Add(this.labelClaimTitle);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.ytable1[this.labelClaimTitle]));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(3));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.labelProtectionToolsName = new global::Gamma.GtkWidgets.yLabel();
			this.labelProtectionToolsName.Name = "labelProtectionToolsName";
			this.labelProtectionToolsName.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel4");
			this.ytable1.Add(this.labelProtectionToolsName);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytable1[this.labelProtectionToolsName]));
			w8.TopAttach = ((uint)(1));
			w8.BottomAttach = ((uint)(2));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.labelTitleProtectionTools = new global::Gamma.GtkWidgets.yLabel();
			this.labelTitleProtectionTools.Name = "labelTitleProtectionTools";
			this.labelTitleProtectionTools.Xalign = 1F;
			this.labelTitleProtectionTools.LabelProp = global::Mono.Unix.Catalog.GetString("Потребность:");
			this.ytable1.Add(this.labelTitleProtectionTools);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.ytable1[this.labelTitleProtectionTools]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.Xalign = 1F;
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Тема:");
			this.ytable1.Add(this.ylabel1);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel1]));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			this.yvbox2.Add(this.ytable1);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.yvbox2[this.ytable1]));
			w11.Position = 0;
			w11.Expand = false;
			w11.Fill = false;
			// Container child yvbox2.Gtk.Box+BoxChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow();
			this.scrolledwindow1.CanFocus = true;
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow1.Gtk.Container+ContainerChild
			this.ytreeClaimMessages = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeClaimMessages.CanFocus = true;
			this.ytreeClaimMessages.Name = "ytreeClaimMessages";
			this.ytreeClaimMessages.HeadersVisible = false;
			this.scrolledwindow1.Add(this.ytreeClaimMessages);
			this.yvbox2.Add(this.scrolledwindow1);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.yvbox2[this.scrolledwindow1]));
			w13.Position = 1;
			// Container child yvbox2.Gtk.Box+BoxChild
			this.yvbox3 = new global::Gamma.GtkWidgets.yVBox();
			this.yvbox3.Name = "yvbox3";
			this.yvbox3.Spacing = 6;
			// Container child yvbox3.Gtk.Box+BoxChild
			this.yhbox2 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox2.Name = "yhbox2";
			this.yhbox2.Spacing = 6;
			// Container child yhbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.yentryMessage = new global::Gamma.GtkWidgets.yTextView();
			this.yentryMessage.CanFocus = true;
			this.yentryMessage.Name = "yentryMessage";
			this.GtkScrolledWindow.Add(this.yentryMessage);
			this.yhbox2.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.yhbox2[this.GtkScrolledWindow]));
			w15.Position = 0;
			// Container child yhbox2.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.buttonAnswer = new global::Gamma.GtkWidgets.yButton();
			this.buttonAnswer.CanFocus = true;
			this.buttonAnswer.Name = "buttonAnswer";
			this.buttonAnswer.UseUnderline = true;
			this.buttonAnswer.Label = global::Mono.Unix.Catalog.GetString("Ответить");
			global::Gtk.Image w16 = new global::Gtk.Image();
			w16.Pixbuf = global::Gdk.Pixbuf.LoadFromResource("Workwear.icon.buttons.send.png");
			this.buttonAnswer.Image = w16;
			this.vbox2.Add(this.buttonAnswer);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.buttonAnswer]));
			w17.Position = 0;
			w17.Expand = false;
			w17.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.buttonClose = new global::Gamma.GtkWidgets.yButton();
			this.buttonClose.CanFocus = true;
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.UseUnderline = true;
			this.buttonClose.Label = global::Mono.Unix.Catalog.GetString("Закрыть вопрос");
			global::Gtk.Image w18 = new global::Gtk.Image();
			w18.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-stop", global::Gtk.IconSize.Menu);
			this.buttonClose.Image = w18;
			this.vbox2.Add(this.buttonClose);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.buttonClose]));
			w19.Position = 1;
			w19.Expand = false;
			w19.Fill = false;
			this.yhbox2.Add(this.vbox2);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.yhbox2[this.vbox2]));
			w20.Position = 1;
			w20.Expand = false;
			w20.Fill = false;
			this.yvbox3.Add(this.yhbox2);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.yvbox3[this.yhbox2]));
			w21.Position = 0;
			w21.Expand = false;
			w21.Fill = false;
			this.yvbox2.Add(this.yvbox3);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.yvbox2[this.yvbox3]));
			w22.Position = 2;
			w22.Expand = false;
			w22.Fill = false;
			this.yhbox1.Add(this.yvbox2);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.yvbox2]));
			w23.Position = 1;
			this.Add(this.yhbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonAnswer.Clicked += new global::System.EventHandler(this.OnButtonAnswerClicked);
			this.buttonClose.Clicked += new global::System.EventHandler(this.OnButtonCloseClicked);
		}
	}
}
