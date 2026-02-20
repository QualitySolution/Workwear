using System.Linq;
using Gamma.Binding.Converters;
using Gamma.ColumnConfig;
using QS.Views.Dialog;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Stock;
using Workwear.ViewModels.ClothingService;

namespace Workwear.Views.ClothingService {
	public partial class ServiceView : EntityDialogViewBase<ServiceViewModel, Service> {
		public ServiceView(ServiceViewModel viewModel) : base(viewModel) {
			this.Build();
			CommonButtonSubscription();
			
			yentryName.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yspinbuttonSaleCost.Binding
				.AddBinding(Entity, e => e.Cost, w=> w.ValueAsDecimal).InitializeFromSource();
			ytextComment.Binding
        		.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			
			ytreeNomenclatures.ColumnsConfig = FluentColumnsConfig<Nomenclature>.Create()
				.AddColumn("ИД").AddReadOnlyTextRenderer(n => n.Id.ToString())
				.AddColumn("Название").AddTextRenderer(p => p.Name).WrapWidth(600)
				.Finish();
			ytreeNomenclatures.ItemsDataSource = ViewModel.ObservableNomenclatures;

			buttonAddNomenclature.Clicked += (sender, args) => ViewModel.AddNomenclature();
			buttonRemoveNomenclature.Clicked += (sender, args) => ViewModel.RemoveNomenclature(ytreeNomenclatures.GetSelectedObjects<Nomenclature>().First());

		}
	}
}
