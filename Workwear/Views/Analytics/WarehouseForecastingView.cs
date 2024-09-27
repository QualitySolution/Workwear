using System;
using System.ComponentModel;
using System.Linq;
using Gamma.Utilities;
using QS.Views;
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

			entryWarehouse.ViewModel = ViewModel.WarehouseEntry;
			dateEnd.HideButtonClearDate = true;
			dateEnd.Binding.AddBinding(ViewModel, v => v.EndDate, w => w.Date).InitializeFromSource();
			comboDetail.ItemsEnum = typeof(Granularity);
			comboDetail.Binding.AddBinding(ViewModel, v => v.Granularity, w => w.SelectedItem).InitializeFromSource();
		}

		private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(ViewModel.ForecastColumns)) {
				RecreateColumns();
			}
		}

		void RecreateColumns() {
			var conf = treeItems.CreateFluentColumnsConfig<WarehouseForecastingItem>()
				.AddColumn("Номенклатура нормы").HeaderAlignment(0.5f).AddReadOnlyTextRenderer(x => x.ProtectionTool.Name).WrapWidth(500)
				.AddColumn("Пол").HeaderAlignment(0.5f).AddReadOnlyTextRenderer(x => x.Sex.GetEnumShortTitle()).XAlign(0.5f)
				.AddColumn("Размер/Рост").HeaderAlignment(0.5f).AddReadOnlyTextRenderer(x => x.SizeText).XAlign(0.5f)
				.AddColumn("На\nскладе").HeaderAlignment(0.5f).AddReadOnlyTextRenderer(x => x.InStock > 0 ? $"{x.InStock}" : "").XAlign(0.5f)
				.AddColumn("Просро-\nченное").HeaderAlignment(0.5f).AddReadOnlyTextRenderer(x => x.Unissued > 0 ? $"-{x.Unissued}" : "").XAlign(0.5f);
			
			for(int i = 0; i < ViewModel.ForecastColumns.Length; i++) {
				int col = i;
				conf.AddColumn(ViewModel.ForecastColumns[i].Title).HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Forecast[col] > 0 ? $"-{x.Forecast[col]}" : "")
					.AddSetter((c,n) => c.Foreground = n.ForecastColours[col])
					.XAlign(0.5f);
			}
			conf.AddColumn("Остаток без \nпросроченной")
				.AddReadOnlyTextRenderer(x => $"{x.InStock - x.Forecast.Sum()}")
				.AddSetter((c,n) => c.Foreground = n.InStock - n.Forecast.Sum() < 0 ? "red" : "green")
				.XAlign(0.5f);
			
			conf.AddColumn("Остаток c \nпросроченной")
				.AddReadOnlyTextRenderer(x => $"{x.InStock - x.Unissued - x.Forecast.Sum()}")
				.AddSetter((c,n) => c.Foreground = n.InStock - n.Unissued - n.Forecast.Sum() < 0 ? "red" : "green")
				.XAlign(0.5f);

			conf.AddColumn("").Finish();
		}

		protected void OnButtonFillClicked(object sender, EventArgs e) {
			ViewModel.Fill();
		}
	}
}
