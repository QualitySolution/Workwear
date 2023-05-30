using System;
using System.Reflection;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Journal.GtkUI;
using QS.Utilities.Numeric;
using workwear.Journal.ViewModels.Communications;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Statements;
using workwear.Journal.ViewModels.Stock;
using workwear.Journal.ViewModels.Tools;
using Workwear.Tools.Features;

namespace workwear.Journal
{
	public static class JournalsColumnsConfigs
	{
		public static void RegisterColumns()
		{
			#region Communications
			TreeViewColumnsConfigFactory.Register<EmployeeNotificationJournalViewModel>(
				() => FluentColumnsConfig<EmployeeNotificationJournalNode>.Create()
					.AddColumn("☑").AddToggleRenderer(node => node.Selected)
						.AddSetter((c, n) => c.Activatable = n.CanSelect)
					.AddColumn("Номер").AddTextRenderer(node => node.CardNumberText)
					.AddColumn("Табельный №").AddTextRenderer(node => node.PersonnelNumber)
					.AddColumn("Ф.И.О.").AddTextRenderer(node => node.FIO)
					.AddColumn("Телефон").AddTextRenderer(node => node.Phone)
					.AddColumn("Состояние личного кабинета").AddTextRenderer(node => node.StatusText)
					.AddColumn("Последний визит").AddTextRenderer(node => node.LastVisit)
					.AddColumn("Не прочитано").AddTextRenderer(node => node.UnreadMessagesText)
					.AddColumn("К выдаче").AddTextRenderer(n => n.IssueCount.ToString())
					.AddColumn("Должность").AddTextRenderer(node => node.Post)
					.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision)
					.AddColumn("День рождения").AddTextRenderer(node => node.BirthDate != null ? node.BirthDate.Value.ToShortDateString() : null)
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.StatusColor)
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

