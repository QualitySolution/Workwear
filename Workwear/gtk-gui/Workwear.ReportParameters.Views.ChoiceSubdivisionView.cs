
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.ReportParameters.Views
{
	public partial class ChoiceSubdivisionView
	{
		private global::Gtk.VBox vbox1;

		private global::Gamma.GtkWidgets.yButton ycheckbuttonChoiseAllSubdivisions;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeChoiseSubdivision;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.ReportParameters.Views.ChoiceSubdivisionView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.ReportParameters.Views.ChoiceSubdivisionView";
			// Container child Workwear.ReportParameters.Views.ChoiceSubdivisionView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ycheckbuttonChoiseAllSubdivisions = new global::Gamma.GtkWidgets.yButton();
			this.ycheckbuttonChoiseAllSubdivisions.CanFocus = true;
			this.ycheckbuttonChoiseAllSubdivisions.Name = "ycheckbuttonChoiseAllSubdivisions";
			this.ycheckbuttonChoiseAllSubdivisions.UseUnderline = true;
			this.ycheckbuttonChoiseAllSubdivisions.Label = global::Mono.Unix.Catalog.GetString("Выделить/снять выделение");
			this.vbox1.Add(this.ycheckbuttonChoiseAllSubdivisions);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ycheckbuttonChoiseAllSubdivisions]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeChoiseSubdivision = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeChoiseSubdivision.CanFocus = true;
			this.ytreeChoiseSubdivision.Name = "ytreeChoiseSubdivision";
			this.GtkScrolledWindow.Add(this.ytreeChoiseSubdivision);
			this.vbox1.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow]));
			w3.Position = 1;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
