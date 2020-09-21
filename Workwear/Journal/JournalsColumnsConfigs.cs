using System.ComponentModel.DataAnnotations;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Journal.GtkUI;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Statements;
using workwear.Journal.ViewModels.Stock;

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
			() => FluentColumnsConfig<EmployeeJournalNode>.Create()
				//.AddColumn("Номер").AddTextRenderer(node => node.CardNumberText)
				.AddColumn("Табельный №").AddTextRenderer(node => node.PersonnelNumber)
				.AddColumn("Ф.И.О.").AddTextRenderer(node => node.FIO)
				.AddColumn("Должность").AddTextRenderer(node => node.Post)
				.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision)
				.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.Dismiss ? "gray" : "black")
				.Finish()
			);

			TreeViewColumnsConfigFactory.Register<EmployeeCardJournalViewModel>(
			() => FluentColumnsConfig<EmployeeCardJournalNode>.Create()
				.AddColumn("Номер").AddTextRenderer(node => node.CardNumberText)
				.AddColumn("Табельный №").AddTextRenderer(node => node.PersonnelNumber)
				.AddColumn("Ф.И.О.").AddTextRenderer(node => node.FIO)
				//.AddColumn("Должность").AddTextRenderer(node => node.Post)
				.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision)
				.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.Dismiss ? "gray" : "black")
				.Finish()
			);

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
					//.AddColumn("Адрес").AddTextRenderer(node => node.Address).SearchHighlight()
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
					.AddColumn("Профессии").AddTextRenderer(node => node.Professions)
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
					.Finish()
			);

			#endregion

			#region Statements

			TreeViewColumnsConfigFactory.Register<IssuanceSheetJournalViewModel>(
				() => FluentColumnsConfig<IssuanceSheetJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Дата").AddTextRenderer(node => node.Date.ToShortDateString())
					.AddColumn("Организация").AddTextRenderer(node => node.Organigation).SearchHighlight()
					.AddColumn("Код подр.").AddTextRenderer(node => node.SubdivisionCode).SearchHighlight()
					.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision).SearchHighlight()
					.Finish()
			);

			#endregion

			#region Stock

			TreeViewColumnsConfigFactory.Register<ItemsTypeJournalViewModel>(
				() => FluentColumnsConfig<ItemsTypeJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Вид одежды").AddTextRenderer(node => node.WearCategoryText)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<NomenclatureJournalViewModel>(
				() => FluentColumnsConfig<NomenclatureJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("ОЗМ").AddTextRenderer(node => $"{node.Ozm}").SearchHighlight()
					.AddColumn("Тип").AddTextRenderer(node => node.ItemType)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockBalanceJournalViewModel>(
				() => FluentColumnsConfig<StockBalanceJournalNode>.Create()
					.AddColumn("Наименование").AddTextRenderer(e => e.NomenclatureName).SearchHighlight()
					.AddColumn("Размер").AddTextRenderer(e => e.Size).SearchHighlight()
					.AddColumn("Рост").AddTextRenderer(e => e.Growth).SearchHighlight()
					.AddColumn("Количество").AddTextRenderer(e => e.BalanceText, useMarkup: true)
					.AddColumn("Процент износа").AddTextRenderer(e => e.WearPercentText)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockBalanceShortSummaryJournalViewModel>(
				() => FluentColumnsConfig<StockBalanceShortSummaryJournalNode>.Create()
						.AddColumn("Наименование").AddTextRenderer(e => e.NomenclatureName).SearchHighlight()
						.AddColumn("Размер").AddTextRenderer(e => e.Size).SearchHighlight()
						.AddColumn("Пол одежды").AddTextRenderer(e => e.Sex != null ? e.Sex.GetAttribute<DisplayAttribute>().Name : "").SearchHighlight()
						.AddColumn("Количество").AddTextRenderer(e => e.BalanceText, useMarkup: true)
						.Finish()
			);

			TreeViewColumnsConfigFactory.Register<WarehouseJournalViewModel>(
				() => FluentColumnsConfig<WarehouseJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.Finish()
			);

			#endregion
		}
	}
}
