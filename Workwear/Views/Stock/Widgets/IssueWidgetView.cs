using System;
using Gtk;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using QS.Views.Dialog;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.Views.Stock.Widgets {
	public partial class IssueWidgetView : DialogViewBase<IssueWidgetViewModel> {
		public IssueWidgetView(IssueWidgetViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg() {

			var rows = ViewModel.Items.Count;
			ItemListTable.Resize((uint)(rows + 2), 9);

			var label1 = new Label {Markup = "<b>Добавить</b>"};
			ItemListTable.Attach(label1, 2, 3, 1, 4, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label2 = new Label {Markup = "<b>Номенклатура нормы</b>"};
			ItemListTable.Attach(label2, 4, 5, 1, 4, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

			var label3 = new Label {Markup = "<b>Тип выдачи</b>"};
			ItemListTable.Attach(label3, 6, 7, 1, 4, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label4 = new Label {Markup = "<b>Потребности</b>"};
			ItemListTable.Attach(label4, 8, 12, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label5 = new Label {Markup = "<b>Количество</b>"};
			ItemListTable.Attach(label5, 11, 15, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label6 = new Label {Markup = "<b><i>Неудовлетворённые</i></b>"};
			ItemListTable.Attach(label6, 8, 9, 3, 4, AttachOptions.Shrink, AttachOptions.Shrink, 3, 0);

			var label7 = new Label {Markup = "<b><i>Всего</i></b>"};
			ItemListTable.Attach(label7, 10, 11, 3, 4, AttachOptions.Shrink, AttachOptions.Shrink, 3, 0);

			var label8 = new Label {Markup = "<b><i>К выдаче</i></b>"};
			ItemListTable.Attach(label8, 12, 13, 3, 4, AttachOptions.Shrink, AttachOptions.Shrink, 3, 0);

			var label9 = new Label {Markup = "<b><i>На складе</i></b>"};
			ItemListTable.Attach(label9, 14, 15, 3, 4, AttachOptions.Shrink, AttachOptions.Shrink, 3, 0);

			ItemListTable.Attach(new VSeparator(), 3, 4, 0, (uint)(rows + 5));
			ItemListTable.Attach(new VSeparator(), 5, 6, 0, (uint)(rows + 5));
			ItemListTable.Attach(new VSeparator(), 7, 8, 0, (uint)(rows + 5));
			ItemListTable.Attach(new VSeparator(), 9, 10, 3, (uint)(rows + 5));
			ItemListTable.Attach(new VSeparator(), 11, 12, 0, (uint)(rows + 5));
			ItemListTable.Attach(new VSeparator(), 13, 14, 3, (uint)(rows + 5));
			
			ItemListTable.Attach(new HSeparator(), 0, 15, 4, 5);
			ItemListTable.Attach(new HSeparator(), 8, 15, 2, 3);
			
			var items = ViewModel.Items;
			uint i = 5;
				foreach(var item in items.Values) {
					var check = new yCheckButton(); //Добавить
					check.Binding.AddBinding(item, x => x.Active, w => w.Active).InitializeFromSource();
					ItemListTable.Attach(check, 2, 3, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

					var label = new Label {LabelProp = item.ProtectionTools.Name}; //Номенклатура нормы
					label.Justify = Justification.Left;
					label.Xalign = 0;
					label.Wrap = true;
					ItemListTable.Attach(label, 4, 5, i, i + 1, AttachOptions.Fill, AttachOptions.Shrink, 0, 0);

					label = new Label {LabelProp = item.Type.GetEnumTitle()}; //Тип выдачи
					ItemListTable.Attach(label, 6, 7, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

					label = new Label {LabelProp = item.NumberOfCurrentNeeds.ToString()}; //Неудовлетворённых потребностей
					ItemListTable.Attach(label, 8, 9, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
	
					label = new Label {LabelProp = item.NumberOfNeeds.ToString()}; //Всего потребностей
					ItemListTable.Attach(label, 10, 11, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
					
					label = new Label {LabelProp = item.ItemQuantityForIssuse.ToString()}; //Количество К выдаче
					ItemListTable.Attach(label, 12, 13, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
					
					label = new Label {LabelProp = item.ItemStockBalance.ToString()}; // Количество на складе
					if(item.ItemStockBalance<item.ItemQuantityForIssuse)
						label.ModifyFg(StateType.Normal, new Gdk.Color(255, 0, 0));
					ItemListTable.Attach(label, 14, 15, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
					
					i++;
				}

				if(rows > 17) {
					scrolledwindow1.VscrollbarPolicy = PolicyType.Always;
					HeightRequest = 600;
				}
				else
					scrolledwindow1.VscrollbarPolicy = PolicyType.Never;
				
				ItemListTable.ShowAll();
		}
		
		protected void OnAddToDocumentYbuttonClicked(object sender, EventArgs e) {
			ViewModel.AddItems(ViewModel.Items);
		}

		protected void OnSelectAllYbuttonClicked(object sender, EventArgs e) {
			ViewModel.SelectAll();
		}

		protected void OnUnSelectAllYbuttonClicked(object sender, EventArgs e) {
			ViewModel.UnSelectAll();
		}
	}
}

