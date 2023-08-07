using System;
using System.Linq;
using Gamma.ColumnConfig;
using QS.Views;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Stock.NomenclatureChildren;

namespace Workwear.Views.Stock.NomenclatureChildren {
	public partial class NomenclatureProtectionToolsView : ViewBase<NomenclatureProtectionToolsViewModel> {
		public NomenclatureProtectionToolsView(NomenclatureProtectionToolsViewModel viewModel) : base(viewModel) {
			this.Build();

			ytreeItems.ColumnsConfig = FluentColumnsConfig<ProtectionTools>.Create()
				.AddColumn("ИД").AddReadOnlyTextRenderer(n => n.Id.ToString())
				.AddColumn("Тип").AddReadOnlyTextRenderer(p => p.Type?.Name).WrapWidth(500)
				.AddColumn("Наименование").AddTextRenderer(p => p.Name).WrapWidth(900)
				.AddColumn("Оценочная стоимость").AddReadOnlyTextRenderer(n => n.AssessedCost?.ToString("C"))
				.Finish();
			ytreeItems.ItemsDataSource = ViewModel.ObservableProtectionTools;
			ytreeItems.Selection.Changed += Nomenclature_Selection_Changed;
			ytreeItems.RowActivated += YtreeItems_RowActivated;
		}

		protected void OnButtonAddNomenclatureClicked(object sender, EventArgs e) {
			ViewModel.Add();
		}

		protected void OnButtonRemoveNomeclatureClicked(object sender, EventArgs e) {
			ViewModel.Remove(ytreeItems.GetSelectedObjects<ProtectionTools>().First());
		}

		void Nomenclature_Selection_Changed(object sender, EventArgs e) {
			buttonRemoveNomeclature.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}

		void YtreeItems_RowActivated(object o, Gtk.RowActivatedArgs args) {
			ViewModel.Open(ytreeItems.GetSelectedObjects<ProtectionTools>().First());
		}

	}
}
