using QS.Views.Dialog;
using Workwear.Domain.Visits;
using Workwear.ViewModels.Visits;

namespace Workwear.Views.Visits {
	public partial class VisitView : EntityDialogViewBase<VisitViewModel, Visit> {
		public VisitView(VisitViewModel viewModel): base(viewModel) {
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}
		
		private void ConfigureDlg() {

			
			ydateVisitDate.Binding
				.AddBinding(ViewModel, vm => vm.VisitDate, w => w.Date)
				.AddBinding(ViewModel,vm => vm.CanEditVisitDate, w => w.Sensitive)
				.InitializeFromSource();
			yentryEmployee.ViewModel = ViewModel.EmployeeCardEntryViewModel;
			
			ylabelCreateDateValue.Binding
				.AddFuncBinding(Entity, e => e.CreateDate.ToShortDateString(), w => w.LabelProp)
				.InitializeFromSource();
			ytextComment.Binding
				.AddBinding(ViewModel,v => v.Comment, w => w.Buffer.Text)
				.InitializeFromSource();
			
			ylabelDone.Binding
				.AddBinding(ViewModel, vm => vm.Done, w => w.Visible).InitializeFromSource();
			ylabelCanceled.Binding
            	.AddBinding(ViewModel, vm => vm.Cancelled, w => w.Visible).InitializeFromSource();
		}
	}
}
