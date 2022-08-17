using System;
using System.Linq;
using Autofac;
using Gamma.Binding.Converters;
using NLog;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Measurements;
using workwear.Repository;
using workwear.Repository.Stock;
using workwear.Tools.Features;
using workwear.Tools.Import;
using workwear.ViewModels.Company;
using workwear.ViewModels.Stock;

namespace workwear
{
	public partial class IncomeDocDlg : EntityDialogBase<Income>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		ILifetimeScope AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
		private readonly SizeService sizeService;
		private readonly IInteractiveService interactiveService;
		private readonly INavigationManager navigationManager;
		private readonly IProgressBarDisplayable progressBar;

		private FeaturesService featuresService;

		public IncomeDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Income> ();
			featuresService = AutofacScope.Resolve<FeaturesService>();
			sizeService = AutofacScope.Resolve<SizeService>();
			interactiveService = AutofacScope.Resolve<IInteractiveService>();
			navigationManager = AutofacScope.Resolve<INavigationManager>();
			progressBar = AutofacScope.Resolve<IProgressBarDisplayable>();
			
			
			Entity.Date = DateTime.Today;
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			if(Entity.Warehouse == null)
				Entity.Warehouse = new StockRepository()
					.GetDefaultWarehouse(UoW,featuresService, AutofacScope.Resolve<IUserService>().CurrentUserId);

