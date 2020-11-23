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
using workwear.Repository.Stock;
using workwear.Tools.Features;
using workwear.ViewModels.Stock;

namespace workwear
{
	public partial class IncomeDocDlg : EntityDialogBase<Income>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		ILifetimeScope AutofacScope;

		private FeaturesService featuresService;
		public FeaturesService FeaturesService { get => FeaturesService; private set => featuresService = value; }

		public IncomeDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Income> ();
			Entity.Date = DateTime.Today;
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			featuresService = new FeaturesService();
			if(Entity.Warehouse == null)
				Entity.Warehouse = new StockRepository().GetDefaultWarehouse(UoW,featuresService);

			ConfigureDlg ();
		}

		public IncomeDocDlg (EmployeeCard employee) : this () 
		{
			Entity.Operation = IncomeOperations.Return;
			Entity.EmployeeCard = UoW.GetById<EmployeeCard>(employee.Id);
		}

		public IncomeDocDlg (Subdivision subdivision) : this () 
		{
			Entity.Operation = IncomeOperations.Object;
			Entity.Subdivision = UoW.GetById<Subdivision>(subdivision.Id);
		}

		public IncomeDocDlg (Income item) : this (item.Id) {}

		public IncomeDocDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Income> (id);
			featuresService = new FeaturesService();
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
			//Метод отключает модули спецодежды, которые недоступны для пользователя
			DisableFeatures();
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
				Entity.UpdateEmployeeWearItems();
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
				string str = String.Join("\n", readerIncomeFromXML1C.listDontFindOZMInDoc.Take(10).Select(x => " * " + x));
				if(readerIncomeFromXML1C.listDontFindOZMInDoc.Count > 10)
					str += $"\n и еще {readerIncomeFromXML1C.listDontFindOZMInDoc.Count - 10}...";
				if(!MessageDialogHelper.RunQuestionDialog($"Не найден ОЗМ у номенклатур:\n{str}\n Продолжить создание документа прихода?")) {
					MessageDialogHelper.RunWarningDialog("Создание документа прихода невозможно.");
					return;
				}
			}

			if(readerIncomeFromXML1C.listDontFindNomenclature.Count > 0) {
				string str = String.Join("\n", readerIncomeFromXML1C.listDontFindNomenclature.Take(10).Select(x => $" * [ОЗМ:{x.Ozm}]\t{x.Name}"));
				if (readerIncomeFromXML1C.listDontFindNomenclature.Count > 10)
					str += $"\n и еще {readerIncomeFromXML1C.listDontFindNomenclature.Count - 10}...";
				if(MessageDialogHelper.RunQuestionDialog($"Следующих номенклатур нет в справочнике:\n{str}\n Создать?")) {
					foreach(var nom in readerIncomeFromXML1C.listDontFindNomenclature)
						MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<NomenclatureViewModel, IEntityUoWBuilder, LineIncome>(this, EntityUoWBuilder.ForCreate(), nom, OpenPageOptions.AsSlave);
					MessageDialogHelper.RunInfoDialog("Сохраните номенклатуру(ы) и повторите загрузку документа.","Загрузка документа");
				}
				else {
					MessageDialogHelper.RunErrorDialog("Создание документа прихода невозможно.");
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

		#region Workwear featrures
		private void DisableFeatures()
		{
			if(!featuresService.Available(WorkwearFeature.Warehouses)) {
				label3.Visible = false;
				entityWarehouseIncome.Visible = false;
				if(Entity.Warehouse == null)
					entityWarehouseIncome.ViewModel.Entity = Entity.Warehouse = new StockRepository().GetDefaultWarehouse(UoW, featuresService);
			}
		}
		#endregion
	}
}

