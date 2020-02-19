using System;
using Gtk;
using QS.DomainModel.Entity;
using QS.Views.Dialog;
using QSOrmProject;
using workwear.Domain.Stock;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class WarehouseTransferView : EntityDialogViewBase<WarehouseTransferViewModel, Transfer>
	{
		public WarehouseTransferView(WarehouseTransferViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			datepicker.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			entryNumber.Binding.AddBinding(Entity, e => e.Id, w => w.Text, new IdToStringConverter()).InitializeFromSource();
			entryUser.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? DomainHelper.GetObjectTilte(e.CreatedbyUser) : String.Empty, w => w.Text).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer).InitializeFromSource();
			entityentryWarehouseFrom.ViewModel = ViewModel.WarehouseFromEntryViewModel;
			entityentryWarehouseTo.ViewModel = ViewModel.WarehouseToEntryViewModel;


			table.CreateFluentColumnsConfig<TransferItem>()
			.AddColumn("Наименование").Tag("Name").AddTextRenderer(x => x.Nomenclature!= null ? x.Nomenclature.Name : String.Empty)
			.AddColumn("Размер").Tag("Size").AddTextRenderer(x => x.Nomenclature.Size)
			.AddColumn("Рост").AddTextRenderer(x => x.Nomenclature != null ? x.Nomenclature.WearGrowth : String.Empty)
			.AddColumn("Количество").Tag("Count")
				.AddNumericRenderer(x => x.Amount, false).Editing(true).Adjustment(new Adjustment(1, 0, 100000, 1, 10, 10)).WidthChars(8)
				.AddTextRenderer(x => x.Nomenclature != null && x.Nomenclature.Type.Units != null ? x.Nomenclature.Type.Units.Name : String.Empty,  false)
			.Finish();

			table.Selection.Changed += Selection_Changed;
			table.Selection.Mode = SelectionMode.Multiple;
			table.ItemsDataSource = ViewModel.Entity.ObservableItems;
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveItem.Sensitive =  table.Selection.CountSelectedRows() > 0 ;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddItems();
		}

		protected void OnButtonDelClicked(object sender, EventArgs e)
		{
			var items = table.GetSelectedObjects<TransferItem>();
			ViewModel.RemoveItems(items);
		}

		protected void OnTableRowActivated(object o, RowActivatedArgs args)
		{
			if(!ViewModel.CanEditItems)
				return;
		}
	}
}