			#region Company
			TreeViewColumnsConfigFactory.Register<CostCenterJournalViewModel>(
				() => FluentColumnsConfig<CostCenterJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Код ВМЗ").AddTextRenderer(node => node.Code).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.Finish()
			);

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
						.AddPixbufRenderer(x => String.IsNullOrEmpty(x.CardKey) ? null : new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "Workwear.icon.buttons.smart-card.png"))
					.AddColumn("Ф.И.О.").AddTextRenderer(node => node.FIO)
					.AddColumn("Должность").AddTextRenderer(node => node.Post)
					.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision)
					.AddColumn("Отдел").AddTextRenderer(node => node.Department).SearchHighlight()
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
				(jwm) => FluentColumnsConfig<PostJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Профессия").AddTextRenderer(node => node.Profession).SearchHighlight()
					.AddColumn("Отдел").AddTextRenderer(node => node.Department).SearchHighlight()
					.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision).SearchHighlight()
					.AddColumn("МВЗ").Visible(jwm.FeaturesService.Available(WorkwearFeature.CostCenter)).AddTextRenderer(node => node.CostCenterText).SearchHighlight()
					.AddColumn("Комментарий").AddTextRenderer(node => node.Comments).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<SubdivisionJournalViewModel>(
				() => FluentColumnsConfig<SubdivisionJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Code).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.IndentedName).SearchHighlight()
					.AddColumn("Адрес").AddTextRenderer(node => node.Address).SearchHighlight()
					.AddColumn("Головное подразделение").AddTextRenderer(node => node.ParentName)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<EmployeeBalanceJournalViewModel>(
				(jwm) => FluentColumnsConfig<EmployeeBalanceJournalNode>.Create()
					.AddColumn("Сотрудник")
					.Visible(jwm.Filter.Employee is null).AddTextRenderer(e => e.EmployeeName)
					.AddColumn ("Наименование")
					.AddTextRenderer(e => e.NomenclatureName).WrapWidth(1000)
					.AddColumn ("Размер").AddTextRenderer (e => e.WearSize)
					.AddColumn ("Рост").AddTextRenderer (e => e.Height)
					.AddColumn ("Количество").AddTextRenderer (e => e.BalanceText)
					.AddColumn ("Стоимость").AddTextRenderer (e => e.AvgCostText)
					.AddColumn ("Износ на сегодня").AddProgressRenderer (e => ((int)(e.Percentage * 100)).Clamp(0, 100))
						.AddSetter ((w, e) => w.Text = (e.ExpiryDate.HasValue ? $"до {e.ExpiryDate.Value:d}" : "до износа"))
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.AutoWriteoffDate < jwm.Filter.Date ? "gray": "black")
					.Finish ()
			);

			TreeViewColumnsConfigFactory.Register<SubdivisionBalanceJournalViewModel>(
				(jwm) => FluentColumnsConfig<SubdivisionBalanceJournalNode>.Create()
					.AddColumn("Подразделение").Visible(jwm.Filter.Subdivision is null)
					.AddTextRenderer(e => e.SubdivisionName)
					.AddColumn("Наименование").AddTextRenderer(e => e.NomenclatureName).WrapWidth(1000)
					.AddColumn("Количество").AddTextRenderer(e => e.BalanceText)
					.AddColumn("Срок службы").AddProgressRenderer(e => 
						(int) (100 - e.Percentage * 100))
					.AddSetter((w, e) => 
						w.Text = e.ExpiryDate.HasValue ? $"до {e.ExpiryDate.Value:d}" : String.Empty)
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
					.AddColumn("Должности[Подразделения›Отдел]").AddTextRenderer(node => node.Posts).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<NormConditionJournalViewModel>(
				() => FluentColumnsConfig<NormConditionJournalNode>.Create()
				.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
				.AddColumn("Ограничение по полу").AddTextRenderer(node => node.Sex.GetEnumTitle())
				.AddColumn("Дата начала периода").AddTextRenderer(node => node.DateStringStart)
				.AddColumn("Дата окончания периода").AddTextRenderer(node => node.DateStringEnd)
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
					.AddColumn("Тип номенклатуры").AddTextRenderer(node => node.TypeName)
					.Finish()
			);
			TreeViewColumnsConfigFactory.Register<VacationTypeJournalViewModel>(
				() => FluentColumnsConfig<VacationTypeJournalNode>.Create()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Исключать из носки").AddTextRenderer(node => node.ExcludeFromWearing ? "Да" : "Нет")
					.AddColumn("Комментарий").AddTextRenderer(node => node.Comments)
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
					.AddColumn("Тип выдачи").Visible(jvm.FeaturesService.Available(WorkwearFeature.CollectiveExpense))
						.AddTextRenderer(n => n.IssueTypeText)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<NomenclatureJournalViewModel>(
				(jvm) => FluentColumnsConfig<NomenclatureJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name + (node.Archival? "(архивная)": String.Empty)).WrapWidth(1000).SearchHighlight()
					.AddColumn("Номер").AddTextRenderer(node => node.Number).SearchHighlight()
					.AddColumn("Тип").AddTextRenderer(node => node.ItemType)
					.AddColumn("Стоимость продажи").Visible(false).Visible(jvm.FeaturesService.Available(WorkwearFeature.Selling))
						.AddTextRenderer(node => node.SaleCostText)
					.AddColumn("Средняя оценка").Visible(jvm.FeaturesService.Available(WorkwearFeature.Ratings))
						.AddTextRenderer(node => node.RatingText)
					.AddColumn("Штрихкод").Visible(jvm.FeaturesService.Available(WorkwearFeature.Barcodes))
						.AddTextRenderer(n => n.UseBarcodeText)
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.Archival? "gray": "black")
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockBalanceJournalViewModel>(
				sbjvm => FluentColumnsConfig<StockBalanceJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(e => e.NomenclatureNumber).SearchHighlight()
					.AddColumn("Наименование").AddTextRenderer(e => e.NomenclatureName).WrapWidth(1000).SearchHighlight()
					.AddColumn("Размер").AddTextRenderer(e => e.SizeName).SearchHighlight()
					.AddColumn("Рост").AddTextRenderer(e => e.HeightName).SearchHighlight()
					.AddColumn("Количество").AddTextRenderer(e => e.BalanceText, useMarkup: true)
					.AddColumn("Процент износа").AddTextRenderer(e => e.WearPercentText)
					.AddColumn("Собственник имущества")
						.Visible(sbjvm.FeaturesService.Available(WorkwearFeature.Owners))
						.AddTextRenderer(e => e.OwnerName)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockDocumentsJournalViewModel>(
				(jvm) => FluentColumnsConfig<StockDocumentsJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString()).SearchHighlight().XAlign(0.5f)
					.AddColumn("Тип документа").AddTextRenderer(node => node.DocTypeString)
					.AddColumn("Дата").AddTextRenderer(node => node.DateString).XAlign(0.5f)
					.AddColumn("Ведомость").AddTextRenderer(node => $"{node.IssueSheetId}").SearchHighlight().XAlign(0.5f)
					.AddColumn("Склад").Visible(jvm.FeaturesService.Available(WorkwearFeature.Warehouses)).AddTextRenderer(x => x.Warehouse)
					.AddColumn("Автор").AddTextRenderer(node => node.Author).SearchHighlight()
					.AddColumn("Детали").AddTextRenderer(node => node.Description).SearchHighlight()
					.AddColumn("Дата создания").AddTextRenderer(x => x.CreationDateString)
					.AddColumn("Комментарий").AddTextRenderer(x => x.Comment)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockMovmentsJournalViewModel>(
				() => FluentColumnsConfig<StockMovementsJournalNode>.Create()
					.AddColumn("Дата").AddTextRenderer(node => node.OperationTimeText)
					.AddColumn("Документ").AddTextRenderer(node => node.DocumentText)
					.AddColumn("Наименование").AddTextRenderer(e => e.NomenclatureName).WrapWidth(700).SearchHighlight()
					.AddColumn("Сотрудник").AddTextRenderer(e => e.Employee).SearchHighlight()
					.AddColumn("Размер").AddTextRenderer(e => e.WearSizeName).SearchHighlight()
					.AddColumn("Рост").AddTextRenderer(e => e.HeightName).SearchHighlight()
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

			TreeViewColumnsConfigFactory.Register<OwnerJournalViewModel>(
				() => FluentColumnsConfig<OwnerJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Приоритет выдачи").AddNumericRenderer(node => node.Priority)
					.AddColumn("Описание").AddTextRenderer(node => node.ShortDescription)
					.Finish()
			);
			
			TreeViewColumnsConfigFactory.Register<BarcodeJournalViewModel>(
				() => FluentColumnsConfig<BarcodeJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Значение").AddTextRenderer(node => node.Value)
					.Finish()
				);
			#endregion
			#region Sizes
			TreeViewColumnsConfigFactory.Register<SizeJournalViewModel>(
				() => FluentColumnsConfig<SizeJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Значение").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Другое значение").AddTextRenderer(node => node.AlternativeName).SearchHighlight()
					.AddColumn("Для сотрудника").AddToggleRenderer(n => n.ShowInEmployee).Editing(false)
					.AddColumn("Для номенклатуры").AddToggleRenderer(n => n.ShowInNomenclature).Editing(false)
					.AddColumn("Тип размера").AddTextRenderer(node => node.SizeTypeName).SearchHighlight()
					.Finish()
			);
			
			TreeViewColumnsConfigFactory.Register<SizeTypeJournalViewModel>(
				() => FluentColumnsConfig<SizeTypeJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Категория").AddTextRenderer(node => node.CategorySizeType.GetEnumTitle()).SearchHighlight()
					.AddColumn("Позиция").AddNumericRenderer(node => node.Position)
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.UseInEmployee ? null : "gray")
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
			#endregion
		}
	}
}
