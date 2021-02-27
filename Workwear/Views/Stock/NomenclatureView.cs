using Gamma.Binding.Converters;
using NLog;
using QS.Views.Dialog;
using workwear.Domain.Stock;
using workwear.Measurements;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class NomenclatureView : EntityDialogViewBase<NomenclatureViewModel, Nomenclature>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public NomenclatureView(NomenclatureViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			yentryNumber.Binding.AddBinding(Entity, e => e.Number, w => w.Text, new NumbersToStringConverter()).InitializeFromSource();

			yentryName.Binding.AddBinding (Entity, e => e.Name, w => w.Text).InitializeFromSource ();

			ycomboClothesSex.ItemsEnum = typeof(ClothesSex);
			ycomboClothesSex.Binding.AddBinding (Entity, e => e.Sex, w => w.SelectedItemOrNull).InitializeFromSource ();
			ycomboClothesSex.Binding.AddBinding(ViewModel, vm => vm.VisibleClothesSex, w => w.Visible).InitializeFromSource();

			ylabelClothesSex.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.VisibleClothesSex, w => w.Visible)
				.AddBinding(vm => vm.ClothesSexLabel, w => w.LabelProp)
				.InitializeFromSource();

			var stdConverter = new SizeStandardCodeConverter ();

			ycomboWearStd.Binding.AddSource(ViewModel)
				.AddBinding(v => v.SizeStdEnum, w => w.ItemsEnum)
				.AddBinding(v => v.VisibleSizeStd, w => w.Visible)
				.InitializeFromSource();
			ycomboWearStd.Binding.AddBinding (Entity, e => e.SizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();

			labelSize.Binding.AddBinding(ViewModel, v => v.VisibleSizeStd, w => w.Visible).InitializeFromSource();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			yentryItemsType.ViewModel = ViewModel.ItemTypeEntryViewModel;
		}
	}
}

