using System;
using QS.Views.Dialog;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company;

namespace Workwear.Views.Company
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
			entryCostCenter.ViewModel = ViewModel.EntryCostCenter;
			entryCostCenter.Binding.AddBinding(ViewModel, v => v.VisibleCostCenter, w => w.Visible).InitializeFromSource();
			labelCostCenter.Binding.AddBinding(ViewModel, v => v.VisibleCostCenter, w => w.Visible).InitializeFromSource();
			ytextComments.Binding.AddBinding(Entity, e => e.Comments, w => w.Buffer.Text).InitializeFromSource();
		}
	}
}
