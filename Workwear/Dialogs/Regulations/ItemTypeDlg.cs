using System;
using Gamma.ColumnConfig;
using NLog;
using QS.BusinessCommon.Repository;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using workwear.Domain.Regulations;
using workwear.Measurements;

namespace workwear.Dialogs.Regulations
{
	public partial class ItemTypeDlg : EntityDialogBase<ItemsType>
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

			ytreeNormAnalog.ColumnsConfig = FluentColumnsConfig<ItemsType>.Create()
			.AddColumn("Аналог нормы").AddTextRenderer(p => p.Name)
			.Finish();
			ytreeNormAnalog.ItemsDataSource = Entity.ObservableitemsType;
			ytreeNormAnalog.Selection.Changed += YtreeItemsType_Selection_Changed;
		}

		public override bool Save ()
		{
			logger.Info ("Запись типа номенклатуры...");
			var valid = new QS.Validation.QSValidator<ItemsType> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save ();

			logger.Info ("Ok");
			return true;
		}

		void YtreeItemsType_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveNormAnalog.Sensitive = ytreeNormAnalog.Selection.CountSelectedRows() > 0;
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

		protected void OnButtonAddNormAnalogClicked(object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference(typeof(ItemsType));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ObjectSelected += SelectDialog_ItemsTypeSelected;

			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void SelectDialog_ItemsTypeSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var type = e.Subject as ItemsType;
			if (type.Id != Entity.Id)
				Entity.AddAnalog(UoW.GetById< ItemsType >(type.Id));
		}

		protected void OnButtonRemoveNormAnalogClicked(object sender, EventArgs e)
		{
			Entity.RemoveAnalog(ytreeNormAnalog.GetSelectedObject<ItemsType>());
		}
	}
}

