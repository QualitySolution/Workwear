using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Dialogs.GtkUI;
using QSOrmProject;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Repository.Company;

namespace workwear.Dialogs.Regulations
{
	public partial class NormDlg : EntityDialogBase<Norm>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		IProgressBarDisplayable progressBar;

		public NormDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Norm> ();
			ConfigureDlg ();
		}

		public NormDlg (Norm norm) : this(norm.Id) { }

		public NormDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Norm> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg()
		{
			//FIXME Добавить в будущем получение экземпляра через конструктор
			progressBar = MainClass.MainWin.ProgressBar;

			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource ();

			ycomboAnnex.SetRenderTextFunc<RegulationDocAnnex>(x => x.Title);
			yentryRegulationDoc.SubjectType = typeof(RegulationDoc);
			yentryRegulationDoc.Binding.AddBinding(Entity, e => e.Document, w => w.Subject).InitializeFromSource();
			ycomboAnnex.Binding.AddBinding(Entity, e => e.Annex, w => w.SelectedItem).InitializeFromSource();
			datefrom.Binding.AddBinding(Entity, e => e.DateFrom, w => w.DateOrNull).InitializeFromSource();
			dateto.Binding.AddBinding(Entity, e => e.DateTo, w => w.DateOrNull).InitializeFromSource();

			yentryTonParagraph.Binding.AddBinding (Entity, e => e.TONParagraph, w => w.Text).InitializeFromSource ();
			labelOldTon.Visible = !String.IsNullOrWhiteSpace(Entity.TONAttachment) || !String.IsNullOrWhiteSpace(Entity.TONNumber);
			labelOldTon.Text = String.Format("Старые значения ТОН № {0} и приложение № {1}", Entity.TONNumber, Entity.TONAttachment);

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ytreeProfessions.ColumnsConfig = FluentColumnsConfig<Post>.Create ()
				.AddColumn ("Профессия").AddTextRenderer (p => p.Name)
				.Finish (); 
			ytreeProfessions.ItemsDataSource = Entity.ObservableProfessions;
			ytreeProfessions.Selection.Changed += YtreeProfessions_Selection_Changed;

			ytreeItems.ColumnsConfig = FluentColumnsConfig<NormItem>.Create ()
				.AddColumn ("Наименование").AddTextRenderer (p => p.Item.Name)
				.AddColumn ("Количество")
				.AddNumericRenderer (i => i.Amount).WidthChars (9).Editing ().Adjustment (new Gtk.Adjustment(1, 0, 1000000, 1, 10, 10))
				.AddTextRenderer (i => i.Item != null && i.Item.Units != null ? i.Item.Units.Name : String.Empty)
				.AddColumn ("Период")
				.AddNumericRenderer (i => i.PeriodCount).WidthChars(6).Editing ().Adjustment (new Gtk.Adjustment(1, 0, 100, 1, 10, 10))
				.AddEnumRenderer (i => i.NormPeriod).Editing ()
				.AddColumn(String.Empty)
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
			var valid = new QS.Validation.QSValidator<Norm> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save ();

			logger.Info ("Ok");
			return true;
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
				logger.Info("Поиск ссылок на удаляемую строку нормы...");
				worksEmployees = EmployeeRepository.GetEmployeesDependenceOnNormItem(UoW, toRemove);
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
					logger.Info("Ок");
					if (!MessageDialogHelper.RunQuestionDialog(mes))
						return;
				}
			}
			Entity.RemoveItem (ytreeItems.GetSelectedObject<NormItem> ());

			if(worksEmployees != null)
			{
				buttonSave.Sensitive = buttonCancel.Sensitive = false;
				progressBar.Start(worksEmployees.Count);

				foreach(var emp in worksEmployees)
				{
					emp.UoW = UoW;
					emp.UpdateWorkwearItems();
					UoW.Save(emp);
					progressBar.Add();
				}

				buttonSave.Sensitive = buttonCancel.Sensitive = true;
				progressBar.Close();
			}
		}

		protected void OnButtonNewProfessionClicked (object sender, EventArgs e)
		{
			var prof = EntityEditSimpleDialog.RunSimpleDialog ((Gtk.Window)this.Toplevel, typeof(Post), null) as Post;
			if (prof != null)
				Entity.AddProfession (prof);
		}

		protected void OnYentryRegulationDocChanged(object sender, EventArgs e)
		{
			ycomboAnnex.ItemsList = Entity.Document?.Annexess;
		}
	}
}

