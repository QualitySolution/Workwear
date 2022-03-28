using QS.Views.Dialog;
using workwear.Domain.Stock;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class IncomeView : EntityDialogViewBase<IncomeViewModel, Income>
	{
		public IncomeView(IncomeViewModel viewModel): base(viewModel)
		{
			this.Build();
		}
	}
}
