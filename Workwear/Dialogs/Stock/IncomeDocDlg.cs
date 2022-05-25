using System;
using System.Linq;
using Autofac;
using Gamma.Binding.Converters;
using NLog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.Repository;
using workwear.ViewModels.Company;
using workwear.Repository.Stock;
using workwear.Tools.Features;
using workwear.ViewModels.Stock;
using QS.Services;

namespace workwear
{
	public partial class IncomeDocDlg : EntityDialogBase<Income>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		ILifetimeScope AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();

		private FeaturesService featuresService;
		public FeaturesService FeaturesService { get => FeaturesService; private set => featuresService = value; }

		public IncomeDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Income> ();
			featuresService = AutofacScope.Resolve<FeaturesService>();
			Entity.Date = DateTime.Today;
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			if(Entity.Warehouse == null)
				Entity.Warehouse = new StockRepository().GetDefaultWarehouse(UoW,featuresService, AutofacScope.Resolve<IUserService>().CurrentUserId);

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
			featuresService = AutofacScope.Resolve<FeaturesService>();
			ConfigureDlg ();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource ();

			ylabelCreatedBy.Binding.AddFuncBinding (Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource ();

			ydateDoc.Binding.AddBinding (Entity, e => e.Date, w => w.Date).InitializeFromSource ();

			yentryNumber.Binding.AddBinding (Entity, e => e.Number, w => w.Text).InitializeFromSource ();

			ycomboOperation.ItemsEnum = typeof(IncomeOperations);
			ycomboOperation.Binding.AddBinding (Entity, e => e.Operation, w => w.SelectedItemOrNull).InitializeFromSource ();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ItemsTable.IncomeDoc = Entity;

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
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;
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
					TabName = "Новый возврат c подразделения";
				break;
			}

		}

		public override void Destroy()
		{
			base.Destroy();
			AutofacScope.Dispose();
		}

		#region Workwear featrures
		private void DisableFeatures()
		{
			if(!featuresService.Available(WorkwearFeature.Warehouses)) {
				label3.Visible = false;
				entityWarehouseIncome.Visible = false;
				if(Entity.Warehouse == null)
					entityWarehouseIncome.ViewModel.Entity = Entity.Warehouse = new StockRepository().GetDefaultWarehouse(UoW, featuresService, AutofacScope.Resolve<IUserService>().CurrentUserId);
			}
		}
		#endregion
	}
}

