using System;
using System.Linq;
using Workwear.ReportParameters.ViewModels;

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

		protected void OnYcheckbuttonChoiseAllProtectionToolsClicked(object sender, EventArgs e) {
			ViewModel.SelectUnselectAll();
		}
	}
}
