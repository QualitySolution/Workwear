using System;
using QS.Views.Dialog;
using Workwear.ViewModels.Stock.Widgets;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;

using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using QS.DomainModel.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using Workwear.Domain.Sizes;
using System.ComponentModel;


namespace Workwear.Views.Stock.Widgets {
	public partial class IssueWidgetView : DialogViewBase<IssueWidgetViewModel> {
		public IssueWidgetView(IssueWidgetViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg() {

			var rows = ViewModel.Items.Count;
			ItemListTable.Resize((uint)(rows + 1), 4);

			var label1 = new Label {LabelProp = "Добавить"};
			ItemListTable.Attach(label1, 1, 2, 0, 0 + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label2 = new Label {LabelProp = "Номенклатура нормы"};
			ItemListTable.Attach(label2, 2, 3, 0, 0 + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

			//var label3 = new Label {LabelProp = "Основная номенклатура"};
			//ItemListTable.Attach(label3, 3, 4, 0, 0 + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);
			
			var label3 = new Label {LabelProp = "Выдача"};
			ItemListTable.Attach(label3, 3, 4, 0, 0 + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var label4 = new Label {LabelProp = "Сотрудников"};
			ItemListTable.Attach(label4, 4, 5, 0, 0 + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			var items = ViewModel.Items;
			uint i = 1;
				foreach(var item in items.Values) {
					var check = new yCheckButton();
					check.Binding.AddBinding(item, x => x.Active, w => w.Active).InitializeFromSource();
					ItemListTable.Attach(check, 1, 2, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

					var label = new Label {LabelProp = item.ProtectionTools.Name};
					label.Justify = Justification.Left;
					label.Xalign = 0;
					label.Wrap = true;
					//label.WidthChars = 60;
					ItemListTable.Attach(label, 2, 3, i, i + 1, AttachOptions.Fill, AttachOptions.Shrink, 0, 0);

					//label = new Label{LabelProp = item?.Nomenclature.Name};
					//ItemListTable.Attach(label, 3, 4, i, i + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

					label = new Label {LabelProp = item.Type.GetEnumTitle()};
					ItemListTable.Attach(label, 3, 4, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

					label = new Label {LabelProp = item.NumberOfNeeds.ToString()};
					ItemListTable.Attach(label, 4, 5, i, i + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
	
					i++;
				}

				if(rows > 17) {
					scrolledwindow1.VscrollbarPolicy = PolicyType.Automatic;
					HeightRequest = 600;
				}
				ItemListTable.ShowAll();
		}
		
		protected void OnAddToDocumentYbuttonClicked(object sender, EventArgs e) {
			ViewModel.AddItems();
		}

		protected void OnSelectAllYbuttonClicked(object sender, EventArgs e) {
			ViewModel.SelectAll();
		}

		protected void OnUnSelectAllYbuttonClicked(object sender, EventArgs e) {
			ViewModel.UnSelectAll();
		}
	}
}

