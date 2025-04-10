
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Export
{
	public partial class FutureIssueExportView
	{
		private global::Gamma.GtkWidgets.yVBox yvbox1;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::QS.Views.Control.EntityEntry entityentryOrganization;

		private global::Gtk.Label labelOrg;

		private global::Gtk.Label labelPeriod;

		private global::Gamma.Widgets.yDatePeriodPicker ydateperiodpicker;

		private global::Gamma.GtkWidgets.yTable ytable3;

		private global::Gamma.GtkWidgets.yCheckButton checkMoveDebt;

		private global::Gamma.Widgets.yEnumComboBox comboCost;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::QS.Views.Control.ChoiceListView choiceprotectiontoolsview1;

		private global::QS.Widgets.ProgressWidget yprogressLocal;

		private global::QS.Widgets.ProgressWidget yprogressTotal;

		private global::Gamma.GtkWidgets.yHBox yhbox2;

		private global::Gamma.GtkWidgets.yButton ybuttonRun;

		private global::Gamma.GtkWidgets.yLabel ylabel_done;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Export.FutureIssueExportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Export.FutureIssueExportView";
			// Container child Workwear.Views.Export.FutureIssueExportView.Gtk.Container+ContainerChild
			this.yvbox1 = new global::Gamma.GtkWidgets.yVBox();
			this.yvbox1.Name = "yvbox1";
			this.yvbox1.Spacing = 6;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(2));
			this.ytable1.NColumns = ((uint)(2));
			this.ytable1.RowSpacing = ((uint)(6));
			this.ytable1.ColumnSpacing = ((uint)(6));
			// Container child ytable1.Gtk.Table+TableChild
			this.entityentryOrganization = new global::QS.Views.Control.EntityEntry();
			this.entityentryOrganization.Events = ((global::Gdk.EventMask)(256));
			this.entityentryOrganization.Name = "entityentryOrganization";
			this.ytable1.Add(this.entityentryOrganization);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.ytable1[this.entityentryOrganization]));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.labelOrg = new global::Gtk.Label();
			this.labelOrg.Name = "labelOrg";
			this.labelOrg.Xalign = 1F;
			this.labelOrg.LabelProp = global::Mono.Unix.Catalog.GetString("Организация:");
			this.ytable1.Add(this.labelOrg);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytable1[this.labelOrg]));
			w2.XOptions = ((global::Gtk.AttachOptions)(0));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.labelPeriod = new global::Gtk.Label();
			this.labelPeriod.Name = "labelPeriod";
			this.labelPeriod.Xalign = 1F;
			this.labelPeriod.LabelProp = global::Mono.Unix.Catalog.GetString("Период:");
			this.ytable1.Add(this.labelPeriod);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.ytable1[this.labelPeriod]));
			w3.TopAttach = ((uint)(1));
			w3.BottomAttach = ((uint)(2));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ydateperiodpicker = new global::Gamma.Widgets.yDatePeriodPicker();
			this.ydateperiodpicker.Events = ((global::Gdk.EventMask)(256));
			this.ydateperiodpicker.Name = "ydateperiodpicker";
			this.ydateperiodpicker.StartDate = new global::System.DateTime(0);
			this.ydateperiodpicker.EndDate = new global::System.DateTime(0);
			this.ytable1.Add(this.ydateperiodpicker);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ydateperiodpicker]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			this.yhbox1.Add(this.ytable1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ytable1]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ytable3 = new global::Gamma.GtkWidgets.yTable();
			this.ytable3.Name = "ytable3";
			this.ytable3.NRows = ((uint)(2));
			this.ytable3.NColumns = ((uint)(2));
			this.ytable3.RowSpacing = ((uint)(6));
			this.ytable3.ColumnSpacing = ((uint)(6));
			// Container child ytable3.Gtk.Table+TableChild
			this.checkMoveDebt = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkMoveDebt.TooltipMarkup = "Рассчитываем так как будто все долги выданы в первый день периода. В этом случае " +
				"все последующие даты будут считаться для должников от этого дня.";
			this.checkMoveDebt.CanFocus = true;
			this.checkMoveDebt.Name = "checkMoveDebt";
			this.checkMoveDebt.Label = global::Mono.Unix.Catalog.GetString("Перенести долги на начало периода");
			this.checkMoveDebt.DrawIndicator = true;
			this.checkMoveDebt.UseUnderline = true;
			this.ytable3.Add(this.checkMoveDebt);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytable3[this.checkMoveDebt]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable3.Gtk.Table+TableChild
			this.comboCost = new global::Gamma.Widgets.yEnumComboBox();
			this.comboCost.Name = "comboCost";
			this.comboCost.ShowSpecialStateAll = false;
			this.comboCost.ShowSpecialStateNot = false;
			this.comboCost.UseShortTitle = false;
			this.comboCost.DefaultFirst = true;
			this.ytable3.Add(this.comboCost);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.ytable3[this.comboCost]));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable3.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.Xalign = 1F;
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Показывать стоимость:");
			this.ytable3.Add(this.ylabel1);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytable3[this.ylabel1]));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			this.yhbox1.Add(this.ytable3);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ytable3]));
			w9.PackType = ((global::Gtk.PackType)(1));
			w9.Position = 2;
			w9.Expand = false;
			w9.Fill = false;
			this.yvbox1.Add(this.yhbox1);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.yhbox1]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.choiceprotectiontoolsview1 = new global::QS.Views.Control.ChoiceListView();
			this.choiceprotectiontoolsview1.Events = ((global::Gdk.EventMask)(256));
			this.choiceprotectiontoolsview1.Name = "choiceprotectiontoolsview1";
			this.yvbox1.Add(this.choiceprotectiontoolsview1);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.choiceprotectiontoolsview1]));
			w11.Position = 1;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.yprogressLocal = new global::QS.Widgets.ProgressWidget();
			this.yprogressLocal.Name = "yprogressLocal";
			this.yvbox1.Add(this.yprogressLocal);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.yprogressLocal]));
			w12.Position = 2;
			w12.Expand = false;
			w12.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.yprogressTotal = new global::QS.Widgets.ProgressWidget();
			this.yprogressTotal.Name = "yprogressTotal";
			this.yvbox1.Add(this.yprogressTotal);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.yprogressTotal]));
			w13.Position = 3;
			w13.Expand = false;
			w13.Fill = false;
			// Container child yvbox1.Gtk.Box+BoxChild
			this.yhbox2 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox2.Name = "yhbox2";
			this.yhbox2.Spacing = 6;
			// Container child yhbox2.Gtk.Box+BoxChild
			this.ybuttonRun = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonRun.CanFocus = true;
			this.ybuttonRun.Name = "ybuttonRun";
			this.ybuttonRun.UseUnderline = true;
			this.ybuttonRun.Label = global::Mono.Unix.Catalog.GetString("Выгрузить");
			this.yhbox2.Add(this.ybuttonRun);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.yhbox2[this.ybuttonRun]));
			w14.Position = 0;
			w14.Expand = false;
			w14.Fill = false;
			// Container child yhbox2.Gtk.Box+BoxChild
			this.ylabel_done = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel_done.Name = "ylabel_done";
			this.ylabel_done.LabelProp = global::Mono.Unix.Catalog.GetString("Готово");
			this.yhbox2.Add(this.ylabel_done);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.yhbox2[this.ylabel_done]));
			w15.Position = 1;
			this.yvbox1.Add(this.yhbox2);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.yvbox1[this.yhbox2]));
			w16.Position = 4;
			w16.Expand = false;
			w16.Fill = false;
			this.Add(this.yvbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.yprogressLocal.Hide();
			this.yprogressTotal.Hide();
			this.ylabel_done.Hide();
			this.Hide();
			this.ybuttonRun.Clicked += new global::System.EventHandler(this.OnYbuttonRunClicked);
		}
	}
}
