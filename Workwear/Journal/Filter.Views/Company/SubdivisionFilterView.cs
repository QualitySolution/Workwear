using QS.Views;
using workwear.Journal.Filter.ViewModels.Company;

namespace workwear.Journal.Filter.Views.Company
{
	public partial class SubdivisionFilterView : ViewBase<SubdivisionFilterViewModel>
	{
		public SubdivisionFilterView(SubdivisionFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ycheckSortByParent.Binding
			.AddBinding(viewModel, vm => vm.SortByParent, w => w.Active)
				.InitializeFromSource();
		}
	}
}