			ConfigureDlg ();
		}
		//Конструктор используется при возврате от сотрудника
		public IncomeDocDlg(EmployeeCard employee) : this () {
			Entity.Operation = IncomeOperations.Return;
			Entity.EmployeeCard = UoW.GetById<EmployeeCard>(employee.Id);
		}
		//Конструктор используется при возврате С поздразделения
		public IncomeDocDlg(Subdivision subdivision) : this () {
			Entity.Operation = IncomeOperations.Object;
			Entity.Subdivision = UoW.GetById<Subdivision>(subdivision.Id);
		}
		//Конструктор используется в журнале документов
		public IncomeDocDlg (Income item) : this (item.Id) {}
		public IncomeDocDlg (int id) {
			Build ();
			AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Income> (id);
			featuresService = AutofacScope.Resolve<FeaturesService>();
			sizeService = AutofacScope.Resolve<SizeService>();
			ConfigureDlg ();
		}

		private void ConfigureDlg() {
			ylabelId.Binding
				.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter())
				.InitializeFromSource ();
			ylabelCreatedBy.Binding
				.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp)
				.InitializeFromSource ();

			ydateDoc.Binding
				.AddBinding(Entity, e => e.Date, w => w.Date)
				.InitializeFromSource ();

			yentryNumber.Binding
				.AddBinding(Entity, e => e.Number, w => w.Text)
				.InitializeFromSource();

			ycomboOperation.ItemsEnum = typeof(IncomeOperations);
			ycomboOperation.Binding
				.AddBinding(Entity, e => e.Operation, w => w.SelectedItemOrNull)
				.InitializeFromSource ();

			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			ItemsTable.IncomeDoc = Entity;
			ItemsTable.SizeService = sizeService;

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

			ybuttonReadInFile.Clicked += OnReadFileClicked;
		}

		private void OnReadFileClicked(object sender, EventArgs e) {
			var file = Open1CFile();
			if(String.IsNullOrEmpty(file)) 
				return;
			var useAlternativeSize = interactiveService.Question("Использовать альтернативные значения размеров?");
			var reader = new ReaderDocumentFromXml1C(file, UoW, progressBar, useAlternativeSize);
			
			if(reader.DocumentDate != null)
				Entity.Date = reader.DocumentDate.Value;

			if (reader.NotFoundNomenclatureNumbers.Any()) {
				var message = String.Join("\n", reader.NotFoundNomenclatureNumbers.Take(10).Select(x => " * " + x));
				if(reader.NotFoundNomenclatureNumbers.Count > 10)
					message += $"\n и еще {reader.NotFoundNomenclatureNumbers.Count - 10}...";
				if(!interactiveService.Question($"Не найден номенклатурный номер у номенклатур:\n{message}\n " +
				                                "Продолжить создание документа прихода?"))
					return;
			}
			
			if (reader.UnreadableArticle.Any()) {
				var message = String.Join("\n", reader.UnreadableArticle.Take(10).Select(x => " * " + x));
				if(reader.UnreadableArticle.Count > 10)
					message += $"\n и еще {reader.UnreadableArticle.Count - 10}...";
				if(!interactiveService.Question($"Не удалось определить значение у следующих номенклатурных номеров:\n{message}\n " +
				                                "Продолжить создание документа прихода?"))
					return;
			}

			if(reader.NotFoundNomenclatures.Count > 0) {
				var message = String.Join("\n", reader.NotFoundNomenclatures.Take(10)
					.Select(x => $" * [Номенклатурный номер:{x.Article}]\t{x.Name}"));
				if (reader.NotFoundNomenclatures.Count > 10)
					message += $"\n и еще {reader.NotFoundNomenclatures.Count - 10}...";
				if(interactiveService.Question($"Следующих номенклатур нет в справочнике:\n{message}\n Создать?")) {
					foreach(var notFoundNomenclature in reader.NotFoundNomenclatures) {
						var page = navigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(null, 
							EntityUoWBuilder.ForCreate(), 
							OpenPageOptions.AsSlave);
						page.ViewModel.Entity.Name = notFoundNomenclature.Name;
						page.ViewModel.Entity.Number = notFoundNomenclature.Article;
					}
					interactiveService.ShowMessage(ImportanceLevel.Info,
						"Сохраните номенклатуру(ы) и повторите загрузку документа.","Загрузка документа");
				}
			}

			if(reader.DocumentItems.Any())
				foreach(var item in reader.DocumentItems)
					Entity.AddItem(item.Nomenclature, item.Size, item.Height, item.Amount, null, item.Cost);
			else
				interactiveService.ShowMessage(ImportanceLevel.Info, "Указанный файл не содержит строк поступления");

		}

		private string Open1CFile() {
			var param = new object[] { "Cancel", Gtk.ResponseType.Cancel, "Open", Gtk.ResponseType.Accept};
			var fileChooserDialog = new Gtk.FileChooserDialog("Open File", null, Gtk.FileChooserAction.Open, param);
			var nameFile = String.Empty;
			if(fileChooserDialog.Run() == (int)Gtk.ResponseType.Accept)
				if(fileChooserDialog.Filename.ToLower().EndsWith(".xml"))
					nameFile = fileChooserDialog.Filename;
				else
					interactiveService.ShowMessage(ImportanceLevel.Error, "Формат файла не поддерживается");
			fileChooserDialog.Destroy();
			return nameFile;
		}

		public override bool Save() {
			logger.Info ("Запись документа...");
			var valid = new QSValidator<Income> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)Toplevel))
				return false;

			var ask = new GtkQuestionDialogsInteractive();
			Entity.UpdateOperations(UoW, ask);
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;
			UoWGeneric.Save ();
			if(Entity.Operation == IncomeOperations.Return) {
				logger.Debug ("Обновляем записи о выданной одежде в карточке сотрудника...");
				Entity.UpdateEmployeeWearItems();
				UoWGeneric.Commit ();
			}

			logger.Info ("Ok");
			return true;
		}

		private void OnYcomboOperationChanged (object sender, EventArgs e) {
			labelTTN.Visible = yentryNumber.Visible = Entity.Operation == IncomeOperations.Enter;
			labelWorker.Visible = yentryEmployee.Visible = Entity.Operation == IncomeOperations.Return;
			labelObject.Visible = entrySubdivision.Visible = Entity.Operation == IncomeOperations.Object;

			if (UoWGeneric.IsNew)
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
		public override void Destroy() {
			base.Destroy();
			AutofacScope.Dispose();
		}
		#region Workwear featrures
		private void DisableFeatures() {
			if (!featuresService.Available(WorkwearFeature.Warehouses))
			{
				label3.Visible = false;
				entityWarehouseIncome.Visible = false;
				if (Entity.Warehouse == null)
					entityWarehouseIncome.ViewModel.Entity = Entity.Warehouse = new StockRepository()
						.GetDefaultWarehouse(UoW, featuresService, AutofacScope.Resolve<IUserService>().CurrentUserId);
			}
		}
		#endregion
	}
}

