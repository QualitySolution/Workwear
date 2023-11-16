using System;
using System.Linq;
using workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ChoiceSubdivisionView : Gtk.Bin {
		public ChoiceSubdivisionView() {
			this.Build();
		}

		protected void OnYcheckbuttonAllChoiceSubdivisionClicked(object sender, EventArgs e) {
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
			ycheckbuttonChoiseAllSubdivisions.Sensitive = ViewModel.Subdivisions.Any();
			ycheckbuttonChoiseAllSubdivisions.Clicked += OnYcheckbuttonAllSubdivisionClicked;
		}

		private bool selectAllState = true;
		protected void OnYcheckbuttonAllSubdivisionClicked(object sender, EventArgs e) {
			selectAllState = !selectAllState;
			foreach(var pt in ViewModel.Subdivisions)
				pt.Select = selectAllState;
		}
	}
}
