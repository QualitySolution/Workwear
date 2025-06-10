using System;
using System.ComponentModel;
using Gamma.GtkWidgets;
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
				
				if(item.FirstOfDay) {
					ItemListTable.Attach(new HSeparator(), 0, 15, i, i+1);
					i++;
				}

				label = new Label {LabelProp = item.VisitTime.ToString()}; //Время
                ItemListTable.Attach(label, 4, 5, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

                label = new Label {LabelProp = item.FIO}; //Сотрудник
                ItemListTable.Attach(label, 6, 7, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

                if(item.CreateTime != null) {
	                label = new Label { LabelProp = ((DateTime)item.CreateTime).ToShortDateString() }; //Дата создания
	                ItemListTable.Attach(label, 8, 9, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
                }

                label = new Label() {LabelProp = item.Documents}; //Документы
                ItemListTable.Attach(label, 10, 11, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

                if(item.Employee != null) {
	                var addButton = new Button(); //Добавить
	                addButton.Clicked += (sender, args) =>  ViewModel.AddExpance(item.Employee, item.Visit);
	                Image w3 = new Image();
	                w3.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-add", IconSize.Menu);
	                addButton.Image = w3;
	                ItemListTable.Attach(addButton, 12, 13, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
                
	                if(!string.IsNullOrEmpty(item.Documents))
						ItemListTable.Attach(new VSeparator(), 11, 12, i, i + 1, AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
				}

                label = new Label {LabelProp = item.Comment}; //Коментарий
                ItemListTable.Attach(label, 14, 15, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

				ItemListTable.Attach(new HSeparator(), 0, 15, i+1, i+2);
				
				i += 3;
			}
	
			//Прокрутка
			if(rows > 5) {
				scrolledwindow1.VscrollbarPolicy = PolicyType.Always;
				HeightRequest = 600;
			}
			else
				scrolledwindow1.VscrollbarPolicy = PolicyType.Never;
			
			ItemListTable.ShowAll();
		}

	}
}
