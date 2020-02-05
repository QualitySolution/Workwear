using System;
using QS.Views.Dialog;
using workwear.Domain.Stock;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{

	public partial class WarehouseView : EntityDialogViewBase<WarehouseViewModel, WareHouse>
	{
		public WarehouseView(WarehouseViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			entityname.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entitysubdivision.ViewModel = ViewModel.SubdivisionEntryViewModel;
	
		}


	}
}
