using System;
using QS.Views.Dialog;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
	public partial class InspectionView : EntityDialogViewBase<InspectionViewModel, Inspection> {
		public InspectionView(InspectionViewModel viewModel) : base(viewModel) {
			this.Build();
		}
	}
}
