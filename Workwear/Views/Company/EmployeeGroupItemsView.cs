using System;
using Gamma.ColumnConfig;
using QS.Views;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company;

namespace Workwear.Views.Company {
	public partial class EmployeeGroupItemsView : ViewBase<EmployeeGroupItemsViewModel> {
		public EmployeeGroupItemsView(EmployeeGroupItemsViewModel viewModel) : base(viewModel) 
		{
			this.Build();
			
			ytreeItems.ColumnsConfig = FluentColumnsConfig<EmployeeGroupItem>.Create()
				.AddColumn("Номер\nкарточки").Resizable().AddReadOnlyTextRenderer(item => item.CardNumberText)
				.AddColumn("Табельный №").Resizable().AddReadOnlyTextRenderer(item => item.EmployeePersonnelNumber)
				.AddColumn("Ф.И.О.").Resizable().AddReadOnlyTextRenderer(item => item.FullName)
				.AddColumn("Комментарий").AddTextRenderer(item => item.Comment).Editable()
				.RowCells().AddSetter<Gtk.CellRendererText>((c, item) => c.Foreground = item.Dismiss ? "grey" : null)
				.Finish();
			
			ytreeItems.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeItems.Binding.AddBinding(ViewModel, vm => vm.Items, w => w.ItemsDataSource).InitializeFromSource();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;

			ytreeItems.RowActivated += OnRowActivated;
			buttonтOpen.Clicked += OnButtonOpenClicked;
			buttonAdd.Clicked += OnButtonAddClicked;
			buttonRemove.Clicked += OnButtonRemoveClicked;
		}

		private void OnButtonOpenClicked(object sender, EventArgs e) {
			ViewModel.OpenEmployees(ytreeItems.GetSelectedObjects<EmployeeGroupItem>());
		}

		private void OnRowActivated(object sender, EventArgs e) {
			ViewModel.OpenEmployees(ytreeItems.GetSelectedObjects<EmployeeGroupItem>());
		}

		private void YtreeItems_Selection_Changed(object sender, EventArgs e) {
			buttonRemove.Sensitive = buttonтOpen.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}

		private void OnButtonAddClicked(object sender, EventArgs e) {
			ViewModel.AddEmployees();
		}
		
		private void OnButtonRemoveClicked(object sender, EventArgs e) {
			ViewModel.Remove(ytreeItems.GetSelectedObjects<EmployeeGroupItem>());
		}
	}
}
