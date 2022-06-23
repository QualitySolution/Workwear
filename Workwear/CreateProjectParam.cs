using System;
using System.Data.Common;
using System.IO;
using Autofac;
using QS.BaseParameters;
using QS.BusinessCommon.Domain;
using QS.BusinessCommon;
using QS.Cloud.Client;
using QS.Cloud.WearLk.Client;
using QS.Configuration;
using QS.Deletion.Views;
using QS.Deletion;
using QS.Dialog.GtkUI;
using QS.Dialog.ViewModels;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.ErrorReporting;
using QS.ErrorReporting.Handlers;
using QS.Features;
using QS.HistoryLog.ViewModels;
using QS.HistoryLog.Views;
using QS.HistoryLog;
using QS.Navigation;
using QS.NewsFeed;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI.ServiceDlg;
using QS.Project.Domain;
using QS.Project.Search.GtkUI;
using QS.Project.Services.GtkUI;
using QS.Project.Services;
using QS.Project.Versioning.Product;
using QS.Project.Versioning;
using QS.Project.ViewModels;
using QS.Project.Views;
using QS.Report.ViewModels;
using QS.Report.Views;
using QS.Report;
using QS.Serial.Views;
using QS.Services;
using QS.Tdi;
using QS.Tools;
using QS.Updater.DB.Views;
using QS.Updater;
using QS.Utilities.Numeric;
using QS.Validation;
using QS.ViewModels.Resolve;
using QS.ViewModels;
using QS.Views.Resolve;
using QSOrmProject;
using Workwear.Measurements;
using Workwear.Sql;
using Workwear.Tools;
using workwear.Dialogs.Regulations;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Domain.Users;
using workwear.Journal;
using workwear.Models.Company;
using workwear.Models.Import;
using workwear.Models.Stock;
using workwear.Repository.Operations;
using workwear.Tools.Features;
using workwear.Tools.IdentityCards;
using workwear.Tools.Navigation;
using workwear.Tools.Nhibernate;
using workwear.Tools;
using workwear.ViewModels.Communications;
using workwear.ViewModels.Company;
using workwear.Views.Company;

namespace workwear
{
	static partial class MainClass
	{
		static void CreateBaseConfig ()
		{
			logger.Info ("Настройка параметров базы...");

			// Настройка ORM
			var db = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57ExtendedDialect>()
				.ConnectionString (QSProjectsLib.QSMain.ConnectionString)
				.AdoNetBatchSize(100)
				.ShowSql ()
				.FormatSql ();

			OrmConfig.ConfigureOrm (db, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(MainClass)),
				System.Reflection.Assembly.GetAssembly (typeof(MeasurementUnits)),
				System.Reflection.Assembly.GetAssembly (typeof(UserBase)),
				System.Reflection.Assembly.GetAssembly (typeof(HistoryMain)),

			});

			#if DEBUG
			NLog.LogManager.Configuration.RemoveRuleByName("HideNhibernate");
			#endif

			//Настраиваем классы сущностей
			OrmMain.AddObjectDescription(MeasurementUnitsOrmMapping.GetOrmMapping());
			//Спецодежда
			OrmMain.AddObjectDescription<RegulationDoc>().Dialog<RegulationDocDlg>().DefaultTableView().SearchColumn("Документ", i => i.Title).OrderAsc(i => i.Name).End();
			//Общее
			OrmMain.AddObjectDescription<UserBase>().DefaultTableView ().Column ("Имя", e => e.Name).End ();
			OrmMain.AddObjectDescription<UserSettings>();
			//Склад
			OrmMain.AddObjectDescription<Income>().Dialog<Dialogs.Stock.IncomeDocDlg>();

