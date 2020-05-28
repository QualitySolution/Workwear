using System;
using QS.Views.Dialog;
using workwear.Domain.Company;
using workwear.ViewModels.Company;

namespace workwear.Views.Company
{
	public partial class PostView : EntityDialogViewBase<PostViewModel, Post>
	{

		public PostView(PostViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		void ConfigureDlg()
		{
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entityProfession.ViewModel = ViewModel.EntryProfession;
			entitySubdivision.ViewModel = ViewModel.EntrySubdivision;
			entityDepartment.ViewModel = ViewModel.EntryDepartment;
			ytextComments.Binding.AddBinding(Entity, e => e.Comments, w => w.Buffer.Text).InitializeFromSource();
		}
	}
}
