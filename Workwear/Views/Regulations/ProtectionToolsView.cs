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

			yspinAssessedCost.Binding.AddBinding(Entity, e => e.AssessedCost, w=> w.ValueAsDecimal, new NullToZeroConverter()).InitializeFromSource();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ytreeNormAnalog.ColumnsConfig = FluentColumnsConfig<ProtectionTools>.Create()
				.AddColumn("Аналог СИЗ").AddTextRenderer(p => p.Name)
				.Finish();
			ytreeNormAnalog.ItemsDataSource = Entity.Analogs;
			ytreeNormAnalog.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeNormAnalog.Selection.Changed += YtreeItemsType_Selection_Changed;

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
			ViewModel.AddNomenclature();
		}

		protected void OnButtonRemoveNomeclatureClicked(object sender, EventArgs e)
		{
			ViewModel.RemoveNomenclature(ytreeItems.GetSelectedObjects<Nomenclature>());
		}

		void Nomenclature_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveNomeclature.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonCreateNomenclatureClicked(object sender, EventArgs e)
		{
			ViewModel.CreateNomenclature();
		}
		#endregion

	}
}
