using System;
using QS.Views.Dialog;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock 
{
	public partial class BarcodeView : EntityDialogViewBase<BarcodeViewModel, Barcode> 
	{
		public BarcodeView(BarcodeViewModel viewModel) : base(viewModel) 
		{
			this.Build();
			CommonButtonSubscription();
			
			ylabelCodeValue.Binding
				.AddBinding(Entity, e => e.Title, w => w.Text)
				.InitializeFromSource();

			treeviewOperations.CreateFluentColumnsConfig<BarcodeOperation>()
				.AddColumn("Дата").AddTextRenderer(x => $"{x.OperationDate:d}")
				.AddColumn("Операция").AddTextRenderer(x => x.OperationTitle)
				.Finish();

			treeviewOperations.ItemsDataSource = Entity.BarcodeOperations;
		}
	}
}
