using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using NLog;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Utilities;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Models.Import;
using Workwear.Repository.Stock;
using Workwear.Tools.Features;
using workwear.Tools.Import;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Stock;

namespace workwear
{
	public partial class IncomeDocDlg : EntityDialogBase<Income>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		ILifetimeScope AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
		private readonly IUserService userService;
		private readonly SizeService sizeService;
		private readonly IInteractiveService interactiveService;
		private readonly ITdiCompatibilityNavigation tdiNavigationManager;
		private readonly IProgressBarDisplayable progressBar;

		private FeaturesService featuresService;

		public IncomeDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Income> ();
			featuresService = AutofacScope.Resolve<FeaturesService>();
			userService = AutofacScope.Resolve<IUserService>();
			sizeService = AutofacScope.Resolve<SizeService>();
			interactiveService = AutofacScope.Resolve<IInteractiveService>();
			tdiNavigationManager = AutofacScope.Resolve<ITdiCompatibilityNavigation>();
			progressBar = AutofacScope.Resolve<IProgressBarDisplayable>();
			
			Entity.Date = DateTime.Today;
			Entity.CreatedbyUser = userService.GetCurrentUser();
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
		
		//Конструктор используется в журнале документов
		public IncomeDocDlg (Income item) : this (item.Id) {}
		public IncomeDocDlg (int id) {
			Build ();
			AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Income> (id);
			featuresService = AutofacScope.Resolve<FeaturesService>();
			sizeService = AutofacScope.Resolve<SizeService>(); 
			tdiNavigationManager = AutofacScope.Resolve<ITdiCompatibilityNavigation>();
			interactiveService = AutofacScope.Resolve<IInteractiveService>();
			
			if(UoW.IsNew) 
				autoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
			
			ConfigureDlg ();
		}

		
		#region Свойства View
		//public bool SensitiveDocNumber => !AutoDocNumber;
		public bool autoDocNumber = true;
		public bool AutoDocNumber {
			get => autoDocNumber;
			set {
				autoDocNumber = value;
				entryId.Sensitive = !autoDocNumber;
				checkAuto.Active = autoDocNumber;
			}
		}

		//[PropertyChangedAlso(nameof(DocNumber))]
		//[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		//public bool AutoDocNumber { get => autoDocNumber; set => SetField(ref autoDocNumber, value); }
		//public string DocNumber {
	//		get => AutoDocNumber ? (Entity.Id != 0 ? Entity.Id.ToString() : "авто" ) : Entity.DocNumber;
	//		set => Entity.DocNumber = (AutoDocNumber || value == "авто") ? null : value;
	//	}
		#endregion
		
		private void ConfigureDlg() {
			//ylabelId.Binding
			//	.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter())
			//	.InitializeFromSource ();
			yentryNumber.Binding
				.AddBinding(Entity, e => e.DocNumber ?? e.Id.ToString(), w => w.Text)
			//	.AddBinding(() => AutoDocNumber, w => w.Sensitive)
				.InitializeFromSource();
			//entryId.Binding//.AddSource(Entity)
			//	.AddFuncBinding(Entity, e => (e.DocNumber != null ? e.DocNumber : e.Id.ToString()), w => w.Text)
			//	.InitializeFromSource ();

			//checkAuto.Binding.AddFuncBinding(Entity, e => e,)
				
				//Binding.AddFuncBinding(Entity, AutoDocNumber, w => w.Active) //,  //(e => (e.DocNumber != null ? e.DocNumber : e.Id.ToString()), w => w.Active)
				//.InitializeFromSource ();
			
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
			
			enumPrint.ItemsEnum = typeof(IncomeDocReportEnum);

			ItemsTable.IncomeDoc = Entity;
			ItemsTable.SizeService = sizeService;
			ItemsTable.Interactive = interactiveService;

			var builder = new LegacyEEVMBuilderFactory<Income>(this, Entity, UoW, MainClass.MainWin.NavigationManager, AutofacScope);

			entityWarehouseIncome.ViewModel = builder.ForProperty(x => x.Warehouse)
									.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
									.UseViewModelDialog<WarehouseViewModel>()
									.Finish();

			yentryEmployee.ViewModel = builder.ForProperty(x => x.EmployeeCard)
						.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
						.UseViewModelDialog<EmployeeViewModel>()
						.Finish();
			
			//Метод отключает модули спецодежды, которые недоступны для пользователя
			DisableFeatures();

			ybuttonReadInFile.Clicked += OnReadFileClicked;
			
			Entity.Items.CollectionChanged += (o, a) => ycomboOperation.Sensitive = !Entity.Items.Any();
		}

