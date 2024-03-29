﻿using System;
using Gamma.ColumnConfig;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Validation;
using QSOrmProject;
using Workwear.Domain.Regulations;

namespace workwear.Dialogs.Regulations
{
	public partial class RegulationDocDlg : EntityDialogBase<RegulationDoc>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public RegulationDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RegulationDoc>();
			ConfigureDlg();
		}

		public RegulationDocDlg(RegulationDoc doc) : this(doc.Id) { }

		public RegulationDocDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RegulationDoc>(id);
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			ydateDocDate.Binding.AddBinding(Entity, e => e.DocDate, w => w.DateOrNull).InitializeFromSource();
			yentryNumber.Binding.AddBinding(Entity, e => e.Number, w => w.Text).InitializeFromSource();

			ytreeAnnexes.ColumnsConfig = FluentColumnsConfig<RegulationDocAnnex>.Create()
				.AddColumn("Номер").AddNumericRenderer(x => x.Number).Editing(new Gtk.Adjustment(1, 1, 127, 1, 10, 10))
				.AddColumn("Наименование").AddTextRenderer(p => p.Name).Editable()
				.Finish();
			ytreeAnnexes.Selection.Changed += treeAnnexes_Selection_Changed;

			ytreeAnnexes.SetItemsSource(Entity.Annexes);
		}

		void treeAnnexes_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveItem.Sensitive = ytreeAnnexes.Selection.CountSelectedRows() > 0;
		}

		public override bool Save()
		{
			logger.Info("Запись нормативного документа...");
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if (!validator.Validate(Entity))
				return false;

			UoWGeneric.Save();

			logger.Info("Ok");
			return true;
		}

		protected void OnButtonAddItemClicked(object sender, EventArgs e)
		{
			Entity.AddAnnex();
		}

		protected void OnButtonRemoveItemClicked(object sender, EventArgs e)
		{
			var annex = ytreeAnnexes.GetSelectedObject<RegulationDocAnnex>();
			if (annex.Id > 0)
			{
				OrmMain.DeleteObject(annex, UoW, () => Entity.RemoveAnnex(annex));
			}
			else
				Entity.RemoveAnnex(annex);			
		}
	}
}
