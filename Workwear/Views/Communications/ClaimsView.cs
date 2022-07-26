using System;
using Gamma.GtkWidgets;
using QS.Cloud.WearLk.Manage;
using QS.Views.Dialog;
using workwear.ViewModels.Communications;

namespace workwear.Views.Communications 
{
	public partial class ClaimsView : DialogViewBase<ClaimsViewModel> 
	{
		public ClaimsView(ClaimsViewModel viewModel) : base(viewModel) 
		{
			this.Build();

			yenumcomboStatus.ItemsEnum = typeof(ClaimState);
			yenumcomboStatus.Binding.AddBinding(
				ViewModel, vm => vm.SelectClaimState, v => v.SelectedItem);
			ytreeClaims.ColumnsConfig = ColumnsConfigFactory.Create<Claim>()
				.AddColumn("Обращение").AddTextRenderer(c => c.Title)
				.RowCells()
				.AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = GetRowColor(x))
				.Finish();
			ViewModel.RefreshClaims();
			ytreeClaims.Binding
				.AddBinding(ViewModel, vm => vm.Claims, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeClaimMessages.ColumnsConfig = ColumnsConfigFactory.Create<ClaimMessage>()
				.AddColumn("Автор").AddTextRenderer(c => c.SenderName)
				.AddColumn("Текст").AddTextRenderer(c => c.Text)
				.AddColumn("Время").AddTextRenderer(c => c.SendTime.ToString())
				.RowCells()
				.AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.UserRead ? String.Empty : "gray")
				.Finish();
			yentryMessage.Binding
				.AddBinding(ViewModel, vm => vm.TextMessage, w => w.Text)
				.InitializeFromSource();
			ycheckbuttonShowClosed.Binding
				.AddBinding(ViewModel, vm => vm.ShowClosed, w => w.Active)
				.InitializeFromSource();
			ybuttonSend.Binding
				.AddBinding(ViewModel, vm => vm.SensitiveSend, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonChangeStatus.Binding
				.AddBinding(ViewModel, vm => vm.SensitiveChangeState, w => w.Sensitive)
				.InitializeFromSource();
			
			ybuttonSend.Clicked += ViewModel.Send;
			ybuttonChangeStatus.Clicked += ViewModel.ChangeStatusClaim;
		}

		private string GetRowColor(Claim claim) {
			switch(claim.ClaimState) {
				case ClaimState.Closed:
					return "gray";
				case ClaimState.WaitSupport:
					return "red";
				case  ClaimState.WaitUser:
					return "yellow";
				default:
					throw new ArgumentException();
			}
		}
	}
}
