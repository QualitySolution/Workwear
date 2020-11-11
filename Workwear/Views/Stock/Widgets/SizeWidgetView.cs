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
		private string currentGrowth = "";
		private IList<CheckBoxItem> checkBoxItemList = new List<CheckBoxItem>();
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

			//Выбираем первый элемент из ViewModel.WearGrowths, если рост используется
			if(ViewModel.IsUseGrowth)
				GrowthBox.SelectedItem = ViewModel.WearGrowths.First();

			#region OtherSettings
			#endregion

		}

		/// <summary>
		/// Заполняет CheckBoxPlace таблицу на основе модели
		/// </summary>
		private void ConfigureCheckBoxPlace()
		{
			checkBoxItemList.Clear();
			CheckBoxPlace.Children.ToList().ForEach(e => CheckBoxPlace.Remove(e));
			uint rows = (uint)ViewModel.WearSizes.Count;
			CheckBoxPlace.Resize(rows+1, 4);

			#region пояснения к таблице
			Label label_1 = new Label() { LabelProp = "Размер"};
			CheckBoxPlace.Attach(label_1, 1, 2, 0, 0 + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

			Label label_2 = new Label() { LabelProp = "Добавить?" };
			CheckBoxPlace.Attach(label_2, 2, 3, 0, 0 + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

			Label label_3 = new Label() { LabelProp = "Количество" };
			CheckBoxPlace.Attach(label_3, 3, 4, 0, 0 + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);
			#endregion

			var sizes = ViewModel.WearSizes;
			for(uint i = 1; i <= rows; i++) {

				Label label = new Label() { LabelProp = sizes[(int)i-1] };
				CheckBoxPlace.Attach(label, 1, 2, i, i + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

				CheckButton check = new CheckButton();
				check.Clicked += CheckButton_Clicked; 
				CheckBoxPlace.Attach(check, 2, 3, i, i + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

				SpinButton spin = new SpinButton(0,int.MaxValue,1);
				spin.Sensitive = false;
				CheckBoxPlace.Attach(spin, 3, 4, i, i + 1, AttachOptions.Expand, AttachOptions.Shrink, 0, 0);

				CheckBoxItem checkBoxItem = new CheckBoxItem(label, sizes[(int)i-1], check,spin);
				checkBoxItemList.Add(checkBoxItem);
			}

			if(this.HeightRequest > Screen.Height)
				table1.SetScrollAdjustments(new Adjustment(new IntPtr(checkBoxItemList.Count)),null);
			CheckBoxPlace.ShowAll();

			ExcludeExistingSizes();
		}

		/// <summary>
		/// Исключает существующие размеры ,
		/// </summary>
		private void ExcludeExistingSizes()
		{
			if(ViewModel.ExcludedSizesDictionary != null && ViewModel.ExcludedSizesDictionary.Keys!= null && ViewModel.ExcludedSizesDictionary.Keys.Any(growth => growth == currentGrowth)) {
				foreach(var vs in checkBoxItemList) {
					foreach(var s in ViewModel.ExcludedSizesDictionary[currentGrowth]) {
						if(vs.Size == s) {
							vs.Label.TooltipText = "Данный размер с данным ростом уже добавлен в список накладной";
							vs.Check.TooltipText = "Данный размер с данным ростом уже добавлен в список накладной";
							vs.Check.Active = true;
							vs.Spin.TooltipText = "Данный размер с данным ростом уже добавлен в список накладной";
							vs.Spin.Visibility = false;
							vs.isUsed = true;
							vs.SetSensitive(false);
						}
					}
				}
			}
		}

		private void GrowthInfoComboBox_Changed(object sender, EventArgs e)
		{
			currentGrowth = (string)GrowthBox.SelectedItem;
			ConfigureCheckBoxPlace();
		}

		private void CheckButton_Clicked(object sender , EventArgs e)
		{
			CheckButton check = (CheckButton)sender;
			var item = checkBoxItemList.Where(i => i.Check == check).FirstOrDefault().Spin;
			if(item != null)
				item.Sensitive = check.Active;
			AddButton.Sensitive = checkBoxItemList.Any(i => i.Check.Active == true && i.isUsed == false);
		}

		/// <summary>
		/// Если true - кнопка выделяет всё , если false кнопка снимает все выделения
		/// </summary>
		private bool checkAll = true;
		private void selectAllButton_Clicked (object sender,EventArgs e)
		{
			checkBoxItemList.Where(i=> i.Label.Sensitive != false).ToList().ForEach(i => i.Check.Active = checkAll);
			if(checkAll)
				selectAllButton.Label = "  Снять все ";
			else
				selectAllButton.Label = "Выделить всё";
			checkAll = !checkAll;
		}

		protected void OnAddButtonClicked(object sender, EventArgs e)
		{
			Dictionary<string, int> sizes = new Dictionary<string, int>();
			checkBoxItemList.Where(i => i.Check.Active == true && i.isUsed == false).ToList().ForEach(i => 
			{
				sizes.Add(i.Size, i.Spin.ValueAsInt);
			});
			ViewModel.AddSizes(currentGrowth,sizes);
		}
	}

	/// <summary>
	/// Класс для обработки CheckBox-ов
	/// </summary>
	public class CheckBoxItem
	{
		public Label Label { get; set; }
		public string Size { get;private set; }
		public CheckButton Check { get;private set; }
		public SpinButton Spin { get; private set; }
		public bool isUsed { get; set; } = false;

		public CheckBoxItem(Label label,string size,CheckButton check,SpinButton spin)
		{
			Label = label;
			Size = size;
			Check = check;
			Spin = spin;
		}

		/// <summary>
		/// Устанавливает sensetive для всех входящих в класс элементов
		/// </summary>
		public void SetSensitive(bool sensitive)
		{
			Label.Sensitive = sensitive;
			Check.Sensitive = sensitive;
			Spin.Sensitive = sensitive;
		}
	}
}
