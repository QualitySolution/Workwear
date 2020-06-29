using System;
using Gamma.Binding.Converters;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NLog;
using QS.BusinessCommon.Repository;
using QS.Views.Dialog;
using workwear.Domain.Stock;
using workwear.Measurements;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class ItemTypeView : EntityDialogViewBase<ItemTypeViewModel, ItemsType>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ItemTypeView(ItemTypeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource ();

			yentryName.Binding.AddBinding (Entity, e => e.Name, w => w.Text).InitializeFromSource ();

			ycomboCategory.ItemsEnum = typeof(ItemTypeCategory);
			ycomboCategory.Binding.AddBinding (Entity, e => e.Category, w => w.SelectedItemOrNull).InitializeFromSource ();

			ycomboWearCategory.ItemsEnum = typeof(СlothesType);
			ycomboWearCategory.Binding.AddBinding (Entity, e => e.WearCategory, w => w.SelectedItemOrNull).InitializeFromSource ();

			ycomboUnits.ItemsList = MeasurementUnitsRepository.GetActiveUnits (ViewModel.UoW);
			ycomboUnits.Binding.AddBinding (Entity, e => e.Units, w => w.SelectedItem).InitializeFromSource ();

			yspinMonths.Binding.AddBinding(Entity, e => e.LifeMonths, w => w.ValueAsInt, new QSOrmProject.NullToZeroConverter()).InitializeFromSource();
			ycheckLife.Active = Entity.LifeMonths.HasValue;

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ytreeItems.ColumnsConfig = FluentColumnsConfig<Nomenclature>.Create()
			.AddColumn("Тип").AddTextRenderer(p => p.TypeName)
			.AddColumn("Наименование").AddTextRenderer(p => p.Name)
			.AddColumn("Пол").AddTextRenderer(p => p.Sex != null ? p.Sex.GetEnumTitle() : String.Empty)
			.Finish();
			ytreeItems.ItemsDataSource = Entity.ObservableNomenclatures;
		}

		protected void OnYcomboCategoryChanged (object sender, EventArgs e)
		{
			ycomboWearCategory.Sensitive = Entity.Category == ItemTypeCategory.wear;
			hboxLife.Visible = labelLife.Visible = Entity.Category == ItemTypeCategory.property;
		}

		protected void OnYcheckLifeToggled(object sender, EventArgs e)
		{
			yspinMonths.Sensitive = ycheckLife.Active;
			if(!ycheckLife.Active)
			{
				Entity.LifeMonths = null;
			}
		}
	}
}

