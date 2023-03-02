using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using Workwear.Domain.Sizes;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.Views.Stock.Widgets {
	public partial class SizeWidgetView : DialogViewBase<SizeWidgetViewModel>
	{
		public SizeWidgetView(SizeWidgetViewModel model) : base(model) {
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg() {
			labelTotal.Binding.AddBinding(ViewModel, v => v.TotalText, w => w.LabelProp).InitializeFromSource();
			AddButton.Binding.AddBinding(ViewModel, v => v.SensitiveAddButton, w => w.Sensitive).InitializeFromSource();
			if(ViewModel.IsUseHeight) {
				GrowthBox.SetRenderTextFunc<Size>(s => s.Name);
				GrowthBox.ItemsList = ViewModel.WearHeights;
				GrowthBox.Binding.AddBinding(ViewModel, v => v.Height, w => w.SelectedItem).InitializeFromSource();
				GrowthBox.ShowAll();
			}
			else {
				GrowthInfoBox.Visible = false;
			}
			ConfigureCheckBoxPlace();
		}
		/// <summary>
		/// Заполняет CheckBoxPlace таблицу на основе модели
		/// </summary>
		private void ConfigureCheckBoxPlace() {
			var rows = (uint)ViewModel.SizeItems.Count;
			CheckBoxPlace.Resize(rows+1, 4);

			#region пояснения к таблице
			var label1 = new Label { LabelProp = "Размер"};
			CheckBoxPlace.Attach(label1, 1, 2, 0, 0 + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

			var label2 = new Label { LabelProp = "Добавить?" };
			CheckBoxPlace.Attach(label2, 2, 3, 0, 0 + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

			var label3 = new Label { LabelProp = "Количество" };
			CheckBoxPlace.Attach(label3, 3, 4, 0, 0 + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);
			#endregion

			var items = ViewModel.SizeItems;
			for(uint i = 1; i <= rows; i++) {
				var item = items[(int)i - 1];
				var label = new Label { LabelProp = item.Size.Name };
				CheckBoxPlace.Attach(label, 1, 2, i, i + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

				var check = new yCheckButton();
				check.Binding.AddBinding(item, x => x.IsUsed, w => w.Active).InitializeFromSource();
				CheckBoxPlace.Attach(check, 2, 3, i, i + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

				var spin = new ySpinButton(0,int.MaxValue,1);
				spin.Value = 0;
				spin.Binding.AddSource(item)
					.AddBinding(x => x.IsUsed, w => w.Sensitive)
					.AddBinding(x => x.Amount, w => w.ValueAsInt)
					.InitializeFromSource();
				CheckBoxPlace.Attach(spin, 3, 4, i, i + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);
			}
			if(HeightRequest > Screen.Height)
				table1.SetScrollAdjustments(new Adjustment(new IntPtr(ViewModel.SizeItems.Count)),null);
			CheckBoxPlace.ShowAll();
		}

		/// <summary>
		/// Если true - кнопка выделяет всё , если false кнопка снимает все выделения
		/// </summary>
		private bool checkAll = true;
		private void selectAllButton_Clicked (object sender,EventArgs e) {
			ViewModel.SizeItems.ToList().ForEach(x => x.IsUsed = checkAll);
			selectAllButton.Label = checkAll ? "  Снять все " : "Выделить всё";
			checkAll = !checkAll;
		}

		private void OnAddButtonClicked(object sender, EventArgs e) {
			ViewModel.AddSizes();
		}
	}
}
