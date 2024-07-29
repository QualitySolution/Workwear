using System;
using System.Reflection;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Cloud.Postomat.Manage;
using QS.Journal.GtkUI;
using QS.Utilities.Numeric;
using Workwear.Journal.ViewModels.Analytics;
using workwear.Journal.ViewModels.ClothingService;
using workwear.Journal.ViewModels.Communications;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Postomats;
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
			#region Analytics

			TreeViewColumnsConfigFactory.Register<ProtectionToolsCategoryJournalViewModel>(
				() => FluentColumnsConfig<ProtectionToolsCategoryNode>.Create()
					.AddColumn("ИД").AddReadOnlyTextRenderer(node => node.Id.ToString()).XAlign(0.5f).SearchHighlight()
					.AddColumn("Название").AddReadOnlyTextRenderer(node => node.Name).XAlign(0.5f).SearchHighlight()
					.AddColumn("Комментарий").AddReadOnlyTextRenderer(node => node.Comment)
					.Finish()
				);

			#endregion
			
			#region ClothingService

			TreeViewColumnsConfigFactory.Register<ClaimsJournalViewModel>(
				jvm => FluentColumnsConfig<ClaimsJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString()).XAlign(0.5f)
					.AddColumn("Штрихкод").AddTextRenderer(node => node.Barcode).SearchHighlight().XAlign(0.5f)
					.AddColumn("Сотрудник").AddTextRenderer(node => node.Employee)
					.AddColumn("Статус").AddReadOnlyTextRenderer(node => node.State.GetEnumTitle())
					.AddColumn("Изменен").AddReadOnlyTextRenderer(x => x.OperationTime.ToString("g")).XAlign(0.5f)
					.AddColumn("Номенклатура").AddReadOnlyTextRenderer(x => x.Nomenclature).SearchHighlight()
					.AddColumn("Ремонт").AddTextRenderer(node => node.NeedForRepair ? "Да" : "Нет")
					.AddColumn("Дефект").AddTextRenderer(node => node.Defect)
					.AddColumn("Предпочтительный постомат выдачи")
						.AddTextRenderer(x => jvm.GetTerminalLabel(x.ReferredTerminalId))
					.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.RowColor)
					.Finish()
				);

			#endregion
			#region Communications
			TreeViewColumnsConfigFactory.Register<EmployeeNotificationJournalViewModel>(
				() => FluentColumnsConfig<EmployeeNotificationJournalNode>.Create()
					.AddColumn("☑").AddToggleRenderer(node => node.Selected)
						.AddSetter((c, n) => c.Activatable = n.CanSandNotification)
					.AddColumn("Номер").Resizable().AddTextRenderer(node => node.CardNumberText)
					.AddColumn("Табельный №").Resizable().AddTextRenderer(node => node.PersonnelNumber)
					.AddColumn("Ф.И.О.").Resizable().AddTextRenderer(node => node.FIO)
					.AddColumn("Телефон").Resizable().AddTextRenderer(node => node.Phone)
					.AddColumn("Эл. почта").Resizable().AddTextRenderer(node => node.Email)
					.AddColumn("Состояние личного кабинета").Resizable().AddTextRenderer(node => node.StatusText)
					.AddColumn("Последний визит").Resizable().AddTextRenderer(node => node.LastVisit)
					.AddColumn("Не прочитано").Resizable().AddTextRenderer(node => node.UnreadMessagesText)
					.AddColumn("К выдаче").Resizable().AddTextRenderer(n => n.IssueCount.ToString())
					.AddColumn("Должность").Resizable().AddTextRenderer(node => node.Post)
					.AddColumn("Подразделение").Resizable().AddTextRenderer(node => node.Subdivision)
					.AddColumn("День рождения").AddTextRenderer(node => node.BirthDate != null ? node.BirthDate.Value.ToShortDateString() : null)
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.StatusColor)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<MessageTemplateJournalViewModel>(
				() => FluentColumnsConfig<NotificationTemplateJournalNode>.Create()
					.AddColumn("Имя").AddTextRenderer(node => node.Name)
					.AddColumn("Заголовок").AddTextRenderer(node => node.MessageTitle)
					.AddColumn("Текст").AddTextRenderer(node => node.MessageText)
					.AddColumn("Заголовок ссылки").AddTextRenderer(node => node.LinkTitleText)
					.AddColumn("Ссылка").AddTextRenderer(node => node.LinkText)
					.Finish()
			);
			
			TreeViewColumnsConfigFactory.Register<SpecCoinsBalanceJournalViewModel>(
				() => FluentColumnsConfig<SpecCoinsBalanceJournalNode>.Create()
					.AddColumn("Табельный №").AddTextRenderer(x => x.PersonnelNumber)
					.AddColumn("Ф.И.О.").AddTextRenderer(x => x.EmployeeText)
					.AddColumn("Телефон").AddTextRenderer(x => x.EmployeePhone)
					.AddColumn("Баланс").AddTextRenderer(x => x.EmployeeBalanceText)
					.Finish()
			);
			#endregion

			#region Company
			TreeViewColumnsConfigFactory.Register<CostCenterJournalViewModel>(
				() => FluentColumnsConfig<CostCenterJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Код").AddTextRenderer(node => node.Code).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<DepartmentJournalViewModel>(
				() => FluentColumnsConfig<DepartmentJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Код").AddTextRenderer(node => node.Code).SearchHighlight()
					.AddColumn("Название").Resizable().AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Подразделение").Resizable().AddTextRenderer(node => node.Subdivision).SearchHighlight()
					.AddColumn("Комментарий").AddTextRenderer(x => x.Comments)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<EmployeeJournalViewModel>(
				(jvm) => FluentColumnsConfig<EmployeeJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.CardNumberText)
					.AddColumn("Табельный №").Resizable().AddTextRenderer(node => node.PersonnelNumber)
					.AddColumn("Карта").Resizable().Visible(jvm.FeaturesService.Available(WorkwearFeature.IdentityCards))
						.AddPixbufRenderer(x => String.IsNullOrEmpty(x.CardKey) ? null : new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "Workwear.icon.buttons.smart-card.png"))
					.AddColumn("Ф.И.О.").Resizable().AddTextRenderer(node => node.FIO)
					.AddColumn("Должность").Resizable().AddTextRenderer(node => node.Post)
					.AddColumn("Подразделение").Resizable().AddTextRenderer(node => node.Subdivision)
					.AddColumn("Отдел").Resizable().AddTextRenderer(node => node.Department).SearchHighlight()
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
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Фамилия").AddTextRenderer(node => node.SurName).SearchHighlight()
					.AddColumn("Имя").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Отчество").AddTextRenderer(node => node.Patronymic).SearchHighlight()
					.AddColumn("Должность").AddTextRenderer(node => node.Position).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<OrganizationJournalViewModel>(
				() => FluentColumnsConfig<OrganizationJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Адрес").AddTextRenderer(node => node.Address).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<PostJournalViewModel>(
				(jwm) => FluentColumnsConfig<PostJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Код").AddTextRenderer(node => node.Code).SearchHighlight()
					.AddColumn("Название").Resizable().AddTextRenderer(node => node.Name).WrapWidth(700).SearchHighlight()
					.AddColumn("Сотрудников").Resizable().AddReadOnlyTextRenderer(n => n.Employees.ToString()).XAlign(0.5f)
					.AddColumn("Профессия").Resizable().AddTextRenderer(node => node.Profession).WrapWidth(700).SearchHighlight()
					.AddColumn("Отдел").Resizable().AddTextRenderer(node => node.Department).WrapWidth(700).SearchHighlight()
					.AddColumn("Подразделение").Resizable().AddTextRenderer(node => node.Subdivision).WrapWidth(700).SearchHighlight()
					.AddColumn("МВЗ").Resizable().Visible(jwm.FeaturesService.Available(WorkwearFeature.CostCenter)).AddTextRenderer(node => node.CostCenterText).WrapWidth(700).SearchHighlight()
					.AddColumn("Комментарий").AddTextRenderer(node => node.Comments).WrapWidth(700).SearchHighlight()
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.Archival? "gray": "black")
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<SubdivisionJournalViewModel>(
				() => FluentColumnsConfig<SubdivisionJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Код").AddTextRenderer(node => node.Code).SearchHighlight()
					.AddColumn("Название").Resizable().AddTextRenderer(node => node.IndentedName).SearchHighlight()
					.AddColumn("Сотрудников").Resizable().AddReadOnlyTextRenderer(n => n.Employees.ToString()).XAlign(0.5f)
					.AddColumn("Головное подразделение").Resizable().AddTextRenderer(node => node.ParentName)
					.AddColumn("Адрес").AddTextRenderer(node => node.Address).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<EmployeeBalanceJournalViewModel>(
				(jwm) => FluentColumnsConfig<EmployeeBalanceJournalNode>.Create()
					.AddColumn("Сотрудник").Resizable()
					.Visible(jwm.Filter.Employee is null).AddTextRenderer(e => e.EmployeeName)
					.AddColumn ("Наименование").Resizable()
					.AddTextRenderer(e => e.ItemName).WrapWidth(1000)
					.AddSetter((w, item) => w.Foreground = item.NomenclatureName != null ? "black" : "blue")
					.AddColumn ("Размер").Resizable().AddTextRenderer (e => e.WearSize)
					.AddColumn ("Рост").Resizable().AddTextRenderer (e => e.Height)
					.AddColumn ("Количество").AddTextRenderer (e => e.BalanceText)
					.AddColumn ("Стоимость").AddTextRenderer (e => e.AvgCostText)
					.AddColumn ("Износ на сегодня").AddProgressRenderer (e => ((int)(e.Percentage * 100)).Clamp(0, 100))
						.AddSetter ((w, e) => w.Text = (e.ExpiryDate.HasValue ? $"до {e.ExpiryDate.Value:d}" : "до износа"))
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.AutoWriteoffDate < jwm.Filter.Date ? "gray": "black")
					.Finish ()
			);
			
			TreeViewColumnsConfigFactory.Register<EmployeeGroupJournalViewModel>(
					() => FluentColumnsConfig<EmployeeGroupJournalNode>.Create()
						.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
						.AddColumn("Название").Resizable().AddTextRenderer(node => node.Name).SearchHighlight()
						.AddColumn("Кол-во").AddReadOnlyTextRenderer(node => node.Count.ToString())
						.AddColumn("Комментарий").AddTextRenderer(node => node.Comment).SearchHighlight()
						.Finish()
			);

			#endregion

			#region Postomats
			TreeViewColumnsConfigFactory.Register<PostomatDocumentsJournalViewModel>(
				jvm => FluentColumnsConfig<PostomatDocumentJournalNode>.Create()
					.AddColumn("Номер").AddReadOnlyTextRenderer(x => x.Id.ToString()).XAlign(0.5f)
					.AddColumn("Дата").AddReadOnlyTextRenderer(x => x.CreateTime.ToShortDateString()).XAlign(0.5f)
					.AddColumn("Тип").AddReadOnlyTextRenderer(x => x.Type.GetEnumTitle()).XAlign(0.5f)
					.AddColumn("Статус").AddReadOnlyTextRenderer(x => x.Status.GetEnumTitle()).XAlign(0.5f)
					.AddColumn("Постамат")
						.AddReadOnlyTextRenderer(x => x.TerminalId.ToString()).XAlign(0.5f)
						.AddTextRenderer(x => jvm.GetTerminalName(x.TerminalId))
					.AddColumn("Размещение постамата").AddReadOnlyTextRenderer(x => jvm.GetTerminalLocation(x.TerminalId))
					.Finish()
				);
			
			TreeViewColumnsConfigFactory.Register<PostomatDocumentsWithdrawJournalViewModel>(jvm =>
				FluentColumnsConfig<PostomatDocumentWithdrawJournalNode>.Create()
					.AddColumn("Номер").AddReadOnlyTextRenderer(x => x.Id.ToString()).XAlign(0.5f)
					.AddColumn("Дата").AddReadOnlyTextRenderer(x => x.CreateTime.ToShortDateString()).XAlign(0.5f)
					.AddColumn("Пользователь").AddReadOnlyTextRenderer(x => x.User?.Name)
					.Finish()
				);
			
			TreeViewColumnsConfigFactory.Register<FullnessJournalViewModel>(
				() => FluentColumnsConfig<FullnessInfo>.Create()
					.AddColumn("ИД").AddReadOnlyTextRenderer(node => node.Id.ToString())
					.AddColumn("Название").AddReadOnlyTextRenderer(n => n.Name)
					.AddColumn("Размещение").AddReadOnlyTextRenderer(n => n.Location)
					.AddColumn("Тип").AddReadOnlyTextRenderer(n => n.Type.ToString())
					.AddColumn("Заполненность")
						.AddProgressRenderer(n => n.Capacity == 0 ? 0 : (int)(100f * n.Filling / n.Capacity))
						.Text(x => $"{x.Filling} из {x.Capacity}")
					.Finish()
				);
			#endregion
			
			#region Regulations

			TreeViewColumnsConfigFactory.Register<NormJournalViewModel>(
				() => FluentColumnsConfig<NormJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").Resizable().AddTextRenderer(node => node.Name).WrapWidth(700).SearchHighlight()
					.AddColumn("№ ТОН").Resizable().AddTextRenderer(node => node.TonNumber)
					.AddColumn("№ Приложения").Resizable().AddTextRenderer(node => node.TonAttachment)
					.AddColumn("№ Пункта").Resizable().AddTextRenderer(node => node.TonParagraph).SearchHighlight()
					.AddColumn("Использована").ToolTipText(n => n.UsageToolTip).AddTextRenderer(node => node.UsageText)
					.AddColumn("Должности[Подразделения›Отдел]").AddTextRenderer(node => node.Posts).SearchHighlight()
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.Archival? "gray": "black")
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
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Код").AddTextRenderer(node => $"{node.Code}").SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<ProtectionToolsJournalViewModel>(
				() => FluentColumnsConfig<ProtectionToolsJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Название").Resizable().AddTextRenderer(node => node.Name).WrapWidth(900).SearchHighlight()
					.AddColumn("Тип номенклатуры").AddTextRenderer(node => node.TypeName)
					.AddColumn("Категория для аналитики").AddTextRenderer(node => node.CategoryForAnalytic)
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
					.AddColumn("Документ").Resizable().AddTextRenderer(node => node.Document).SearchHighlight()
					.AddColumn("Организация").Resizable().AddTextRenderer(node => node.Organigation).SearchHighlight()
					.AddColumn("Код подр.").AddTextRenderer(node => node.SubdivisionCode).SearchHighlight()
					.AddColumn("Подразделение").Resizable().AddTextRenderer(node => node.Subdivision).SearchHighlight()
					.AddColumn("Сотрудники").AddTextRenderer(node => node.Employees)
					.Finish()
			);

			#endregion

			#region Stock

			TreeViewColumnsConfigFactory.Register<ItemsTypeJournalViewModel>(
				(jvm) => FluentColumnsConfig<ItemsTypeJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Название").Resizable().AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Тип выдачи").Visible(jvm.FeaturesService.Available(WorkwearFeature.CollectiveExpense))
						.AddTextRenderer(n => n.IssueTypeText)
					.AddColumn("Тип размера").AddReadOnlyTextRenderer(x => x.TypeOfSize)
					.AddColumn("Тип роста").AddReadOnlyTextRenderer(x => x.TypeOfHeight)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<NomenclatureJournalViewModel>(
				(jvm) => FluentColumnsConfig<NomenclatureJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Номер").AddTextRenderer(node => node.Number).SearchHighlight()
					.AddColumn("Название").Resizable().AddTextRenderer(node => node.Name + (node.Archival? "(архивная)": String.Empty)).WrapWidth(1000).SearchHighlight()
					.AddColumn("Тип").Resizable().AddTextRenderer(node => node.ItemType)
					.AddColumn("Пол").AddTextRenderer(node => node.SexText)
					.AddColumn("Стоимость продажи").Visible(false).Visible(jvm.FeaturesService.Available(WorkwearFeature.Selling))
						.AddTextRenderer(node => node.SaleCostText)
					.AddColumn("Средняя оценка").Visible(jvm.FeaturesService.Available(WorkwearFeature.Ratings))
						.AddTextRenderer(node => node.RatingText)
					.AddColumn("Штрихкод").Visible(jvm.FeaturesService.Available(WorkwearFeature.Barcodes))
						.AddTextRenderer(n => n.UseBarcodeText)
					.AddColumn("Можно стирать").Visible(jvm.FeaturesService.Available(WorkwearFeature.ClothingService))
						.AddTextRenderer(n => n.WashableText)
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.Archival? "gray": "black")
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockBalanceJournalViewModel>(
				sbjvm => FluentColumnsConfig<StockBalanceJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(e => e.NomeclatureId.ToString()).SearchHighlight()
					.AddColumn("Номер").Resizable().AddTextRenderer(e => e.NomenclatureNumber).SearchHighlight()
					.AddColumn("Наименование").Resizable().AddTextRenderer(e => e.NomenclatureName).WrapWidth(1000).SearchHighlight()
					.AddColumn("Пол").Resizable().AddTextRenderer(e => e.SexText).SearchHighlight()
					.AddColumn("Размер").Resizable().AddTextRenderer(e => e.SizeName).SearchHighlight()
					.AddColumn("Рост").Resizable().AddTextRenderer(e => e.HeightName).SearchHighlight()
					.AddColumn("Количество").AddTextRenderer(e => e.BalanceText, useMarkup: true)
					.AddColumn("Процент износа").AddTextRenderer(e => e.WearPercentText)
					.AddColumn("Собственник имущества")
						.Visible(sbjvm.FeaturesService.Available(WorkwearFeature.Owners))
						.AddTextRenderer(e => e.OwnerName)
					.AddColumn("Цена продажи")
						.Visible(sbjvm.FeaturesService.Available(WorkwearFeature.Selling))
						.AddTextRenderer(e => e.SaleCostText)
					.AddColumn("Цена продажи")
					.Visible(sbjvm.FeaturesService.Available(WorkwearFeature.Selling))
					.AddTextRenderer(e => e.SumSaleCostText)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockDocumentsJournalViewModel>(
				(jvm) => FluentColumnsConfig<StockDocumentsJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.DocNumberText).SearchHighlight().XAlign(0.5f)
					.AddColumn("Тип документа").Resizable().AddTextRenderer(node => node.DocTypeString)
					.AddColumn("Дата").Resizable().AddTextRenderer(node => node.DateString).XAlign(0.5f)
					.AddColumn("Ведомость").Resizable().AddTextRenderer(node => $"{node.IssueSheetId}").SearchHighlight().XAlign(0.5f)
					.AddColumn("Склад").Resizable().Visible(jvm.FeaturesService.Available(WorkwearFeature.Warehouses)).AddTextRenderer(x => x.Warehouse)
					.AddColumn("Автор").Resizable().AddTextRenderer(node => node.Author).SearchHighlight()
					.AddColumn("Детали").Resizable().AddTextRenderer(node => node.Description).SearchHighlight()
					.AddColumn("Дата создания").AddTextRenderer(x => x.CreationDateString)
					.AddColumn("Комментарий").AddTextRenderer(x => x.Comment)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<StockMovmentsJournalViewModel>(
				() => FluentColumnsConfig<StockMovementsJournalNode>.Create()
					.AddColumn("Ведомость").Resizable().AddTextRenderer(node => $"{node.IssuanceSheetId}").SearchHighlight()
					.AddColumn("Дата").ToolTipText(n => n.RowTooltip).AddTextRenderer(node => node.OperationTimeText)
					.AddColumn("Документ").ToolTipText(n => n.RowTooltip).Resizable().AddTextRenderer(node => node.DocumentText)
					.AddColumn("Наименование").ToolTipText(n => n.RowTooltip).Resizable().AddTextRenderer(e => e.NomenclatureName).WrapWidth(700).SearchHighlight()
					.AddColumn("Сотрудник").Resizable().AddTextRenderer(e => e.Employee).SearchHighlight()
					.AddColumn("Размер").Resizable().AddTextRenderer(e => e.WearSizeName).SearchHighlight()
					.AddColumn("Рост").Resizable().AddTextRenderer(e => e.HeightName).SearchHighlight()
					.AddColumn("Собственник").Resizable().AddTextRenderer(node => node.OwnerName)
					.AddColumn("Процент износа").AddTextRenderer(e => e.WearPercentText)
					.AddColumn("Поступление\\расход").AddTextRenderer(node => node.AmountText, useMarkup: true)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<WarehouseJournalViewModel>(
				() => FluentColumnsConfig<WarehouseJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<OwnerJournalViewModel>(
				() => FluentColumnsConfig<OwnerJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Приоритет выдачи").AddNumericRenderer(node => node.Priority)
					.AddColumn("Описание").AddTextRenderer(node => node.ShortDescription)
					.Finish()
			);
			
			TreeViewColumnsConfigFactory.Register<BarcodeJournalViewModel>(
				() => FluentColumnsConfig<BarcodeJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Значение").AddTextRenderer(node => node.Value).SearchHighlight()
					.AddColumn("Создан").AddReadOnlyTextRenderer(x => x.CreateDate.ToShortDateString())
					.AddColumn("Номенклатура").AddTextRenderer(node => node.Nomenclature).SearchHighlight()
					.AddColumn("Размер").AddTextRenderer(node => node.Size)
					.AddColumn("Рост").AddTextRenderer(node => node.Height)
					.AddColumn("Сотрудник").AddReadOnlyTextRenderer(x => x.FullName).SearchHighlight()
					.AddColumn("Комментарий").AddTextRenderer(node => node.Comment).SearchHighlight()
					.Finish()
				);
			#endregion
			#region Sizes
			TreeViewColumnsConfigFactory.Register<SizeJournalViewModel>(
				() => FluentColumnsConfig<SizeJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Значение").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Другое значение").AddTextRenderer(node => node.AlternativeName).SearchHighlight()
					.AddColumn("Для сотрудника").AddToggleRenderer(n => n.ShowInEmployee).Editing(false)
					.AddColumn("Для номенклатуры").AddToggleRenderer(n => n.ShowInNomenclature).Editing(false)
					.AddColumn("Тип размера").AddTextRenderer(node => node.SizeTypeName).SearchHighlight()
					.Finish()
			);
			
			TreeViewColumnsConfigFactory.Register<SizeTypeJournalViewModel>(
				() => FluentColumnsConfig<SizeTypeJournalNode>.Create()
					.AddColumn("ИД").AddTextRenderer(node => $"{node.Id}").SearchHighlight()
					.AddColumn("Название").Resizable().AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Категория").AddTextRenderer(node => node.CategorySizeType.GetEnumTitle()).SearchHighlight()
					.AddColumn("Позиция").AddNumericRenderer(node => node.Position)
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.UseInEmployee ? null : "gray")
					.Finish()
			);
			#endregion

			#region Tools

			TreeViewColumnsConfigFactory.Register<EmployeeProcessingJournalViewModel>(
				() => FluentColumnsConfig<EmployeeProcessingJournalNode>.Create()
					.AddColumn("Номер").Resizable().AddTextRenderer(node => node.CardNumberText)
					.AddColumn("Таб.№").Resizable().AddTextRenderer(node => node.PersonnelNumber)
					.AddColumn("Ф.И.О.").Resizable().AddTextRenderer(node => node.FIO)
					.AddColumn("Результат").Resizable().AddTextRenderer(node => node.Result)
					.AddSetter((c, x) => c.Foreground = x.ResultColor)
					.AddColumn("Нормы").Resizable().AddTextRenderer(node => node.Norms).WrapWidth(700)
					.AddColumn("Должность").Resizable().AddTextRenderer(node => node.Post).WrapWidth(400)
					.AddColumn("Отдел").Resizable().AddTextRenderer(node => node.Department).WrapWidth(400)
					.AddColumn("Подразделение").AddTextRenderer(node => node.Subdivision)
					.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Background = x.Dismiss ? "White Smoke" : null)
					.Finish()
			);
			#endregion
		}
	}
}
