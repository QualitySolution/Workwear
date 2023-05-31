using QS.Views;
using workwear.Journal.Filter.ViewModels.Company;

namespace workwear.Journal.Filter.Views.Company 
{
	public partial class PostFilterView : ViewBase<PostFilterViewModel>
	{
		public PostFilterView(PostFilterViewModel viewModel) : base(viewModel) 
		{
			this.Build();
			entitySubdivision.ViewModel = viewModel.EntrySubdivision;
			entryDepartment.ViewModel = ViewModel.EntryDepartment;
		}
	}
}
