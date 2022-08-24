using System;
using QS.Views.Dialog;
using workwear.ViewModels.Import;

namespace workwear.Views.Import 
{
	public partial class IncomeImportView : DialogViewBase<IncomeImportViewModel>
	{
		public IncomeImportView(IncomeImportViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
