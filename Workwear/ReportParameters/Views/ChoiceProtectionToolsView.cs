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
			ytreeProtectionTools.CreateFluentColumnsConfig<SelectedProtectionTools>()
				.AddColumn("☑").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.Finish();
			
			ytreeProtectionTools.ItemsDataSource = ViewModel.ProtectionTools;
			
			ycheckbuttonAllProtectionTools.Sensitive = ViewModel.ProtectionTools.Any();
			ycheckbuttonAllProtectionTools.Clicked += OnYcheckbuttonAllProtectionToolsClicked;
		}

		private bool selectAllState = true;
		protected void OnYcheckbuttonAllProtectionToolsClicked(object sender, EventArgs e) {
			selectAllState = !selectAllState;
			foreach (var pt in ViewModel.ProtectionTools)
				pt.Select = selectAllState;
		}
	}
}
