
// This file has been generated by the GUI designer. Do not modify.
namespace workwear.Journal.Filter.Views.Communications
{
	public partial class EmployeeNotificationFilterView
	{
		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yTable ytable1;

		private global::Gamma.GtkWidgets.yCheckButton checkLkEnabled;

		private global::Gamma.GtkWidgets.yCheckButton checkShowOnlyWork;

		private global::Gamma.Widgets.yDatePeriodPicker datePeriodBirth;

		private global::Gamma.Widgets.yDatePeriodPicker datePeriodIssue;

		private global::QS.Views.Control.EntityEntry entitySubdivision;

		private global::Gamma.GtkWidgets.yCheckButton ycheckBirthday;

		private global::Gamma.GtkWidgets.yCheckButton ycheckOffPeriod;

		private global::Gamma.GtkWidgets.yCheckButton ycheckShowOverdue;

		private global::Gamma.GtkWidgets.yCheckButton ycheckStockAvailability;

		private global::Gamma.Widgets.yListComboBox ycomboListWarehouses;

		private global::Gamma.GtkWidgets.yHBox yhbox2;

		private global::Gamma.GtkWidgets.yHBox yhbox3;

		private global::Gamma.GtkWidgets.yLabel ylabelSex;

		private global::Gamma.Widgets.yEnumComboBox yenumcomboboxSex;

		private global::Gamma.Widgets.yEnumComboBox yIssueType;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

		private global::Gamma.GtkWidgets.yLabel ylabel4;

		private global::Gamma.GtkWidgets.yLabel ylabel5;

