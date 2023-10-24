using System;
using System.Linq;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace Workwear.Views.Company.EmployeeChildren {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmploeeInGroupsView : Gtk.Bin {
		public EmploeeInGroupsView() {
			this.Build();
		}

		private EmployeeInGroupsViewModel viewModel;
		public EmployeeInGroupsViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				CreateTable();
			}
		}

		private void CreateTable() {
			ytreeItems.CreateFluentColumnsConfig<EmployeeGroupItem>()
				.AddColumn("Группа").Resizable().AddTextRenderer(e => e.Group.Name)
				.AddColumn("Комментарий группы").Resizable().AddTextRenderer(e => e.Group.Comment)
				.AddColumn("Комментарий").Resizable().AddTextRenderer(e => e.Comment).Editable()
				.AddColumn("") //Заглушка, чтобы не расширялось
				.Finish();
			ytreeItems.Binding.AddBinding(ViewModel, v => v.EmployeeGroupItems, w => w.ItemsDataSource);
			ytreeItems.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;

			buttonAdd.Clicked += OnButtonAddClicked;
			buttonRemove.Clicked += OnButtonRemoveClicked;
		}
		
		private void OnButtonAddClicked(object sender, EventArgs e) {
			ViewModel.AddGroups();
		}
		
		private void OnButtonRemoveClicked(object sender, EventArgs e) {
			ViewModel.DeleteItems(ytreeItems.GetSelectedObjects<EmployeeGroupItem>());
		}
		
		private void YtreeItems_Selection_Changed(object sender, EventArgs e) {
			buttonRemove.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}
	}
}
