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
				.AddColumn("Время").AddTextRenderer(c => c.SendTime.ToDateTime().ToLocalTime().ToString("dd MMM HH:mm:ss")).YAlign(0)
				.AddColumn("Автор").AddTextRenderer(c => c.SenderName).YAlign(0)
				.AddColumn("Текст").AddTextRenderer(c => c.Text).WrapWidth(800)
				.RowCells()
				.AddSetter<Gtk.CellRendererText>((c, x) => c.Weight = x.UserRead ? 400 : 600)
				.Finish();
			ytreeClaimMessages.Selection.Mode = SelectionMode.None;
			ytreeClaimMessages.Binding
				.AddBinding(ViewModel, vm => vm.MessagesSelectClaims, w => w.ItemsDataSource)
				.InitializeFromSource();
			
			labelClaimTitle.Binding.AddBinding(ViewModel, v => v.ClaimTitle, w => w.LabelProp).InitializeFromSource();
			labelEmployee.Binding.AddBinding(ViewModel, v => v.EmployeeName, w => w.LabelProp).InitializeFromSource();
			buttonOpenEmployee.Binding.AddBinding(ViewModel, v => v.SensitiveOpenEmployee, w => w.Sensitive).InitializeFromSource();
			buttonOpenEmployee.Clicked += (sender, args) => ViewModel.OpenEmployee();
			labelProtectionToolsName.Binding.AddSource(ViewModel)
				.AddBinding(v => v.ProtectionToolsTitle, w => w.LabelProp)
				.AddBinding(v => v.VisibleProtectionTools, w => w.Visible)
				.InitializeFromSource();
			labelTitleProtectionTools.Binding.AddBinding(ViewModel, v => v.VisibleProtectionTools, w => w.Visible).InitializeFromSource();
			buttonOpenProtectionTools.Binding.AddSource(ViewModel)
				.AddBinding(v => v.VisibleProtectionTools, w => w.Visible)
				.AddBinding(v => v.SensitiveOpenProtectionTools, w => w.Sensitive)
				.InitializeFromSource();
			buttonOpenProtectionTools.Clicked += (sender, args) => ViewModel.OpenProtectionTools();
			
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
			var lastPos = ytreeClaims.Vadjustment.Value;
			if(!ViewModel.UploadClaims())
				return;
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
