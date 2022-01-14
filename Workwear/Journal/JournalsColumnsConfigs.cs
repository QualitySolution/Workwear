using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Journal.GtkUI;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Statements;
using workwear.Journal.ViewModels.Stock;
using workwear.Journal.ViewModels.Tools;
using workwear.Tools.Features;

namespace workwear.Journal
{
	public static class JournalsColumnsConfigs
	{
		public static void RegisterColumns()
		{
			#region Company

			TreeViewColumnsConfigFactory.Register<DepartmentJournalViewModel>(
				() => FluentColumnsConfig<DepartmentJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision).SearchHighlight()
					.AddColumn("Комментарий").AddTextRenderer(x => x.Comments)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<EmployeeJournalViewModel>(
				(jvm) => FluentColumnsConfig<EmployeeJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.CardNumberText)
					.AddColumn("Табельный №").AddTextRenderer(node => node.PersonnelNumber)
					.AddColumn("Карта").Visible(jvm.FeaturesService.Available(WorkwearFeature.IdentityCards))
						.AddPixbufRenderer(x => String.IsNullOrEmpty(x.CardKey) ? null : new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "workwear.icon.buttons.smart-card.png"))
					.AddColumn("Ф.И.О.").AddTextRenderer(node => node.FIO)
					.AddColumn("Должность").AddTextRenderer(node => node.Post)
					.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision)
					.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = ForegroundColor(x))
					.Finish()
			);

			string ForegroundColor(EmployeeJournalNode n)
			{
				if(n.Dismiss) return "gray"; 
				if(n.InVocation) return "blue"; 
				return "black";
			}

			TreeViewColumnsConfigFactory.Register<LeadersJournalViewModel>(
				() => FluentColumnsConfig<LeaderJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Фамилия").AddTextRenderer(node => node.SurName).SearchHighlight()
					.AddColumn("Имя").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Отчество").AddTextRenderer(node => node.Patronymic).SearchHighlight()
					.AddColumn("Должность").AddTextRenderer(node => node.Position).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<OrganizationJournalViewModel>(
				() => FluentColumnsConfig<OrganizationJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Адрес").AddTextRenderer(node => node.Address).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<PostJournalViewModel>(
				() => FluentColumnsConfig<PostJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Профессия").AddTextRenderer(node => node.Profession).SearchHighlight()
					.AddColumn("Отдел").AddTextRenderer(node => node.Department).SearchHighlight()
					.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<SubdivisionJournalViewModel>(
				() => FluentColumnsConfig<SubdivisionJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Code).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Адрес").AddTextRenderer(node => node.Address).SearchHighlight()
					.Finish()
			);

			#endregion

			#region Regulations

			TreeViewColumnsConfigFactory.Register<NormJournalViewModel>(
				() => FluentColumnsConfig<NormJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("№ ТОН").AddTextRenderer(node => node.TonNumber)
					.AddColumn("№ Приложения").AddTextRenderer(node => node.TonAttachment)
					.AddColumn("№ Пункта").AddTextRenderer(node => node.TonParagraph).SearchHighlight()
					.AddColumn("Использована").ToolTipText(n => n.UsageToolTip).AddTextRenderer(node => node.UsageText)
					.AddColumn("Должности[Подразделения]").AddTextRenderer(node => node.Posts).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<ProfessionJournalViewModel>(
				() => FluentColumnsConfig<ProfessionJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => $"{node.Code}").SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<ProtectionToolsJournalViewModel>(
				() => FluentColumnsConfig<ProtectionToolsJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Тип номеклатуры").AddTextRenderer(node => node.TypeName)
					.Finish()
			);

			#endregion

			#region Statements

			TreeViewColumnsConfigFactory.Register<IssuanceSheetJournalViewModel>(
				() => FluentColumnsConfig<IssuanceSheetJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Дата").AddTextRenderer(node => node.Date.ToShortDateString())
					.AddColumn("Документ").AddTextRenderer(node => node.Document).SearchHighlight()
					.AddColumn("Организация").AddTextRenderer(node => node.Organigation).SearchHighlight()
					.AddColumn("Код подр.").AddTextRenderer(node => node.SubdivisionCode).SearchHighlight()
					.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision).SearchHighlight()
					.AddColumn("Сотрудники").AddTextRenderer(node => node.Employees)
					.Finish()
			);

			#endregion

			#region Stock

			TreeViewColumnsConfigFactory.Register<ItemsTypeJournalViewModel>(
				(jvm) => FluentColumnsConfig<ItemsTypeJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Вид одежды").AddTextRenderer(node => node.WearCategoryText)
					.AddColumn("Тип выдачи").Visible(jvm.FeaturesService.Available(WorkwearFeature.CollectiveExpense))
						.AddTextRenderer(n => n.IssueTypeText)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<NomenclatureJournalViewModel>(
				() => FluentColumnsConfig<NomenclatureJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Номер").AddTextRenderer(node => $"{node.Number}").SearchHighlight()
					.AddColumn("Тип").AddTextRenderer(node => node.ItemType)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockBalanceJournalViewModel>(
				() => FluentColumnsConfig<StockBalanceJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(e => $"{e.NomenclatureNumber}").SearchHighlight()
					.AddColumn("Наименование").AddTextRenderer(e => e.NomenclatureName).SearchHighlight()
					.AddColumn("Размер").AddTextRenderer(e => e.Size).SearchHighlight()
					.AddColumn("Рост").AddTextRenderer(e => e.Growth).SearchHighlight()
					.AddColumn("Количество").AddTextRenderer(e => e.BalanceText, useMarkup: true)
					.AddColumn("Процент износа").AddTextRenderer(e => e.WearPercentText)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockBalanceShortSummaryJournalViewModel>(
				() => FluentColumnsConfig<StockBalanceShortSummaryJournalNode>.Create()
						.AddColumn("Номер").AddTextRenderer(e => $"{e.NomenclatureNumber}").SearchHighlight()
						.AddColumn("Наименование").AddTextRenderer(e => e.NomenclatureName).SearchHighlight()
						.AddColumn("Размер").AddTextRenderer(e => e.Size).SearchHighlight()
						.AddColumn("Пол одежды").AddTextRenderer(e => e.Sex != null ? e.Sex.GetAttribute<DisplayAttribute>().Name : "").SearchHighlight()
						.AddColumn("Количество").AddTextRenderer(e => e.BalanceText, useMarkup: true)
						.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockDocumentsJournalViewModel>(
				(jvm) => FluentColumnsConfig<StockDocumentsJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString()).SearchHighlight().XAlign(0.5f)
					.AddColumn("Тип документа").AddTextRenderer(node => node.DocTypeString)
					.AddColumn("Операция").AddTextRenderer(node => node.Operation)
					.AddColumn("Дата").AddTextRenderer(node => node.DateString).XAlign(0.5f)
					.AddColumn("Ведомость").AddTextRenderer(node => $"{node.IssueSheetId}").SearchHighlight().XAlign(0.5f)
					.AddColumn("Склад").Visible(jvm.FeaturesService.Available(WorkwearFeature.Warehouses)).AddTextRenderer(x => x.Warehouse)
					.AddColumn("Автор").AddTextRenderer(node => node.Author).SearchHighlight()
					.AddColumn("Детали").AddTextRenderer(node => node.Description).SearchHighlight()
					.AddColumn("Комментарий").AddTextRenderer(x => x.Comment)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockMovmentsJournalViewModel>(
				() => FluentColumnsConfig<StockMovmentsJournalNode>.Create()
					.AddColumn("Дата").AddTextRenderer(node => node.OperationTimeText)
					.AddColumn("Документ").AddTextRenderer(node => node.DocumentText)
					.AddColumn("Наименование").AddTextRenderer(e => e.NomenclatureName).SearchHighlight()
					.AddColumn("Размер").AddTextRenderer(e => e.Size).SearchHighlight()
					.AddColumn("Рост").AddTextRenderer(e => e.Growth).SearchHighlight()
					.AddColumn("Процент износа").AddTextRenderer(e => e.WearPercentText)
					.AddColumn("Поступление\\расход").AddTextRenderer(node => node.AmountText, useMarkup: true)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<WarehouseJournalViewModel>(
				() => FluentColumnsConfig<WarehouseJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.Finish()
			);

			#endregion

			#region Tools

			TreeViewColumnsConfigFactory.Register<EmployeeProcessingJournalViewModel>(
				() => FluentColumnsConfig<EmployeeProcessingJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.CardNumberText)
					.AddColumn("Табельный №").AddTextRenderer(node => node.PersonnelNumber)
					.AddColumn("Ф.И.О.").AddTextRenderer(node => node.FIO)
					.AddColumn("Результат").AddTextRenderer(node => node.Result)
					.AddSetter((c, x) => c.Foreground = x.Result == "ОК" ? "green" : "red")
					.AddColumn("Нормы").AddTextRenderer(node => node.Norms)
					.AddColumn("Должность").AddTextRenderer(node => node.Post)
					.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision)
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Background = x.Dismiss ? "White Smoke" : null)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<EmployeeNotificationJournalViewModel>(
				() => FluentColumnsConfig<EmployeeNotificationJournalNode>.Create()
					.AddColumn("☑").AddToggleRenderer(node => node.Selected)
						.AddSetter((c, n) => c.Activatable = n.CanSelect)
					.AddColumn("Номер").AddTextRenderer(node => node.CardNumberText)
					.AddColumn("Табельный №").AddTextRenderer(node => node.PersonnelNumber)
					.AddColumn("Ф.И.О.").AddTextRenderer(node => node.FIO)
					.AddColumn("Телефон").AddTextRenderer(node => node.Phone)
					.AddColumn("Должность").AddTextRenderer(node => node.Post)
					.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision)
					.AddColumn("Состояние личного кабинета").AddTextRenderer(node => node.PersonalAccountStatus)
					.AddColumn("Последний раз заходил в ЛК").AddTextRenderer(node => node.LastVisit)
					.AddColumn("Результат").AddTextRenderer(node => node.Result)
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.Dismiss ? "gray" : null)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<MessageTemplateJournalViewModel>(
				() => FluentColumnsConfig<NotificationTemplateJournalNode>.Create()
					.AddColumn("Имя").AddTextRenderer(node => node.Name)
					.AddColumn("Заголовок").AddTextRenderer(node => node.MessageTitle)
					.AddColumn("Текст").AddTextRenderer(node => node.MessageText)
					.Finish()
			);
			#endregion
		}
	}
}
