using System;
using System.ComponentModel;
using Gamma.Utilities;
using QS.Dialog.GtkUI;
using QS.Views;
using Workwear.Models.Analytics.WarehouseForecasting;
using Workwear.ViewModels.Analytics;

namespace Workwear.Views.Analytics {

	public partial class WarehouseForecastingView : ViewBase<WarehouseForecastingViewModel> {

		public WarehouseForecastingView(WarehouseForecastingViewModel viewModel) : base(viewModel) {
			this.Build();
			
			treeItems.Binding.AddBinding(ViewModel, v => v.Items, w => w.ItemsDataSource);
			RecreateColumns();
			ViewModel.PropertyChanged += ViewModelOnPropertyChanged;

			ViewModel.ProgressTotal = progressTotal;
			ViewModel.ProgressLocal = progressLocal;

			buttonFill.Binding.AddBinding(ViewModel, v => v.SensitiveFill, w => w.Sensitive).InitializeFromSource();
			buttonExcel.Binding.AddBinding(ViewModel, v => v.SensitiveExport, w => w.Sensitive).InitializeFromSource();

			entryWarehouse.ViewModel = ViewModel.WarehouseEntry;
			entryWarehouse.Binding.AddBinding(ViewModel, v => v.SensitiveSettings, w => w.Sensitive).InitializeFromSource();
			dateEnd.HideButtonClearDate = true;
			dateEnd.Binding.AddSource(ViewModel)
				.AddBinding(v => v.EndDate, w => w.Date)
				.AddBinding(v => v.SensitiveSettings, w => w.Sensitive)
				.InitializeFromSource();
			comboDetail.ItemsEnum = typeof(Granularity);
			comboDetail.Binding.AddSource(ViewModel)
				.AddBinding(v => v.Granularity, w => w.SelectedItem)
				.AddBinding(v => v.SensitiveSettings, w => w.Sensitive)
				.InitializeFromSource();
			comboShowMode.ItemsEnum = typeof(WarehouseForecastingShowMode);
			comboShowMode.Binding.AddSource(ViewModel)
				.AddBinding(v => v.ShowMode, w => w.SelectedItem)
				.AddBinding(v => v.SensitiveSettings, w => w.Sensitive)
				.InitializeFromSource();
			
			comboNomenclatureMode.ItemsEnum = typeof(ForecastingNomenclatureType);
			comboNomenclatureMode.Binding.AddSource(ViewModel)
				.AddBinding(v => v.NomenclatureType, w => w.SelectedItem)
				.AddBinding(v => v.SensitiveSettings, w => w.Sensitive)
				.InitializeFromSource();

			comboPriceMode.ItemsEnum = typeof(ForecastingPriceType);
			comboPriceMode.Binding.AddSource(ViewModel)
				.AddBinding(v => v.PriceType, w => w.SelectedItem)
				.AddBinding(v => v.SensitiveSettings, w => w.Sensitive)
				.InitializeFromSource();
			
			enumbuttonCreateShipment.ItemsEnum = typeof(ShipmentCreateType);
			enumbuttonCreateShipment.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateShipment, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.ShowCreateShipment, w => w.Visible)
				.InitializeFromSource();
			
			choiceNomenclature.Binding.AddBinding(ViewModel, v => v.ChoiceGoodsViewModel, w => w.ViewModel).InitializeFromSource();
		}

		private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(ViewModel.ForecastColumns)) 
				RecreateColumns();
		}

		void RecreateColumns() {
			var conf = treeItems.CreateFluentColumnsConfig<WarehouseForecastingItem>()
				.AddColumn(ViewModel.NomenclatureType.GetEnumTitle()).HeaderAlignment(0.5f)
					.ToolTipText(n => n.NomenclaturesText)
					.AddReadOnlyTextRenderer(x => x.Name).WrapWidth(500)
						.AddSetter((c,n) => c.Foreground = n.NameColor)
				.AddColumn("Пол").HeaderAlignment(0.5f).AddReadOnlyTextRenderer(x => x.Sex.GetEnumShortTitle()).XAlign(0.5f)
				.AddColumn("Размер/Рост").HeaderAlignment(0.5f).AddReadOnlyTextRenderer(x => x.SizeText).XAlign(0.5f)
				.AddColumn("На\nскладе").HeaderAlignment(0.5f)
					.ToolTipText(x => x.StockText)
					.AddReadOnlyTextRenderer(x => x.InStock > 0 ? $"{x.InStock}" : "").XAlign(0.5f)
				.AddColumn("Просро-\nченное").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Unissued > 0 ? $"-{x.Unissued}" : "")
					.AddSetter((c,n) => c.Foreground = n.InStock - n.Unissued < 0 ? "red" : "green")
					.XAlign(0.5f);
			
			for(int i = 0; i < ViewModel.ForecastColumns.Length; i++) {
				int col = i;
				conf.AddColumn(ViewModel.ForecastColumns[i].Title).HeaderAlignment(0.5f)
					.ToolTipText(x => $"Прогнозируемый остаток: {x.ForecastBalance[col]}")
					.AddReadOnlyTextRenderer(x => x.Forecast[col] > 0 ? $"-{x.Forecast[col]}" : "")
					.AddSetter((c,n) => c.Foreground = n.ForecastColours[col])
					.XAlign(0.5f);
			}
			conf.AddColumn("Остаток без \nпросроченной")
				.AddReadOnlyTextRenderer(x => x.WithoutDebt.ToString())
				.AddSetter((c,n) => c.Foreground = n.WithoutDebt < 0 ? "red" : "green")
				.XAlign(0.5f);
			
			conf.AddColumn("Остаток c \nпросроченной")
				.AddReadOnlyTextRenderer(x => x.WithDebt.ToString())
				.AddSetter((c,n) => c.Foreground = n.WithoutDebt < 0 ? "red" : "green")
				.XAlign(0.5f);

			conf.AddColumn("").Finish();
		}

		protected void OnButtonFillClicked(object sender, EventArgs e) =>
			ViewModel.Fill();
		protected void OnButtonExcelClicked(object sender, EventArgs e) =>
			ViewModel.ExportToExcel();
		protected void OnEnumbuttonCreateShipmentEnumItemClicked(object sender, QS.Widgets.EnumItemClickedEventArgs e) =>
			ViewModel.CreateShipment((ShipmentCreateType)e.ItemEnum);

		protected void OnButtonColorsLegendClicked(object sender, EventArgs e) {
			MessageDialogHelper.RunInfoDialog(
				"<b>Колонки с прогнозом выдач:</b>\n" +
				"<span color='green'>●</span> — склад полностью обеспечивает потребность (планируемые + просрочка)\n" +
				"<span color='orange'>●</span> — на складе достаточное количество для планируемых выдач, без просрочки\n" +
				"<span color='red'>●</span> — недостаточное количество для планируемых выдач"
			);
		}
	}
}
