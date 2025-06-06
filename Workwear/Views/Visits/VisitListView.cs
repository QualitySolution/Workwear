using System;
using Gtk;
using QS.Views.Dialog;
using Workwear.ViewModels.Visits;

namespace Workwear.Views.Visits {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class VisitListView : DialogViewBase<VisitListViewModel> {
		public VisitListView(VisitListViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg() {
			
			var rows = ViewModel.Items.Count;
			ItemListTable.Resize((uint)(rows + 3), 5);

			var label1 = new Label {Markup = "<b>Дата</b>"};
			ItemListTable.Attach(label1, 2, 3, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label2 = new Label {Markup = "<b>Интервал</b>"};
			ItemListTable.Attach(label2, 4, 5, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label3 = new Label {Markup = "<b>Сотрудник</b>"};
			ItemListTable.Attach(label3, 6, 7, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label4 = new Label {Markup = "<b>Создан</b>"};
			ItemListTable.Attach(label4, 8, 9, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label5 = new Label {Markup = "<b>Комментарий</b>", Xalign = 0, };
			ItemListTable.Attach(label5, 10, 11, 0, 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

			ItemListTable.Attach(new VSeparator(), 3, 4, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
			ItemListTable.Attach(new VSeparator(), 5, 6, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
			ItemListTable.Attach(new VSeparator(), 7, 8, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
			ItemListTable.Attach(new VSeparator(), 9, 10, 0, (uint)(3 * rows + 3), AttachOptions.Shrink, AttachOptions.Fill, 0, 0);
			
			ItemListTable.Attach(new HSeparator(), 0, 12, 1, 2);
	
			uint i = 3;
			foreach(var item in ViewModel.Items.Values) {
				Label label;
				
				if(item.FirstOfDay) {
					ItemListTable.Attach(new HSeparator(), 0, 12, i, i+1);
					i++;
				}

				if(item.FirstOfDay) {
					label = new Label { LabelProp = item.VisitTime.ToShortDateString() }; //Дата
					ItemListTable.Attach(label, 2, 3, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
				}

				label = new Label {LabelProp = item.VisitTime.ToShortTimeString()}; //Время
                ItemListTable.Attach(label, 4, 5, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

                label = new Label {LabelProp = item.FIO}; //Сотрудник
                ItemListTable.Attach(label, 6, 7, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

                if(item.CreateTime != null) {
	                label = new Label { LabelProp = ((DateTime)item.CreateTime).ToShortDateString() }; //Дата создания
	                ItemListTable.Attach(label, 8, 9, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
                }

                label = new Label {LabelProp = item.Comment}; //Коментарий
                ItemListTable.Attach(label, 10, 11, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

				ItemListTable.Attach(new HSeparator(), 4, 12, i+1, i+2);
				
				i += 3;
		
			}
	
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
