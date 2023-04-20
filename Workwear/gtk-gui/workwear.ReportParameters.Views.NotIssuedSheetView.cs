
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.ReportParameters.Views
{
	public partial class NotIssuedSheetView
	{
		private global::Gtk.VBox dialog1_VBox;

		private global::Gtk.Table table1;

		private global::Gamma.Widgets.yEnumComboBox comboIssueType;

		private global::QS.Widgets.GtkUI.DatePicker dateExcludeBefore;

		private global::QS.Views.Control.EntityEntry entitySubdivision;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gamma.GtkWidgets.yLabel labelIssueType;

		private global::Gamma.GtkWidgets.yCheckButton ycheckExcludeInVacation;

		private global::Gamma.Widgets.yDatePicker ydateReport;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

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
			this.table1 = new global::Gtk.Table(((uint)(6)), ((uint)(2)), false);
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
			w1.TopAttach = ((uint)(3));
			w1.BottomAttach = ((uint)(4));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.dateExcludeBefore = new global::QS.Widgets.GtkUI.DatePicker();
			this.dateExcludeBefore.Events = ((global::Gdk.EventMask)(256));
			this.dateExcludeBefore.Name = "dateExcludeBefore";
			this.dateExcludeBefore.WithTime = false;
			this.dateExcludeBefore.HideCalendarButton = false;
			this.dateExcludeBefore.Date = new global::System.DateTime(0);
			this.dateExcludeBefore.IsEditable = true;
			this.dateExcludeBefore.AutoSeparation = true;
			this.table1.Add(this.dateExcludeBefore);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.dateExcludeBefore]));
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
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Дата отчета:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.WidthRequest = 400;
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("<i>Обратите внимание:</i> Отчет показывает недополученное по текущим потребностям" +
					". То есть при значительном удалении даты отчет от сегодняшнего дня, данные будут" +
					" некорректны.");
			this.label2.UseMarkup = true;
			this.label2.Wrap = true;
			this.label2.Justify = ((global::Gtk.Justification)(3));
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w5.TopAttach = ((uint)(5));
			w5.BottomAttach = ((uint)(6));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение:");
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelIssueType = new global::Gamma.GtkWidgets.yLabel();
			this.labelIssueType.Name = "labelIssueType";
			this.labelIssueType.Xalign = 1F;
			this.labelIssueType.LabelProp = global::Mono.Unix.Catalog.GetString("Тип выдачи:");
			this.table1.Add(this.labelIssueType);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.labelIssueType]));
			w7.TopAttach = ((uint)(3));
			w7.BottomAttach = ((uint)(4));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckExcludeInVacation = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckExcludeInVacation.CanFocus = true;
			this.ycheckExcludeInVacation.Name = "ycheckExcludeInVacation";
			this.ycheckExcludeInVacation.Label = "";
			this.ycheckExcludeInVacation.Active = true;
			this.ycheckExcludeInVacation.DrawIndicator = true;
			this.ycheckExcludeInVacation.UseUnderline = true;
			this.table1.Add(this.ycheckExcludeInVacation);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckExcludeInVacation]));
			w8.TopAttach = ((uint)(4));
			w8.BottomAttach = ((uint)(5));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ydateReport = new global::Gamma.Widgets.yDatePicker();
			this.ydateReport.Events = ((global::Gdk.EventMask)(256));
			this.ydateReport.Name = "ydateReport";
			this.ydateReport.WithTime = false;
			this.ydateReport.Date = new global::System.DateTime(0);
			this.ydateReport.IsEditable = true;
			this.ydateReport.AutoSeparation = true;
			this.table1.Add(this.ydateReport);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.ydateReport]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(2));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Исключать сотрудников в отпуске:");
			this.table1.Add(this.ylabel2);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel2]));
			w10.TopAttach = ((uint)(4));
			w10.BottomAttach = ((uint)(5));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 1F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Исключить невыданное до:");
			this.table1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel3]));
			w11.TopAttach = ((uint)(2));
			w11.BottomAttach = ((uint)(3));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			this.dialog1_VBox.Add(this.table1);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.table1]));
			w12.Position = 0;
			w12.Expand = false;
			w12.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.buttonRun = new global::Gamma.GtkWidgets.yButton();
			this.buttonRun.Sensitive = false;
			this.buttonRun.CanFocus = true;
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.UseUnderline = true;
			this.buttonRun.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.dialog1_VBox.Add(this.buttonRun);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox[this.buttonRun]));
			w13.PackType = ((global::Gtk.PackType)(1));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
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
