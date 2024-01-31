using System.Linq;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ChoiceEmployeeGroupView : Gtk.Bin {
		public ChoiceEmployeeGroupView() {
			this.Build();
		}

		private ChoiceEmployeeGroupViewModel viewModel;
		public ChoiceEmployeeGroupViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				CreateTable();
			}
		}

		private void CreateTable() {
			ytreeChoiseEmployeeGroups.CreateFluentColumnsConfig<SelectedChoiceEmployeeGroups>()
				.AddColumn("☑").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Название").AddTextRenderer(x => x.Name).SearchHighlight()
				.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Sensitive = x.Highlighted)
				.Finish();
			ytreeChoiseEmployeeGroups.ItemsDataSource = ViewModel.EmployeeGroups;
			
			yentrySearch.Changed += delegate {
				ytreeChoiseEmployeeGroups.SearchHighlightText = yentrySearch.Text;
				ViewModel.SelectLike(yentrySearch.Text);
				ytreeChoiseEmployeeGroups.YTreeModel.EmitModelChanged();
			};
			ycheckbuttonChooseAll.Sensitive = ycheckbuttonUnChooseAll.Sensitive = ViewModel.EmployeeGroups.Any();
			ycheckbuttonChooseAll.Clicked += (s,e) => ViewModel.SelectAll();
			ycheckbuttonUnChooseAll.Clicked += (s,e) => ViewModel.UnSelectAll();
		}
	}
}
