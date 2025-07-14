using QS.BusinessCommon.Domain;
using QS.Deletion;
using QS.Project.Domain;
using Workwear.Domain.Analytics;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Communications;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Postomats;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Supply;
using Workwear.Domain.Users;

namespace Workwear
{
	public class Configure
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static void ConfigureDeletion ()
		{
			logger.Info ("Настройка параметров удаления...");

			#region Обслуживание спецодежды
			DeleteConfig.AddHibernateDeleteInfo<ServiceClaim>()
				.AddDeleteDependence<StateOperation>(x => x.Claim)
				.AddClearDependence<PostomatDocumentItem>(x => x.ServiceClaim);

			DeleteConfig.AddHibernateDeleteInfo<StateOperation>();
			#endregion
			#region Связь
			DeleteConfig.AddHibernateDeleteInfo<MessageTemplate>();
			#endregion
			#region Организация
			DeleteConfig.AddHibernateDeleteInfo<CostCenter>()
				.AddDeleteDependence<EmployeeCostCenter>(x => x.CostCenter)
				.AddClearDependence<Post>(x => x.CostCenter);

			DeleteConfig.AddHibernateDeleteInfo<Department>()
				.AddDeleteDependence<Post>(x => x.Department)
				.AddClearDependence<EmployeeCard>(x => x.Department);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeCard>()
				.AddDeleteDependence<CollectiveExpenseItem>(x => x.Employee)
				.AddDeleteDependence<EmployeeCardItem>(x => x.EmployeeCard)
				.AddDeleteDependence<EmployeeCostCenter>(x => x.Employee)
				.AddDeleteDependence<EmployeeGroupItem>(x => x.Employee)
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.Employee)
				.AddDeleteDependence<EmployeeSize>(x => x.Employee)
				.AddDeleteDependence<EmployeeVacation>(x => x.Employee)
				.AddDeleteDependence<Expense>(x => x.Employee)
				.AddDeleteDependence<ReturnItem>(x => x.EmployeeCard)
				.AddDeleteDependence<IssuanceSheetItem>(x => x.Employee)
				.AddDeleteDependence<PostomatDocumentItem>(x => x.Employee)
				.AddDeleteDependence<PostomatDocumentWithdrawItem>(x => x.Employee)
				.AddDeleteDependence<ServiceClaim>(x => x.Employee)
				.AddClearDependence<CollectiveExpense>(x => x.TransferAgent)
				.AddClearDependence<DutyNorm>(x => x.ResponsibleEmployee)
				.AddClearDependence<ExpenseDutyNorm>(x => x.ResponsibleEmployee)
				.AddClearDependence<Leader>(x => x.Employee)
				.AddClearDependence<IssuanceSheet>(x => x.TransferAgent)
				;

			DeleteConfig.AddHibernateDeleteInfo<EmployeeCardItem>();

			DeleteConfig.AddHibernateDeleteInfo<EmployeeVacation>();
			
			DeleteConfig.AddHibernateDeleteInfo<EmployeeCostCenter>();

			DeleteConfig.AddHibernateDeleteInfo<Leader>()
				.AddClearDependence<EmployeeCard>(x => x.Leader)
				.AddClearDependence<IssuanceSheet>(x => x.HeadOfDivisionPerson)
				.AddClearDependence<IssuanceSheet>(x => x.ResponsiblePerson)
				.AddClearDependence<UserSettings>(x => x.DefaultResponsiblePerson)
				.AddClearDependence<UserSettings>(x => x.DefaultLeader)
				.AddClearDependence<Inspection>(x => x.Director)
				.AddClearDependence<Inspection>(x => x.Chairman)
				.AddRemoveFromDependence<Inspection>(x => x.Members)
				.AddClearDependence<Writeoff>(x => x.Director)
				.AddClearDependence<Writeoff>(x => x.Chairman)
				.AddClearDependence<DutyNorm>(x => x.ResponsibleLeader)
				.AddRemoveFromDependence<Writeoff>(x => x.Members);

			DeleteConfig.AddHibernateDeleteInfo<Organization>()
				.AddClearDependence<Inspection>(x => x.Organization)
				.AddClearDependence<IssuanceSheet>(x => x.Organization)
				.AddClearDependence<Transfer>(x => x.Organization)
				.AddClearDependence<UserSettings>(x => x.DefaultOrganization)
				.AddClearDependence<Writeoff>(x => x.Organization);

