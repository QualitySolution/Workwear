using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.Cloud.WearLk.Manage;
using QS.Views.Dialog;
using Workwear.ViewModels.Communications;

namespace Workwear.Views.Communications 
{
	public partial class ClaimsView : DialogViewBase<ClaimsViewModel> 
	{
		public ClaimsView(ClaimsViewModel viewModel) : base(viewModel) 
		{
			this.Build();
			
			ytreeClaims.ColumnsConfig = ColumnsConfigFactory.Create<Claim>()
				.AddColumn("Сотрудник").AddTextRenderer(c => ViewModel.GetEmployeeName(c.UserPhone))
				.AddColumn("Тема").AddTextRenderer(c => c.Title)
				.RowCells()
				.AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = GetClaimColor(x))
				.Finish();
			ViewModel.RefreshClaims();
			ytreeClaims.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Claims, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectClaim, w=> w.SelectedRow)
				.InitializeFromSource();
			ytreeClaimMessages.ColumnsConfig = ColumnsConfigFactory.Create<ClaimMessage>()
				.AddColumn("Время").AddTextRenderer(c => c.SendTime.ToDateTime().ToLocalTime().ToString("dd MMM HH:mm:ss"))
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
			buttonAnswer.Binding
				.AddBinding(ViewModel, vm => vm.SensitiveSend, w => w.Sensitive)
				.InitializeFromSource();
			buttonClose.Binding
				.AddBinding(ViewModel, vm => vm.SensitiveCloseClaim, w => w.Sensitive)
				.InitializeFromSource();

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

		private string GetClaimColor(Claim claim) {
			switch(claim.ClaimState) {
				case ClaimState.Closed:
					return "gray";
				case ClaimState.WaitSupport:
					return "blue";
				case  ClaimState.WaitUser:
					return "black";
				default:
					throw new ArgumentException();
			}
		}

		protected void OnButtonAnswerClicked(object sender, EventArgs e) {
			ViewModel.SendAnswer();
		}

		protected void OnButtonCloseClicked(object sender, EventArgs e) {
			ViewModel.CloseClaim();
		}
	}
}
