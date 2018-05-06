using System;
using NLog;
using QSBusinessCommon.Repository;
using QSOrmProject;
using QSProjectsLib;
using workwear.Domain.Regulations;
using workwear.Measurements;

namespace workwear.Dialogs.Regulations
{
	public partial class ItemTypeDlg : OrmGtkDialogBase<ItemsType>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		
		public ItemTypeDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<ItemsType> ();
			ConfigureDlg ();
		}

		public ItemTypeDlg (ItemsType item) : this (item.Id) {}

		public ItemTypeDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<ItemsType> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource ();

			yentryName.Binding.AddBinding (Entity, e => e.Name, w => w.Text).InitializeFromSource ();

			ycomboCategory.ItemsEnum = typeof(ItemTypeCategory);
			ycomboCategory.Binding.AddBinding (Entity, e => e.Category, w => w.SelectedItemOrNull).InitializeFromSource ();

			ycomboWearCategory.ItemsEnum = typeof(СlothesType);
			ycomboWearCategory.Binding.AddBinding (Entity, e => e.WearCategory, w => w.SelectedItemOrNull).InitializeFromSource ();

			ycomboUnits.ItemsList = MeasurementUnitsRepository.GetActiveUnits (UoWGeneric);
			ycomboUnits.Binding.AddBinding (Entity, e => e.Units, w => w.SelectedItem).InitializeFromSource ();

			yspinMonths.Binding.AddBinding(Entity, e => e.LifeMonths, w => w.ValueAsInt, new NullToZeroConverter()).InitializeFromSource();
			ycheckLife.Active = Entity.LifeMonths.HasValue;

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
		}

		public override bool Save ()
		{
			logger.Info ("Запись типа номенклатуры...");
			var valid = new QSValidation.QSValidator<ItemsType> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			try {
				UoWGeneric.Save ();
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog ("Не удалось записать тип номенклатуры.", logger, ex);
				return false;
			}
			logger.Info ("Ok");
			return true;
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

