using System;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using workwear.Domain;
using workwear.Domain.Stock;
using workwear.Measurements;

namespace workwear
{
	public partial class NomenclatureDlg : FakeTDIEntityGtkDialogBase<Nomenclature>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public NomenclatureDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Nomenclature> ();
			ConfigureDlg ();
		}

		public NomenclatureDlg (Nomenclature item) : this (item.Id) {}

		public NomenclatureDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Nomenclature> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource ();

			yentryName.Binding.AddBinding (Entity, e => e.Name, w => w.Text).InitializeFromSource ();

			yentryItemsType.SubjectType = typeof(ItemsType);
			yentryItemsType.Binding.AddBinding (Entity, e => e.Type, w => w.Subject).InitializeFromSource ();

			var stdConverter = new SizeStandardCodeConverter ();

			ycomboWearStd.Binding.AddBinding (Entity, e => e.SizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();
			ycomboWearSize.Binding.AddBinding (Entity, e => e.Size, w => w.ActiveText).InitializeFromSource ();
			ycomboWearGrowth.Binding.AddBinding (Entity, e => e.WearGrowth, w => w.ActiveText).InitializeFromSource ();
		}
			
		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if (Save ())
			{
				OnEntitySaved (true);
				Respond (Gtk.ResponseType.Ok);
			}
		}

		public override bool Save ()
		{
			logger.Info ("Запись номенклатуры...");
			var valid = new QSValidation.QSValidator<Nomenclature> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid (this))
				return false;

			try {
				UoWGeneric.Save ();
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog (this, "Не удалось записать номенклатуру.", logger, ex);
				return false;
			}
			logger.Info ("Ok");
			return true;
		}

		protected void OnYentryItemsTypeChanged (object sender, EventArgs e)
		{
			if (Entity.Type != null && String.IsNullOrWhiteSpace (Entity.Name))
				Entity.Name = Entity.Type.Name;

			if(Entity.Type != null && Entity.Type.Category == ItemTypeCategory.wear && Entity.Type.WearCategory.HasValue)
			{
				ycomboWearStd.ItemsEnum = SizeHelper.GetSizeStandartsEnum (Entity.Type.WearCategory.Value);
				ycomboWearStd.Sensitive = ycomboWearSize.Sensitive = true;

				var growthStd = SizeHelper.GetGrowthStandart (Entity.Type.WearCategory.Value);
				ycomboWearGrowth.Sensitive = growthStd != null;
				if (growthStd != null) {
					SizeHelper.FillSizeCombo (ycomboWearGrowth, SizeHelper.GetSizesList (growthStd.Value));
				} else
					ycomboWearGrowth.Clear ();
			}
			else
				ycomboWearStd.Sensitive = ycomboWearSize.Sensitive = ycomboWearGrowth.Sensitive = false;
		}

		protected void OnYcomboWearStdChanged (object sender, EventArgs e)
		{
			SizeHelper.FillSizeCombo (ycomboWearSize, SizeHelper.GetSizesList (ycomboWearStd.SelectedItem));
		}
	}
}

