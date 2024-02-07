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
				.AddColumn("Название").AddTextRenderer(x => x.Name).SearchHighlight()
				.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Sensitive = x.Highlighted)
				.Finish();
			ytreeChoiseProtectionTools.ItemsDataSource = ViewModel.Items;
			
			yentrySearch.Changed += delegate {
				ytreeChoiseProtectionTools.SearchHighlightText = yentrySearch.Text;
				ViewModel.SelectLike(yentrySearch.Text);
				ytreeChoiseProtectionTools.YTreeModel.EmitModelChanged();
			};
			ycheckbuttonChooseAll.Sensitive = ycheckbuttonUnChooseAll.Sensitive = ViewModel.Items.Any();
			ycheckbuttonChooseAll.Clicked += (s,e) => ViewModel.SelectAll();
			ycheckbuttonUnChooseAll.Clicked += (s,e) => ViewModel.UnSelectAll();
		}
	}
}
