﻿using Gamma.Binding.Converters;
using NLog;
using QS.Views.Dialog;
using workwear.Domain.Stock;
using workwear.Measurements;
using Workwear.Measurements;
using workwear.ViewModels.Stock;
using Gtk;

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
				

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ycheckArchival.Binding.AddBinding(Entity, e => e.Archival, w => w.Active).InitializeFromSource();

			yentryItemsType.ViewModel = ViewModel.ItemTypeEntryViewModel;
			MakeMenu();
		}

		void MakeMenu()
		{
			var menu = new Menu();
			var item = new MenuItem("Складские движения по номенклатуре");
			item.Activated += (sender, e) => ViewModel.OpenMovements();
			item.Sensitive = ViewModel.SensitiveOpenMovements;
			menu.Add(item);
			menuInternal.Menu = menu;
			menu.ShowAll();
		}
	}
}