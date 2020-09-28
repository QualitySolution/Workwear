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
		private WearGrowth currentGrowth;
		private IList<CheckBoxItem> checkBoxItemList { get; set; }
		public SizeWidgetView(SizeWidgetViewModel model) : base(model)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			if(ViewModel.IsUseGrowth) {
				GrowthBox.SetRenderTextFunc<WearGrowth>((growth) => growth.Name);
				GrowthBox.ItemsList = ViewModel.WearGrowths;
				GrowthBox.Changed += GrowthInfoComboBox_Changed;
				GrowthInfoBox.PackEnd(GrowthBox);

				GrowthBox.ShowAll();

				WearViewTypePlace.Visible = true;

			}
			else {
				GrowthInfoBox.Visible = false;
				WearViewTypePlace.Visible = false;
				ConfigureCheckBoxPlace();
			}
			AddButton.Sensitive = false;
			checkBoxItemList = new List<CheckBoxItem>();

			//Выбираем первый элемент из ViewModel.WearGrowths, если рост используется
			if(ViewModel.IsUseGrowth)
				GrowthBox.SelectedItem = ViewModel.WearGrowths.First();

		}

		private void ConfigureCheckBoxPlace()
		{
			uint rows = (uint)ViewModel.WearSizes.Count;
			CheckBoxPlace.Resize(rows, 4);

			var sizes = ViewModel.WearSizes;
			for(uint i = 0; i < rows; i++) {

				Label label = new Label() { LabelProp = sizes[(int)i].Names.First() };
				CheckBoxPlace.Attach(label, 1, 2, i, i + 1, AttachOptions.Expand, AttachOptions.Expand, 0, 0);

				CheckButton check = new CheckButton();
				check.Clicked += CheckButton_Clicked; 
				CheckBoxPlace.Attach(check, 2, 3, i, i + 1, AttachOptions.Expand, AttachOptions.Expand, 0, 0);

				CheckBoxItem checkBoxItem = new CheckBoxItem(label, sizes[(int)i], check);
				checkBoxItemList.Add(checkBoxItem);

				DesignationTypeChangeEvent += checkBoxItem.RefreshSizeLabel;
			}

			CheckBoxPlace.ShowAll();
		}

		private System.Action<WearSizeDesignationType> DesignationTypeChangeEvent;

		private void GrowthInfoComboBox_Changed(object sender, EventArgs e)
		{
			currentGrowth = (WearGrowth)GrowthBox.SelectedItem;
			ConfigureCheckBoxPlace();
		}

		private WearSizeDesignationType currentType = WearSizeDesignationType.Number;

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

		private void RadiButton_group1_Toggled(object sender, EventArgs e)
		{
			string prop = ((sender as RadioButton).Child as Label).LabelProp;
			if(prop == "\"44\"" && currentType != WearSizeDesignationType.Number) {
				DesignationTypeChangeEvent(WearSizeDesignationType.Number);
				currentType = WearSizeDesignationType.Number;
			}
			else if(prop == "\"XXS\"" && currentType != WearSizeDesignationType.Symbols) {
				DesignationTypeChangeEvent(WearSizeDesignationType.Symbols);
				currentType = WearSizeDesignationType.Symbols;
			}
		}

		private void selectAllButton_Clicked (object sender,EventArgs e)
		{
			checkBoxItemList.ToList().ForEach(i => i.Check.Active = true);
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
		public WearSize Size { get;private set; }
		public CheckButton Check { get; set; }


		public CheckBoxItem(Label label,WearSize size,CheckButton check )
		{
			Label = label;
			Size = size;
			Check = check;
		}

		public void RefreshSizeLabel(WearSizeDesignationType designationType)
		{
			if(designationType == WearSizeDesignationType.Number)
				Label.LabelProp = Size.Names.First();
			if(designationType == WearSizeDesignationType.Symbols) {
				if(Size.Names.Last() == null) {
					string prop = "";
					foreach(var s in Size.Appropriated) {
						prop += "/" + s;
					}
				}
				else
					Label.LabelProp = Size.Names.Last();
			}
		}
	}

	/// <summary>
	/// Отображение размеров одежды
	/// </summary>
	public enum WearSizeDesignationType
	{
		/// <summary>
		/// Возвращать размеры в виде числе (пример: 44)
		/// </summary>
		Number ,
		/// <summary>
		/// Отображать размеры в виде символов (пример: XXS)
		/// </summary>
		Symbols
	}
}
