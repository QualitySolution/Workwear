using QS.Views;
using Workwear.ViewModels.ClothingService;

namespace Workwear.Views.ClothingService {
	public partial class ClothingAddView  : ViewBase<ClothingAddViewModel> {
		public ClothingAddView(ClothingAddViewModel viewModel) : base(viewModel) {
			this.Build();
			
			barcodeinfoview1.ViewModel = ViewModel.BarcodeInfoViewModel;

			entrySearchBarcode.Binding
				.AddBinding(ViewModel.BarcodeInfoViewModel, e => e.BarcodeText, w => w.Text).InitializeFromSource();
			ycheckbuttonAutoAdd.Binding
				.AddBinding(ViewModel, vm => vm.AutoAdd, w => w.Active).InitializeFromSource();
			
			treeClaims.Binding
				.AddBinding(ViewModel, vm => vm.Items, w => w.ItemsDataSource)
				.InitializeFromSource();
			treeClaims.CreateFluentColumnsConfig<AddServiceClaimNode>()
				.AddColumn("☑").AddToggleRenderer(x => x.Add).Editing()
				.AddColumn("Штрихкод").AddReadOnlyTextRenderer(x => x.BarcodeText)
				.AddColumn("Сотруддник").AddReadOnlyTextRenderer(x => x.EmployeeText)
				.AddColumn("Номенклатура").AddReadOnlyTextRenderer(x => x.NomenclatureText)
				.AddColumn("Размер/Рост").AddReadOnlyTextRenderer(x =>  x.SizeText)
				.Finish();
			
			buttonAdd.Binding
				.AddBinding(ViewModel, vm => vm.CanAdd, w => w.Sensitive).InitializeFromSource();
			buttonAdd.Clicked += (sender, args) => ViewModel.AddClaim();
			buttonAccept.Clicked += (sender, args) => ViewModel.Accept();
		}
	}
}