			DeleteConfig.AddHibernateDeleteInfo<Post>()
				.AddRemoveFromDependence<Norm>(x => x.Posts)
				.AddClearDependence<EmployeeCard>(x => x.Post);

			DeleteConfig.AddHibernateDeleteInfo<Subdivision>()
				.AddDeleteDependence<Subdivision>(x => x.ParentSubdivision)
				.AddDeleteDependence<Department>(x => x.Subdivision)
				.AddDeleteDependence<Post>(x => x.Subdivision)
				.AddClearDependence<EmployeeCard>(x => x.Subdivision)
				.AddClearDependence<IssuanceSheet>(x => x.Subdivision)
				.AddClearDependence<DutyNorm>(x => x.Subdivision);
				
			DeleteConfig.AddHibernateDeleteInfo<VacationType>()
				.AddDeleteDependence<EmployeeVacation>(x => x.VacationType);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeGroup>()
				.AddDeleteDependence<EmployeeGroupItem>(x => x.Group);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeGroupItem>();

			#endregion
			#region Операции
			DeleteConfig.AddHibernateDeleteInfo<BarcodeOperation>();
			
			DeleteConfig.AddHibernateDeleteInfo<EmployeeIssueOperation>()
				.RequiredCascadeDeletion()
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.IssuedOperation)
				.AddDeleteDependence<ExpenseItem>(x => x.EmployeeIssueOperation)
				.AddDeleteDependence<CollectiveExpenseItem>(x => x.EmployeeIssueOperation)
				.AddDeleteDependence<ReturnItem>(x => x.ReturnFromEmployeeOperation)
				.AddDeleteDependence<WriteoffItem>(x => x.EmployeeWriteoffOperation)
				.AddDeleteDependence<BarcodeOperation>(x => x.EmployeeIssueOperation)
				.AddClearDependence<IssuanceSheetItem>(x => x.IssueOperation)
				.AddDeleteDependence<InspectionItem>(x => x.OperationIssue)
				.AddDeleteDependence<InspectionItem>(x => x.NewOperationIssue);

			DeleteConfig.AddHibernateDeleteInfo<DutyNormIssueOperation>()
				.RequiredCascadeDeletion()
				.AddDeleteDependence<DutyNormIssueOperation>(x => x.IssuedOperation)
				.AddDeleteDependence<ExpenseDutyNormItem>(x => x.Operation)
				.AddDeleteDependence<ReturnItem>(x => x.ReturnFromDutyNormOperation)
				.AddDeleteDependence<WriteoffItem>(x => x.DutyNormWriteOffOperation);
				
			DeleteConfig.AddHibernateDeleteInfo<WarehouseOperation>()
				.RequiredCascadeDeletion()
				.AddDeleteDependence<ExpenseItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<CollectiveExpenseItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<ExpenseDutyNormItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<IncomeItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<ReturnItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<WriteoffItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<TransferItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.WarehouseOperation)
				.AddDeleteDependence<DutyNormIssueOperation>(x => x.WarehouseOperation)
				.AddDeleteDependence<CompletionResultItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<CompletionSourceItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<BarcodeOperation>(x => x.WarehouseOperation);

			#endregion
			#region Постаматы
			DeleteConfig.AddHibernateDeleteInfo<PostomatDocument>()
				.AddDeleteDependence<PostomatDocumentItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<PostomatDocumentItem>();

			DeleteConfig.AddHibernateDeleteInfo<PostomatDocumentWithdraw>()
				.AddDeleteDependence<PostomatDocumentWithdrawItem>(x => x.DocumentWithdraw);

			DeleteConfig.AddHibernateDeleteInfo<PostomatDocumentWithdrawItem>();
			#endregion
			#region Нормы выдачи

			DeleteConfig.AddHibernateDeleteInfo<Norm>()
				.AddRemoveFromDependence<EmployeeCard>(x => x.UsedNorms, x => x.RemoveUsedNorm)
				.AddDeleteDependence<NormItem>(x => x.Norm);

			DeleteConfig.AddHibernateDeleteInfo<NormItem>()
				.AddClearDependence<EmployeeIssueOperation>(x => x.NormItem)
				//Ну нужна так как должна удалятся через пересчет. .AddClearDependence<EmployeeCardItem> (x => x.ActiveNormItem) //FIXME После этого нужно пересчитать требования к выдаче, то новому списку норм.
				;

			DeleteConfig.AddHibernateDeleteInfo<DutyNorm>()
				.AddDeleteDependence<DutyNormIssueOperation>(x => x.DutyNorm)
				.AddDeleteDependence<DutyNormItem>(x => x.DutyNorm)
				.AddDeleteDependence<ExpenseDutyNorm>(x => x.DutyNorm)
				.AddDeleteDependence<ReturnItem>(x => x.DutyNorm);

			DeleteConfig.AddHibernateDeleteInfo<DutyNormItem>()
				.AddClearDependence<DutyNormIssueOperation>(x => x.DutyNormItem);
			
			DeleteConfig.AddHibernateDeleteInfo<NormCondition>()
				.AddClearDependence<NormItem>(x => x.NormCondition);

			DeleteConfig.AddHibernateDeleteInfo<Profession>()
				.AddClearDependence<Post>(x => x.Profession);

			DeleteConfig.AddHibernateDeleteInfo<ProtectionTools>()
				.AddDeleteDependence<EmployeeCardItem>(x => x.ProtectionTools)
				.AddDeleteDependence<NormItem>(x => x.ProtectionTools)
				.AddDeleteDependence<DutyNormItem>(x => x.ProtectionTools)
				.AddDeleteDependence<IssuanceSheetItem>(x => x.ProtectionTools)
				.AddClearDependence<ExpenseItem>(x => x.ProtectionTools)
				.AddClearDependence<CollectiveExpenseItem>(x => x.ProtectionTools)
				.AddClearDependence<EmployeeIssueOperation>(x => x.ProtectionTools)
				.AddClearDependence<DutyNormIssueOperation>(x => x.ProtectionTools);

			DeleteConfig.AddHibernateDeleteInfo<RegulationDoc>()
			            .AddDeleteDependence<Norm>(x => x.Document)
						.AddDeleteDependence<RegulationDocAnnex>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<RegulationDocAnnex>()
				.AddClearDependence<Norm>(x => x.Annex);

			#endregion
			#region Размеры
			DeleteConfig.AddHibernateDeleteInfo<Size>()
				.AddClearDependence<Barcode>(x => x.Height)
				.AddClearDependence<Barcode>(x => x.Size)
				.AddClearDependence<CollectiveExpenseItem>(x => x.Height)
				.AddClearDependence<CollectiveExpenseItem>(x => x.WearSize)
				.AddClearDependence<DutyNormIssueOperation>(x => x.Height)
				.AddClearDependence<DutyNormIssueOperation>(x => x.WearSize)
				.AddClearDependence<EmployeeIssueOperation>(x => x.Height)
				.AddClearDependence<EmployeeIssueOperation>(x => x.WearSize)
				.AddClearDependence<ExpenseItem>(x => x.Height)
				.AddClearDependence<ExpenseItem>(x => x.WearSize)
				.AddClearDependence<IncomeItem>(x => x.Height)
				.AddClearDependence<IncomeItem>(x => x.WearSize)
				.AddClearDependence<IssuanceSheetItem>(x => x.Height)
				.AddClearDependence<IssuanceSheetItem>(x => x.WearSize)
				.AddClearDependence<ReturnItem>(x => x.Height)
				.AddClearDependence<ReturnItem>(x => x.WearSize)
				.AddClearDependence<ShipmentItem>(x => x.Height)
				.AddClearDependence<ShipmentItem>(x => x.WearSize)
				.AddClearDependence<WarehouseOperation>(x => x.Height)
				.AddClearDependence<WarehouseOperation>(x => x.WearSize)
				.AddClearDependence<WriteoffItem>(x => x.Height)
				.AddClearDependence<WriteoffItem>(x => x.WearSize)
				.AddDeleteDependence<EmployeeSize>(x => x.Size)
				;

			DeleteConfig.AddHibernateDeleteInfo<SizeType>()
				.AddDeleteDependence<Size>(x => x.SizeType)
				.AddDeleteDependence<EmployeeSize>(x => x.SizeType)
				.AddClearDependence<ItemsType>(x => x.SizeType)
				.AddClearDependence<ItemsType>(x => x.HeightType);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeSize>();
			#endregion
			#region Statements

			DeleteConfig.AddHibernateDeleteInfo<IssuanceSheet>()
				.AddDeleteDependence<IssuanceSheetItem>(x => x.IssuanceSheet);

			DeleteConfig.AddHibernateDeleteInfo<IssuanceSheetItem>();

			#endregion
			#region Склад
			DeleteConfig.AddHibernateDeleteInfo<Barcode>()
				.AddDeleteDependence<BarcodeOperation>(x => x.Barcode)
				.AddDeleteDependence<PostomatDocumentItem>(x => x.Barcode)
				.AddDeleteDependence<PostomatDocumentWithdrawItem>(x => x.Barcode)
				.AddDeleteDependence<ServiceClaim>(x => x.Barcode)
				;
			
			DeleteConfig.AddHibernateDeleteInfo<ItemsType>()
				.AddDeleteDependence<Nomenclature>(x => x.Type)
				.AddDeleteDependence<ProtectionTools>(x => x.Type);
			
			DeleteConfig.AddHibernateDeleteInfo<Warehouse>()
				.AddDeleteDependence<Income>(x => x.Warehouse)
				.AddDeleteDependence<Return>(x => x.Warehouse)
				.AddDeleteDependence<Expense>(x => x.Warehouse)
				.AddDeleteDependence<CollectiveExpense>(x => x.Warehouse)
				.AddDeleteDependence<ExpenseDutyNorm>(x => x.Warehouse)
				.AddDeleteDependence<WriteoffItem>(x => x.Warehouse)
				.AddDeleteDependence<Transfer>(x => x.WarehouseFrom)
				.AddDeleteDependence<Transfer>(x => x.WarehouseTo)
				.AddDeleteDependence<WarehouseOperation>(x => x.ReceiptWarehouse)
				.AddDeleteDependence<WarehouseOperation>(x => x.ExpenseWarehouse)
				.AddClearDependence<Subdivision>(x => x.Warehouse)
				.AddClearDependence<UserSettings>(x => x.DefaultWarehouse)
				.AddDeleteDependence<Completion>(x => x.ResultWarehouse)
				.AddDeleteDependence<Completion>(x => x.SourceWarehouse);

			DeleteConfig.AddHibernateDeleteInfo<MeasurementUnits>()
				.AddClearDependence<ItemsType>(x => x.Units);

			DeleteConfig.AddHibernateDeleteInfo<Nomenclature> ()
				.AddClearDependence<ProtectionTools>(x => x.SupplyNomenclatureFemale)
				.AddClearDependence<ProtectionTools>(x => x.SupplyNomenclatureMale)
				.AddClearDependence<ProtectionTools>(x => x.SupplyNomenclatureUnisex)
				.AddDeleteDependence<Barcode>(x => x.Nomenclature)
				.AddDeleteDependence<CollectiveExpenseItem> (x => x.Nomenclature)
				.AddDeleteDependence<DutyNormIssueOperation>(x => x.Nomenclature)
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.Nomenclature)
				.AddDeleteDependence<ExpenseItem> (x => x.Nomenclature)
				.AddDeleteDependence<IncomeItem> (x => x.Nomenclature)
				.AddDeleteDependence<IssuanceSheetItem>(x => x.Nomenclature)
				.AddDeleteDependence<PostomatDocumentItem>(x => x.Nomenclature)
				.AddDeleteDependence<PostomatDocumentWithdrawItem>(x => x.Nomenclature)
				.AddDeleteDependence<ReturnItem> (x => x.Nomenclature)
				.AddDeleteDependence<ShipmentItem>(x => x.Nomenclature)
				.AddDeleteDependence<TransferItem>(x => x.Nomenclature)
				.AddDeleteDependence<WarehouseOperation>(x => x.Nomenclature)
				.AddDeleteDependence<WriteoffItem> (x => x.Nomenclature)
				;

			DeleteConfig.AddHibernateDeleteInfo<Owner>()
				.AddClearDependence<WarehouseOperation>(x => x.Owner);
			#endregion
			#region Складские документы
			DeleteConfig.AddHibernateDeleteInfo<Expense> ()
				.AddDeleteDependence<ExpenseItem> (x => x.ExpenseDoc)
				.AddDeleteDependence<IssuanceSheet>(x => x.Expense);

			DeleteConfig.AddHibernateDeleteInfo<ExpenseItem> ()
				.AddDeleteDependence<IssuanceSheetItem>(x => x.ExpenseItem)
				.AddDeleteCascadeDependence(x => x.EmployeeIssueOperation)
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);

			DeleteConfig.AddHibernateDeleteInfo<CollectiveExpense>()
				.AddDeleteDependence<CollectiveExpenseItem>(x => x.Document)
				.AddDeleteDependence<IssuanceSheet>(x => x.CollectiveExpense);

			DeleteConfig.AddHibernateDeleteInfo<CollectiveExpenseItem> ()
				.AddDeleteDependence<IssuanceSheetItem>(x => x.CollectiveExpenseItem)
				.AddDeleteCascadeDependence(x => x.EmployeeIssueOperation)
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);

			DeleteConfig.AddHibernateDeleteInfo<ExpenseDutyNorm>()
				.AddDeleteDependence<ExpenseDutyNormItem>(x => x.Document)
				.AddDeleteDependence<IssuanceSheet>(x => x.ExpenseDutyNorm);

			DeleteConfig.AddHibernateDeleteInfo<ExpenseDutyNormItem>()
				.AddDeleteDependence<IssuanceSheetItem>(x => x.ExpenseDutyNormItem)
				.AddDeleteCascadeDependence(x => x.Operation)
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);

			DeleteConfig.AddHibernateDeleteInfo<Income> ()
				.AddDeleteDependence<IncomeItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<IncomeItem> ()
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);
			
			DeleteConfig.AddHibernateDeleteInfo<Return> ()
				.AddDeleteDependence<ReturnItem>(x => x.Document);
			
			DeleteConfig.AddHibernateDeleteInfo<ReturnItem> ()
				.AddDeleteCascadeDependence(x => x.ReturnFromDutyNormOperation)
				.AddDeleteCascadeDependence(x => x.ReturnFromEmployeeOperation)
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);
			
			DeleteConfig.AddHibernateDeleteInfo<Writeoff> ()
				.AddDeleteDependence<WriteoffItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<WriteoffItem> ()
				.AddRemoveFromDependence<Writeoff>(x => x.Items)//Необходимо иначе будут исключения при удалении строк выдачи которые создают списание. 
				.AddDeleteCascadeDependence(x => x.DutyNormWriteOffOperation)
				.AddDeleteCascadeDependence(x => x.EmployeeWriteoffOperation)
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);

			DeleteConfig.AddHibernateDeleteInfo<Transfer>()
				.AddDeleteDependence<TransferItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<TransferItem>()
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);

			DeleteConfig.AddHibernateDeleteInfo<Completion>()
				.AddDeleteDependence<CompletionSourceItem>(x => x.Completion)
				.AddDeleteDependence<CompletionResultItem>(x => x.Completion);
			
			DeleteConfig.AddHibernateDeleteInfo<CompletionSourceItem>()
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);
			DeleteConfig.AddHibernateDeleteInfo<CompletionResultItem>()
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);

			DeleteConfig.AddHibernateDeleteInfo<Inspection>()
				.AddDeleteDependence<InspectionItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<InspectionItem>()
				.AddDeleteCascadeDependence(x => x.NewOperationIssue);

			DeleteConfig.AddHibernateDeleteInfo<CausesWriteOff>()
				.AddClearDependence<WriteoffItem>(x => x.CausesWriteOff);
			
			#endregion
			#region Поставка
			DeleteConfig.AddHibernateDeleteInfo<Shipment>()
				.AddClearDependence<Income>(x => x.Shipment)
				.AddDeleteDependence<ShipmentItem>(x => x.Shipment);
			DeleteConfig.AddHibernateDeleteInfo<ShipmentItem>();
			#endregion
			#region Пользователь

			DeleteConfig.AddHibernateDeleteInfo<UserBase>()
				.AddClearDependence<CollectiveExpense>(x => x.CreatedbyUser)
				.AddClearDependence<Completion>(x => x.CreatedbyUser)
				.AddClearDependence<EmployeeCard>(x => x.CreatedbyUser)
				.AddClearDependence<Expense>(x => x.CreatedbyUser)
				.AddClearDependence<ExpenseDutyNorm>(x => x.CreatedbyUser)
				.AddClearDependence<Income>(x => x.CreatedbyUser)
				.AddClearDependence<Inspection>(x => x.CreatedbyUser)
				.AddClearDependence<PostomatDocumentWithdraw>(x => x.User)
				.AddClearDependence<Return>(x => x.CreatedbyUser)
				.AddClearDependence<Shipment>(x => x.CreatedbyUser)
				.AddClearDependence<StateOperation>(x => x.User)
				.AddClearDependence<Transfer>(x => x.CreatedbyUser)
				.AddClearDependence<Writeoff>(x => x.CreatedbyUser)
				.AddDeleteDependence<UserSettings>(x => x.User)
				;

			DeleteConfig.AddHibernateDeleteInfo<UserSettings>();

			#endregion
			#region Analytics
			DeleteConfig.AddHibernateDeleteInfo<ProtectionToolsCategory>()
				.AddClearDependence<ProtectionTools>(x => x.CategoryForAnalytic);
			#endregion
			
			logger.Info ("Ок");
		}
	}
}
