using System;
using QS.Views.Dialog;
using Workwear.Domain.Communications;
using Workwear.ViewModels.Communications;

namespace Workwear.Views.Communications
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