		private global::Workwear.ReportParameters.Views.ChoiceProtectionToolsView choiceProtectionToolsView;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget workwear.Journal.Filter.Views.Communications.EmployeeNotificationFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "workwear.Journal.Filter.Views.Communications.EmployeeNotificationFilterView";
			// Container child workwear.Journal.Filter.Views.Communications.EmployeeNotificationFilterView.Gtk.Container+ContainerChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.ytable1 = new global::Gamma.GtkWidgets.yTable();
			this.ytable1.Name = "ytable1";
			this.ytable1.NRows = ((uint)(5));
			this.ytable1.NColumns = ((uint)(4));
			this.ytable1.RowSpacing = ((uint)(6));
			this.ytable1.ColumnSpacing = ((uint)(6));
			// Container child ytable1.Gtk.Table+TableChild
			this.checkLkEnabled = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkLkEnabled.CanFocus = true;
			this.checkLkEnabled.Name = "checkLkEnabled";
			this.checkLkEnabled.Label = global::Mono.Unix.Catalog.GetString("Только с мобильным кабинетом");
			this.checkLkEnabled.Active = true;
			this.checkLkEnabled.DrawIndicator = true;
			this.checkLkEnabled.UseUnderline = true;
			this.ytable1.Add(this.checkLkEnabled);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.ytable1[this.checkLkEnabled]));
			w1.TopAttach = ((uint)(2));
			w1.BottomAttach = ((uint)(3));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.checkShowOnlyWork = new global::Gamma.GtkWidgets.yCheckButton();
			this.checkShowOnlyWork.CanFocus = true;
			this.checkShowOnlyWork.Name = "checkShowOnlyWork";
			this.checkShowOnlyWork.Label = global::Mono.Unix.Catalog.GetString("Только работающие");
			this.checkShowOnlyWork.Active = true;
			this.checkShowOnlyWork.DrawIndicator = true;
			this.checkShowOnlyWork.UseUnderline = true;
			this.ytable1.Add(this.checkShowOnlyWork);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytable1[this.checkShowOnlyWork]));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.datePeriodBirth = new global::Gamma.Widgets.yDatePeriodPicker();
			this.datePeriodBirth.Events = ((global::Gdk.EventMask)(256));
			this.datePeriodBirth.Name = "datePeriodBirth";
			this.datePeriodBirth.StartDate = new global::System.DateTime(0);
			this.datePeriodBirth.EndDate = new global::System.DateTime(0);
			this.ytable1.Add(this.datePeriodBirth);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.ytable1[this.datePeriodBirth]));
			w3.TopAttach = ((uint)(2));
			w3.BottomAttach = ((uint)(3));
			w3.LeftAttach = ((uint)(3));
			w3.RightAttach = ((uint)(4));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.datePeriodIssue = new global::Gamma.Widgets.yDatePeriodPicker();
			this.datePeriodIssue.Events = ((global::Gdk.EventMask)(256));
			this.datePeriodIssue.Name = "datePeriodIssue";
			this.datePeriodIssue.StartDate = new global::System.DateTime(0);
			this.datePeriodIssue.EndDate = new global::System.DateTime(0);
			this.ytable1.Add(this.datePeriodIssue);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.ytable1[this.datePeriodIssue]));
			w4.TopAttach = ((uint)(2));
			w4.BottomAttach = ((uint)(3));
			w4.LeftAttach = ((uint)(2));
			w4.RightAttach = ((uint)(3));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.entitySubdivision = new global::QS.Views.Control.EntityEntry();
			this.entitySubdivision.Events = ((global::Gdk.EventMask)(256));
			this.entitySubdivision.Name = "entitySubdivision";
			this.ytable1.Add(this.entitySubdivision);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.ytable1[this.entitySubdivision]));
			w5.LeftAttach = ((uint)(2));
			w5.RightAttach = ((uint)(3));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ycheckBirthday = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckBirthday.CanFocus = true;
			this.ycheckBirthday.Name = "ycheckBirthday";
			this.ycheckBirthday.Label = global::Mono.Unix.Catalog.GetString("День рождения");
			this.ycheckBirthday.DrawIndicator = true;
			this.ycheckBirthday.UseUnderline = true;
			this.ytable1.Add(this.ycheckBirthday);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ycheckBirthday]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.LeftAttach = ((uint)(3));
			w6.RightAttach = ((uint)(4));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ycheckOffPeriod = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckOffPeriod.CanFocus = true;
			this.ycheckOffPeriod.Name = "ycheckOffPeriod";
			this.ycheckOffPeriod.Label = global::Mono.Unix.Catalog.GetString("Следующее получение в период");
			this.ycheckOffPeriod.DrawIndicator = true;
			this.ycheckOffPeriod.UseUnderline = true;
			this.ytable1.Add(this.ycheckOffPeriod);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ycheckOffPeriod]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.LeftAttach = ((uint)(2));
			w7.RightAttach = ((uint)(3));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ycheckShowOverdue = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckShowOverdue.CanFocus = true;
			this.ycheckShowOverdue.Name = "ycheckShowOverdue";
			this.ycheckShowOverdue.Label = global::Mono.Unix.Catalog.GetString("Просроченный");
			this.ycheckShowOverdue.DrawIndicator = true;
			this.ycheckShowOverdue.UseUnderline = true;
			this.ytable1.Add(this.ycheckShowOverdue);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ycheckShowOverdue]));
			w8.TopAttach = ((uint)(3));
			w8.BottomAttach = ((uint)(4));
			w8.LeftAttach = ((uint)(2));
			w8.RightAttach = ((uint)(3));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ycheckStockAvailability = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckStockAvailability.CanFocus = true;
			this.ycheckStockAvailability.Name = "ycheckStockAvailability";
			this.ycheckStockAvailability.Label = global::Mono.Unix.Catalog.GetString("Наличие на складе");
			this.ycheckStockAvailability.DrawIndicator = true;
			this.ycheckStockAvailability.UseUnderline = true;
			this.ytable1.Add(this.ycheckStockAvailability);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ycheckStockAvailability]));
			w9.TopAttach = ((uint)(3));
			w9.BottomAttach = ((uint)(4));
			w9.LeftAttach = ((uint)(3));
			w9.RightAttach = ((uint)(4));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ycomboListWarehouses = new global::Gamma.Widgets.yListComboBox();
			this.ycomboListWarehouses.Name = "ycomboListWarehouses";
			this.ycomboListWarehouses.AddIfNotExist = false;
			this.ycomboListWarehouses.DefaultFirst = false;
			this.ytable1.Add(this.ycomboListWarehouses);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ycomboListWarehouses]));
			w10.TopAttach = ((uint)(4));
			w10.BottomAttach = ((uint)(5));
			w10.LeftAttach = ((uint)(3));
			w10.RightAttach = ((uint)(4));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yhbox2 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox2.Name = "yhbox2";
			this.yhbox2.Spacing = 6;
			this.ytable1.Add(this.yhbox2);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yhbox2]));
			w11.TopAttach = ((uint)(3));
			w11.BottomAttach = ((uint)(4));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yhbox3 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox3.Name = "yhbox3";
			this.yhbox3.Spacing = 6;
			// Container child yhbox3.Gtk.Box+BoxChild
			this.ylabelSex = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelSex.Name = "ylabelSex";
			this.ylabelSex.LabelProp = global::Mono.Unix.Catalog.GetString("Пол:");
			this.yhbox3.Add(this.ylabelSex);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.yhbox3[this.ylabelSex]));
			w12.Position = 0;
			w12.Expand = false;
			w12.Fill = false;
			// Container child yhbox3.Gtk.Box+BoxChild
			this.yenumcomboboxSex = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumcomboboxSex.Name = "yenumcomboboxSex";
			this.yenumcomboboxSex.ShowSpecialStateAll = false;
			this.yenumcomboboxSex.ShowSpecialStateNot = false;
			this.yenumcomboboxSex.UseShortTitle = false;
			this.yenumcomboboxSex.DefaultFirst = false;
			this.yhbox3.Add(this.yenumcomboboxSex);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.yhbox3[this.yenumcomboboxSex]));
			w13.Position = 1;
			this.ytable1.Add(this.yhbox3);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yhbox3]));
			w14.TopAttach = ((uint)(4));
			w14.BottomAttach = ((uint)(5));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.yIssueType = new global::Gamma.Widgets.yEnumComboBox();
			this.yIssueType.Name = "yIssueType";
			this.yIssueType.ShowSpecialStateAll = false;
			this.yIssueType.ShowSpecialStateNot = false;
			this.yIssueType.UseShortTitle = false;
			this.yIssueType.DefaultFirst = false;
			this.ytable1.Add(this.yIssueType);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.ytable1[this.yIssueType]));
			w15.TopAttach = ((uint)(4));
			w15.BottomAttach = ((uint)(5));
			w15.LeftAttach = ((uint)(2));
			w15.RightAttach = ((uint)(3));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 1F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение:");
			this.ytable1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel3]));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel4 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel4.Name = "ylabel4";
			this.ylabel4.Xalign = 1F;
			this.ylabel4.LabelProp = global::Mono.Unix.Catalog.GetString("Период получения:");
			this.ytable1.Add(this.ylabel4);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel4]));
			w17.TopAttach = ((uint)(2));
			w17.BottomAttach = ((uint)(3));
			w17.LeftAttach = ((uint)(1));
			w17.RightAttach = ((uint)(2));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable1.Gtk.Table+TableChild
			this.ylabel5 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel5.Name = "ylabel5";
			this.ylabel5.Xalign = 1F;
			this.ylabel5.LabelProp = global::Mono.Unix.Catalog.GetString("Тип выдачи:");
			this.ytable1.Add(this.ylabel5);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.ytable1[this.ylabel5]));
			w18.TopAttach = ((uint)(4));
			w18.BottomAttach = ((uint)(5));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(2));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			this.yhbox1.Add(this.ytable1);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.ytable1]));
			w19.Position = 0;
			w19.Expand = false;
			w19.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.choiceProtectionToolsView = new global::Workwear.ReportParameters.Views.ChoiceProtectionToolsView();
			this.choiceProtectionToolsView.Events = ((global::Gdk.EventMask)(256));
			this.choiceProtectionToolsView.Name = "choiceProtectionToolsView";
			this.yhbox1.Add(this.choiceProtectionToolsView);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.choiceProtectionToolsView]));
			w20.Position = 1;
			w20.Expand = false;
			w20.Fill = false;
			this.Add(this.yhbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Show();
		}
	}
}
