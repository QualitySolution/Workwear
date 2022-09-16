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
				.AddColumn("Номенклатура")
					.Binding(b => b.AddBinding(ViewModel, v => v.NomenclatureColumnVisible, col => col.Visible).InitializeFromSource())
					.AddTextRenderer(n => ViewModel.GetNomenclatureName(n))
				.AddColumn("Описание").AddTextRenderer(node => node.Description)
				.Finish();
			
			ytreeviewRatings.Binding
				.AddBinding(ViewModel, vm => vm.Ratings, w => w.ItemsDataSource)
				.InitializeFromSource();
		}
	}
}
