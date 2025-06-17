using System;
using System.ComponentModel;
using Gtk;
using QS.Views.Dialog;
using Workwear.ViewModels.Visits;

namespace Workwear.Views.Visits {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class VisitListView : DialogViewBase<VisitListViewModel> {
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
			ItemListTable.Resize((uint)(rows + 3), 6);

			var label2 = new Label {Markup = "<b>Интервал</b>"};
			ItemListTable.Attach(label2, 4, 5, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label3 = new Label {Markup = "<b>Сотрудник</b>"};
			ItemListTable.Attach(label3, 6, 7, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label4 = new Label {Markup = "<b>Создан</b>"};
			ItemListTable.Attach(label4, 8, 9, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			
			var label5 = new Label {Markup = "<b>Документы</b>"};
			ItemListTable.Attach(label5, 10, 13, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label6 = new Label {Markup = "<b>Комментарий</b>", Xalign = 0, };
			ItemListTable.Attach(label6, 14, 15, 0, 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

			//Границы таблицы (разделители)
			ItemListTable.Attach(new VSeparator(), 5, 6, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
			ItemListTable.Attach(new VSeparator(), 7, 8, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
			ItemListTable.Attach(new VSeparator(), 9, 10, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
			ItemListTable.Attach(new VSeparator(), 13, 14, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);			
			
            ItemListTable.Attach(new HSeparator(), 0, 15, 1, 2);
	
            //Заполнение данными
			uint i = 3;
			foreach(var item in ViewModel.Items.Values) {
				Label label;
				Entry entry;
				TextView textView;
				Button button;
				uint j = (uint) item.Documents.Count; //смещение многострочных элементов
				
				if(item.FirstOfDay) {
					ItemListTable.Attach(new HSeparator(), 0, 15, i, i+1);
					i++;
				}

				label = new Label {LabelProp = item.VisitTime.ToShortTimeString()}; //Время
                ItemListTable.Attach(label, 4, 5, i, i+j+1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

                label = new Label {LabelProp = item.FIO}; //Сотрудник
                ItemListTable.Attach(label, 6, 7, i, i+j+1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

                if(item.CreateTime != null) {
	                label = new Label { LabelProp = ((DateTime)item.CreateTime).ToShortDateString() }; //Дата создания
	                ItemListTable.Attach(label, 8, 9, i, i+j+1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
                }
                
                if(item.Documents.Count != 0) { //Документы
	                uint n = 0;
	                foreach(var doc in item.Documents) {
		                button = new Button(); //открыть
		                button.Label = doc.label;
		                button.Clicked += (sender, args) =>  ViewModel.OpenDocument(doc.doc, doc.id);
		                ItemListTable.Attach(button, 10, 11, i+n, i+n+1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
		                n++;
	                }
                }

                if(item.Employee != null) {
	                button = new Button(); //добавить
	                button.Clicked += (sender, args) =>  ViewModel.AddExpance(item.Employee, item.Visit);
	                Image w3 = new Image();
	                w3.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-add", IconSize.Menu);
	                button.Image = w3;
	                ItemListTable.Attach(button, 12, 13, i, i+j+1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
                
	                if(!string.IsNullOrEmpty(item.DocumentsString))
						ItemListTable.Attach(new VSeparator(), 11, 12, i, i+j+1, AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
				}

				if(item.Visit != null) { //Пока не даём создавать из программы новые
					textView = new TextView(); //Коментарий
					textView.Buffer.Text = item.Comment;
					textView.FocusOutEvent += (sender, args) => ViewModel.AddComment(item, textView.Buffer.Text);
					ItemListTable.Attach(textView, 14, 15, i, i+j+1, AttachOptions.Fill, AttachOptions.Fill, 0, 0);
				}

				ItemListTable.Attach(new HSeparator(), 0, 15, i+j+1, i+j+2);
				
				i += j+2;
			}
			ItemListTable.ShowAll();
		}

	}
}
