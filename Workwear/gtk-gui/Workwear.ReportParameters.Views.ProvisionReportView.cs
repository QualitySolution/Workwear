
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.ReportParameters.Views
{
	public partial class ProvisionReportView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.Table table1;

		private global::Gamma.Widgets.yEnumComboBox comboReportType;

		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gtk.Label label5;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonExcludeInVacation;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonGroupBySubdivision;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonShowSex;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonShowSize;

		private global::Gamma.GtkWidgets.yCheckButton ycheckShowEmployees;

		private global::Gamma.GtkWidgets.yCheckButton ycheckShowStock;

		private global::Gamma.GtkWidgets.yLabel ylabel5;

		private global::Gtk.Label ylabelGroupBySubdivision;

		private global::Gamma.GtkWidgets.yLabel ylabelShowEmployees;

		private global::Gamma.GtkWidgets.yLabel ylabelShowStock;

		private global::Gtk.Expander expander1;

		private global::Workwear.ReportParameters.Views.ChoiceProtectionToolsView choiceprotectiontoolsview1;

		private global::Gtk.Label GtkLabel7;

		private global::Gtk.Expander expander2;

		private global::Workwear.ReportParameters.Views.ChoiceSubdivisionView choicesubdivisionview1;

		private global::Gtk.Label GtkLabel10;

		private global::Gamma.GtkWidgets.yButton ybuttonRun;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.ReportParameters.Views.ProvisionReportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.ReportParameters.Views.ProvisionReportView";
			// Container child Workwear.ReportParameters.Views.ProvisionReportView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(7)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.comboReportType = new global::Gamma.Widgets.yEnumComboBox();
			this.comboReportType.Name = "comboReportType";
			this.comboReportType.ShowSpecialStateAll = false;
			this.comboReportType.ShowSpecialStateNot = false;
			this.comboReportType.UseShortTitle = false;
			this.comboReportType.DefaultFirst = true;
			this.table1.Add(this.comboReportType);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.comboReportType]));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Детализировать по полу");
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w2.TopAttach = ((uint)(2));
			w2.BottomAttach = ((uint)(3));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Детализировать по размерам");
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w3.TopAttach = ((uint)(3));
			w3.BottomAttach = ((uint)(4));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Исключить сотрудников в отпуске");
			this.table1.Add(this.label5);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.label5]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckbuttonExcludeInVacation = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonExcludeInVacation.CanFocus = true;
			this.ycheckbuttonExcludeInVacation.Name = "ycheckbuttonExcludeInVacation";
			this.ycheckbuttonExcludeInVacation.Label = "";
			this.ycheckbuttonExcludeInVacation.DrawIndicator = true;
			this.ycheckbuttonExcludeInVacation.UseUnderline = true;
			this.table1.Add(this.ycheckbuttonExcludeInVacation);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckbuttonExcludeInVacation]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckbuttonGroupBySubdivision = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonGroupBySubdivision.CanFocus = true;
			this.ycheckbuttonGroupBySubdivision.Name = "ycheckbuttonGroupBySubdivision";
			this.ycheckbuttonGroupBySubdivision.Label = "";
			this.ycheckbuttonGroupBySubdivision.DrawIndicator = true;
			this.ycheckbuttonGroupBySubdivision.UseUnderline = true;
			this.table1.Add(this.ycheckbuttonGroupBySubdivision);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckbuttonGroupBySubdivision]));
			w6.TopAttach = ((uint)(4));
			w6.BottomAttach = ((uint)(5));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckbuttonShowSex = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonShowSex.CanFocus = true;
			this.ycheckbuttonShowSex.Name = "ycheckbuttonShowSex";
			this.ycheckbuttonShowSex.Label = "";
			this.ycheckbuttonShowSex.DrawIndicator = true;
			this.ycheckbuttonShowSex.UseUnderline = true;
			this.table1.Add(this.ycheckbuttonShowSex);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckbuttonShowSex]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckbuttonShowSize = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonShowSize.CanFocus = true;
			this.ycheckbuttonShowSize.Name = "ycheckbuttonShowSize";
			this.ycheckbuttonShowSize.Label = "";
			this.ycheckbuttonShowSize.DrawIndicator = true;
			this.ycheckbuttonShowSize.UseUnderline = true;
			this.table1.Add(this.ycheckbuttonShowSize);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckbuttonShowSize]));
			w8.TopAttach = ((uint)(3));
			w8.BottomAttach = ((uint)(4));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckShowEmployees = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckShowEmployees.CanFocus = true;
			this.ycheckShowEmployees.Name = "ycheckShowEmployees";
			this.ycheckShowEmployees.Label = "";
			this.ycheckShowEmployees.DrawIndicator = true;
			this.ycheckShowEmployees.UseUnderline = true;
			this.table1.Add(this.ycheckShowEmployees);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckShowEmployees]));
			w9.TopAttach = ((uint)(6));
			w9.BottomAttach = ((uint)(7));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckShowStock = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckShowStock.CanFocus = true;
			this.ycheckShowStock.Name = "ycheckShowStock";
			this.ycheckShowStock.Label = "";
			this.ycheckShowStock.DrawIndicator = true;
			this.ycheckShowStock.UseUnderline = true;
			this.table1.Add(this.ycheckShowStock);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckShowStock]));
			w10.TopAttach = ((uint)(5));
			w10.BottomAttach = ((uint)(6));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel5 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel5.Name = "ylabel5";
			this.ylabel5.Xalign = 1F;
			this.ylabel5.LabelProp = global::Mono.Unix.Catalog.GetString("Вид отчета");
			this.table1.Add(this.ylabel5);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel5]));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelGroupBySubdivision = new global::Gtk.Label();
			this.ylabelGroupBySubdivision.Name = "ylabelGroupBySubdivision";
			this.ylabelGroupBySubdivision.Xalign = 1F;
			this.ylabelGroupBySubdivision.LabelProp = global::Mono.Unix.Catalog.GetString("Группировать по подразделениям");
			this.table1.Add(this.ylabelGroupBySubdivision);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelGroupBySubdivision]));
			w12.TopAttach = ((uint)(4));
			w12.BottomAttach = ((uint)(5));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelShowEmployees = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelShowEmployees.Name = "ylabelShowEmployees";
			this.ylabelShowEmployees.Xalign = 1F;
			this.ylabelShowEmployees.LabelProp = global::Mono.Unix.Catalog.GetString("Показывать списки сотрудников");
			this.table1.Add(this.ylabelShowEmployees);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelShowEmployees]));
			w13.TopAttach = ((uint)(6));
			w13.BottomAttach = ((uint)(7));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelShowStock = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelShowStock.Name = "ylabelShowStock";
			this.ylabelShowStock.Xalign = 1F;
			this.ylabelShowStock.LabelProp = global::Mono.Unix.Catalog.GetString("Показывать количество на складе");
			this.table1.Add(this.ylabelShowStock);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelShowStock]));
			w14.TopAttach = ((uint)(5));
			w14.BottomAttach = ((uint)(6));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox1.Add(this.table1);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.table1]));
			w15.Position = 0;
			w15.Expand = false;
			w15.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.expander1 = new global::Gtk.Expander(null);
			this.expander1.CanFocus = true;
			this.expander1.Name = "expander1";
			this.expander1.Expanded = true;
			// Container child expander1.Gtk.Container+ContainerChild
			this.choiceprotectiontoolsview1 = new global::Workwear.ReportParameters.Views.ChoiceProtectionToolsView();
			this.choiceprotectiontoolsview1.Events = ((global::Gdk.EventMask)(256));
			this.choiceprotectiontoolsview1.Name = "choiceprotectiontoolsview1";
			this.expander1.Add(this.choiceprotectiontoolsview1);
			this.GtkLabel7 = new global::Gtk.Label();
			this.GtkLabel7.Name = "GtkLabel7";
			this.GtkLabel7.LabelProp = global::Mono.Unix.Catalog.GetString("Номенклатура нормы:");
			this.GtkLabel7.UseUnderline = true;
			this.expander1.LabelWidget = this.GtkLabel7;
			this.vbox1.Add(this.expander1);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.expander1]));
			w17.Position = 1;
			// Container child vbox1.Gtk.Box+BoxChild
			this.expander2 = new global::Gtk.Expander(null);
			this.expander2.CanFocus = true;
			this.expander2.Name = "expander2";
			this.expander2.Expanded = true;
			// Container child expander2.Gtk.Container+ContainerChild
			this.choicesubdivisionview1 = new global::Workwear.ReportParameters.Views.ChoiceSubdivisionView();
			this.choicesubdivisionview1.Events = ((global::Gdk.EventMask)(256));
			this.choicesubdivisionview1.Name = "choicesubdivisionview1";
			this.expander2.Add(this.choicesubdivisionview1);
			this.GtkLabel10 = new global::Gtk.Label();
			this.GtkLabel10.Name = "GtkLabel10";
			this.GtkLabel10.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение:");
			this.GtkLabel10.UseUnderline = true;
			this.expander2.LabelWidget = this.GtkLabel10;
			this.vbox1.Add(this.expander2);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.expander2]));
			w19.Position = 2;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ybuttonRun = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonRun.CanFocus = true;
			this.ybuttonRun.Name = "ybuttonRun";
			this.ybuttonRun.UseUnderline = true;
			this.ybuttonRun.Label = global::Mono.Unix.Catalog.GetString("Построить отчёт");
			this.vbox1.Add(this.ybuttonRun);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ybuttonRun]));
			w20.Position = 3;
			w20.Expand = false;
			w20.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.expander1.Activated += new global::System.EventHandler(this.OnExpander1Activated);
			this.expander2.Activated += new global::System.EventHandler(this.OnExpander2Activated);
			this.ybuttonRun.Clicked += new global::System.EventHandler(this.OnYbuttonRunClicked);
		}
	}
}
