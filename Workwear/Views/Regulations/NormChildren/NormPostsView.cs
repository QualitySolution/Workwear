using System;
using Gamma.ColumnConfig;
using QS.Views;
using Workwear.Domain.Company;
using Workwear.ViewModels.Regulations.NormChildren;

namespace Workwear.Views.Regulations.NormChildren {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NormPostsView : ViewBase<NormPostsViewModel> {
		public NormPostsView(NormPostsViewModel viewModel) : base(viewModel) {
			this.Build();

			ytreeProfessions.ColumnsConfig = FluentColumnsConfig<Post>.Create()
				.AddColumn("Должность").AddTextRenderer(p => p.Name).WrapWidth(700)
				.AddColumn("Подразделение").AddReadOnlyTextRenderer(p => p.Subdivision?.Name).WrapWidth(700)
				.AddColumn("Отдел").AddReadOnlyTextRenderer(p => p.Department?.Name).WrapWidth(700)
				.Finish();
			ytreeProfessions.Selection.Mode = Gtk.SelectionMode.Multiple;

			ytreeProfessions.Binding.AddBinding(ViewModel, v => v.Posts, w => w.ItemsDataSource); //Не инициализируем чтобы не запрашивалось из базы пока вкладка не открыта
			ytreeProfessions.Selection.Changed += YtreeProfessions_Selection_Changed;
		}

		#region Обработчики
		void YtreeProfessions_Selection_Changed(object sender, EventArgs e) {
			buttonRemoveProfession.Sensitive = ytreeProfessions.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddProfessionClicked(object sender, EventArgs e) {
			ViewModel.AddProfession();
		}

		protected void OnButtonRemoveProfessionClicked(object sender, EventArgs e) {
			ViewModel.RemoveProfession(ytreeProfessions.GetSelectedObjects<Post>());
		}

		protected void OnButtonNewProfessionClicked(object sender, EventArgs e) {
			ViewModel.NewProfession();
		}

		#endregion
	}
}
