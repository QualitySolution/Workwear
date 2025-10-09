using System;
using System.Collections.Generic;
using System.ComponentModel;
using Gtk;
using QS.Views.Dialog;
using Workwear.Models.Visits;
using Workwear.ViewModels.Visits;

namespace Workwear.Views.Visits {
	public partial class VisitListView : DialogViewBase<VisitListViewModel> {
		private readonly Dictionary<VisitListItem, List<Button>> itemActionButtons = new Dictionary<VisitListItem, List<Button>>();
		public VisitListView(VisitListViewModel viewModel) : base(viewModel) {
			Build();
			ConfigureHead();
			ConfigureTabel();

			ViewModel.PropertyChanged += VmPropertyChanged;
		}

		private void VmPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(ViewModel.Items)) 
				ConfigureTabel();
		}
			
		private void ConfigureHead() {
			ylabelDate.Binding
				.AddBinding(ViewModel, vm => vm.PeriodString, w => w.Text)
				.InitializeFromSource();
			buttonNext.Clicked += (sender, args) => ViewModel.NextDay();
			buttonPrev.Clicked += (sender, args) => ViewModel.PrevDay();
		}

		private void ConfigureTabel() {
			//Очистка
			foreach (Widget child in ItemListTable.Children) {
				ItemListTable.Remove(child);
				child.Destroy(); 
			}
			
			//Шапка
			var rows = ViewModel.Items.Count;
			ItemListTable.Resize((uint)(rows + 3), 7);

			var label2 = new Label {Markup = "<b>Интервал</b>"};
			ItemListTable.Attach(label2, 0, 1, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label3 = new Label {Markup = "<b>Сотрудник</b>"};
			ItemListTable.Attach(label3, 2, 3, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label4 = new Label {Markup = "<b>Создан</b>"};
			ItemListTable.Attach(label4, 4, 5, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			
			var label5 = new Label {Markup = "<b>Документы</b>"};
			ItemListTable.Attach(label5, 6, 9, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			
			var label6 = new Label { Markup = "<b>Действия</b>" };
			ItemListTable.Attach(label6, 10, 11, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0 ,0);
			
			var label7 = new Label {Markup = "<b>Комментарий</b>", Xalign = 0, };
			ItemListTable.Attach(label7, 12, 13, 0, 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);
			
			//Границы таблицы (разделители)
			ItemListTable.Attach(new VSeparator(), 1, 2, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
			ItemListTable.Attach(new VSeparator(), 3, 4, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
			ItemListTable.Attach(new VSeparator(), 5, 6, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
			ItemListTable.Attach(new VSeparator(), 9, 10,0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0 ,0);
			ItemListTable.Attach(new VSeparator(), 11, 12, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);			
			
			
           ItemListTable.Attach(new HSeparator(), 0, 13, 1, 2);
           ItemListTable.Attach(new HSeparator(), 0, 13, 3, 4);
	
            //Заполнение данными
			uint i = 6;
			foreach(var item in ViewModel.Items.Values) {
				Label label;
				Entry entry;
				TextView textView;
				Button button;
				HBox buttonBox = new HBox(false, 2);
				ScrolledWindow scrolledWindow;
				uint j = (uint) item.Documents.Count; //смещение многострочных элементов

				label = new Label {LabelProp = item.VisitTime.ToShortTimeString()}; //Время
                ItemListTable.Attach(label, 0, 1, i, i+j+1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

                label = new Label {LabelProp = item.FIO}; //Сотрудник
                ItemListTable.Attach(label, 2, 3, i, i+j+1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

                if(item.CreateTime != null) {
	                label = new Label { LabelProp = ((DateTime)item.CreateTime).ToShortDateString() }; //Дата создания
	                ItemListTable.Attach(label, 4, 5, i, i+j+1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
                }
                
                if(item.Documents.Count != 0) { //Документы
	                uint n = 0;
	                foreach(var doc in item.Documents) {
		                button = new Button(); //открыть
		                button.Label = doc.label;
		                button.Clicked += (sender, args) =>  ViewModel.OpenDocument(doc.doc, doc.id);
		                ItemListTable.Attach(button, 6, 7, i+n, i+n+1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
		                n++;
	                }
                }
                
                if(item.Employee != null) {
	                button = new Button(); //добавить
	                button.Clicked += (sender, args) =>  ViewModel.AddExpance(item.Employee, item.Visit);
	                Image w3 = new Image();
	                w3.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-add", IconSize.Menu);
	                button.Image = w3;
	                ItemListTable.Attach(button, 8, 9, i, i+j+1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
                
	                if(!string.IsNullOrEmpty(item.DocumentsString))
						ItemListTable.Attach(new VSeparator(), 7, 8, i, i+j+1, AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
				}

				if(item.Visit != null) {
					var buttons = new List<Button>();
					for (uint l =0; l < 4; l++) {
						uint padding = 0;
						button = new Button();
						Image img = new Image();
						string name = String.Empty;
						string toolTip = String.Empty;
						ActionType type = ActionType.Play;
						switch(l) {
							case 0:
								name = "gtk-media-play";
								toolTip = "Начать";
								type = ActionType.Play;
								padding = 12;
								break;
							case 1:
								name = "gtk-ok";
								toolTip = "Завершено";
								type = ActionType.Done;
								break;
							case 2:
								name = "gtk-close";
								toolTip = "Отменить";
								type = ActionType.Cancel;
								break;
							case 3:
								name = "gtk-dialog-error";
								toolTip = "Не пришёл";
								type = ActionType.Close;
								break;
						}
						img.Pixbuf = Stetic.IconLoader.LoadIcon(this, name, IconSize.Menu);
						button.Image = img;
						button.TooltipText = toolTip;
						button.Sensitive = item.SensitiveActionButtons;
						button.Clicked += (sender, args) => {
							ViewModel.ChangeAction(item, type);
							if(type == ActionType.Done || type == ActionType.Cancel || type == ActionType.Close)
								UpdateButtonsSensitive(item, false);
						};
						buttonBox.PackStart(button, false, false, padding);
						buttons.Add(button);
					}
					itemActionButtons[item] = buttons;
				}
				
				ItemListTable.Attach(buttonBox, 10, 11, i, i+1, AttachOptions.Shrink, AttachOptions.Shrink, 0 ,0);
                
				if(item.Visit != null) { //Пока не даём создавать из программы новые
					scrolledWindow = new ScrolledWindow();
					textView = new TextView(); //Коментарий
					textView.WrapMode = (WrapMode)3;
					textView.Buffer.Text = item.Comment;
					scrolledWindow.Add(textView);
					textView.FocusOutEvent += (sender, args) => ViewModel.AddComment(item, textView.Buffer.Text);
					ItemListTable.Attach(scrolledWindow, 12, 13, i, i+j+1, AttachOptions.Fill, AttachOptions.Fill, 0, 0);
				}

				ItemListTable.Attach(new HSeparator(), 0, 13, i+j+1, i+j+2);
				
				i += j+2;
			}
			ItemListTable.ShowAll();
		}
		private void UpdateButtonsSensitive(VisitListItem item, bool sensitive)
		{
			foreach(var btn in itemActionButtons[item])
				btn.Sensitive = sensitive;
		}

	}
}
