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
			ItemListTable.Resize((uint)(rows + 1), 5);

			var label1 = new Label {LabelProp = "Добавить"};
			ItemListTable.Attach(label1, 1, 2, 0, 2, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label2 = new Label {LabelProp = "Номенклатура нормы"};
			ItemListTable.Attach(label2, 2, 3, 0, 2, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);
			
			var label3 = new Label {LabelProp = "Тип выдачи"};
			ItemListTable.Attach(label3, 3, 4, 0, 2, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label4 = new Label {LabelProp = "Потребности"};
			ItemListTable.Attach(label4, 4, 6, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			
			var label5 = new Label {LabelProp = "Количество"};
			ItemListTable.Attach(label5, 6, 8, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			
			var label6 = new Label {LabelProp = "Неудовлетворённые"};
			ItemListTable.Attach(label6, 4, 5, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			
			var label7 = new Label {LabelProp = "Всего"};
			ItemListTable.Attach(label7, 5, 6, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			
			var label8 = new Label {LabelProp = "К выдаче"};
			ItemListTable.Attach(label8, 6, 7, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			
			var label9 = new Label {LabelProp = "На складе"};
			ItemListTable.Attach(label9, 7, 8, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var items = ViewModel.Items;
			uint i = 2;
				foreach(var item in items.Values) {
					var check = new yCheckButton(); //Добавить
					check.Binding.AddBinding(item, x => x.Active, w => w.Active).InitializeFromSource();
					ItemListTable.Attach(check, 1, 2, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

					var label = new Label {LabelProp = item.ProtectionTools.Name}; //Номенклатура нормы
					label.Justify = Justification.Left;
					label.Xalign = 0;
					label.Wrap = true;
					ItemListTable.Attach(label, 2, 3, i, i + 1, AttachOptions.Fill, AttachOptions.Shrink, 0, 0);

					label = new Label {LabelProp = item.Type.GetEnumTitle()}; //Тип выдачи
					ItemListTable.Attach(label, 3, 4, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

					label = new Label {LabelProp = item.NumberOfCurrentNeeds.ToString()}; //Неудовлетворённых потребностей
					ItemListTable.Attach(label, 4, 5, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
	
					label = new Label {LabelProp = item.NumberOfNeeds.ToString()}; //Всего потребностей
					ItemListTable.Attach(label, 5, 6, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
					
					label = new Label {LabelProp = item.ItemQuantityForIssuse.ToString()}; //Количество К выдаче
					ItemListTable.Attach(label, 6, 7, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
					
					label = new Label {LabelProp = item.ItemStockBalance.ToString()}; // Количество на складе
					if(item.ItemStockBalance<item.ItemQuantityForIssuse)label.ModifyFg(StateType.Normal, new Gdk.Color(255, 0, 0));
					ItemListTable.Attach(label, 7, 8, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
					
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

