
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.ReportParameters.Views
{
	public partial class RequestSheetView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.Table table1;

		private global::Gamma.GtkWidgets.yCheckButton checkShowSex;

		private global::QS.Views.Control.EntityEntry entityDepartment;

		private global::QS.Views.Control.EntityEntry entitySubdivision;

		private global::Gtk.HBox hbox1;

		private global::Gamma.Widgets.yListComboBox comboStartMonth;

		private global::Gamma.GtkWidgets.ySpinButton spinStartYear;

		private global::Gtk.HBox hbox3;

		private global::Gamma.Widgets.yListComboBox comboEndMonth;

		private global::Gamma.GtkWidgets.ySpinButton spinEndYear;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gamma.GtkWidgets.yLabel labelIssueType;

		private global::Gamma.GtkWidgets.yLabel labelPeriodType;

		private global::Gamma.GtkWidgets.yCheckButton ycheckChild;

		private global::Gamma.GtkWidgets.yCheckButton ycheckExcludeInVacation;

		private global::Gamma.Widgets.yEnumComboBox yenumIssueType;

		private global::Gtk.Expander expander1;

		private global::Workwear.ReportParameters.Views.ChoiceProtectionToolsView choiceprotectiontoolsview1;

		private global::Gtk.Label GtkLabel10;

		private global::Gamma.GtkWidgets.yButton buttonRun;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.ReportParameters.Views.RequestSheetView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.ReportParameters.Views.RequestSheetView";
			// Container child workwear.ReportParameters.Views.RequestSheetView.Gtk.Container+ContainerChild
			this.dialog1_VBox = new global::Gtk.VBox();
			this.dialog1_VBox.Name = "dialog1_VBox";
			this.dialog1_VBox.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(8)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.checkShowSex = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkShowSex.CanFocus = true;
			this.checkShowSex.Name = "checkShowSex";
			this.checkShowSex.Label = global::Mono.Unix.Catalog.GetString("Учитывать пол");
			this.checkShowSex.DrawIndicator = true;
			this.checkShowSex.UseUnderline = true;
			this.table1.Add(this.checkShowSex);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.checkShowSex]));
			w1.TopAttach = ((uint)(6));
			w1.BottomAttach = ((uint)(7));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entityDepartment = new global::QS.Views.Control.EntityEntry();
			this.entityDepartment.Events = ((global::Gdk.EventMask)(256));
			this.entityDepartment.Name = "entityDepartment";
			this.table1.Add(this.entityDepartment);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.entityDepartment]));
			w2.TopAttach = ((uint)(2));
			w2.BottomAttach = ((uint)(3));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entitySubdivision = new global::QS.Views.Control.EntityEntry();
			this.entitySubdivision.Events = ((global::Gdk.EventMask)(256));
			this.entitySubdivision.Name = "entitySubdivision";
			this.table1.Add(this.entitySubdivision);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.entitySubdivision]));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.comboStartMonth = new global::Gamma.Widgets.yListComboBox();
			this.comboStartMonth.Name = "comboStartMonth";
			this.comboStartMonth.AddIfNotExist = false;
			this.comboStartMonth.DefaultFirst = false;
			this.hbox1.Add(this.comboStartMonth);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.comboStartMonth]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.spinStartYear = new global::Gamma.GtkWidgets.ySpinButton(0D, 3000D, 1D);
			this.spinStartYear.CanFocus = true;
			this.spinStartYear.Name = "spinStartYear";
			this.spinStartYear.Adjustment.PageIncrement = 10D;
			this.spinStartYear.ClimbRate = 1D;
			this.spinStartYear.Numeric = true;
			this.spinStartYear.ValueAsDecimal = 0m;
			this.spinStartYear.ValueAsInt = 0;
			this.hbox1.Add(this.spinStartYear);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.spinStartYear]));
			w5.Position = 1;
			w5.Expand = false;
			w5.Fill = false;
			this.table1.Add(this.hbox1);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox1]));
			w6.TopAttach = ((uint)(3));
			w6.BottomAttach = ((uint)(4));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.comboEndMonth = new global::Gamma.Widgets.yListComboBox();
			this.comboEndMonth.Name = "comboEndMonth";
			this.comboEndMonth.AddIfNotExist = false;
			this.comboEndMonth.DefaultFirst = false;
			this.hbox3.Add(this.comboEndMonth);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.comboEndMonth]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child hbox3.Gtk.Box+BoxChild
			this.spinEndYear = new global::Gamma.GtkWidgets.ySpinButton(0D, 3000D, 1D);
			this.spinEndYear.CanFocus = true;
			this.spinEndYear.Name = "spinEndYear";
			this.spinEndYear.Adjustment.PageIncrement = 10D;
			this.spinEndYear.ClimbRate = 1D;
			this.spinEndYear.Numeric = true;
			this.spinEndYear.ValueAsDecimal = 0m;
			this.spinEndYear.ValueAsInt = 0;
			this.hbox3.Add(this.spinEndYear);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.spinEndYear]));
			w8.Position = 1;
			w8.Expand = false;
			w8.Fill = false;
			this.table1.Add(this.hbox3);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox3]));
			w9.TopAttach = ((uint)(4));
			w9.BottomAttach = ((uint)(5));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Период c:");
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w11.TopAttach = ((uint)(3));
			w11.BottomAttach = ((uint)(4));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Отдел:");
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w12.TopAttach = ((uint)(2));
			w12.BottomAttach = ((uint)(3));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelIssueType = new global::Gamma.GtkWidgets.yLabel();
			this.labelIssueType.Name = "labelIssueType";
			this.labelIssueType.Xalign = 1F;
			this.labelIssueType.LabelProp = global::Mono.Unix.Catalog.GetString("Тип выдачи:");
			this.table1.Add(this.labelIssueType);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.labelIssueType]));
			w13.TopAttach = ((uint)(5));
			w13.BottomAttach = ((uint)(6));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelPeriodType = new global::Gamma.GtkWidgets.yLabel();
			this.labelPeriodType.Name = "labelPeriodType";
			this.labelPeriodType.Xalign = 1F;
			this.labelPeriodType.LabelProp = global::Mono.Unix.Catalog.GetString("по:");
			this.table1.Add(this.labelPeriodType);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.labelPeriodType]));
			w14.TopAttach = ((uint)(4));
			w14.BottomAttach = ((uint)(5));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckChild = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckChild.CanFocus = true;
			this.ycheckChild.Name = "ycheckChild";
			this.ycheckChild.Label = global::Mono.Unix.Catalog.GetString("Учитывать дочерние");
			this.ycheckChild.DrawIndicator = true;
			this.ycheckChild.UseUnderline = true;
			this.table1.Add(this.ycheckChild);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckChild]));
			w15.TopAttach = ((uint)(1));
			w15.BottomAttach = ((uint)(2));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckExcludeInVacation = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckExcludeInVacation.CanFocus = true;
			this.ycheckExcludeInVacation.Name = "ycheckExcludeInVacation";
			this.ycheckExcludeInVacation.Label = global::Mono.Unix.Catalog.GetString("Исключать сотрудников в отпуске");
			this.ycheckExcludeInVacation.DrawIndicator = true;
			this.ycheckExcludeInVacation.UseUnderline = true;
			this.table1.Add(this.ycheckExcludeInVacation);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckExcludeInVacation]));
			w16.TopAttach = ((uint)(7));
			w16.BottomAttach = ((uint)(8));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yenumIssueType = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumIssueType.Name = "yenumIssueType";
			this.yenumIssueType.ShowSpecialStateAll = true;
			this.yenumIssueType.ShowSpecialStateNot = false;
			this.yenumIssueType.UseShortTitle = false;
			this.yenumIssueType.DefaultFirst = true;
			this.table1.Add(this.yenumIssueType);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table1[this.yenumIssueType]));
			w17.TopAttach = ((uint)(5));
			w17.BottomAttach = ((uint)(6));
			w17.LeftAttach = ((uint)(1));
			w17.RightAttach = ((uint)(2));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			this.dialog1_VBox.Add(this.table1);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.table1]));
			w18.Position = 0;
			w18.Expand = false;
			w18.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.expander1 = new global::Gtk.Expander(null);
			this.expander1.CanFocus = true;
			this.expander1.Name = "expander1";
			this.expander1.Expanded = true;
			// Container child expander1.Gtk.Container+ContainerChild
			this.choiceprotectiontoolsview1 = new global::Workwear.ReportParameters.Views.ChoiceProtectionToolsView();
			this.choiceprotectiontoolsview1.Events = ((global::Gdk.EventMask)(256));
			this.choiceprotectiontoolsview1.Name = "choiceprotectiontoolsview1";
			this.expander1.Add(this.choiceprotectiontoolsview1);
			this.GtkLabel10 = new global::Gtk.Label();
			this.GtkLabel10.Name = "GtkLabel10";
			this.GtkLabel10.LabelProp = global::Mono.Unix.Catalog.GetString("Номенклатура нормы");
			this.GtkLabel10.UseUnderline = true;
			this.expander1.LabelWidget = this.GtkLabel10;
			this.dialog1_VBox.Add(this.expander1);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.expander1]));
			w20.Position = 1;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.buttonRun = new global::Gamma.GtkWidgets.yButton();
			this.buttonRun.CanFocus = true;
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.UseUnderline = true;
			this.buttonRun.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.dialog1_VBox.Add(this.buttonRun);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.buttonRun]));
			w21.PackType = ((global::Gtk.PackType)(1));
			w21.Position = 3;
			w21.Expand = false;
			w21.Fill = false;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Show();
			this.buttonRun.Clicked += new global::System.EventHandler(this.OnButtonRunClicked);
		}
	}
}
