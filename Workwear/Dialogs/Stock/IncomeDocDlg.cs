using System;
using System.Linq;
using Autofac;
using Gamma.Binding.Converters;
using NLog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.Repository;
using workwear.Tools;
using workwear.ViewModels.Company;
using workwear.ViewModels.Stock;

namespace workwear
{
	public partial class IncomeDocDlg : EntityDialogBase<Income>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		ILifetimeScope AutofacScope;

		public IncomeDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Income> ();
			Entity.Date = DateTime.Today;
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			ConfigureDlg ();
		}

		public IncomeDocDlg (EmployeeCard employee) : this () 
		{
			Entity.Operation = IncomeOperations.Return;
			Entity.EmployeeCard = employee;
		}

		public IncomeDocDlg (Subdivision facility) : this () 
		{
			Entity.Operation = IncomeOperations.Object;
			Entity.Subdivision = facility;
		}

		public IncomeDocDlg (Income item) : this (item.Id) {}

		public IncomeDocDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Income> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource ();

			ylabelCreatedBy.Binding.AddFuncBinding (Entity, e => e.CreatedbyUser.Name, w => w.LabelProp).InitializeFromSource ();

			ydateDoc.Binding.AddBinding (Entity, e => e.Date, w => w.Date).InitializeFromSource ();

			yentryNumber.Binding.AddBinding (Entity, e => e.Number, w => w.Text).InitializeFromSource ();

			ycomboOperation.ItemsEnum = typeof(IncomeOperations);
			ycomboOperation.Binding.AddBinding (Entity, e => e.Operation, w => w.SelectedItemOrNull).InitializeFromSource ();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ItemsTable.IncomeDoc = Entity;

			AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();

			var builder = new LegacyEEVMBuilderFactory<Income>(this, Entity, UoW, MainClass.MainWin.NavigationManager, AutofacScope);


			entityWarehouseIncome.ViewModel = builder.ForProperty(x => x.Warehouse)
									.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
									.UseViewModelDialog<WarehouseViewModel>()
									.Finish();

			yentryEmployee.ViewModel = builder.ForProperty(x => x.EmployeeCard)
						.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
						.UseViewModelDialog<EmployeeViewModel>()
						.Finish();

			entrySubdivision.ViewModel = builder.ForProperty(x => x.Subdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();
		}

		public override bool Save()
		{
			logger.Info ("Запись документа...");
			var valid = new QSValidator<Income> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			var ask = new GtkQuestionDialogsInteractive();
			Entity.UpdateOperations(UoW, ask);
			UoWGeneric.Save ();
			if(Entity.Operation == IncomeOperations.Return)
			{
				logger.Debug ("Обновляем записи о выданной одежде в карточке сотрудника...");
				foreach(var itemsGroup in Entity.Items.GroupBy (i => i.Nomenclature.Type.Id))
				{
					var wearItem = Entity.EmployeeCard.WorkwearItems.FirstOrDefault (i => i.Item.Id == itemsGroup.Key);
					if(wearItem == null)
					{
						logger.Debug ("Позиции <{0}> не требуется к выдаче, пропускаем...", itemsGroup.First ().Nomenclature.Type.Name);
						continue;
					}

					wearItem.UpdateNextIssue (UoW);
				}
				UoWGeneric.Commit ();
			}

			logger.Info ("Ok");
			return true;
		}

		protected void OnYcomboOperationChanged (object sender, EventArgs e)
		{
			labelTTN.Visible = yentryNumber.Visible = Entity.Operation == IncomeOperations.Enter;
			labelWorker.Visible = yentryEmployee.Visible = Entity.Operation == IncomeOperations.Return;
			labelObject.Visible = entrySubdivision.Visible = Entity.Operation == IncomeOperations.Object;
			btnOpenDoc1C.Visible = Entity.Operation == IncomeOperations.Enter;

			if (!UoWGeneric.IsNew)
				return;
			
			switch (Entity.Operation)
			{
			case IncomeOperations.Enter:
					TabName = "Новая приходная накладная";
				break;
			case IncomeOperations.Return:
					TabName = "Новый возврат от работника";
				break;
			case IncomeOperations.Object:
					TabName = "Новый возврат c объекта";
				break;
			}

		}

		protected void OnBtnOpenDoc1CClicked(object sender, EventArgs e)
		{
			string file = Open1CFile();
			if(file.Length < 1) return;

			ReaderIncomeFromXML1C readerIncomeFromXML1C = new ReaderIncomeFromXML1C(file);
			readerIncomeFromXML1C.StartReadDoc1C();

			Entity.Date = readerIncomeFromXML1C.Date;

			if (readerIncomeFromXML1C.listDontFindOZMInDoc.Count > 0) {
				string str = "";
				foreach(var nom in readerIncomeFromXML1C.listDontFindOZMInDoc)
					str += nom + "\n";
				if(!MessageDialogHelper.RunQuestionDialog($"У номенклатур:\n{str}не найден озм. Продолжить создание документа прихода?")) {
					MessageDialogHelper.RunWarningDialog("Создание документа прихода невозможно.");
					return;
				}

			}

			if(readerIncomeFromXML1C.listDontFindNomenclature.Count > 0) {
				string str = "";
				foreach(var nom in readerIncomeFromXML1C.listDontFindNomenclature)
					str += nom.Name + "\n";

				if(MessageDialogHelper.RunQuestionDialog($"Таких номенклатур:\n{str}нет в справочнике. Создать?")) {
					foreach(var nom in readerIncomeFromXML1C.listDontFindNomenclature)
						MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<NomenclatureViewModel, IEntityUoWBuilder, LineIncome>(this, EntityUoWBuilder.ForCreate(), nom, OpenPageOptions.AsSlave);
					MessageDialogHelper.RunWarningDialog("Сохраните номенклатуру(ы) и повторите загрузку документа.");
				}
				else {
					MessageDialogHelper.RunWarningDialog("Создание документа прихода невозможно.");
					return;
				}

			}
			else {
				foreach(var r in readerIncomeFromXML1C.ListLineIncomes)
					Entity.AddItem(r.Nomenclature, r.Count, r.Size, r.Growth);
			}
		}

		protected string Open1CFile()
		{
			object[] param = new object[4];
			param[0] = "Cancel";
			param[1] = Gtk.ResponseType.Cancel;
			param[2] = "Open";
			param[3] = Gtk.ResponseType.Accept;

			Gtk.FileChooserDialog fc =
				new Gtk.FileChooserDialog("Open File",
					null,
					Gtk.FileChooserAction.Open,
					param);

			Gtk.FileFilter xmlFilter = new Gtk.FileFilter();
			xmlFilter.Name = "XML";
			string nameFile = "";
			if(fc.Run() == (int)Gtk.ResponseType.Accept)
				if(fc.Filename.ToLower().EndsWith(".xml"))
					nameFile = fc.Filename;
			fc.Destroy();
			return nameFile;
		}
	}
}

