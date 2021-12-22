using System;
using Gamma.Binding.Converters;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Views.Dialog;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.ViewModels.Regulations;

namespace workwear.Views.Regulations
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

			yspinAssessedCost.Binding.AddBinding(Entity, e => e.AssessedCost, w=> w.ValueAsDecimal, new NullToZeroConverter()).InitializeFromSource();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ytreeNormAnalog.ColumnsConfig = FluentColumnsConfig<ProtectionTools>.Create()
				.AddColumn("Аналог СИЗ").AddTextRenderer(p => p.Name)
				.Finish();
			ytreeNormAnalog.ItemsDataSource = Entity.ObservableAnalog;
			ytreeNormAnalog.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeNormAnalog.Selection.Changed += YtreeItemsType_Selection_Changed;

			ytreeItems.ColumnsConfig = FluentColumnsConfig<Nomenclature>.Create()
			.AddColumn("Тип").AddTextRenderer(p => p.TypeName)
			.AddColumn("Номер").AddTextRenderer(n => $"{n.Number}")
			.AddColumn("Наименование").AddTextRenderer(p => p.Name)
			.AddColumn("Пол").AddTextRenderer(p => p.Sex != null ? p.Sex.GetEnumTitle() : String.Empty)
			.Finish();
			ytreeItems.ItemsDataSource = Entity.ObservableNomenclatures;
			ytreeItems.Selection.Changed += Nomenclature_Selection_Changed;

			buttonCreateNomenclature.Binding.AddBinding(ViewModel, v => v.SensetiveCreateNomenclature, w => w.Sensitive).InitializeFromSource();
		}

		#region Аналоги

		void YtreeItemsType_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveNormAnalog.Sensitive = ytreeNormAnalog.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddNormAnalogClicked(object sender, EventArgs e)
		{
			ViewModel.AddAnalog();
		}

		protected void OnButtonRemoveNormAnalogClicked(object sender, EventArgs e)
		{
			ViewModel.RemoveAnalog(ytreeNormAnalog.GetSelectedObjects<ProtectionTools>());
		}
		#endregion
		#region Номенклатуры
		protected void OnButtonAddNomenclatureClicked(object sender, EventArgs e)
		{
			ViewModel.AddNomeclature();
		}

		protected void OnButtonRemoveNomeclatureClicked(object sender, EventArgs e)
		{
			ViewModel.RemoveNomeclature(ytreeItems.GetSelectedObjects<Nomenclature>());
		}

		void Nomenclature_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveNomeclature.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonCreateNomenclatureClicked(object sender, EventArgs e)
		{
			ViewModel.CreateNomeclature();
		}
		#endregion

	}
}
