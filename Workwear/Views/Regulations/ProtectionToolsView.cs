using System;
using Gamma.Binding.Converters;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Views.Dialog;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Regulations;

namespace Workwear.Views.Regulations
{
	public partial class ProtectionToolsView : EntityDialogViewBase<ProtectionToolsViewModel, ProtectionTools>
	{
		public ProtectionToolsView(ProtectionToolsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource();

			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			yentryItemsType.ViewModel = ViewModel.ItemTypeEntryViewModel;
			ycheckbutton_dermal_PPE.Binding.AddBinding(ViewModel, vm => vm.WashingPPE, w => w.Active).InitializeFromSource();
			ycheckbutton_dispenser.Binding
				.AddBinding(ViewModel, e => e.Dispenser, w => w.Active)
				.AddBinding(ViewModel, vm => vm.SensitiveDispenser, w => w.Sensitive) 
				.InitializeFromSource();
			ycheckArchival.Binding.AddBinding(ViewModel, e=> e.Archival, w=> w.Active).InitializeFromSource();
			
			entryCategories.ViewModel = ViewModel.CategoriesEntryViewModel;
			entryCategories.Visible = ViewModel.ShowCategoryForAnalytics;
			labelCategories.Visible = ViewModel.ShowCategoryForAnalytics;

			yspinAssessedCost.Binding.AddBinding(Entity, e => e.AssessedCost, w=> w.ValueAsDecimal, new NullToZeroConverter()).InitializeFromSource();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			#region Планирование закупок

			ylabelSupply.Visible = ViewModel.ShowSupply;
			ytableSupply.Visible = ViewModel.ShowSupply;
			
			ybuttonSupplyUni.Binding.AddBinding(ViewModel, vm => vm.ShowSupplyTwosex, w => w.Sensitive).InitializeFromSource();
			ybuttonSupplyTwoSex.Binding.AddBinding(ViewModel, vm => vm.ShowSupplyUnisex, w => w.Sensitive).InitializeFromSource();
			
			ylabelSupplyUni.Binding.AddBinding(ViewModel, vm => vm.ShowSupplyUnisex, w => w.Visible).InitializeFromSource();
			ybutton_remUni.Binding.AddBinding(ViewModel, vm => vm.ShowSupplyUnisex, w => w.Visible).InitializeFromSource();
			ylistcomboboxSupplyUni.SetRenderTextFunc<Nomenclature>((n => $"({n.Sex.GetEnumShortTitle()}) {n.Name}"));
			ylistcomboboxSupplyUni.Binding.AddSource(Entity)
				.AddBinding(e => e.Nomenclatures, w => w.ItemsList)
				.AddBinding(e => e.SupplyNomenclatureUnisex, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.ShowSupplyUnisex, w => w.Visible)
				.InitializeFromSource();
			ylabelSupplyMale.Binding.AddBinding(ViewModel, vm => vm.ShowSupplyTwosex, w => w.Visible).InitializeFromSource();
			ylistcomboboxSupplyMale.SetRenderTextFunc<Nomenclature>((n => $"({n.Sex.GetEnumShortTitle()}) {n.Name}"));			
			ybutton_remMale.Binding.AddBinding(ViewModel, vm => vm.ShowSupplyTwosex, w => w.Visible).InitializeFromSource();
			ylistcomboboxSupplyMale.Binding.AddSource(Entity)
				.AddBinding(e => e.Nomenclatures, w => w.ItemsList)
				.AddBinding(e => e.SupplyNomenclatureMale, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.ShowSupplyTwosex, w => w.Visible)
				.InitializeFromSource();
			ylabelSupplyFemale.Binding.AddBinding(ViewModel, vm => vm.ShowSupplyTwosex, w => w.Visible).InitializeFromSource(); 
			ylistcomboboxSupplyFemale.SetRenderTextFunc<Nomenclature>((n => $"({n.Sex.GetEnumShortTitle()}) {n.Name}"));			
			ybutton_remFemale.Binding.AddBinding(ViewModel, vm => vm.ShowSupplyTwosex, w => w.Visible).InitializeFromSource();
			ylistcomboboxSupplyFemale.Binding.AddSource(Entity)
				.AddBinding(e => e.Nomenclatures, w => w.ItemsList)
				.AddBinding(e => e.SupplyNomenclatureFemale, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.ShowSupplyTwosex, w => w.Visible)
				.InitializeFromSource();
			#endregion

			ytreeItems.ColumnsConfig = FluentColumnsConfig<Nomenclature>.Create()
				.AddColumn("ИД").AddReadOnlyTextRenderer(n => n.Id.ToString())
				.AddColumn("Тип").AddTextRenderer(p => p.TypeName).WrapWidth(500)
				.AddColumn("Номер").AddTextRenderer(n => n.Number)
				.AddColumn("Наименование").AddTextRenderer(p => p.Name + (p.Archival? "(архивная)" : String.Empty)).WrapWidth(700)
				.AddColumn("Цена").Visible(ViewModel.VisibleSaleCost)
					.AddReadOnlyTextRenderer(n => n.SaleCost?.ToString("C"))
				.AddColumn("Пол").AddTextRenderer(p => p.Sex.GetEnumTitle())
				.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.Archival? "gray": "black")
				.Finish();
			ytreeItems.ItemsDataSource = Entity.Nomenclatures;
			ytreeItems.Selection.Changed += Nomenclature_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;

			buttonCreateNomenclature.Binding.AddBinding(ViewModel, v => v.SensitiveCreateNomenclature, w => w.Sensitive).InitializeFromSource();
		}

		private void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
			if (args.Event.Button != 3) return;
			var menu = new Menu();
			var selected = ytreeItems.GetSelectedObject<Nomenclature>();
			
			var itemNomenclature = new MenuItem("Открыть номенклатуру");
			itemNomenclature.Sensitive = selected != null;
			itemNomenclature.Activated += (sender, eventArgs) => ViewModel.OpenNomenclature(selected);
			menu.Add(itemNomenclature);
			
			menu.ShowAll();
			menu.Popup();

		}
		#region Номенклатуры
		protected void OnButtonAddNomenclatureClicked(object sender, EventArgs e) {
			ViewModel.AddNomenclature();
		}

		protected void OnButtonRemoveNomeclatureClicked(object sender, EventArgs e) {
			ViewModel.RemoveNomenclature(ytreeItems.GetSelectedObjects<Nomenclature>());
		}

		void Nomenclature_Selection_Changed(object sender, EventArgs e){
			buttonRemoveNomeclature.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonCreateNomenclatureClicked(object sender, EventArgs e) {
			ViewModel.CreateNomenclature();
		}

		protected void OnYbuttonSupplyUniClicked(object sender, EventArgs e) {
			ViewModel.SupplyType = SupplyType.Unisex;
		}

		protected void OnYbuttonSupplyTwoSexClicked(object sender, EventArgs e) {
			ViewModel.SupplyType = SupplyType.TwoSex;
		}

		protected void OnYbuttonRemUniClicked(object sender, EventArgs e) {
			ViewModel.ClearSupplyNomenclatureUnisex();
		}

		protected void OnYbuttonRemMaleClicked(object sender, EventArgs e) {
			ViewModel.ClearSupplyNomenclatureMale();
		}

		protected void OnYbuttonRemFemaleClicked(object sender, EventArgs e) {
			ViewModel.ClearSupplyNomenclatureFemale();
		}
		#endregion

	}
}
