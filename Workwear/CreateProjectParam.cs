using System.Data.Common;
using Autofac;
using QS.BusinessCommon;
using QS.BusinessCommon.Domain;
using QS.Deletion;
using QS.Deletion.Views;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.NewsFeed;
using QS.Permissions;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Search.GtkUI;
using QS.Project.Services;
using QS.Project.Services.GtkUI;
using QS.Report;
using QS.Report.ViewModels;
using QS.Report.Views;
using QS.Services;
using QS.Tdi;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Resolve;
using QS.Views.Resolve;
using QSOrmProject;
using workwear.Dialogs.Organization;
using workwear.Dialogs.Regulations;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Domain.Users;
using workwear.Journal;
using workwear.Repository.Operations;
using workwear.Tools;
using workwear.Tools.Features;
using workwear.ViewModels.Company;
using workwear.Views.Company;

namespace workwear
{
	partial class MainClass
	{
		static void CreateBaseConfig ()
		{
			logger.Info ("Настройка параметров базы...");

			// Настройка ORM
			var db = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.ConnectionString (QSProjectsLib.QSMain.ConnectionString)
				.ShowSql ()
				.FormatSql ();

			OrmConfig.ConfigureOrm (db, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(MainClass)),
				System.Reflection.Assembly.GetAssembly (typeof(MeasurementUnits)),
			});

			//Настраиваем классы сущьностей

			OrmMain.AddObjectDescription(MeasurementUnitsOrmMapping.GetOrmMapping());
			//Спецодежда
			OrmMain.AddObjectDescription<RegulationDoc>().Dialog<RegulationDocDlg>().DefaultTableView().SearchColumn("Документ", i => i.Title).OrderAsc(i => i.Name).End();
			//Организация
			OrmMain.AddObjectDescription<EmployeeVacation>().Dialog<EmployeeVacationDlg>();
			OrmMain.AddObjectDescription<VacationType>().Dialog<VacationTypeDlg>().DefaultTableView().SearchColumn("Название", e => e.Name).Column("Исключать из носки", e => e.ExcludeFromWearing ? "Да" : "Нет").SearchColumn("Комментарий", e => e.Comments).End();
			//Общее
			OrmMain.AddObjectDescription<UserBase>().DefaultTableView ().Column ("Имя", e => e.Name).End ();
			OrmMain.AddObjectDescription<UserSettings>();
			//Склад
			OrmMain.AddObjectDescription<Income>().Dialog<IncomeDocDlg>();
			OrmMain.AddObjectDescription<Writeoff>().Dialog<WriteOffDocDlg>();

			NotifyConfiguration.Enable();
			BuisnessLogicGlobalEventHandler.Init(new GtkQuestionDialogsInteractive());
			JournalsColumnsConfigs.RegisterColumns();
		}

		public static Autofac.IContainer AppDIContainer;

		static void AutofacClassConfig()
		{
			var builder = new ContainerBuilder();

			#region База
			builder.RegisterType<DefaultUnitOfWorkFactory>().As<IUnitOfWorkFactory>();
			builder.RegisterType<DefaultSessionProvider>().As<ISessionProvider>();
			builder.Register<DbConnection>(c => Connection.ConnectionDB).AsSelf();
			#endregion

			#region Сервисы
			#region GtkUI
			builder.RegisterType<GtkMessageDialogsInteractive>().As<IInteractiveMessage>();
			builder.RegisterType<GtkQuestionDialogsInteractive>().As<IInteractiveQuestion>();
			builder.RegisterType<GtkInteractiveService>().As<IInteractiveService>();
			builder.RegisterType<GtkValidationViewFactory>().As<IValidationViewFactory>();
			#endregion GtkUI
			#region Удаление
			builder.RegisterModule(new DeletionAutofacModule());
			builder.RegisterType<DeleteEntityGUIService>().As<IDeleteEntityService>();
			builder.Register(x => DeleteConfig.Main).AsSelf();
 			#endregion
			//FIXME Нужно в конечнои итоге попытаться избавится от CommonServce вообще.
			builder.RegisterType<CommonServices>().As<ICommonServices>();
			builder.RegisterType<UserService>().As<IUserService>();
			builder.RegisterType<ObjectValidator>().As<IValidator>();
			//FIXME Реализовать везде возможность отсутствия сервиса прав, чтобы не приходилось создавать то что не используется
			builder.RegisterType<DefaultAllowedPermissionService>().As<IPermissionService>();
			builder.RegisterType<CommonMessages>().AsSelf();
			#endregion

			#region Навигация
			builder.Register(ctx => new ClassNamesHashGenerator(new[] {new RDLReportsHashGenerator() })).As<IPageHashGenerator>();
			builder.Register((ctx) => new AutofacViewModelsTdiPageFactory(AppDIContainer)).As<IViewModelsPageFactory>();
			builder.Register((ctx) => new AutofacTdiPageFactory(AppDIContainer)).As<ITdiPageFactory>();
			builder.Register((ctx) => new AutofacViewModelsGtkPageFactory(AppDIContainer)).AsSelf();
			builder.RegisterType<TdiNavigationManager>().AsSelf().As<INavigationManager>().As<ITdiCompatibilityNavigation>().SingleInstance();
			builder.RegisterType<BasedOnNameTDIResolver>().As<ITDIWidgetResolver>();
			builder.Register(cc => new ClassNamesBaseGtkViewResolver(typeof(RdlViewerView), typeof(OrganizationView), typeof(DeletionView))).As<IGtkViewResolver>();
			#endregion

			#region Главное окно
			builder.Register((ctx) => MainWin.ProgressBar).As<IProgressBarDisplayable>();
			#endregion

			#region Старые диалоги
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(IncomeDocDlg)))
				.Where(t => t.IsAssignableTo<ITdiTab>() && t.Name.EndsWith("Dlg"))
				.AsSelf();
			#endregion

			#region Старые общие диалоги
			builder.RegisterType<OrmReference>().AsSelf();
			builder.RegisterType<ReferenceRepresentation>().AsSelf();
			#endregion

			#region Rdl
			builder.RegisterType<RdlViewerViewModel>().AsSelf();
			#endregion

			#region Журналы
			builder.RegisterType<OneEntrySearchView>().Named<Gtk.Widget>("GtkJournalSearchView");
			#endregion

			#region ViewModels
			builder.Register(x => new AutofacViewModelResolver(AppDIContainer)).As<IViewModelResolver>();
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(OrganizationViewModel)))
				.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
				.AsSelf();
			#endregion

			#region Repository
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(EmployeeIssueRepository)))
				.Where(t => t.Name.EndsWith("Repository"))
				.AsSelf();
			#endregion

			#region News
			builder.RegisterType<FeedReader>().AsSelf();
			#endregion

			#region Разделение версий
			builder.RegisterType<FeaturesService>().AsSelf();
			#endregion
			AppDIContainer = builder.Build();
		}
	}
}
