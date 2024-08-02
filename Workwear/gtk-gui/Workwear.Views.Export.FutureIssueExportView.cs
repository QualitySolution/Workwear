
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Export
{
	public partial class FutureIssueExportView
	{
		private global::Gamma.GtkWidgets.yTable ytable2;

		private global::Gamma.GtkWidgets.yCheckButton checkNoDebt;

		private global::QS.Views.Control.EntityEntry entityentryOrganization;

		private global::Gtk.Label labelOrg;

		private global::Gtk.Label labelPeriod;

		private global::Gamma.GtkWidgets.yButton ybuttonRun;

		private global::Gamma.Widgets.yDatePeriodPicker ydateperiodpicker;

		private global::QS.Widgets.ProgressWidget yprogressLocal;

		private global::QS.Widgets.ProgressWidget yprogressTotal;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Export.FutureIssueExportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Export.FutureIssueExportView";
			// Container child Workwear.Views.Export.FutureIssueExportView.Gtk.Container+ContainerChild
			this.ytable2 = new global::Gamma.GtkWidgets.yTable();
			this.ytable2.Name = "ytable2";
			this.ytable2.NRows = ((uint)(6));
			this.ytable2.NColumns = ((uint)(2));
			this.ytable2.RowSpacing = ((uint)(6));
			this.ytable2.ColumnSpacing = ((uint)(6));
			// Container child ytable2.Gtk.Table+TableChild
			this.checkNoDebt = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkNoDebt.TooltipMarkup = "Рассчитываем так как будто все долги выданы до начала периода. В этом случае даты" +
				" будут считаться иначе.";
			this.checkNoDebt.CanFocus = true;
			this.checkNoDebt.Name = "checkNoDebt";
			this.checkNoDebt.Label = global::Mono.Unix.Catalog.GetString("Без задолжности");
			this.checkNoDebt.DrawIndicator = true;
			this.checkNoDebt.UseUnderline = true;
			this.ytable2.Add(this.checkNoDebt);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.ytable2[this.checkNoDebt]));
			w1.TopAttach = ((uint)(2));
			w1.BottomAttach = ((uint)(3));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable2.Gtk.Table+TableChild
			this.entityentryOrganization = new global::QS.Views.Control.EntityEntry();
			this.entityentryOrganization.Events = ((global::Gdk.EventMask)(256));
			this.entityentryOrganization.Name = "entityentryOrganization";
			this.ytable2.Add(this.entityentryOrganization);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytable2[this.entityentryOrganization]));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable2.Gtk.Table+TableChild
			this.labelOrg = new global::Gtk.Label();
			this.labelOrg.Name = "labelOrg";
			this.labelOrg.Xalign = 1F;
			this.labelOrg.LabelProp = global::Mono.Unix.Catalog.GetString("Организация:");
			this.ytable2.Add(this.labelOrg);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.ytable2[this.labelOrg]));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable2.Gtk.Table+TableChild
			this.labelPeriod = new global::Gtk.Label();
			this.labelPeriod.Name = "labelPeriod";
			this.labelPeriod.Xalign = 1F;
			this.labelPeriod.LabelProp = global::Mono.Unix.Catalog.GetString("Период:");
			this.ytable2.Add(this.labelPeriod);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.ytable2[this.labelPeriod]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable2.Gtk.Table+TableChild
			this.ybuttonRun = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonRun.CanFocus = true;
			this.ybuttonRun.Name = "ybuttonRun";
			this.ybuttonRun.UseUnderline = true;
			this.ybuttonRun.Label = global::Mono.Unix.Catalog.GetString("Выгрузить");
			this.ytable2.Add(this.ybuttonRun);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.ytable2[this.ybuttonRun]));
			w5.TopAttach = ((uint)(5));
			w5.BottomAttach = ((uint)(6));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable2.Gtk.Table+TableChild
			this.ydateperiodpicker = new global::Gamma.Widgets.yDatePeriodPicker();
			this.ydateperiodpicker.Events = ((global::Gdk.EventMask)(256));
			this.ydateperiodpicker.Name = "ydateperiodpicker";
			this.ydateperiodpicker.StartDate = new global::System.DateTime(0);
			this.ydateperiodpicker.EndDate = new global::System.DateTime(0);
			this.ytable2.Add(this.ydateperiodpicker);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytable2[this.ydateperiodpicker]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable2.Gtk.Table+TableChild
			this.yprogressLocal = new global::QS.Widgets.ProgressWidget();
			this.yprogressLocal.Name = "yprogressLocal";
			this.ytable2.Add(this.yprogressLocal);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.ytable2[this.yprogressLocal]));
			w7.TopAttach = ((uint)(3));
			w7.BottomAttach = ((uint)(4));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable2.Gtk.Table+TableChild
			this.yprogressTotal = new global::QS.Widgets.ProgressWidget();
			this.yprogressTotal.Name = "yprogressTotal";
			this.ytable2.Add(this.yprogressTotal);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytable2[this.yprogressTotal]));
			w8.TopAttach = ((uint)(4));
			w8.BottomAttach = ((uint)(5));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.ytable2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.yprogressLocal.Hide();
			this.yprogressTotal.Hide();
			this.Hide();
			this.ybuttonRun.Clicked += new global::System.EventHandler(this.OnYbuttonRunClicked);
		}
	}
}
