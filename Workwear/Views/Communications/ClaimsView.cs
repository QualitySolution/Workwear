using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.Cloud.WearLk.Manage;
using QS.Views.Dialog;
using Workwear.ViewModels.Communications;

namespace workwear.Views.Communications 
{
	public partial class ClaimsView : DialogViewBase<ClaimsViewModel> 
	{
		public ClaimsView(ClaimsViewModel viewModel) : base(viewModel) 
		{
			this.Build();

			yComboStatus.ItemsEnum = typeof(ClaimsViewModel.TranslateClaimState);
			yComboStatus.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.SelectClaimState, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			ytreeClaims.ColumnsConfig = ColumnsConfigFactory.Create<Claim>()
				.AddColumn("Обращение").AddTextRenderer(c => c.Title)
				.RowCells()
				.AddSetter<Gtk.CellRendererText>((c, x) => c.Weight = GetWeight(x))
				.Finish();
			ViewModel.RefreshClaims();
			ytreeClaims.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Claims, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectClaim, w=> w.SelectedRow)
				.InitializeFromSource();
			ytreeClaimMessages.ColumnsConfig = ColumnsConfigFactory.Create<ClaimMessage>()
				.AddColumn("Время").AddTextRenderer(c => c.SendTime.ToDateTime().ToString("dd MMM HH:mm:ss"))
				.AddColumn("Автор").AddTextRenderer(c => c.SenderName)
				.AddColumn("Текст").AddTextRenderer(c => c.Text)
				.RowCells()
				.AddSetter<Gtk.CellRendererText>((c, x) => c.Weight = x.UserRead ? 400 : 600)
				.Finish();
			ytreeClaimMessages.Selection.Mode = SelectionMode.None;
			ytreeClaimMessages.Binding
				.AddBinding(ViewModel, vm => vm.MessagesSelectClaims, w => w.ItemsDataSource)
				.InitializeFromSource();
			yentryMessage.Binding
				.AddBinding(ViewModel, vm => vm.TextMessage, w => w.Buffer.Text)
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
			ytreeClaims.Vadjustment.ValueChanged += OnScroll;
		}
		

		private void OnScroll(object sender, EventArgs e) {
			if(ytreeClaims.Vadjustment.Value + ytreeClaims.Vadjustment.PageSize < ytreeClaims.Vadjustment.Upper)
				return;
			if(!ViewModel.UploadClaims())
				return;
			var lastPos = ytreeClaims.Vadjustment.Upper;
			ytreeClaims.Vadjustment.Value = lastPos;
		}

		private int GetWeight(Claim claim) {
			switch(claim.ClaimState) {
				case ClaimState.Closed:
					return 200;
				case ClaimState.WaitSupport:
					return 600;
				case  ClaimState.WaitUser:
					return 400;
				default:
					throw new ArgumentException();
			}
		}
	}
}
