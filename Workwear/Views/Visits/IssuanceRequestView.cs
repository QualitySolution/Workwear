using QS.Views.Dialog;
using Workwear.Domain.Visits;
using Workwear.ViewModels.Visits;

namespace Workwear.Views.Visits {
	public partial class IssuanceRequestView : EntityDialogViewBase<IssuanceRequestViewModel, IssuanceRequest> {
		public IssuanceRequestView(IssuanceRequestViewModel viewModel): base(viewModel) {
			this.Build();
			ConfigureMainInfo();
			CommonButtonSubscription();
		}

		#region Вкладка Основное

		private void ConfigureMainInfo() {
			ylabelId.Binding.AddBinding(ViewModel, vm => vm.Id, w => w.LabelProp).InitializeFromSource();
			ylabelUser.Binding.AddFuncBinding(ViewModel, vm => vm.CreatedByUser != null ? vm.CreatedByUser.Name : null, w => w.LabelProp).InitializeFromSource();
			receiptDate.Binding.AddBinding(ViewModel, vm => vm.ReceiptDate, w => w.DateOrNull).InitializeFromSource();
			enumStatus.ItemsEnum = typeof(IssuanceRequestStatus);
			enumStatus.Binding.AddBinding(ViewModel, vm => vm.Status, w => w.SelectedItem).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.Comment, w => w.Buffer.Text).InitializeFromSource();
		}

		#endregion
	}
}
