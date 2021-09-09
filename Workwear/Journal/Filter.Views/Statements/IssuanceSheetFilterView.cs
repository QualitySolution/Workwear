using QS.Views;
using workwear.Journal.Filter.ViewModels.Statements;

namespace workwear.Journal.Filter.Views.Statements
{
	public partial class IssuanceSheetFilterView : ViewBase<IssuanceSheetFilterViewModel>
	{
		public IssuanceSheetFilterView(IssuanceSheetFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			datePeriodDocs.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartDate, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
		}
	}
}
