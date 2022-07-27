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
				ViewModel, vm => vm.SelectClaimState, v => v.SelectedItemOrNull);
			ytreeClaims.ColumnsConfig = ColumnsConfigFactory.Create<Claim>()
				.AddColumn("Обращение").AddTextRenderer(c => c.Title)
				.RowCells()
				.AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = GetRowColor(x))
				.Finish();
			ViewModel.RefreshClaims();
			ytreeClaims.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Claims, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectClaim, w=> w.SelectedRow)
				.InitializeFromSource();
			ytreeClaimMessages.ColumnsConfig = ColumnsConfigFactory.Create<ClaimMessage>()
				.AddColumn("Автор").AddTextRenderer(c => c.SenderName)
				.AddColumn("Текст").AddTextRenderer(c => c.Text)
				.AddColumn("Время").AddTextRenderer(c => c.SendTime.ToDateTime().ToString("dd MMM HH:mm:ss"))
				.RowCells()
				.AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.UserRead ? null : "gray")
				.Finish();
			ytreeClaimMessages.Binding
				.AddBinding(ViewModel, vm => vm.MessagesSelectClaims, w => w.ItemsDataSource)
				.InitializeFromSource();
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
			ytreeClaims.Vadjustment.ValueChanged += OnScroll;
		}

		private void OnScroll(object sender, EventArgs e) {
			if(ytreeClaims.Vadjustment.Value + ytreeClaims.Vadjustment.PageSize < ytreeClaims.Vadjustment.Upper)
				return;
			if(!ViewModel.UploadClaims())
				return;
			var lastPos = ytreeClaims.Vadjustment.Upper;
			ytreeClaims.ItemsDataSource = ViewModel.Claims;
			ytreeClaims.Vadjustment.Value = lastPos;
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
