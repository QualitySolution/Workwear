using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Measurements;

namespace workwear
{
	public partial class NomenclatureDlg : EntityDialogBase<Nomenclature>
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
			yentryNumber.Binding.AddBinding(Entity, e => e.Number, w => w.Text, new Gamma.Binding.Converters.UintToStringConverter()).InitializeFromSource();

			yentryName.Binding.AddBinding (Entity, e => e.Name, w => w.Text).InitializeFromSource ();

			yentryItemsType.SubjectType = typeof(ItemsType);
			yentryItemsType.Binding.AddBinding (Entity, e => e.Type, w => w.Subject).InitializeFromSource ();

			ycomboClothesSex.ItemsEnum = typeof(ClothesSex);
			ycomboClothesSex.Binding.AddBinding (Entity, e => e.Sex, w => w.SelectedItemOrNull).InitializeFromSource ();

			var stdConverter = new SizeStandardCodeConverter ();

			ycomboWearStd.Binding.AddBinding (Entity, e => e.SizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
		}

		public override bool Save ()
		{
			logger.Info ("Запись номенклатуры...");
			var valid = new QS.Validation.QSValidator<Nomenclature> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save ();

			logger.Info ("Ok");
			return true;
		}

		protected void OnYentryItemsTypeChanged (object sender, EventArgs e)
		{
			if (Entity.Type != null && String.IsNullOrWhiteSpace (Entity.Name))
				Entity.Name = Entity.Type.Name;

			if(Entity.Type != null && Entity.Type.Category == ItemTypeCategory.wear && Entity.Type.WearCategory.HasValue)
			{
				ylabelClothesSex.Text = Entity.Type.WearCategory.Value.GetEnumTitle() + ":";

				//Скрываем лишние варианты пола.
				ycomboClothesSex.ClearEnumHideList();
				var standarts = SizeHelper.GetStandartsForСlothes(Entity.Type.WearCategory.Value);
				var toHide = new List<object>();
				foreach(ClothesSex sex in Enum.GetValues(typeof(ClothesSex)))
				{
					if (!standarts.Any(x => x.Sex == sex && x.Use != SizeUse.HumanOnly))
						toHide.Add(sex);
				}
				if(toHide.Count > 0)
					ycomboClothesSex.AddEnumToHideList(toHide.ToArray());

				ylabelClothesSex.Visible = ycomboClothesSex.Visible = toHide.Count < Enum.GetValues(typeof(ClothesSex)).Length;

				if(!SizeHelper.HasСlothesSizeStd(Entity.Type.WearCategory.Value))
				{
					Entity.SizeStd = Entity.WearGrowthStd = null;
				}

				OnYcomboClothesSexChanged (null, null);
			}
			else
			{
				ylabelClothesSex.Visible = ycomboClothesSex.Visible = false;
				ycomboWearStd.Sensitive = false;
			}

			labelSize.Visible = ycomboWearStd.Visible =
				Entity.Type?.WearCategory != null && SizeHelper.HasСlothesSizeStd(Entity.Type.WearCategory.Value);
		}

		protected void OnYcomboClothesSexChanged (object sender, EventArgs e)
		{
			if (!Entity.Sex.HasValue)
				return;

			ycomboWearStd.ItemsEnum = SizeHelper.GetSizeStandartsEnum (Entity.Type.WearCategory.Value, Entity.Sex.Value);
			if (ycomboWearStd.ItemsEnum != null)
				ycomboWearStd.Sensitive = true;

			var growthStd = SizeHelper.GetGrowthStandart (Entity.Type.WearCategory.Value, Entity.Sex.Value);
			if (growthStd != null) {
				Entity.WearGrowthStd = SizeHelper.GetSizeStdCode (growthStd);
			} 
			else
			{
				Entity.WearGrowthStd = null;
			}
		}
	}
}

