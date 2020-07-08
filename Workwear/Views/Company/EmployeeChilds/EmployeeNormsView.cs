using System;
using Gtk;
using workwear.Domain.Regulations;
using workwear.ViewModels.Company.EmployeeChilds;

namespace workwear.Views.Company.EmployeeChilds
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeNormsView : Gtk.Bin
	{
		public EmployeeNormsView()
		{
			this.Build();

			ytreeNorms.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<Norm>()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("№ ТОН").AddTextRenderer(node => node.DocumentNumberText)
				.AddColumn("№ Приложения").AddNumericRenderer(node => node.AnnexNumberText)
				.AddColumn("№ Пункта").SetDataProperty(node => node.TONParagraph)
				.AddColumn("Профессии").AddTextRenderer(node => node.ProfessionsText)
				.Finish();
			ytreeNorms.Selection.Changed += YtreeNorms_Selection_Changed;
		}
		#region ViewModel

		private EmployeeNormsViewModel viewModel;

		public EmployeeNormsViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				viewModel.PropertyChanged += ViewModel_PropertyChanged;
			}
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.ObservableUsedNorms)) {
				ytreeNorms.ItemsDataSource = ViewModel.ObservableUsedNorms;
			}
		}

		#endregion

		#region События

		void YtreeNorms_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveNorm.Sensitive = buttonNormOpen.Sensitive = ytreeNorms.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddNormClicked(object sender, EventArgs e)
		{
			viewModel.AddNorm();
		}

		protected void OnButtonRemoveNormClicked(object sender, EventArgs e)
		{
			ViewModel.RemoveNorm(ytreeNorms.GetSelectedObject<Norm>());
		}

		protected void OnButtonNormFromPostClicked(object sender, EventArgs e)
		{
			ViewModel.NormFromPost();
		}

		protected void OnButtonRefreshWorkwearItemsClicked(object sender, EventArgs e)
		{
			viewModel.RefreshWorkwearItems();
		}

		protected void OnButtonNormOpenClicked(object sender, EventArgs e)
		{
			viewModel.OpenNorm(ytreeNorms.GetSelectedObject<Norm>());
		}

		protected void OnYtreeNormsRowActivated(object o, RowActivatedArgs args)
		{
			buttonNormOpen.Click();
		}

		#endregion
	}
}
