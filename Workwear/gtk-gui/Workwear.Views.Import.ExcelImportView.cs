
// This file has been generated by the GUI designer. Do not modify.
namespace Workwear.Views.Import
{
	public partial class ExcelImportView
	{
		private global::Gtk.VBox vbox1;

		private global::Gamma.GtkWidgets.yNotebook notebookSteps;

		private global::Gtk.VBox vbox2;

		private global::Gamma.GtkWidgets.yFileChooserButton filechooser;

		private global::Gamma.GtkWidgets.yTable ytable2;

		private global::Gamma.Widgets.yListComboBox comboSheets;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yButton buttonLoad;

		private global::Gtk.Label label1;

		private global::Gtk.HBox hbox1;

		private global::Gtk.ScrolledWindow GtkScrolledColumns;

		private global::Gtk.Table tableColumns;

		private global::Gamma.GtkWidgets.yTable tableMatchSettings;

		private global::Gamma.GtkWidgets.ySpinButton spinTitleRow;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gtk.VBox vbox3;

		private global::Gtk.Frame frame1;

		private global::Gtk.Alignment GtkAlignment;

		private global::Gtk.Label labelColumnRecomendations;

		private global::Gtk.Label GtkLabel2;

		private global::Gtk.HBox hbox3;

		private global::Gamma.GtkWidgets.yButton buttonBackToSelectSheet;

		private global::Gamma.GtkWidgets.yButton buttonReadEmployees;

		private global::Gtk.Label label2;

		private global::Gtk.HBox hbox2;

		private global::Gtk.VBox vboxCounters;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yButton buttonBackToDataTypes;

		private global::Gamma.GtkWidgets.yButton buttonSave;

		private global::Gtk.VBox vbox5;

		private global::Gtk.Frame frame2;

		private global::Gtk.Alignment GtkAlignment2;

		private global::Gtk.Table table2;

		private global::Gtk.EventBox eventboxLegendaAmbiguous;

		private global::Gamma.GtkWidgets.yLabel ylabel7;

		private global::Gamma.GtkWidgets.yEventBox eventboxLegendaChanged;

		private global::Gamma.GtkWidgets.yLabel labelCountChangedEmployees;

		private global::Gtk.EventBox eventboxLegendaDublicate;

		private global::Gamma.GtkWidgets.yLabel ylabel8;

		private global::Gtk.EventBox eventboxLegendaError;

		private global::Gamma.GtkWidgets.yLabel ylabel6;

		private global::Gamma.GtkWidgets.yEventBox eventboxLegendaNew;

		private global::Gamma.GtkWidgets.yLabel labelNew;

		private global::Gamma.GtkWidgets.yEventBox eventboxLegendaNotFound;

		private global::Gamma.GtkWidgets.yLabel labelLegendNotFound;

		private global::Gamma.GtkWidgets.yEventBox eventboxLegendaSkipRows;

		private global::Gamma.GtkWidgets.yLabel labelCountSkipRows;

		private global::Gtk.HSeparator hseparator1;

		private global::Gamma.GtkWidgets.yLabel labelLegendaWarning;

		private global::Gtk.Label GtkLabel8;

		private global::Gtk.Label label4;

		private global::QS.Widgets.ProgressWidget progressTotal;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView treeviewRows;

		private global::Gamma.GtkWidgets.yHBox hboxRowActions;

