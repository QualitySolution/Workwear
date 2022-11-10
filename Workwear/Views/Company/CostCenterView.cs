using System;
using Gamma.Binding.Converters;
using QS.Views.Dialog;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company;

namespace Workwear.Views.Company {
	public partial class CostCenterView : EntityDialogViewBase<CostCenterViewModel, CostCenter> {

		public CostCenterView(CostCenterViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		void ConfigureDlg() {
			labelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource();
			entryCode.Binding.AddBinding(Entity, e => e.Code, w => w.Text).InitializeFromSource();
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
		}
	}
}
