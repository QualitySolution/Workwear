using QS.Views.Dialog;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Stock;
using IdToStringConverter = Gamma.Binding.Converters.IdToStringConverter;

namespace Workwear.Views.Stock {

	public partial class CauseIssueView : EntityDialogViewBase<CauseIssueViewModel, CausesIssue> {
		public CauseIssueView(CauseIssueViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();

		}
		private void ConfigureDlg() {
			entityname.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			labelId.Binding.AddBinding(Entity, e => e.Id, w => w.Text, new IdToStringConverter()).InitializeFromSource();
		}
	}
}

