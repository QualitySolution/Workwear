using System;
using System.Linq;
using Gamma.Utilities;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using workwear.Domain;
using workwear.Domain.Stock;
using workwear.Measurements;
using System.Collections.Generic;

namespace workwear
{
	public partial class NomenclatureDlg : OrmGtkDialogBase<Nomenclature>
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

			ycomboClothesSex.ItemsEnum = typeof(ClothesSex);
			ycomboClothesSex.Binding.AddBinding (Entity, e => e.Sex, w => w.SelectedItemOrNull).InitializeFromSource ();

			var stdConverter = new SizeStandardCodeConverter ();

			ycomboWearStd.Binding.AddBinding (Entity, e => e.SizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();
			ycomboWearSize.Binding.AddBinding (Entity, e => e.Size, w => w.ActiveText).InitializeFromSource ();
			ycomboWearGrowth.Binding.AddBinding (Entity, e => e.WearGrowth, w => w.ActiveText).InitializeFromSource ();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
		}

		public override bool Save ()
		{
			logger.Info ("Запись номенклатуры...");
			var valid = new QSValidation.QSValidator<Nomenclature> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			try {
				UoWGeneric.Save ();
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog ("Не удалось записать номенклатуру.", logger, ex);
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
				ylabelClothesSex.Visible = ycomboClothesSex.Visible = true;

				ylabelClothesSex.Text = Entity.Type.WearCategory.Value.GetEnumTitle() + ":";

				//Скрываем лишние варианты пола.
				ycomboClothesSex.ClearEnumHideList();
				var standarts = SizeHelper.GetStandartsForСlothes(Entity.Type.WearCategory.Value);
				var toHide = new List<object>();
				foreach(var sexInfo in typeof(ClothesSex).GetFields())
				{
					if (sexInfo.Name.Equals ("value__"))
						continue;
					
					var sexEnum = (ClothesSex)sexInfo.GetValue(null);
					if (!standarts.Any(x => x.Sex == sexEnum && x.Use != SizeUse.HumanOnly))
						toHide.Add(sexEnum);
				}
				if(toHide.Count > 0)
					ycomboClothesSex.AddEnumToHideList(toHide.ToArray());

				if(!SizeHelper.HasСlothesSizeStd(Entity.Type.WearCategory.Value))
				{
					Entity.Size = Entity.SizeStd = Entity.WearGrowth = Entity.WearGrowthStd = null;
				}

				OnYcomboClothesSexChanged (null, null);
			}
			else
			{
				ylabelClothesSex.Visible = ycomboClothesSex.Visible = false;
				ycomboWearStd.Sensitive = ycomboWearSize.Sensitive = ycomboWearGrowth.Sensitive = false;
			}
				
		}

		protected void OnYcomboWearStdChanged (object sender, EventArgs e)
		{
			if (ycomboWearStd.SelectedItemOrNull != null)
			{
				SizeHelper.FillSizeCombo (ycomboWearSize, SizeHelper.GetSizesList (ycomboWearStd.SelectedItem, SizeUse.HumanOnly));
				ycomboWearSize.Sensitive = true;
			}
			else
			{
				ycomboWearSize.Clear ();
				ycomboWearSize.Sensitive = false;
			}
		}

		protected void OnYcomboClothesSexChanged (object sender, EventArgs e)
		{
			if (!Entity.Sex.HasValue)
				return;

			ycomboWearStd.ItemsEnum = SizeHelper.GetSizeStandartsEnum (Entity.Type.WearCategory.Value, Entity.Sex.Value);
			if (ycomboWearStd.ItemsEnum != null)
				ycomboWearStd.Sensitive = true;

			var growthStd = SizeHelper.GetGrowthStandart (Entity.Type.WearCategory.Value, Entity.Sex.Value);
			ycomboWearGrowth.Sensitive = growthStd != null;
			if (growthStd != null) {
				SizeHelper.FillSizeCombo (ycomboWearGrowth, SizeHelper.GetSizesList (growthStd.Value, SizeUse.HumanOnly));
				Entity.WearGrowthStd = SizeHelper.GetSizeStdCode (growthStd);
			} 
			else
			{
				Entity.WearGrowthStd = null;
				ycomboWearGrowth.Clear ();
			}
		}
	}
}

