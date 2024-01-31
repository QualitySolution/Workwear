
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.ReportParameters.Views
{
	public partial class NotIssuedSheetView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.Table table1;

		private global::Gamma.Widgets.yEnumComboBox comboIssueType;

		private global::Gamma.Widgets.yEnumComboBox comboReportType;

		private global::QS.Widgets.GtkUI.DatePicker dateExcludeBefore;

		private global::QS.Views.Control.EntityEntry entitySubdivision;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gamma.GtkWidgets.yLabel labelIssueType;

		private global::Gamma.GtkWidgets.yCheckButton ycheckCondition;

		private global::Gamma.GtkWidgets.yCheckButton ycheckDontShowZeroStock;

		private global::Gamma.GtkWidgets.yCheckButton ycheckExcludeInVacation;

		private global::Gamma.GtkWidgets.yCheckButton ycheckHideWorn;

		private global::Gamma.GtkWidgets.yCheckButton ycheckShowStock;

		private global::Gamma.Widgets.yDatePicker ydateReport;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

		private global::Gamma.GtkWidgets.yLabel ylabel4;

		private global::Gamma.GtkWidgets.yLabel ylabel5;

		private global::Gamma.GtkWidgets.yLabel ylabelcheckCondition;

		private global::Gamma.GtkWidgets.yLabel ylabelDontShowZeroStock;

		private global::Gamma.GtkWidgets.yLabel ylabelHideWorn;

		private global::Gtk.Expander expander1;

		private global::Workwear.ReportParameters.Views.ChoiceProtectionToolsView choiceprotectiontoolsview1;

		private global::Gtk.Label GtkLabel14;

		private global::Gtk.Expander expander2;

		private global::Workwear.ReportParameters.Views.ChoiceEmployeeGroupView choiceemployeegroupview2;

		private global::Gtk.Label GtkLabel15;

		private global::Gamma.GtkWidgets.yButton buttonRun;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.ReportParameters.Views.NotIssuedSheetView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.ReportParameters.Views.NotIssuedSheetView";
			// Container child workwear.ReportParameters.Views.NotIssuedSheetView.Gtk.Container+ContainerChild
			this.dialog1_VBox = new global::Gtk.VBox();
			this.dialog1_VBox.Name = "dialog1_VBox";
			this.dialog1_VBox.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(11)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.comboIssueType = new global::Gamma.Widgets.yEnumComboBox();
			this.comboIssueType.Name = "comboIssueType";
			this.comboIssueType.ShowSpecialStateAll = true;
			this.comboIssueType.ShowSpecialStateNot = false;
			this.comboIssueType.UseShortTitle = false;
			this.comboIssueType.DefaultFirst = false;
			this.table1.Add(this.comboIssueType);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.comboIssueType]));
			w1.TopAttach = ((uint)(5));
			w1.BottomAttach = ((uint)(6));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.comboReportType = new global::Gamma.Widgets.yEnumComboBox();
			this.comboReportType.Name = "comboReportType";
			this.comboReportType.ShowSpecialStateAll = false;
			this.comboReportType.ShowSpecialStateNot = false;
			this.comboReportType.UseShortTitle = false;
			this.comboReportType.DefaultFirst = true;
			this.table1.Add(this.comboReportType);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.comboReportType]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.dateExcludeBefore = new global::QS.Widgets.GtkUI.DatePicker();
			this.dateExcludeBefore.Events = ((global::Gdk.EventMask)(256));
			this.dateExcludeBefore.Name = "dateExcludeBefore";
			this.dateExcludeBefore.WithTime = false;
			this.dateExcludeBefore.HideCalendarButton = false;
			this.dateExcludeBefore.Date = new global::System.DateTime(0);
			this.dateExcludeBefore.IsEditable = true;
			this.dateExcludeBefore.AutoSeparation = true;
			this.dateExcludeBefore.HideButtonClearDate = false;
			this.table1.Add(this.dateExcludeBefore);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.dateExcludeBefore]));
			w3.TopAttach = ((uint)(4));
			w3.BottomAttach = ((uint)(5));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entitySubdivision = new global::QS.Views.Control.EntityEntry();
			this.entitySubdivision.Events = ((global::Gdk.EventMask)(256));
			this.entitySubdivision.Name = "entitySubdivision";
			this.table1.Add(this.entitySubdivision);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.entitySubdivision]));
			w4.TopAttach = ((uint)(2));
			w4.BottomAttach = ((uint)(3));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Дата отчета:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w5.TopAttach = ((uint)(3));
			w5.BottomAttach = ((uint)(4));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.WidthRequest = 400;
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("<i>Обратите внимание:</i> Отчет показывает недополученное по текущим потребностям" +
					". Иными словами, при значительном удалении даты отчета от сегодняшнего дня данны" +
					"е будут некорректны.");
			this.label2.UseMarkup = true;
			this.label2.Wrap = true;
			this.label2.Justify = ((global::Gtk.Justification)(3));
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение:");
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelIssueType = new global::Gamma.GtkWidgets.yLabel();
			this.labelIssueType.Name = "labelIssueType";
			this.labelIssueType.Xalign = 1F;
			this.labelIssueType.LabelProp = global::Mono.Unix.Catalog.GetString("Тип выдачи:");
			this.table1.Add(this.labelIssueType);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.labelIssueType]));
			w8.TopAttach = ((uint)(5));
			w8.BottomAttach = ((uint)(6));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckCondition = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckCondition.CanFocus = true;
			this.ycheckCondition.Name = "ycheckCondition";
			this.ycheckCondition.Label = "";
			this.ycheckCondition.Active = true;
			this.ycheckCondition.DrawIndicator = true;
			this.ycheckCondition.UseUnderline = true;
			this.table1.Add(this.ycheckCondition);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckCondition]));
			w9.TopAttach = ((uint)(7));
			w9.BottomAttach = ((uint)(8));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckDontShowZeroStock = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckDontShowZeroStock.CanFocus = true;
			this.ycheckDontShowZeroStock.Name = "ycheckDontShowZeroStock";
			this.ycheckDontShowZeroStock.Label = "";
			this.ycheckDontShowZeroStock.DrawIndicator = true;
			this.ycheckDontShowZeroStock.UseUnderline = true;
			this.ycheckDontShowZeroStock.FocusOnClick = false;
			this.ycheckDontShowZeroStock.Xalign = 0F;
			this.ycheckDontShowZeroStock.Yalign = 0F;
			this.table1.Add(this.ycheckDontShowZeroStock);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckDontShowZeroStock]));
			w10.TopAttach = ((uint)(9));
			w10.BottomAttach = ((uint)(10));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckExcludeInVacation = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckExcludeInVacation.CanFocus = true;
			this.ycheckExcludeInVacation.Name = "ycheckExcludeInVacation";
			this.ycheckExcludeInVacation.Label = "";
			this.ycheckExcludeInVacation.Active = true;
			this.ycheckExcludeInVacation.DrawIndicator = true;
			this.ycheckExcludeInVacation.UseUnderline = true;
			this.table1.Add(this.ycheckExcludeInVacation);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckExcludeInVacation]));
			w11.TopAttach = ((uint)(6));
			w11.BottomAttach = ((uint)(7));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckHideWorn = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckHideWorn.CanFocus = true;
			this.ycheckHideWorn.Name = "ycheckHideWorn";
			this.ycheckHideWorn.Label = "";
			this.ycheckHideWorn.DrawIndicator = true;
			this.ycheckHideWorn.UseUnderline = true;
			this.ycheckHideWorn.FocusOnClick = false;
			this.ycheckHideWorn.Xalign = 0F;
			this.ycheckHideWorn.Yalign = 0F;
			this.table1.Add(this.ycheckHideWorn);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckHideWorn]));
			w12.TopAttach = ((uint)(10));
			w12.BottomAttach = ((uint)(11));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckShowStock = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckShowStock.CanFocus = true;
			this.ycheckShowStock.Name = "ycheckShowStock";
			this.ycheckShowStock.Label = "";
			this.ycheckShowStock.DrawIndicator = true;
			this.ycheckShowStock.UseUnderline = true;
			this.table1.Add(this.ycheckShowStock);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckShowStock]));
			w13.TopAttach = ((uint)(8));
			w13.BottomAttach = ((uint)(9));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ydateReport = new global::Gamma.Widgets.yDatePicker();
			this.ydateReport.Events = ((global::Gdk.EventMask)(256));
			this.ydateReport.Name = "ydateReport";
			this.ydateReport.WithTime = false;
			this.ydateReport.Date = new global::System.DateTime(0);
			this.ydateReport.IsEditable = true;
			this.ydateReport.AutoSeparation = true;
			this.table1.Add(this.ydateReport);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.ydateReport]));
			w14.TopAttach = ((uint)(3));
			w14.BottomAttach = ((uint)(4));
			w14.LeftAttach = ((uint)(1));
			w14.RightAttach = ((uint)(2));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Исключать сотрудников в отпуске:");
			this.table1.Add(this.ylabel2);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel2]));
			w15.TopAttach = ((uint)(6));
			w15.BottomAttach = ((uint)(7));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 1F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Исключить невыданное до:");
			this.table1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel3]));
			w16.TopAttach = ((uint)(4));
			w16.BottomAttach = ((uint)(5));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel4 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel4.Name = "ylabel4";
			this.ylabel4.Xalign = 1F;
			this.ylabel4.LabelProp = global::Mono.Unix.Catalog.GetString("Показывать количество на складе:");
			this.table1.Add(this.ylabel4);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel4]));
			w17.TopAttach = ((uint)(8));
			w17.BottomAttach = ((uint)(9));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel5 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel5.Name = "ylabel5";
			this.ylabel5.Xalign = 1F;
			this.ylabel5.LabelProp = global::Mono.Unix.Catalog.GetString("Вид отчета:");
			this.table1.Add(this.ylabel5);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel5]));
			w18.TopAttach = ((uint)(1));
			w18.BottomAttach = ((uint)(2));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelcheckCondition = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelcheckCondition.Name = "ylabelcheckCondition";
			this.ylabelcheckCondition.Xalign = 1F;
			this.ylabelcheckCondition.LabelProp = global::Mono.Unix.Catalog.GetString("Учитывать период выдачи:");
			this.table1.Add(this.ylabelcheckCondition);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelcheckCondition]));
			w19.TopAttach = ((uint)(7));
			w19.BottomAttach = ((uint)(8));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelDontShowZeroStock = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelDontShowZeroStock.Name = "ylabelDontShowZeroStock";
			this.ylabelDontShowZeroStock.Xalign = 1F;
			this.ylabelDontShowZeroStock.LabelProp = global::Mono.Unix.Catalog.GetString("Не учитывать при отсутствии на складе:");
			this.table1.Add(this.ylabelDontShowZeroStock);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelDontShowZeroStock]));
			w20.TopAttach = ((uint)(9));
			w20.BottomAttach = ((uint)(10));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelHideWorn = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelHideWorn.Name = "ylabelHideWorn";
			this.ylabelHideWorn.Xalign = 1F;
			this.ylabelHideWorn.LabelProp = global::Mono.Unix.Catalog.GetString("Не учитывать позиции с износом:");
			this.table1.Add(this.ylabelHideWorn);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelHideWorn]));
			w21.TopAttach = ((uint)(10));
			w21.BottomAttach = ((uint)(11));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			this.dialog1_VBox.Add(this.table1);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.table1]));
			w22.Position = 0;
			w22.Expand = false;
			w22.Fill = false;
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
			this.GtkLabel14 = new global::Gtk.Label();
			this.GtkLabel14.Name = "GtkLabel14";
			this.GtkLabel14.LabelProp = global::Mono.Unix.Catalog.GetString("Номенклатуры нормы");
			this.GtkLabel14.UseUnderline = true;
			this.expander1.LabelWidget = this.GtkLabel14;
			this.dialog1_VBox.Add(this.expander1);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.expander1]));
			w24.Position = 1;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.expander2 = new global::Gtk.Expander(null);
			this.expander2.CanFocus = true;
			this.expander2.Name = "expander2";
			// Container child expander2.Gtk.Container+ContainerChild
			this.choiceemployeegroupview2 = new global::Workwear.ReportParameters.Views.ChoiceEmployeeGroupView();
			this.choiceemployeegroupview2.Events = ((global::Gdk.EventMask)(256));
			this.choiceemployeegroupview2.Name = "choiceemployeegroupview2";
			this.expander2.Add(this.choiceemployeegroupview2);
			this.GtkLabel15 = new global::Gtk.Label();
			this.GtkLabel15.Name = "GtkLabel15";
			this.GtkLabel15.LabelProp = global::Mono.Unix.Catalog.GetString("Группы сотрудников");
			this.GtkLabel15.UseUnderline = true;
			this.expander2.LabelWidget = this.GtkLabel15;
			this.dialog1_VBox.Add(this.expander2);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.expander2]));
			w26.Position = 2;
			w26.Expand = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.buttonRun = new global::Gamma.GtkWidgets.yButton();
			this.buttonRun.Sensitive = false;
			this.buttonRun.CanFocus = true;
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.UseUnderline = true;
			this.buttonRun.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.dialog1_VBox.Add(this.buttonRun);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.buttonRun]));
			w27.PackType = ((global::Gtk.PackType)(1));
			w27.Position = 3;
			w27.Expand = false;
			w27.Fill = false;
			this.Add(this.dialog1_VBox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Show();
			this.expander1.Activated += new global::System.EventHandler(this.OnExpander1Activated);
			this.expander2.Activated += new global::System.EventHandler(this.OnExpander2Activated);
			this.buttonRun.Clicked += new global::System.EventHandler(this.OnButtonRunClicked);
		}
	}
}
