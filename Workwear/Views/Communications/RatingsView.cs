using Gamma.GtkWidgets;
using QS.Cloud.WearLk.Manage;
using QS.Views.Dialog;
using Workwear.ViewModels.Communications;

namespace Workwear.Views.Communications 
{
	public partial class RatingsView : DialogViewBase<RatingsViewModel>
	{
		public RatingsView(RatingsViewModel viewModel) : base(viewModel) 
		{
			this.Build();

			entityNomenclature.ViewModel = viewModel.EntryNomenclature;

			ytreeviewRatings.ColumnsConfig = ColumnsConfigFactory.Create<Rating>()
				.AddColumn("Дата").AddTextRenderer(n => n.CreateTime.ToDateTime().ToString("g"))
				.AddColumn("Отправитель").AddTextRenderer(node => node.UserPhone)
				.AddColumn("Оценка").AddNumericRenderer(node => node.Rating_)
				.AddColumn("Номенклатура").Resizable()
					.Binding(b => b.AddBinding(ViewModel, v => v.NomenclatureColumnVisible, col => col.Visible).InitializeFromSource())
					.AddTextRenderer(n => ViewModel.GetNomenclatureName(n)).WrapWidth(700)
				.AddColumn("Описание").Resizable()
					.AddTextRenderer(node => node.Description).WrapWidth(700)
				.Finish();
			
			ytreeviewRatings.Binding.AddBinding(ViewModel, v => v.SelectedRating, w => w.SelectedRow).InitializeFromSource();
			ytreeviewRatings.RowActivated += HandleRowActivatedHandler;
			
			ytreeviewRatings.Binding
				.AddBinding(ViewModel, vm => vm.Ratings, w => w.ItemsDataSource)
				.InitializeFromSource();
			buttonOpenEmployee.Binding.AddBinding(ViewModel, vm => vm.SensitiveOpenEmployee, w => w.Sensitive).InitializeFromSource();
		}

		void HandleRowActivatedHandler(object o, Gtk.RowActivatedArgs args) {
			if(args.Column.Title == "Отправитель") {
				ViewModel.OpenEmployee();
			}
		}

		protected void OnButtonOpenEmployeeClicked(object sender, System.EventArgs e) {
			ViewModel.OpenEmployee();
		}
	}
}
