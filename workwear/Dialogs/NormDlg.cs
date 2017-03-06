using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using QSOrmProject;
using QSProjectsLib;
using workwear.Domain;

namespace workwear
{
	public partial class NormDlg : FakeTDIEntityGtkDialogBase<Norm>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public NormDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Norm> ();
			ConfigureDlg ();
		}

		public NormDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Norm> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource ();

			yentryTonNumber.Binding.AddBinding (Entity, e => e.TONNumber, w => w.Text).InitializeFromSource ();
			yentryTonAttachment.Binding.AddBinding (Entity, e => e.TONAttachment, w => w.Text).InitializeFromSource ();
			yentryTonParagraph.Binding.AddBinding (Entity, e => e.TONParagraph, w => w.Text).InitializeFromSource ();

			ytreeProfessions.ColumnsConfig = FluentColumnsConfig<Post>.Create ()
				.AddColumn ("Профессия").AddTextRenderer (p => p.Name)
				.Finish (); 
			ytreeProfessions.ItemsDataSource = Entity.ObservableProfessions;
			ytreeProfessions.Selection.Changed += YtreeProfessions_Selection_Changed;

			ytreeItems.ColumnsConfig = FluentColumnsConfig<NormItem>.Create ()
				.AddColumn ("Наименование").AddTextRenderer (p => p.Item.Name)
				.AddColumn ("Количество")
				.AddNumericRenderer (i => i.Amount).WidthChars (10).Editing ().Adjustment (new Gtk.Adjustment(1, 0, 1000000, 1, 10, 10))
				.AddTextRenderer (i => i.Item != null && i.Item.Units != null ? i.Item.Units.Name : String.Empty)
				.AddColumn ("Период")
				.AddNumericRenderer (i => i.PeriodCount).Editing ().Adjustment (new Gtk.Adjustment(1, 0, 100, 1, 10, 10))
				.AddEnumRenderer (i => i.NormPeriod).Editing ()
				.Finish ();
			ytreeItems.ItemsDataSource = Entity.ObservableItems;
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
		}

		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonRemoveItem.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}

		void YtreeProfessions_Selection_Changed (object sender, EventArgs e)
		{
			buttonRemoveProfession.Sensitive = ytreeProfessions.Selection.CountSelectedRows () > 0;
		}

		public override bool Save ()
		{
			logger.Info ("Запись нормы...");
			var valid = new QSValidation.QSValidator<Norm> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			try {
				UoWGeneric.Save ();
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog (this, "Не удалось сохранить норму.", logger, ex);
				return false;
			}
			logger.Info ("Ok");
			return true;
		}
			
		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if (Save ())
				Respond (Gtk.ResponseType.Ok);
		}

		protected void OnButtonAddProfessionClicked (object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference(typeof(Post));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ObjectSelected += SelectDialog_ObjectSelected;

			TabParent.AddSlaveTab (this, SelectDialog);
		}

		void SelectDialog_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Entity.AddProfession (e.Subject as Post);
		}

		protected void OnButtonRemoveProfessionClicked (object sender, EventArgs e)
		{
			Entity.RemoveProfession (ytreeProfessions.GetSelectedObject<Post> ());
		}

		protected void OnButtonAddItemClicked (object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference(typeof(ItemsType));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ObjectSelected += SelectDialog_ItemsTypeSelected;

			TabParent.AddSlaveTab (this, SelectDialog);
		}

		void SelectDialog_ItemsTypeSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Entity.AddItem (e.Subject as ItemsType);
		}

		protected void OnButtonRemoveItemClicked (object sender, EventArgs e)
		{
			var toRemove = ytreeItems.GetSelectedObject<NormItem>();
			IList<EmployeeCard> worksEmployees = null;

			if (toRemove.Id > 0)
			{
				worksEmployees = Repository.EmployeeRepository.GetEmployeesDependenceOnNormItem(UoW, toRemove);
				if (worksEmployees.Count > 0)
				{
					List<string> operations = new List<string>();
					foreach (var emp in worksEmployees)
					{
						bool canSwitch = emp.UsedNorms.SelectMany(x => x.Items)
							.Any(i => i.Id != toRemove.Id && i.Item.Id == toRemove.Item.Id);
						if (canSwitch)
							operations.Add(String.Format("* У сотрудника {0} требование спецодежды будет пререключено на другую норму.", emp.ShortName));
						else
							operations.Add(String.Format("* У сотрудника {0} будет удалено требование выдачи спецодежды.", emp.ShortName));
					}

					var mes = "При удалении строки нормы будут выполнены следующие операции:\n";
					mes += String.Join("\n", operations.Take(10));
					if (operations.Count > 10)
						mes += String.Format("\n... и еще {0}", operations.Count - 10);
					mes += "\nВы уверены что хотите выполнить удаление?";
					if (!MessageDialogWorks.RunQuestionDialog(mes))
						return;
				}
			}
			Entity.RemoveItem (ytreeItems.GetSelectedObject<NormItem> ());

			if(worksEmployees != null)
			{
				foreach(var emp in worksEmployees)
				{
					emp.UoW = UoW;
					emp.UpdateWorkwearItems();
					UoW.Save(emp);
				}
			}
		}

		protected void OnButtonNewProfessionClicked (object sender, EventArgs e)
		{
			var prof = OrmSimpleDialog.RunSimpleDialog (this, typeof(Post), null) as Post;
			if (prof != null)
				Entity.AddProfession (prof);
		}
	}
}