		private global::Gamma.GtkWidgets.yButton buttonIgnore;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Workwear.Views.Import.ExcelImportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Workwear.Views.Import.ExcelImportView";
			// Container child Workwear.Views.Import.ExcelImportView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.notebookSteps = new global::Gamma.GtkWidgets.yNotebook();
			this.notebookSteps.CanFocus = true;
			this.notebookSteps.Name = "notebookSteps";
			this.notebookSteps.CurrentPage = 2;
			this.notebookSteps.ShowBorder = false;
			// Container child notebookSteps.Gtk.Notebook+NotebookChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.filechooser = new global::Gamma.GtkWidgets.yFileChooserButton();
			this.filechooser.Name = "filechooser";
			this.filechooser.Title = global::Mono.Unix.Catalog.GetString("Выберите Excel файл с данными для импорта");
			this.vbox2.Add(this.filechooser);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.filechooser]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.ytable2 = new global::Gamma.GtkWidgets.yTable();
			this.ytable2.Name = "ytable2";
			this.ytable2.NColumns = ((uint)(3));
			this.ytable2.RowSpacing = ((uint)(6));
			this.ytable2.ColumnSpacing = ((uint)(6));
			// Container child ytable2.Gtk.Table+TableChild
			this.comboSheets = new global::Gamma.Widgets.yListComboBox();
			this.comboSheets.Name = "comboSheets";
			this.comboSheets.AddIfNotExist = false;
			this.comboSheets.DefaultFirst = true;
			this.ytable2.Add(this.comboSheets);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytable2[this.comboSheets]));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytable2.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Лист с данными:");
			this.ytable2.Add(this.ylabel2);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.ytable2[this.ylabel2]));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add(this.ytable2);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.ytable2]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.buttonLoad = new global::Gamma.GtkWidgets.yButton();
			this.buttonLoad.CanFocus = true;
			this.buttonLoad.Name = "buttonLoad";
			this.buttonLoad.UseUnderline = true;
			this.buttonLoad.Label = global::Mono.Unix.Catalog.GetString("Разбор листа ⇒");
			this.vbox2.Add(this.buttonLoad);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.buttonLoad]));
			w5.Position = 2;
			w5.Expand = false;
			w5.Fill = false;
			this.notebookSteps.Add(this.vbox2);
			// Notebook tab
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Выбор файла [Шаг 1 ]");
			this.notebookSteps.SetTabLabel(this.vbox2, this.label1);
			this.label1.ShowAll();
			// Container child notebookSteps.Gtk.Notebook+NotebookChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.GtkScrolledColumns = new global::Gtk.ScrolledWindow();
			this.GtkScrolledColumns.WidthRequest = 450;
			this.GtkScrolledColumns.Name = "GtkScrolledColumns";
			this.GtkScrolledColumns.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledColumns.Gtk.Container+ContainerChild
			global::Gtk.Viewport w7 = new global::Gtk.Viewport();
			w7.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.tableColumns = new global::Gtk.Table(((uint)(2)), ((uint)(2)), false);
			this.tableColumns.Name = "tableColumns";
			this.tableColumns.RowSpacing = ((uint)(6));
			this.tableColumns.ColumnSpacing = ((uint)(6));
			w7.Add(this.tableColumns);
			this.GtkScrolledColumns.Add(w7);
			this.hbox1.Add(this.GtkScrolledColumns);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.GtkScrolledColumns]));
			w10.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.tableMatchSettings = new global::Gamma.GtkWidgets.yTable();
			this.tableMatchSettings.Name = "tableMatchSettings";
			this.tableMatchSettings.NRows = ((uint)(2));
			this.tableMatchSettings.NColumns = ((uint)(2));
			this.tableMatchSettings.RowSpacing = ((uint)(6));
			this.tableMatchSettings.ColumnSpacing = ((uint)(6));
			// Container child tableMatchSettings.Gtk.Table+TableChild
			this.spinTitleRow = new global::Gamma.GtkWidgets.ySpinButton(0D, 100000D, 1D);
			this.spinTitleRow.CanFocus = true;
			this.spinTitleRow.Name = "spinTitleRow";
			this.spinTitleRow.Adjustment.PageIncrement = 10D;
			this.spinTitleRow.ClimbRate = 1D;
			this.spinTitleRow.Numeric = true;
			this.spinTitleRow.ValueAsDecimal = 0m;
			this.spinTitleRow.ValueAsInt = 0;
			this.tableMatchSettings.Add(this.spinTitleRow);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.tableMatchSettings[this.spinTitleRow]));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMatchSettings.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Номер строки с заголовками:");
			this.tableMatchSettings.Add(this.ylabel1);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.tableMatchSettings[this.ylabel1]));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox1.Add(this.tableMatchSettings);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.tableMatchSettings]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.vbox3 = new global::Gtk.VBox();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.frame1 = new global::Gtk.Frame();
			this.frame1.Name = "frame1";
			this.frame1.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame1.Gtk.Container+ContainerChild
			this.GtkAlignment = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment.Name = "GtkAlignment";
			this.GtkAlignment.LeftPadding = ((uint)(12));
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			this.labelColumnRecomendations = new global::Gtk.Label();
			this.labelColumnRecomendations.Name = "labelColumnRecomendations";
			this.labelColumnRecomendations.LabelProp = global::Mono.Unix.Catalog.GetString(@"Установите номер строки с заголовком данных, таким образом чтобы название колонок было корректно. Если в таблице заголовки отсутствуют укажите 0.
Далее для каждой значимой колонки проставьте тип данных которых находится в таблице.
При загрузки листа программа автоматически пытается найти заголовок таблицы и выбрать тип данных.
Обязательными данными являются Фамилия и Имя или ФИО.");
			this.labelColumnRecomendations.Wrap = true;
			this.labelColumnRecomendations.Justify = ((global::Gtk.Justification)(3));
			this.GtkAlignment.Add(this.labelColumnRecomendations);
			this.frame1.Add(this.GtkAlignment);
			this.GtkLabel2 = new global::Gtk.Label();
			this.GtkLabel2.Name = "GtkLabel2";
			this.GtkLabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Рекомендации");
			this.GtkLabel2.UseMarkup = true;
			this.frame1.LabelWidget = this.GtkLabel2;
			this.vbox3.Add(this.frame1);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.frame1]));
			w16.Position = 0;
			w16.Expand = false;
			w16.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.buttonBackToSelectSheet = new global::Gamma.GtkWidgets.yButton();
			this.buttonBackToSelectSheet.CanFocus = true;
			this.buttonBackToSelectSheet.Name = "buttonBackToSelectSheet";
			this.buttonBackToSelectSheet.UseUnderline = true;
			this.buttonBackToSelectSheet.Label = global::Mono.Unix.Catalog.GetString("⇐ Вернутся к выбору листа");
			this.hbox3.Add(this.buttonBackToSelectSheet);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.buttonBackToSelectSheet]));
			w17.Position = 0;
			// Container child hbox3.Gtk.Box+BoxChild
			this.buttonReadEmployees = new global::Gamma.GtkWidgets.yButton();
			this.buttonReadEmployees.CanFocus = true;
			this.buttonReadEmployees.Name = "buttonReadEmployees";
			this.buttonReadEmployees.UseUnderline = true;
			this.buttonReadEmployees.Label = global::Mono.Unix.Catalog.GetString("Сопоставление данных ⇒");
			this.hbox3.Add(this.buttonReadEmployees);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.buttonReadEmployees]));
			w18.Position = 1;
			this.vbox3.Add(this.hbox3);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.hbox3]));
			w19.Position = 1;
			w19.Expand = false;
			w19.Fill = false;
			this.hbox1.Add(this.vbox3);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.vbox3]));
			w20.Position = 2;
			w20.Expand = false;
			w20.Fill = false;
			this.notebookSteps.Add(this.hbox1);
			global::Gtk.Notebook.NotebookChild w21 = ((global::Gtk.Notebook.NotebookChild)(this.notebookSteps[this.hbox1]));
			w21.Position = 1;
			// Notebook tab
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Формат файла [Шаг 2]");
			this.notebookSteps.SetTabLabel(this.hbox1, this.label2);
			this.label2.ShowAll();
			// Container child notebookSteps.Gtk.Notebook+NotebookChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.vboxCounters = new global::Gtk.VBox();
			this.vboxCounters.Name = "vboxCounters";
			this.vboxCounters.Spacing = 6;
			// Container child vboxCounters.Gtk.Box+BoxChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.buttonBackToDataTypes = new global::Gamma.GtkWidgets.yButton();
			this.buttonBackToDataTypes.CanFocus = true;
			this.buttonBackToDataTypes.Name = "buttonBackToDataTypes";
			this.buttonBackToDataTypes.UseUnderline = true;
			this.buttonBackToDataTypes.Label = global::Mono.Unix.Catalog.GetString("⇐ Вернутся к типам данных");
			this.yhbox1.Add(this.buttonBackToDataTypes);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.buttonBackToDataTypes]));
			w22.Position = 0;
			w22.Expand = false;
			w22.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.buttonSave = new global::Gamma.GtkWidgets.yButton();
			this.buttonSave.CanFocus = true;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.UseUnderline = true;
			this.buttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w23 = new global::Gtk.Image();
			w23.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-save", global::Gtk.IconSize.Menu);
			this.buttonSave.Image = w23;
			this.yhbox1.Add(this.buttonSave);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.buttonSave]));
			w24.Position = 1;
			w24.Expand = false;
			w24.Fill = false;
			this.vboxCounters.Add(this.yhbox1);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vboxCounters[this.yhbox1]));
			w25.PackType = ((global::Gtk.PackType)(1));
			w25.Position = 0;
			w25.Expand = false;
			w25.Fill = false;
			this.hbox2.Add(this.vboxCounters);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.vboxCounters]));
			w26.Position = 0;
			w26.Expand = false;
			w26.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.vbox5 = new global::Gtk.VBox();
			this.vbox5.Name = "vbox5";
			this.vbox5.Spacing = 6;
			// Container child vbox5.Gtk.Box+BoxChild
			this.frame2 = new global::Gtk.Frame();
			this.frame2.Name = "frame2";
			this.frame2.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame2.Gtk.Container+ContainerChild
			this.GtkAlignment2 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment2.Name = "GtkAlignment2";
			this.GtkAlignment2.LeftPadding = ((uint)(12));
			// Container child GtkAlignment2.Gtk.Container+ContainerChild
			this.table2 = new global::Gtk.Table(((uint)(9)), ((uint)(1)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.eventboxLegendaAmbiguous = new global::Gtk.EventBox();
			this.eventboxLegendaAmbiguous.TooltipMarkup = "При сопоставлении данных из файла с данными в базе, найденно несколько вариантов " +
				"подходящих значений. Программа не может однозначно понять какой из объектов необ" +
				"ходимо обновлять.";
			this.eventboxLegendaAmbiguous.Name = "eventboxLegendaAmbiguous";
			// Container child eventboxLegendaAmbiguous.Gtk.Container+ContainerChild
			this.ylabel7 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel7.Name = "ylabel7";
			this.ylabel7.LabelProp = global::Mono.Unix.Catalog.GetString("Неоднозначное соответствие");
			this.eventboxLegendaAmbiguous.Add(this.ylabel7);
			this.table2.Add(this.eventboxLegendaAmbiguous);
			global::Gtk.Table.TableChild w28 = ((global::Gtk.Table.TableChild)(this.table2[this.eventboxLegendaAmbiguous]));
			w28.TopAttach = ((uint)(3));
			w28.BottomAttach = ((uint)(4));
			w28.XOptions = ((global::Gtk.AttachOptions)(4));
			w28.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.eventboxLegendaChanged = new global::Gamma.GtkWidgets.yEventBox();
			this.eventboxLegendaChanged.Name = "eventboxLegendaChanged";
			// Container child eventboxLegendaChanged.Gtk.Container+ContainerChild
			this.labelCountChangedEmployees = new global::Gamma.GtkWidgets.yLabel();
			this.labelCountChangedEmployees.Name = "labelCountChangedEmployees";
			this.labelCountChangedEmployees.LabelProp = global::Mono.Unix.Catalog.GetString("Измененные");
			this.eventboxLegendaChanged.Add(this.labelCountChangedEmployees);
			this.table2.Add(this.eventboxLegendaChanged);
			global::Gtk.Table.TableChild w30 = ((global::Gtk.Table.TableChild)(this.table2[this.eventboxLegendaChanged]));
			w30.TopAttach = ((uint)(1));
			w30.BottomAttach = ((uint)(2));
			w30.XOptions = ((global::Gtk.AttachOptions)(4));
			w30.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.eventboxLegendaDublicate = new global::Gtk.EventBox();
			this.eventboxLegendaDublicate.TooltipMarkup = "При сопоставлении данных из файла с данными в базе, найденно несколько вариантов " +
				"подходящих значений. Программа не может однозначно понять какой из объектов необ" +
				"ходимо обновлять.";
			this.eventboxLegendaDublicate.Name = "eventboxLegendaDublicate";
			// Container child eventboxLegendaDublicate.Gtk.Container+ContainerChild
			this.ylabel8 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel8.Name = "ylabel8";
			this.ylabel8.LabelProp = global::Mono.Unix.Catalog.GetString("Дубликат");
			this.eventboxLegendaDublicate.Add(this.ylabel8);
			this.table2.Add(this.eventboxLegendaDublicate);
			global::Gtk.Table.TableChild w32 = ((global::Gtk.Table.TableChild)(this.table2[this.eventboxLegendaDublicate]));
			w32.TopAttach = ((uint)(4));
			w32.BottomAttach = ((uint)(5));
			w32.XOptions = ((global::Gtk.AttachOptions)(4));
			w32.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.eventboxLegendaError = new global::Gtk.EventBox();
			this.eventboxLegendaError.Name = "eventboxLegendaError";
			// Container child eventboxLegendaError.Gtk.Container+ContainerChild
			this.ylabel6 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel6.Name = "ylabel6";
			this.ylabel6.LabelProp = global::Mono.Unix.Catalog.GetString("Неверный формат");
			this.eventboxLegendaError.Add(this.ylabel6);
			this.table2.Add(this.eventboxLegendaError);
			global::Gtk.Table.TableChild w34 = ((global::Gtk.Table.TableChild)(this.table2[this.eventboxLegendaError]));
			w34.TopAttach = ((uint)(5));
			w34.BottomAttach = ((uint)(6));
			w34.XOptions = ((global::Gtk.AttachOptions)(4));
			w34.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.eventboxLegendaNew = new global::Gamma.GtkWidgets.yEventBox();
			this.eventboxLegendaNew.Name = "eventboxLegendaNew";
			// Container child eventboxLegendaNew.Gtk.Container+ContainerChild
			this.labelNew = new global::Gamma.GtkWidgets.yLabel();
			this.labelNew.Name = "labelNew";
			this.labelNew.LabelProp = global::Mono.Unix.Catalog.GetString("Новые");
			this.eventboxLegendaNew.Add(this.labelNew);
			this.table2.Add(this.eventboxLegendaNew);
			global::Gtk.Table.TableChild w36 = ((global::Gtk.Table.TableChild)(this.table2[this.eventboxLegendaNew]));
			w36.XOptions = ((global::Gtk.AttachOptions)(4));
			w36.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.eventboxLegendaNotFound = new global::Gamma.GtkWidgets.yEventBox();
			this.eventboxLegendaNotFound.Name = "eventboxLegendaNotFound";
			// Container child eventboxLegendaNotFound.Gtk.Container+ContainerChild
			this.labelLegendNotFound = new global::Gamma.GtkWidgets.yLabel();
			this.labelLegendNotFound.Name = "labelLegendNotFound";
			this.labelLegendNotFound.LabelProp = global::Mono.Unix.Catalog.GetString("Не найдено");
			this.eventboxLegendaNotFound.Add(this.labelLegendNotFound);
			this.table2.Add(this.eventboxLegendaNotFound);
			global::Gtk.Table.TableChild w38 = ((global::Gtk.Table.TableChild)(this.table2[this.eventboxLegendaNotFound]));
			w38.TopAttach = ((uint)(2));
			w38.BottomAttach = ((uint)(3));
			w38.XOptions = ((global::Gtk.AttachOptions)(4));
			w38.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.eventboxLegendaSkipRows = new global::Gamma.GtkWidgets.yEventBox();
			this.eventboxLegendaSkipRows.Name = "eventboxLegendaSkipRows";
			// Container child eventboxLegendaSkipRows.Gtk.Container+ContainerChild
			this.labelCountSkipRows = new global::Gamma.GtkWidgets.yLabel();
			this.labelCountSkipRows.Name = "labelCountSkipRows";
			this.labelCountSkipRows.LabelProp = global::Mono.Unix.Catalog.GetString("Пропущенные");
			this.eventboxLegendaSkipRows.Add(this.labelCountSkipRows);
			this.table2.Add(this.eventboxLegendaSkipRows);
			global::Gtk.Table.TableChild w40 = ((global::Gtk.Table.TableChild)(this.table2[this.eventboxLegendaSkipRows]));
			w40.TopAttach = ((uint)(6));
			w40.BottomAttach = ((uint)(7));
			w40.XOptions = ((global::Gtk.AttachOptions)(4));
			w40.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.hseparator1 = new global::Gtk.HSeparator();
			this.hseparator1.Name = "hseparator1";
			this.table2.Add(this.hseparator1);
			global::Gtk.Table.TableChild w41 = ((global::Gtk.Table.TableChild)(this.table2[this.hseparator1]));
			w41.TopAttach = ((uint)(7));
			w41.BottomAttach = ((uint)(8));
			w41.XOptions = ((global::Gtk.AttachOptions)(4));
			w41.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.labelLegendaWarning = new global::Gamma.GtkWidgets.yLabel();
			this.labelLegendaWarning.Name = "labelLegendaWarning";
			this.labelLegendaWarning.LabelProp = global::Mono.Unix.Catalog.GetString("Предупреждение");
			this.table2.Add(this.labelLegendaWarning);
			global::Gtk.Table.TableChild w42 = ((global::Gtk.Table.TableChild)(this.table2[this.labelLegendaWarning]));
			w42.TopAttach = ((uint)(8));
			w42.BottomAttach = ((uint)(9));
			w42.XOptions = ((global::Gtk.AttachOptions)(4));
			w42.YOptions = ((global::Gtk.AttachOptions)(4));
			this.GtkAlignment2.Add(this.table2);
			this.frame2.Add(this.GtkAlignment2);
			this.GtkLabel8 = new global::Gtk.Label();
			this.GtkLabel8.Name = "GtkLabel8";
			this.GtkLabel8.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Легенда</b>");
			this.GtkLabel8.UseMarkup = true;
			this.frame2.LabelWidget = this.GtkLabel8;
			this.vbox5.Add(this.frame2);
			global::Gtk.Box.BoxChild w45 = ((global::Gtk.Box.BoxChild)(this.vbox5[this.frame2]));
			w45.Position = 0;
			w45.Expand = false;
			w45.Fill = false;
			this.hbox2.Add(this.vbox5);
			global::Gtk.Box.BoxChild w46 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.vbox5]));
			w46.PackType = ((global::Gtk.PackType)(1));
			w46.Position = 1;
			w46.Expand = false;
			w46.Fill = false;
			this.notebookSteps.Add(this.hbox2);
			global::Gtk.Notebook.NotebookChild w47 = ((global::Gtk.Notebook.NotebookChild)(this.notebookSteps[this.hbox2]));
			w47.Position = 2;
			// Notebook tab
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Обработка данных [Шаг 3]");
			this.notebookSteps.SetTabLabel(this.hbox2, this.label4);
			this.label4.ShowAll();
			this.vbox1.Add(this.notebookSteps);
			global::Gtk.Box.BoxChild w48 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.notebookSteps]));
			w48.Position = 0;
			w48.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.progressTotal = new global::QS.Widgets.ProgressWidget();
			this.progressTotal.Name = "progressTotal";
			this.vbox1.Add(this.progressTotal);
			global::Gtk.Box.BoxChild w49 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.progressTotal]));
			w49.Position = 1;
			w49.Expand = false;
			w49.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.treeviewRows = new global::Gamma.GtkWidgets.yTreeView();
			this.treeviewRows.CanFocus = true;
			this.treeviewRows.Name = "treeviewRows";
			this.GtkScrolledWindow.Add(this.treeviewRows);
			this.vbox1.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w51 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow]));
			w51.Position = 2;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hboxRowActions = new global::Gamma.GtkWidgets.yHBox();
			this.hboxRowActions.Name = "hboxRowActions";
			this.hboxRowActions.Spacing = 6;
			// Container child hboxRowActions.Gtk.Box+BoxChild
			this.buttonIgnore = new global::Gamma.GtkWidgets.yButton();
			this.buttonIgnore.CanFocus = true;
			this.buttonIgnore.Name = "buttonIgnore";
			this.buttonIgnore.UseUnderline = true;
			this.buttonIgnore.Label = global::Mono.Unix.Catalog.GetString("Не загружать");
			this.hboxRowActions.Add(this.buttonIgnore);
			global::Gtk.Box.BoxChild w52 = ((global::Gtk.Box.BoxChild)(this.hboxRowActions[this.buttonIgnore]));
			w52.Position = 0;
			w52.Expand = false;
			w52.Fill = false;
			this.vbox1.Add(this.hboxRowActions);
			global::Gtk.Box.BoxChild w53 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hboxRowActions]));
			w53.Position = 3;
			w53.Expand = false;
			w53.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.progressTotal.Hide();
			this.Hide();
			this.buttonLoad.Clicked += new global::System.EventHandler(this.OnButtonLoadClicked);
			this.buttonBackToSelectSheet.Clicked += new global::System.EventHandler(this.OnButtonBackToSelectSheetClicked);
			this.buttonReadEmployees.Clicked += new global::System.EventHandler(this.OnButtonReadEmployeesClicked);
			this.buttonBackToDataTypes.Clicked += new global::System.EventHandler(this.OnButtonBackToDataTypesClicked);
			this.buttonSave.Clicked += new global::System.EventHandler(this.OnButtonSaveClicked);
			this.buttonIgnore.Clicked += new global::System.EventHandler(this.OnButtonIgnoreClicked);
		}
	}
}
