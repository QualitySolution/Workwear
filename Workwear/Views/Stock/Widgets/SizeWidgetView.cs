using System;
using System.Linq;
using System.Collections.Generic;
using Gamma.Widgets;
using Gtk;
using QS.Project;
using QS.Views.Dialog;
using workwear.Measurements;
using workwear.ViewModels.Stock.Widgets;

namespace workwear.Views.Stock.Widgets
{
	public partial class SizeWidgetView : DialogViewBase<SizeWidgetViewModel>
	{
		private string currentGrowth;
		private IList<CheckBoxItem> checkBoxItemList { get; set; }
		public SizeWidgetView(SizeWidgetViewModel model) : base(model)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			if(ViewModel.IsUseGrowth) {
				GrowthBox.SetRenderTextFunc<string>(s => s);
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

			checkBoxItemList = new List<CheckBoxItem>();

			//Выбираем первый элемент из ViewModel.WearGrowths, если рост используется
			if(ViewModel.IsUseGrowth)
				GrowthBox.SelectedItem = ViewModel.WearGrowths.First();

		}
		/// <summary>
		/// Заполняет CheckBoxPlace таблицу на основе модели
		/// </summary>
		private void ConfigureCheckBoxPlace()
		{
			uint rows = (uint)ViewModel.WearSizes.Count;
			CheckBoxPlace.Resize(rows, 4);

			var sizes = ViewModel.WearSizes;
			for(uint i = 0; i < rows; i++) {

				Label label = new Label() { LabelProp = sizes[(int)i] };
				CheckBoxPlace.Attach(label, 1, 2, i, i + 1, AttachOptions.Expand, AttachOptions.Expand, 0, 0);

				CheckButton check = new CheckButton();
				check.Clicked += CheckButton_Clicked; 
				CheckBoxPlace.Attach(check, 2, 3, i, i + 1, AttachOptions.Expand, AttachOptions.Expand, 0, 0);

				CheckBoxItem checkBoxItem = new CheckBoxItem(label, sizes[(int)i], check);
				checkBoxItemList.Add(checkBoxItem);
			}

			if(this.HeightRequest > Screen.Height)
				table1.SetScrollAdjustments(new Adjustment(new IntPtr(checkBoxItemList.Count)),null);


			CheckBoxPlace.ShowAll();
		}

		private void GrowthInfoComboBox_Changed(object sender, EventArgs e)
		{
			currentGrowth = (string)GrowthBox.SelectedItem;
			ConfigureCheckBoxPlace();
		}

		private void CheckButton_Clicked(object sender , EventArgs e)
		{
			CheckButton check = (CheckButton)sender;
			if(check.Active) {
				AddButton.Sensitive = true;
			}
			else {
				foreach(var item in checkBoxItemList) {
					if(item.Check.Active == true) {
						AddButton.Sensitive = true;
						return;
					}
				}
				AddButton.Sensitive = false;
				return;
			}
		}

		/// <summary>
		/// Если true - кнопка выделяет всё , если false кнопка снимает все выделения
		/// </summary>
		private bool checkAll = true;
		private void selectAllButton_Clicked (object sender,EventArgs e)
		{
			checkBoxItemList.ToList().ForEach(i => i.Check.Active = checkAll);
			if(checkAll)
				selectAllButton.Label = "  Снять все ";
			else
				selectAllButton.Label = "Выделить всё";
			checkAll = !checkAll;
		}

		protected void OnAddButtonClicked(object sender, EventArgs e)
		{
			var items = checkBoxItemList.Where(i => i.Check.Active == true).Select(i => i.Size);
			ViewModel.AddSizes(currentGrowth,items);
		}
	}

	/// <summary>
	/// Класс для обработки CheckBox-ов
	/// </summary>
	public class CheckBoxItem
	{
		public Label Label { get; set; }
		public string Size { get;private set; }
		public CheckButton Check { get; set; }


		public CheckBoxItem(Label label,string size,CheckButton check )
		{
			Label = label;
			Size = size;
			Check = check;
		}

	}
}
