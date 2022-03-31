using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using QS.Views.Dialog;
using workwear.Domain.Sizes;
using workwear.ViewModels.Stock.Widgets;

namespace workwear.Views.Stock.Widgets
{
	public partial class SizeWidgetView : DialogViewBase<SizeWidgetViewModel>
	{
		private Size currentGrowth;
		private readonly IList<CheckBoxItem> checkBoxItemList = new List<CheckBoxItem>();
		public SizeWidgetView(SizeWidgetViewModel model) : base(model) {
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg() {
			if(ViewModel.IsUseGrowth) {
				GrowthBox.SetRenderTextFunc<Size>(s => s.Name);
				GrowthBox.ItemsList = ViewModel.WearGrowths;
				GrowthBox.Changed += GrowthInfoComboBox_Changed;
				GrowthInfoBox.PackEnd(GrowthBox);
				GrowthBox.ShowAll();
			}
			else {
				GrowthInfoBox.Visible = false;
				ConfigureCheckBoxPlace();
			}
			AddButton.Sensitive = false;

			//Выбираем первый элемент из ViewModel.WearGrowths, если рост используется
			if(ViewModel.IsUseGrowth)
				GrowthBox.SelectedItem = ViewModel.WearGrowths.First();
		}
		/// <summary>
		/// Заполняет CheckBoxPlace таблицу на основе модели
		/// </summary>
		private void ConfigureCheckBoxPlace() {
			checkBoxItemList.Clear();
			CheckBoxPlace.Children.ToList().ForEach(e => CheckBoxPlace.Remove(e));
			var rows = (uint)ViewModel.WearSizes.Count;
			CheckBoxPlace.Resize(rows+1, 4);

			#region пояснения к таблице
			var label1 = new Label { LabelProp = "Размер"};
			CheckBoxPlace.Attach(label1, 1, 2, 0, 0 + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

			var label2 = new Label { LabelProp = "Добавить?" };
			CheckBoxPlace.Attach(label2, 2, 3, 0, 0 + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

			var label3 = new Label { LabelProp = "Количество" };
			CheckBoxPlace.Attach(label3, 3, 4, 0, 0 + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);
			#endregion

			var sizes = ViewModel.WearSizes;
			for(uint i = 1; i <= rows; i++) {
				var label = new Label { LabelProp = sizes[(int)i-1].Name };
				CheckBoxPlace.Attach(label, 1, 2, i, i + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

				var check = new CheckButton();
				check.Clicked += CheckButton_Clicked; 
				CheckBoxPlace.Attach(check, 2, 3, i, i + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

				var spin = new SpinButton(0,int.MaxValue,1);
				spin.Sensitive = false;
				CheckBoxPlace.Attach(spin, 3, 4, i, i + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

				var checkBoxItem = new CheckBoxItem(label, sizes[(int)i-1], check,spin);
				checkBoxItemList.Add(checkBoxItem);
			}
			if(HeightRequest > Screen.Height)
				table1.SetScrollAdjustments(new Adjustment(new IntPtr(checkBoxItemList.Count)),null);
			CheckBoxPlace.ShowAll();
		}

		private void GrowthInfoComboBox_Changed(object sender, EventArgs e) {
			currentGrowth = (Size)GrowthBox.SelectedItem;
			ConfigureCheckBoxPlace();
		}

		private void CheckButton_Clicked(object sender , EventArgs e) {
			var check = (CheckButton)sender;
			var item = checkBoxItemList.FirstOrDefault(i => i.Check == check)?.Spin;
			if(item != null)
				item.Sensitive = check.Active;
			AddButton.Sensitive = checkBoxItemList.Any(i => i.Check.Active == true && i.IsUsed == false);
		}
		/// <summary>
		/// Если true - кнопка выделяет всё , если false кнопка снимает все выделения
		/// </summary>
		private bool checkAll = true;
		private void selectAllButton_Clicked (object sender,EventArgs e) {
			checkBoxItemList.Where(i=> i.Label.Sensitive).ToList().ForEach(i => i.Check.Active = checkAll);
			selectAllButton.Label = checkAll ? "  Снять все " : "Выделить всё";
			checkAll = !checkAll;
		}

		private void OnAddButtonClicked(object sender, EventArgs e) {
			var sizes = new Dictionary<Size, int>();
			checkBoxItemList.Where(i => i.Check.Active && i.IsUsed == false).ToList().ForEach(i => 
			{
				sizes.Add(i.Size, i.Spin.ValueAsInt);
			});
			ViewModel.AddSizes(currentGrowth,sizes);
		}
	}

	/// <summary>
	/// Класс для обработки CheckBox-ов
	/// </summary>
	public class CheckBoxItem {
		public Label Label { get; }
		public Size Size { get; }
		public CheckButton Check { get; }
		public SpinButton Spin { get; }
		public bool IsUsed => false;
		public CheckBoxItem(Label label,Size size,CheckButton check,SpinButton spin) {
			Label = label;
			Size = size;
			Check = check;
			Spin = spin;
		}
	}
}
