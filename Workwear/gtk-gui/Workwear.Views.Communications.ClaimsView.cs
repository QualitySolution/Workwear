
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

		private global::Gtk.ScrolledWindow scrolledwindow1;

		private global::Gamma.GtkWidgets.yTreeView ytreeClaimMessages;

		private global::Gamma.GtkWidgets.yVBox yvbox3;

		private global::Gamma.GtkWidgets.yHBox yhbox2;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTextView yentryMessage;

		private global::Gamma.GtkWidgets.yButton ybuttonSend;

		private global::Gamma.GtkWidgets.yHBox yhbox3;

		private global::Gamma.GtkWidgets.yLabel ylabelStatusClaim;

		private global::Gamma.Widgets.yEnumComboBox yComboStatus;

		private global::Gamma.GtkWidgets.yButton ybuttonChangeStatus;

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
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.yvbox2[this.scrolledwindow1]));
			w6.Position = 0;
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
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.yhbox2[this.GtkScrolledWindow]));
			w8.Position = 0;
			// Container child yhbox2.Gtk.Box+BoxChild
			this.ybuttonSend = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSend.CanFocus = true;
			this.ybuttonSend.Name = "ybuttonSend";
			this.ybuttonSend.UseUnderline = true;
			this.ybuttonSend.Label = global::Mono.Unix.Catalog.GetString("Отправить");
			global::Gtk.Image w9 = new global::Gtk.Image();
			w9.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "stock_mail-send", global::Gtk.IconSize.Menu);
			this.ybuttonSend.Image = w9;
			this.yhbox2.Add(this.ybuttonSend);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.yhbox2[this.ybuttonSend]));
			w10.Position = 1;
			w10.Expand = false;
			w10.Fill = false;
			this.yvbox3.Add(this.yhbox2);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.yvbox3[this.yhbox2]));
			w11.Position = 0;
			w11.Expand = false;
			w11.Fill = false;
			// Container child yvbox3.Gtk.Box+BoxChild
			this.yhbox3 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox3.Name = "yhbox3";
			this.yhbox3.Spacing = 6;
			// Container child yhbox3.Gtk.Box+BoxChild
			this.ylabelStatusClaim = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelStatusClaim.Name = "ylabelStatusClaim";
			this.ylabelStatusClaim.Xalign = 1F;
			this.ylabelStatusClaim.LabelProp = global::Mono.Unix.Catalog.GetString("Статус обращения");
			this.yhbox3.Add(this.ylabelStatusClaim);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.yhbox3[this.ylabelStatusClaim]));
			w12.Position = 0;
			w12.Expand = false;
			w12.Fill = false;
			// Container child yhbox3.Gtk.Box+BoxChild
			this.yComboStatus = new global::Gamma.Widgets.yEnumComboBox();
			this.yComboStatus.Name = "yComboStatus";
			this.yComboStatus.ShowSpecialStateAll = false;
			this.yComboStatus.ShowSpecialStateNot = false;
			this.yComboStatus.UseShortTitle = false;
			this.yComboStatus.DefaultFirst = false;
			this.yhbox3.Add(this.yComboStatus);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.yhbox3[this.yComboStatus]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			// Container child yhbox3.Gtk.Box+BoxChild
			this.ybuttonChangeStatus = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonChangeStatus.CanFocus = true;
			this.ybuttonChangeStatus.Name = "ybuttonChangeStatus";
			this.ybuttonChangeStatus.UseUnderline = true;
			this.ybuttonChangeStatus.Label = global::Mono.Unix.Catalog.GetString("Изменить");
			this.yhbox3.Add(this.ybuttonChangeStatus);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.yhbox3[this.ybuttonChangeStatus]));
			w14.Position = 2;
			w14.Expand = false;
			w14.Fill = false;
			this.yvbox3.Add(this.yhbox3);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.yvbox3[this.yhbox3]));
			w15.Position = 1;
			w15.Expand = false;
			w15.Fill = false;
			this.yvbox2.Add(this.yvbox3);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.yvbox2[this.yvbox3]));
			w16.Position = 1;
			w16.Expand = false;
			w16.Fill = false;
			this.yhbox1.Add(this.yvbox2);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.yvbox2]));
			w17.Position = 1;
			this.Add(this.yhbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