			NotifyConfiguration.Enable();
			BuisnessLogicGlobalEventHandler.Init(new GtkQuestionDialogsInteractive());
			JournalsColumnsConfigs.RegisterColumns();
		}
		
		public static ILifetimeScope AppDIContainer;
		static IContainer startupContainer;
		
		static void AutofacStartupConfig(ContainerBuilder containerBuilder)
		{
			#region GtkUI
			containerBuilder.RegisterType<GtkMessageDialogsInteractive>().As<IInteractiveMessage>();
			containerBuilder.RegisterType<GtkQuestionDialogsInteractive>().As<IInteractiveQuestion>();
			containerBuilder.RegisterType<GtkInteractiveService>().As<IInteractiveService>();
			containerBuilder.RegisterType<GtkGuiDispatcher>().As<IGuiDispatcher>();
			containerBuilder.RegisterType<GtkRunOperationService>().As<IRunOperationService>();
			#endregion GtkUI

			#region Versioning
			containerBuilder.RegisterType<ApplicationVersionInfo>().As<IApplicationInfo>();
			#endregion

			#region ErrorReporting
			containerBuilder.RegisterType<DesktopErrorReporter>().As<IErrorReporter>();
			containerBuilder.RegisterType<LogService>().As<ILogService>();
			#if DEBUG
			containerBuilder.Register(c => new ErrorReportingSettings(false, true, false, 300)).As<IErrorReportingSettings>();
			#else
			containerBuilder.Register(c => new  ErrorReportingSettings(true, false, true, 300)).As<IErrorReportingSettings>();
			#endif

			containerBuilder.RegisterType<MySqlException1055OnlyFullGroupBy>().As<IErrorHandler>();
			containerBuilder.RegisterType<MySqlException1366IncorrectStringValue>().As<IErrorHandler>();
			containerBuilder.RegisterType<NHibernateFlushAfterException>().As<IErrorHandler>();
			containerBuilder.RegisterType<ConnectionIsLost>().As<IErrorHandler>();
			#endregion
			
			#region Обновления и версии
			containerBuilder.RegisterModule(new UpdaterAutofacModule());
			containerBuilder.Register(c => ScriptsConfiguration.MakeUpdateConfiguration()).AsSelf();
			containerBuilder.Register(c => ScriptsConfiguration.MakeCreationScript()).AsSelf();
			#endregion

			#region Временные будут переопределены
			containerBuilder.RegisterType<GtkWindowsNavigationManager>().AsSelf().As<INavigationManager>().SingleInstance();
			containerBuilder.Register((ctx) => new AutofacViewModelsGtkPageFactory(startupContainer)).As<IViewModelsPageFactory>();
			containerBuilder.Register(cc => new ClassNamesBaseGtkViewResolver(
				typeof(UpdateProcessView)
			)).As<IGtkViewResolver>();
			#endregion
		}
		
		static void AutofacClassConfig(ContainerBuilder builder)
		{
			#region База
			builder.RegisterType<DefaultUnitOfWorkFactory>().As<IUnitOfWorkFactory>();
			builder.RegisterType<ProgressInterceptor>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<ProgresSessionProvider>().As<ISessionProvider>();
			builder.Register(c => new MySqlConnectionFactory(Connection.ConnectionString)).As<IConnectionFactory>();
			builder.Register<DbConnection>(c => c.Resolve<IConnectionFactory>().OpenConnection()).AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<BaseParameters>().As<ParametersService>().AsSelf();
			builder.Register(c => QSProjectsLib.QSMain.ConnectionStringBuilder).AsSelf().ExternallyOwned();
			builder.RegisterType<NhDataBaseInfo>().As<IDataBaseInfo>();
			builder.RegisterType<MySQLProvider>().As<IMySQLProvider>();
			#endregion

			#region Ошибки
			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var user = new UserService().GetCurrentUser(uow);
				builder.RegisterType<DesktopErrorReporter>().As<IErrorReporter>()
					.WithParameter(new TypedParameter(typeof(UserBase), user));
			}
			#endregion
			
			#region Сервисы
			#region GtkUI
			builder.RegisterType<GtkValidationViewFactory>().As<IValidationViewFactory>();
			#endregion GtkUI
			#region Удаление
			builder.RegisterModule(new DeletionAutofacModule());
			builder.RegisterType<DeleteEntityGUIService>().As<IDeleteEntityService>();
			builder.Register(x => DeleteConfig.Main).AsSelf().ExternallyOwned();
			builder.RegisterType<ReplaceEntity>().AsSelf();
 			#endregion
			builder.RegisterType<UserService>().As<IUserService>();
			builder.RegisterType<ObjectValidator>().As<IValidator>();
			builder.RegisterType<CommonMessages>().AsSelf();
			builder.RegisterType<NHibernateChangeMonitor>().As<IChangeMonitor>();
			#endregion

			#region Навигация
			builder.Register(ctx => new ClassNamesHashGenerator(new IExtraPageHashGenerator[] {new RDLReportsHashGenerator(), new AdditionalIdHashGenerator(
				 new [] {typeof(HistoryNotificationViewModel)}
				) })).As<IPageHashGenerator>();
			builder.Register((ctx) => new AutofacViewModelsTdiPageFactory(AppDIContainer)).As<IViewModelsPageFactory>();
			builder.Register((ctx) => new AutofacTdiPageFactory(AppDIContainer)).As<ITdiPageFactory>();
			builder.Register((ctx) => new AutofacViewModelsGtkPageFactory(AppDIContainer)).AsSelf();
			builder.RegisterType<TdiNavigationManager>().AsSelf().As<INavigationManager>().As<ITdiCompatibilityNavigation>().SingleInstance();
			builder.RegisterType<BasedOnNameTDIResolver>().As<ITDIWidgetResolver>();
			builder.Register(cc => new ClassNamesBaseGtkViewResolver(
				typeof(RdlViewerView), 
				typeof(OrganizationView), 
				typeof(DeletionView), 
				typeof(UpdateProcessView),
				typeof(SerialNumberView),
				typeof(HistoryView)
			)).As<IGtkViewResolver>();
			#endregion

			#region Главное окно
			builder.Register((ctx) => MainWin.ProgressBar).As<IProgressBarDisplayable>().ExternallyOwned();
			#endregion

			#region Размеры
			builder.RegisterType<SizeService>().AsSelf();
			#endregion

			#region Старые диалоги
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(Dialogs.Stock.IncomeDocDlg)))
				.Where(t => t.IsAssignableTo<ITdiTab>() && t.Name.EndsWith("Dlg"))
				.AsSelf();
			#endregion

			#region Старые общие диалоги
			builder.RegisterType<OrmReference>().AsSelf();
			builder.RegisterType<ReferenceRepresentation>().AsSelf();
			#endregion

			#region Отдельные диалоги
			builder.RegisterType<AboutView>().AsSelf();
			builder.RegisterType<AboutViewModel>().AsSelf();
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
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(ProgressWindowViewModel)))
				.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
				.AsSelf();
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(HistoryViewModel)))
				.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
				.AsSelf();
			#endregion

			#region Common Models
			builder.RegisterType<PersonNames>().AsSelf();
			builder.RegisterType<OpenStockDocumentsModel>().AsSelf();
			builder.Register(c => new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated)).AsSelf();
			#endregion

			#region Repository
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(EmployeeIssueRepository)))
				.Where(t => t.Name.EndsWith("Repository"))
				.AsSelf();
			#endregion

			#region News
			builder.RegisterType<FeedReader>().AsSelf();
			#endregion
			
			#region Облако
			builder.Register(c => new CloudClientService(QSSaaS.Session.SessionId)).AsSelf().SingleInstance();
			builder.Register(c => new LkUserManagerService(QSSaaS.Session.SessionId)).AsSelf().SingleInstance();
			builder.Register(c => new MessagesService(QSSaaS.Session.SessionId)).AsSelf().SingleInstance();
			builder.Register(c => new NotificationManagerService(QSSaaS.Session.SessionId)).AsSelf().SingleInstance();
			#endregion

			#region Разделение версий
			builder.RegisterType<FeaturesService>().As<IProductService>().AsSelf();
			builder.RegisterModule<FeaturesAutofacModule>();
			#endregion

			#region Настройка
			builder.Register(c => new IniFileConfiguration(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "workwear.ini"))).As<IChangeableConfiguration>().AsSelf();
			#endregion

			#region Работа со считывателями
			if(QSProjectsLib.WindowStartupFix.IsWindows)//FIXME Было лень реализовывать загрузку библиотеки под линукс.
				builder.RegisterType<RusGuardService>().As<ICardReaderService>();
		#if DEBUG
			else
				builder.RegisterType<VirtualCardReaderService>().As<ICardReaderService>();
		#endif
			#endregion

			#region Импрорт данных
			builder.RegisterType<DataParserEmployee>().AsSelf();
			builder.RegisterType<DataParserNorm>().AsSelf();
			builder.RegisterType<DataParserWorkwearItems>().AsSelf();
			#endregion
		}
	}
}