		private void OnReadFileClicked(object sender, EventArgs e) {
			var file = Open1CFile();
			if(String.IsNullOrEmpty(file)) 
				return;
			var useAlternativeSize = interactiveService.Question("Использовать альтернативные значения размеров?");
			var reader = new ReaderDocumentFromXml1C(file, UoW, progressBar, useAlternativeSize);
			
			if(reader.DocumentDate != null)
				Entity.Date = reader.DocumentDate.Value;
			
			if (reader.UnreadableSizes.Any()) {
				var message = String.Join("\n", reader.UnreadableSizes.Select(x => " * " + x));
				if(!interactiveService.Question("Не удалось определить значение следующих размеров из описания номенклатуры " +
				                                $":\n{message}\n Продолжить создание документа прихода?"))
					return;
			}

			if (reader.NotFoundNomenclatureNumbers.Any()) {
				var message = String.Join("\n", reader.NotFoundNomenclatureNumbers.Take(10).Select(x => " * " + x));
				if(reader.NotFoundNomenclatureNumbers.Count > 10)
					message += $"\n и еще {reader.NotFoundNomenclatureNumbers.Count - 10}...";
				if(!interactiveService.Question($"Не найден номенклатурный номер у номенклатур:\n{message}\n " +
				                                "Продолжить создание документа прихода?"))
					return;
			}

			if(reader.NotFoundNomenclatures.Count > 0) {
				var message = String.Join("\n", reader.NotFoundNomenclatures.Take(10)
					.Select(x => $" * [Номенклатурный номер:{x.Article}]\t{x.Name}"));
				if (reader.NotFoundNomenclatures.Count > 10)
					message += $"\n и еще {reader.NotFoundNomenclatures.Count - 10}...";
				if(interactiveService.Question($"Следующих номенклатур нет в справочнике:\n{message}\n Создать?")) {
					var openNomenclatureDialog = false;
					var nomenclatureTypes = new NomenclatureTypes(UoW, sizeService, true);
					foreach(var notFoundNomenclature in reader.NotFoundNomenclatures) {
						var type = nomenclatureTypes.ParseNomenclatureName(notFoundNomenclature.Name);
						if(type is null) {
							var page = tdiNavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(null,
								EntityUoWBuilder.ForCreate(),
								OpenPageOptions.AsSlave);
							page.ViewModel.Entity.Name = notFoundNomenclature.Name;
							page.ViewModel.Entity.Number = notFoundNomenclature.Article;
							openNomenclatureDialog = true;
						}
						else {
							if(type.Id == 0)
								UoW.Save(type);
							var nomenclature = new Nomenclature {
								Name = notFoundNomenclature.Name, 
								Number = notFoundNomenclature.Article,
								Type = type,
								Comment = "Создано при загрузке поступления из файла"
							};
							UoW.Save(nomenclature);
						}
					}

					interactiveService.ShowMessage(ImportanceLevel.Info,
						openNomenclatureDialog
							? "Сохраните номенклатуру(ы) и повторите загрузку документа."
							: "Созданы новые номенклатуры, повторите загрузку документа.", "Загрузка документа");
					UoW.Commit();
					return;
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
			
			logger.Info ("Валидация");
			var valid = AutofacScope.Resolve<IValidator>();
			if (!valid.Validate(Entity))
				return false;
			
			logger.Info ("Проверка на дубли");
			string duplicateMessage = "";
			foreach(var duplicate in Entity.Items.GroupBy(x => x.StockPosition).Where(x => x.Count() > 1)) {
				duplicateMessage += $"- {duplicate.First().StockPosition.Title} указано " +
				                    $"{NumberToTextRus.FormatCase(duplicate.Count(), "{0} раз", "{0} раза", "{0} раз")}" 
				                    + $", общим количеством {duplicate.Sum(x=>x.Amount)} \n";
			}
			if(!String.IsNullOrEmpty(duplicateMessage) && !interactiveService.Question($"В документе есть повторяющиеся складские позиции:\n{duplicateMessage}\n Сохранить документ?"))
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
			labelTTN.Visible = yentryNumber.Visible = ybuttonReadInFile.Visible = Entity.Operation == IncomeOperations.Enter;
			labelWorker.Visible = yentryEmployee.Visible = enumPrint.Visible = Entity.Operation == IncomeOperations.Return;
			
			if (UoWGeneric.IsNew)
				switch (Entity.Operation)
				{
					case IncomeOperations.Enter:
						TabName = "Новая приходная накладная";
						break;
					case IncomeOperations.Return:
						TabName = "Новый возврат от работника";
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

			ybuttonReadInFile.Visible = featuresService.Available(WorkwearFeature.Exchange1C);
		}
		#endregion
		
		private void OnEnumPrintEnumItemClicked(object sender, QS.Widgets.EnumItemClickedEventArgs e) 
		{
			IncomeDocReportEnum docType = (IncomeDocReportEnum)e.ItemEnum;
			if (UoW.HasChanges && !interactiveService.Question("Перед печатью документ будет сохранён. Продолжить?"))
				return;
			if (!Save())
				return;
			
			var reportInfo = new ReportInfo {
				Title = docType == IncomeDocReportEnum.ReturnSheet ? $"Документ №{Entity.DocNumber ?? Entity.Id.ToString()}"
					: $"Ведомость №{Entity.DocNumber ?? Entity.Id.ToString()}",
				Identifier = docType.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};
			tdiNavigationManager.OpenViewModelOnTdi<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
		
		public enum IncomeDocReportEnum
		{
			[Display(Name = "Лист возврата")]
			[ReportIdentifier("Documents.ReturnSheet")]
			ReturnSheet,
			[Display(Name = "Ведомость возврата книжная")]
			[ReportIdentifier("Statements.ReturnStatementVertical")]
			ReturnStatementVertical
		}
	}
}

