using System;
using Gamma.Binding.Converters;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NLog;
using QS.BusinessCommon.Repository;
using QS.Views.Dialog;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock
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
		private void ConfigureDlg() {
			ylabelId.Binding
				.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter())
				.InitializeFromSource ();

			yentryName.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text)
				.InitializeFromSource ();

			ycomboCategory.ItemsEnum = typeof(ItemTypeCategory);
			ycomboCategory.Binding
				.AddBinding(Entity, e => e.Category, w => w.SelectedItemOrNull)
				.InitializeFromSource ();

			ycomboClothesType.ItemsEnum = typeof(СlothesType);
			ycomboClothesType.Binding
				.AddBinding(Entity, e => e.WearCategory, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, v => v.VisibleWearCategory, w => w.Visible)
				.InitializeFromSource();

			labelSize.Binding.AddBinding(ViewModel, v => v.VisibleSize, w => w.Visible).InitializeFromSource();
			comboSize.ItemsList = ViewModel.SizeService.GetSizeTypeByCategory(ViewModel.UoW, CategorySizeType.Size);
			comboSize.Binding
				.AddBinding(Entity, e => e.SizeType, w => w.SelectedItem)
				.AddBinding(ViewModel, v => v.VisibleSize, w => w.Visible)
				.InitializeFromSource();

			labelHeight.Binding.AddBinding(ViewModel, v => v.VisibleSize, w => w.Visible).InitializeFromSource();
			comboHeight.ItemsList = 
				ViewModel.SizeService.GetSizeTypeByCategory(ViewModel.UoW, CategorySizeType.Height);
			comboHeight.Binding
				.AddBinding (Entity, e => e.HeightType, w => w.SelectedItem)
				.AddBinding(ViewModel, v => v.VisibleSize, w => w.Visible)
				.InitializeFromSource();
			
			labelWearCategory.Binding.AddBinding(ViewModel, v => v.VisibleWearCategory, w => w.Visible).InitializeFromSource();
			
			ycomboUnits.ItemsList = MeasurementUnitsRepository.GetActiveUnits(ViewModel.UoW);
			ycomboUnits.Binding
				.AddBinding (Entity, e => e.Units, w => w.SelectedItem)
				.InitializeFromSource ();

			labelIssueType.Binding.AddBinding(ViewModel, v => v.VisibleIssueType, w => w.Visible).InitializeFromSource();
			comboIssueType.ItemsEnum = typeof(IssueType);
			comboIssueType.Binding
				.AddBinding(Entity, e => e.IssueType, w => w.SelectedItem)
                .AddBinding(ViewModel, v => v.VisibleIssueType, w => w.Visible)
				.InitializeFromSource();

			comboIssueType.Visible = labelIssueType.Visible = ViewModel.VisibleIssueType;

			yspinMonths.Binding
				.AddBinding(Entity, e => e.LifeMonths, w => w.ValueAsInt, new NullToZeroConverter())
				.InitializeFromSource();
			ycheckLife.Active = Entity.LifeMonths.HasValue;

			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			ytreeItems.ColumnsConfig = FluentColumnsConfig<Nomenclature>.Create()
			.AddColumn("Тип").AddTextRenderer(p => p.TypeName)
			.AddColumn("Наименование").AddTextRenderer(p => p.Name)
				.WrapWidth(700)
			.AddColumn("Пол")
				.AddTextRenderer(p => p.Sex != null ? p.Sex.GetEnumTitle() : String.Empty)
			.Finish();
			ytreeItems.ItemsDataSource = Entity.ObservableNomenclatures;
		}

		private void OnYcomboCategoryChanged (object sender, EventArgs e) {
			ycomboClothesType.Sensitive = Entity.Category == ItemTypeCategory.wear;
			hboxLife.Visible = labelLife.Visible = Entity.Category == ItemTypeCategory.property;
		}

		private void OnYcheckLifeToggled(object sender, EventArgs e) {
			yspinMonths.Sensitive = ycheckLife.Active;
			if(!ycheckLife.Active) 
				Entity.LifeMonths = null;
		}
	}
}

