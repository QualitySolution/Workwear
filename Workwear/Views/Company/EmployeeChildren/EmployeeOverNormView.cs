using Gamma.Utilities;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace Workwear.Views.Company.EmployeeChildren {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeOverNormView : Gtk.Bin {
		public EmployeeOverNormView() {
			Build();
		}

		private EmployeeOverNormViewModel viewModel;
		public EmployeeOverNormViewModel ViewModel {
			get => viewModel; 
			set {
				viewModel = value;
				CreateTable();
			}
		}

		private void CreateTable()
		{
			ytreeviewOverNorm.CreateFluentColumnsConfig<EmployeeOverNormNode>()
////1289 Стоит разложить на детали
				.AddColumn("Тип выдачи").AddTextRenderer (e => e.DocType.GetEnumTitle())
				.AddColumn("Дата выдачи").AddTextRenderer (e => e.DateString)
                .AddColumn ("Наименование").AddTextRenderer (e => e.NomenclatureName).WrapWidth(700)
				.AddColumn ("Размер").AddTextRenderer (e => e.WearSize)
				.AddColumn ("Рост").AddTextRenderer (e => e.Height)
				.AddColumn ("Количество").AddTextRenderer (e => e.BalanceText)
				.AddColumn ("Стоимость").AddTextRenderer (e => e.AvgCostText)
				.AddColumn ("Износ при выдаче").AddTextRenderer (e => e.WearPercent.ToString("P0"))
				.AddColumn("") //Заглушка, чтобы не расширялось
				.Finish();
			ytreeviewOverNorm.Binding.AddBinding(ViewModel, v => v.ObservableItems, w => w.ItemsDataSource);
		}
	}
}
