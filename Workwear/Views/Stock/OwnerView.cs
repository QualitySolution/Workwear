using QS.Views.Dialog;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock 
{
	public partial class OwnerView : EntityDialogViewBase<OwnerViewModel, Owner>
	{
		public OwnerView(OwnerViewModel viewModel) : base(viewModel)
		{
			this.Build();

			CommonButtonSubscription();
			
			yentryName.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();
			
			ytextviewComment.Binding
				.AddBinding(Entity, e => e.Description, w => w.Buffer.Text)
				.InitializeFromSource();
		}
	}
}
