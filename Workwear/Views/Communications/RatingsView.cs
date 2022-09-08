using Gamma.GtkWidgets;
using QS.Cloud.WearLk.Manage;
using QS.Views.Dialog;
using workwear.ViewModels.Communications;

namespace workwear.Views.Communications 
{
	public partial class RatingsView : DialogViewBase<RatingsViewModel>
	{
		public RatingsView(RatingsViewModel viewModel) : base(viewModel) 
		{
			this.Build();

			entityNomenclature.ViewModel = viewModel.EntryNomenclature;

			ytreeviewRatings.ColumnsConfig = ColumnsConfigFactory.Create<Rating>()
				.AddColumn("Отправитель").AddTextRenderer(node => node.UserPhone)
				.AddColumn("Оценка").AddNumericRenderer(node => node.Rating_)
				.AddColumn("Описание").AddTextRenderer(node => node.Description)
				.Finish();
			
			ytreeviewRatings.Binding
				.AddBinding(ViewModel, vm => vm.Ratings, w => w.ItemsDataSource)
				.InitializeFromSource();
		}
	}
}
