using System;
using System.Linq;
using QS.Views.Dialog;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
	public partial class BarcodingView : EntityDialogViewBase<BarcodingViewModel, Barcoding> {
		public BarcodingView(BarcodingViewModel viewModel) : base(viewModel) {
			Build();
			ConfigureDlg();
			ConfigureItems();
			CommonButtonSubscription();
		}

		private void ConfigureDlg() {
			
			entryId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocNumberText, w => w.Text)
				.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive)
				.InitializeFromSource();
			checkAuto.Binding.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active).InitializeFromSource(); 
			
			ylabelUser.Binding
				.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource();
			ydateDoc.Binding
				.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			labelSum.Binding
				.AddBinding(ViewModel, vm => vm.Total, w => w.LabelProp).InitializeFromSource();
			ytreeItems.Selection.Changed += Items_Selection_Changed;
			entityWarehouse.ViewModel = ViewModel.EntryWarehouseViewModel;
			ybuttonAdd.Clicked += OnButtonAddClicked;
			ybuttonDel.Clicked += OnButtonDelClicked;
			buttonPrint.Clicked += OnButtonPrintClicked;
		}

		private void ConfigureItems()
		{
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<BarcodingItem> ()
				.AddColumn ("Наименование").Resizable()
					.ToolTipText(x => $"ИД номенклатуры: {x.Nomenclature.Id}")
					.AddTextRenderer (e => e.Nomenclature.Name).WrapWidth(500)
				.AddColumn ("Размер")
					.AddTextRenderer (e => e.SizeName)
				.AddColumn ("Рост")
					.AddTextRenderer (e => e.HeightName)
				.AddColumn("Количество")
					.AddTextRenderer (e => e.Amount.ToString())
				.AddColumn("Собственник").Resizable()
					.Visible(ViewModel.OwnersVisible)
					.AddTextRenderer(e => e.OwnerName)
				.AddColumn("Износ")
					.AddTextRenderer(e => e.WearPercent.ToString("P0"))
				.AddColumn ("Штрихкоды")
					.AddTextRenderer (e => e.Barcodes.Select(b => b.Title)
						.Aggregate((a, b) => (a+'\n'+b)))
				.Finish();
			ytreeItems.Binding
				.AddBinding(Entity, vm => vm.Items, w => w.ItemsDataSource)
				.InitializeFromSource();
		}
		
		private void OnButtonDelClicked(object sender, EventArgs e) => ViewModel.DeleteItem(ytreeItems.GetSelectedObject<BarcodingItem>());
		private void OnButtonAddClicked(object sender, EventArgs e) => ViewModel.AddItems();
		private void OnButtonPrintClicked(object sender, EventArgs e) => ViewModel.Print();
		private void Items_Selection_Changed(object sender, EventArgs e){
			ybuttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}
	}
}

