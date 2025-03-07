
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.ReportParameters.Views
{
	public partial class AmountIssuedWearView
	{
		private global::Gtk.VBox vbox2;

		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::Gamma.Widgets.yEnumComboBox comboIssueType;

		private global::Gamma.Widgets.yEnumComboBox comboReportType;

		private global::Gtk.Label label1;

		private global::Gamma.GtkWidgets.yLabel labelIssueType;

		private global::Gamma.Widgets.yDatePeriodPicker ydateperiodpicker;

		private global::Gamma.GtkWidgets.yEntry yentryMatch;

		private global::Gamma.GtkWidgets.yEntry yentryNoMatch;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

		private global::Gamma.GtkWidgets.yLabel ylabel4;

		private global::Gamma.GtkWidgets.yLabel ylabelOwners;

		private global::Gamma.Widgets.ySpecComboBox yspeccomboboxOwners;

		private global::Gamma.GtkWidgets.yCheckButton checkByOperation;

		private global::Gamma.GtkWidgets.yCheckButton checkBySubdivision;

		private global::Gamma.GtkWidgets.yCheckButton checkByEmployee;

		private global::Gamma.GtkWidgets.yCheckButton checkBySize;

		private global::Gamma.GtkWidgets.yCheckButton checkUseAlterName;

		private global::Gamma.GtkWidgets.yCheckButton checkShowCost;

		private global::Gamma.GtkWidgets.yCheckButton checkShowCostCenter;

		private global::Gamma.GtkWidgets.yCheckButton checkShowOnlyWithoutNorm;

		private global::Gamma.GtkWidgets.yCheckButton ycheckChild;

		private global::Gtk.Expander expander1;

		private global::Workwear.ReportParameters.Views.ChoiceSubdivisionView choicesubdivisionview1;

		private global::Gtk.Label GtkLabel12;

		private global::Gtk.Expander expander2;

		private global::Workwear.ReportParameters.Views.ChoiceEmployeeGroupView choiceemployeegroupview3;

		private global::Gtk.Label GtkLabel14;

		private global::Gamma.GtkWidgets.yButton buttonPrintReport;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.ReportParameters.Views.AmountIssuedWearView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.ReportParameters.Views.AmountIssuedWearView";
			// Container child workwear.ReportParameters.Views.AmountIssuedWearView.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(6));
			this.ytable1.NColumns = ((uint)(2));
			this.ytable1.RowSpacing = ((uint)(6));
			this.ytable1.ColumnSpacing = ((uint)(6));
			// Container child ytable1.Gtk.Table+TableChild
			this.comboIssueType = new global::Gamma.Widgets.yEnumComboBox();
			this.comboIssueType.Name = "comboIssueType";
			this.comboIssueType.ShowSpecialStateAll = true;
			this.comboIssueType.ShowSpecialStateNot = false;
			this.comboIssueType.UseShortTitle = false;
			this.comboIssueType.DefaultFirst = false;
			this.ytable1.Add(this.comboIssueType);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.ytable1[this.comboIssueType]));
			w1.TopAttach = ((uint)(2));
			w1.BottomAttach = ((uint)(3));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.comboReportType = new global::Gamma.Widgets.yEnumComboBox();
			this.comboReportType.Name = "comboReportType";
			this.comboReportType.ShowSpecialStateAll = false;
			this.comboReportType.ShowSpecialStateNot = false;
			this.comboReportType.UseShortTitle = false;
			this.comboReportType.DefaultFirst = true;
			this.ytable1.Add(this.comboReportType);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytable1[this.comboReportType]));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Период:");
			this.ytable1.Add(this.label1);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.ytable1[this.label1]));
			w3.TopAttach = ((uint)(1));
			w3.BottomAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.labelIssueType = new global::Gamma.GtkWidgets.yLabel();
			this.labelIssueType.Name = "labelIssueType";
			this.labelIssueType.Xalign = 1F;
			this.labelIssueType.LabelProp = global::Mono.Unix.Catalog.GetString("Тип выдачи:");
			this.ytable1.Add(this.labelIssueType);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.ytable1[this.labelIssueType]));
			w4.TopAttach = ((uint)(2));
			w4.BottomAttach = ((uint)(3));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ydateperiodpicker = new global::Gamma.Widgets.yDatePeriodPicker();
			this.ydateperiodpicker.Events = ((global::Gdk.EventMask)(256));
			this.ydateperiodpicker.Name = "ydateperiodpicker";
			this.ydateperiodpicker.StartDate = new global::System.DateTime(0);
			this.ydateperiodpicker.EndDate = new global::System.DateTime(0);
			this.ytable1.Add(this.ydateperiodpicker);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ydateperiodpicker]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yentryMatch = new global::Gamma.GtkWidgets.yEntry();
			this.yentryMatch.CanFocus = true;
			this.yentryMatch.Name = "yentryMatch";
			this.yentryMatch.IsEditable = true;
			this.yentryMatch.InvisibleChar = '•';
			this.ytable1.Add(this.yentryMatch);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yentryMatch]));
			w6.TopAttach = ((uint)(4));
			w6.BottomAttach = ((uint)(5));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yentryNoMatch = new global::Gamma.GtkWidgets.yEntry();
			this.yentryNoMatch.CanFocus = true;
			this.yentryNoMatch.Name = "yentryNoMatch";
			this.yentryNoMatch.IsEditable = true;
			this.yentryNoMatch.InvisibleChar = '•';
			this.ytable1.Add(this.yentryNoMatch);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yentryNoMatch]));
			w7.TopAttach = ((uint)(5));
			w7.BottomAttach = ((uint)(6));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Наименование содержит:");
			this.ytable1.Add(this.ylabel2);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel2]));
			w8.TopAttach = ((uint)(4));
			w8.BottomAttach = ((uint)(5));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 1F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Наименование не содержит:");
			this.ytable1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel3]));
			w9.TopAttach = ((uint)(5));
			w9.BottomAttach = ((uint)(6));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel4 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel4.Name = "ylabel4";
			this.ylabel4.Xalign = 1F;
			this.ylabel4.LabelProp = global::Mono.Unix.Catalog.GetString("Вид отчета:");
			this.ytable1.Add(this.ylabel4);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel4]));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabelOwners = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelOwners.Name = "ylabelOwners";
			this.ylabelOwners.Xalign = 1F;
			this.ylabelOwners.LabelProp = global::Mono.Unix.Catalog.GetString("Собственники:");
			this.ytable1.Add(this.ylabelOwners);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabelOwners]));
			w11.TopAttach = ((uint)(3));
			w11.BottomAttach = ((uint)(4));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yspeccomboboxOwners = new global::Gamma.Widgets.ySpecComboBox();
			this.yspeccomboboxOwners.Name = "yspeccomboboxOwners";
			this.yspeccomboboxOwners.AddIfNotExist = false;
			this.yspeccomboboxOwners.DefaultFirst = false;
			this.yspeccomboboxOwners.ShowSpecialStateAll = true;
			this.yspeccomboboxOwners.ShowSpecialStateNot = true;
			this.yspeccomboboxOwners.NameForSpecialStateNot = "Без собственника";
			this.ytable1.Add(this.yspeccomboboxOwners);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yspeccomboboxOwners]));
			w12.TopAttach = ((uint)(3));
			w12.BottomAttach = ((uint)(4));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add(this.ytable1);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.ytable1]));
			w13.Position = 0;
			w13.Expand = false;
			w13.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.checkByOperation = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkByOperation.CanFocus = true;
			this.checkByOperation.Name = "checkByOperation";
			this.checkByOperation.Label = global::Mono.Unix.Catalog.GetString("Детализировать по операциям");
			this.checkByOperation.DrawIndicator = true;
			this.checkByOperation.UseUnderline = true;
			this.vbox2.Add(this.checkByOperation);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.checkByOperation]));
			w14.Position = 1;
			w14.Expand = false;
			w14.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.checkBySubdivision = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkBySubdivision.CanFocus = true;
			this.checkBySubdivision.Name = "checkBySubdivision";
			this.checkBySubdivision.Label = global::Mono.Unix.Catalog.GetString("Детализировать по подразделениям");
			this.checkBySubdivision.DrawIndicator = true;
			this.checkBySubdivision.UseUnderline = true;
			this.vbox2.Add(this.checkBySubdivision);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.checkBySubdivision]));
			w15.Position = 2;
			w15.Expand = false;
			w15.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.checkByEmployee = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkByEmployee.CanFocus = true;
			this.checkByEmployee.Name = "checkByEmployee";
			this.checkByEmployee.Label = global::Mono.Unix.Catalog.GetString("Детализировать по сотрудникам");
			this.checkByEmployee.DrawIndicator = true;
			this.checkByEmployee.UseUnderline = true;
			this.vbox2.Add(this.checkByEmployee);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.checkByEmployee]));
			w16.Position = 3;
			w16.Expand = false;
			w16.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.checkBySize = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkBySize.TooltipMarkup = "Отображать выдачи разных размеров разными строками.";
			this.checkBySize.CanFocus = true;
			this.checkBySize.Name = "checkBySize";
			this.checkBySize.Label = global::Mono.Unix.Catalog.GetString("Детализировать по размерам");
			this.checkBySize.DrawIndicator = true;
			this.checkBySize.UseUnderline = true;
			this.vbox2.Add(this.checkBySize);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.checkBySize]));
			w17.Position = 4;
			w17.Expand = false;
			w17.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.checkUseAlterName = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkUseAlterName.TooltipMarkup = "При наличии отображать альтернативное наименование размеров";
			this.checkUseAlterName.CanFocus = true;
			this.checkUseAlterName.Name = "checkUseAlterName";
			this.checkUseAlterName.Label = global::Mono.Unix.Catalog.GetString("Применить альтернативные размеры");
			this.checkUseAlterName.DrawIndicator = true;
			this.checkUseAlterName.UseUnderline = true;
			this.vbox2.Add(this.checkUseAlterName);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.checkUseAlterName]));
			w18.Position = 5;
			w18.Expand = false;
			w18.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.checkShowCost = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkShowCost.TooltipMarkup = "В отчете показывать столбец с оценочной стоимостью.";
			this.checkShowCost.CanFocus = true;
			this.checkShowCost.Name = "checkShowCost";
			this.checkShowCost.Label = global::Mono.Unix.Catalog.GetString("Показывать стоимость");
			this.checkShowCost.DrawIndicator = true;
			this.checkShowCost.UseUnderline = true;
			this.vbox2.Add(this.checkShowCost);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.checkShowCost]));
			w19.Position = 6;
			w19.Expand = false;
			w19.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.checkShowCostCenter = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkShowCostCenter.TooltipMarkup = "Добавить в отчет группировку по месту возникновения затрат";
			this.checkShowCostCenter.CanFocus = true;
			this.checkShowCostCenter.Name = "checkShowCostCenter";
			this.checkShowCostCenter.Label = global::Mono.Unix.Catalog.GetString("Группировать по МВЗ");
			this.checkShowCostCenter.DrawIndicator = true;
			this.checkShowCostCenter.UseUnderline = true;
			this.vbox2.Add(this.checkShowCostCenter);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.checkShowCostCenter]));
			w20.Position = 7;
			w20.Expand = false;
			w20.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.checkShowOnlyWithoutNorm = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkShowOnlyWithoutNorm.TooltipMarkup = "Отобразить в отчёте только выданное в ручную, например сверех нормы";
			this.checkShowOnlyWithoutNorm.CanFocus = true;
			this.checkShowOnlyWithoutNorm.Name = "checkShowOnlyWithoutNorm";
			this.checkShowOnlyWithoutNorm.Label = global::Mono.Unix.Catalog.GetString("Только выданное без нормы");
			this.checkShowOnlyWithoutNorm.DrawIndicator = true;
			this.checkShowOnlyWithoutNorm.UseUnderline = true;
			this.vbox2.Add(this.checkShowOnlyWithoutNorm);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.checkShowOnlyWithoutNorm]));
			w21.Position = 8;
			w21.Expand = false;
			w21.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.ycheckChild = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckChild.TooltipMarkup = "Включать в отчет так же дочерние подразделения выбранных подразделений.";
			this.ycheckChild.CanFocus = true;
			this.ycheckChild.Name = "ycheckChild";
			this.ycheckChild.Label = global::Mono.Unix.Catalog.GetString("Включая дочерниее подразделения");
			this.ycheckChild.DrawIndicator = true;
			this.ycheckChild.UseUnderline = true;
			this.vbox2.Add(this.ycheckChild);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.ycheckChild]));
			w22.Position = 9;
			w22.Expand = false;
			w22.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.expander1 = new global::Gtk.Expander(null);
			this.expander1.CanFocus = true;
			this.expander1.Name = "expander1";
			this.expander1.Expanded = true;
			// Container child expander1.Gtk.Container+ContainerChild
			this.choicesubdivisionview1 = new global::Workwear.ReportParameters.Views.ChoiceSubdivisionView();
			this.choicesubdivisionview1.Events = ((global::Gdk.EventMask)(256));
			this.choicesubdivisionview1.Name = "choicesubdivisionview1";
			this.expander1.Add(this.choicesubdivisionview1);
			this.GtkLabel12 = new global::Gtk.Label();
			this.GtkLabel12.Name = "GtkLabel12";
			this.GtkLabel12.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение:");
			this.GtkLabel12.UseUnderline = true;
			this.expander1.LabelWidget = this.GtkLabel12;
			this.vbox2.Add(this.expander1);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.expander1]));
			w24.Position = 10;
			// Container child vbox2.Gtk.Box+BoxChild
			this.expander2 = new global::Gtk.Expander(null);
			this.expander2.CanFocus = true;
			this.expander2.Name = "expander2";
			// Container child expander2.Gtk.Container+ContainerChild
			this.choiceemployeegroupview3 = new global::Workwear.ReportParameters.Views.ChoiceEmployeeGroupView();
			this.choiceemployeegroupview3.Events = ((global::Gdk.EventMask)(256));
			this.choiceemployeegroupview3.Name = "choiceemployeegroupview3";
			this.expander2.Add(this.choiceemployeegroupview3);
			this.GtkLabel14 = new global::Gtk.Label();
			this.GtkLabel14.Name = "GtkLabel14";
			this.GtkLabel14.LabelProp = global::Mono.Unix.Catalog.GetString("Группы сотрудников");
			this.GtkLabel14.UseUnderline = true;
			this.expander2.LabelWidget = this.GtkLabel14;
			this.vbox2.Add(this.expander2);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.expander2]));
			w26.Position = 11;
			w26.Expand = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.buttonPrintReport = new global::Gamma.GtkWidgets.yButton();
			this.buttonPrintReport.Sensitive = false;
			this.buttonPrintReport.CanFocus = true;
			this.buttonPrintReport.Name = "buttonPrintReport";
			this.buttonPrintReport.UseUnderline = true;
			this.buttonPrintReport.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vbox2.Add(this.buttonPrintReport);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.buttonPrintReport]));
			w27.PackType = ((global::Gtk.PackType)(1));
			w27.Position = 12;
			w27.Expand = false;
			w27.Fill = false;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.expander1.Activated += new global::System.EventHandler(this.OnExpander1Activated);
			this.expander2.Activated += new global::System.EventHandler(this.OnExpander2Activated);
			this.buttonPrintReport.Clicked += new global::System.EventHandler(this.OnButtonPrintReportClicked);
		}
	}
}
