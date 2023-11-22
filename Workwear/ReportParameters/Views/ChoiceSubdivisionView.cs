using System.Linq;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ChoiceSubdivisionView : Gtk.Bin {
		public ChoiceSubdivisionView() {
			this.Build();
		}

		private ChoiceSubdivisionViewModel viewModel;
		public ChoiceSubdivisionViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				CreateTable();
			}
		}

		private void CreateTable() {
			ytreeChoiseSubdivision.CreateFluentColumnsConfig<SelectedChoiceSubdivision>()
				.AddColumn("☑").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeChoiseSubdivision.ItemsDataSource = ViewModel.Subdivisions;

			ycheckbuttonChooseAll.Sensitive = ycheckbuttonUnChooseAll.Sensitive = ViewModel.Subdivisions.Any();
			ycheckbuttonChooseAll.Clicked += (s,e) => ViewModel.SelectAll();
			ycheckbuttonUnChooseAll.Clicked += (s,e) => ViewModel.UnSelectAll();
		}
	}
}
