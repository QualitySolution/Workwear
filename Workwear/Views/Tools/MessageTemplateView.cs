using System;
using QS.DomainModel.UoW;
using QS.Views.Dialog;
using workwear.Domain.Tools;
using workwear.ViewModels.Tools;

namespace workwear.Views.Tools
{
	public partial class MessageTemplateView : EntityDialogViewBase<MessageTemplateViewModel, MessageTemplate>
	{
		public MessageTemplateView(MessageTemplateViewModel viewModel): base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		void ConfigureDlg()
		{
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entityTitle.Binding.AddBinding(Entity, e => e.MessageTitle, w => w.Text).InitializeFromSource();
			ytextText.Binding.AddBinding(Entity, e => e.MessageText, w => w.Buffer.Text).InitializeFromSource();
		}
	}
}
