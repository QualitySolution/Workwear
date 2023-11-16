using System;
using System.Linq;
using workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ChoiceProtectionToolsView : Gtk.Bin {
		
		public ChoiceProtectionToolsView(){
			this.Build();
		}
		
		private ChoiceProtectionToolsViewModel viewModel;
		public ChoiceProtectionToolsViewModel ViewModel {
			get => viewModel; 
			set {
				viewModel = value;
				CreateTable();
			}
		}

		private void CreateTable(){
			ytreeChoiseProtectionTools.CreateFluentColumnsConfig<SelectedProtectionTools>()
				.AddColumn("☑").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.Finish();
			
			ytreeChoiseProtectionTools.ItemsDataSource = ViewModel.ProtectionTools;
			
			ycheckbuttonChoiseAllProtectionTools.Sensitive = ViewModel.ProtectionTools.Any();
			ycheckbuttonChoiseAllProtectionTools.Clicked += OnYcheckbuttonChoiseAllProtectionToolsClicked;
		}

		private bool selectAllState = true;

		protected void OnYcheckbuttonChoiseAllProtectionToolsClicked(object sender, EventArgs e) {
			selectAllState = !selectAllState;
			foreach (var pt in ViewModel.ProtectionTools)
				pt.Select = selectAllState;
		}
	}
}
