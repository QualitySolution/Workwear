using QS.BusinessCommon.Domain;
using QS.Deletion;
using QS.Project.Domain;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Statements;
using workwear.Domain.Stock;
using workwear.Domain.Users;

namespace workwear
{
	public class Configure
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static void ConfigureDeletion ()
		{
			logger.Info ("Настройка параметров удаления...");

			#region Организация

			DeleteConfig.AddHibernateDeleteInfo<Department>()
			//	.AddClearDependence<EmployeeCard>(x => x.Department)
				.AddClearDependence<Post>(x => x.Department);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeCard>()
				.AddDeleteDependence<EmployeeCardItem>(x => x.EmployeeCard)
				.AddDeleteDependence<EmployeeVacation>(x => x.Employee)
				.AddDeleteDependence<Expense>(x => x.Employee)
				.AddDeleteDependence<Income>(x => x.EmployeeCard)
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.Employee)
				.AddDeleteDependence<IssuanceSheetItem>(x => x.Employee)
				.AddDeleteDependence<MassExpenseEmployee>(x => x.EmployeeCard);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeCardItem>();

			DeleteConfig.AddHibernateDeleteInfo<EmployeeVacation>();

			DeleteConfig.AddHibernateDeleteInfo<Leader>()
				.AddClearDependence<EmployeeCard>(x => x.Leader)
				.AddClearDependence<IssuanceSheet>(x => x.HeadOfDivisionPerson)
				.AddClearDependence<IssuanceSheet>(x => x.ResponsiblePerson);

			DeleteConfig.AddHibernateDeleteInfo<Organization>()
				.AddClearDependence<IssuanceSheet>(x => x.Organization);

			DeleteConfig.AddHibernateDeleteInfo<Post>()
				.AddRemoveFromDependence<Norm>(x => x.Professions);
				//.AddClearDependence<EmployeeCard>(x => x.Post);

			DeleteConfig.AddHibernateDeleteInfo<Subdivision> ()
				.AddDeleteDependence<Department>(x => x.Subdivision) 
				.AddDeleteDependence<SubdivisionPlace>(x => x.Subdivision)
				.AddDeleteDependence<Expense> (x => x.Subdivision)
				.AddDeleteDependence<Income> (x => x.Subdivision)
				.AddDeleteDependence<SubdivisionIssueOperation>(x => x.Subdivision)
				.AddClearDependence<EmployeeCard> (x => x.Subdivision)
				.AddClearDependence<Post>(x => x.Subdivision)
				.AddClearDependence<IssuanceSheet>(x => x.Subdivision);

			DeleteConfig.AddHibernateDeleteInfo<SubdivisionPlace>()
				.AddClearDependence<ExpenseItem>(x => x.SubdivisionPlace)
				.AddClearDependence<SubdivisionIssueOperation>(x => x.SubdivisionPlace);
				
			DeleteConfig.AddHibernateDeleteInfo<VacationType>()
				.AddDeleteDependence<EmployeeVacation>(x => x.VacationType);

			#endregion
			#region Нормы выдачи

			DeleteConfig.AddHibernateDeleteInfo<Norm>()
				.AddRemoveFromDependence<EmployeeCard>(x => x.UsedNorms, x => x.RemoveUsedNorm)
				.AddDeleteDependence<NormItem>(x => x.Norm);

			DeleteConfig.AddHibernateDeleteInfo<NormItem>()
				.AddClearDependence<EmployeeIssueOperation>(x => x.NormItem)
				//Ну нужна так как должна удалятся через пересчет. .AddClearDependence<EmployeeCardItem> (x => x.ActiveNormItem) //FIXME После этого нужно пересчитать требования к выдаче, то новому списку норм.
				;

			DeleteConfig.AddHibernateDeleteInfo<Profession>()
				.AddClearDependence<Post>(x => x.Profession);

			DeleteConfig.AddHibernateDeleteInfo<ProtectionTools>()
				.AddRemoveFromDependence<ProtectionTools>(x => x.Analogs)
				.AddRemoveFromDependence<Nomenclature>(x => x.ProtectionTools)
				.AddDeleteDependence<EmployeeCardItem>(x => x.ProtectionTools)
				.AddDeleteDependence<NormItem>(x => x.ProtectionTools)
				.AddDeleteDependence<IssuanceSheetItem>(x => x.ProtectionTools)
				.AddClearDependence<ExpenseItem>(x => x.ProtectionTools)
				.AddClearDependence<EmployeeIssueOperation>(x => x.ProtectionTools);

			DeleteConfig.AddHibernateDeleteInfo<RegulationDoc>()
			            .AddDeleteDependence<Norm>(x => x.Document)
						.AddDeleteDependence<RegulationDocAnnex>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<RegulationDocAnnex>()
			            .AddDeleteDependence<Norm>(x => x.Annex);

			#endregion
			#region Склад

			DeleteConfig.AddHibernateDeleteInfo<Warehouse>()
				.AddDeleteDependence<Income>(x => x.Warehouse)
				.AddDeleteDependence<Expense>(x => x.Warehouse)
				.AddDeleteDependence<WriteoffItem>(x => x.Warehouse)
				.AddDeleteDependence<Transfer>(x => x.WarehouseFrom)
				.AddDeleteDependence<Transfer>(x => x.WarehouseTo)
				.AddDeleteDependence<MassExpense>(x => x.WarehouseFrom)
				.AddDeleteDependence<WarehouseOperation>(x => x.ReceiptWarehouse)
				.AddDeleteDependence<WarehouseOperation>(x => x.ExpenseWarehouse)
				.AddClearDependence<Subdivision>(x => x.Warehouse);

			DeleteConfig.AddHibernateDeleteInfo<MeasurementUnits> ()
				.AddClearDependence<ItemsType> (x => x.Units)
				.AddClearDependence<ProtectionTools>(x => x.Units);

			DeleteConfig.AddHibernateDeleteInfo<Nomenclature> ()
				.AddRemoveFromDependence<ProtectionTools>(x => x.Nomenclatures)
				.AddDeleteDependence<ExpenseItem> (x => x.Nomenclature)
				.AddDeleteDependence<IncomeItem> (x => x.Nomenclature)
				.AddDeleteDependence<WriteoffItem> (x => x.Nomenclature)
				.AddDeleteDependence<TransferItem>(x => x.Nomenclature)
				.AddDeleteDependence<MassExpenseNomenclature>(x => x.Nomenclature)
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.Nomenclature)
				.AddDeleteDependence<SubdivisionIssueOperation>(x => x.Nomenclature)
				.AddDeleteDependence<WarehouseOperation>(x => x.Nomenclature)
				.AddDeleteDependence<IssuanceSheetItem>(x => x.Nomenclature);

			DeleteConfig.AddHibernateDeleteInfo<Expense> ()
				.AddDeleteDependence<ExpenseItem> (x => x.ExpenseDoc)
				.AddDeleteDependence<IssuanceSheet>(x => x.Expense);

			DeleteConfig.AddHibernateDeleteInfo<ExpenseItem> ()
				.AddDeleteDependence<IssuanceSheetItem>(x => x.ExpenseItem)
				.AddDeleteCascadeDependence(x => x.EmployeeIssueOperation)
				.AddDeleteCascadeDependence(x => x.SubdivisionIssueOperation)
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);

			DeleteConfig.AddHibernateDeleteInfo<Income> ()
				.AddDeleteDependence<IncomeItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<IncomeItem> ()
				.AddDeleteCascadeDependence(x => x.ReturnFromEmployeeOperation)
				.AddDeleteCascadeDependence(x => x.ReturnFromSubdivisionOperation)
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);

			DeleteConfig.AddHibernateDeleteInfo<ItemsType>()
				.AddDeleteDependence<Nomenclature>(x => x.Type);

			DeleteConfig.AddHibernateDeleteInfo<MassExpense>()
				.AddDeleteDependence<IssuanceSheet>(x => x.MassExpense)
				.AddDeleteDependence<MassExpenseEmployee>(x => x.DocumentMassExpense)
				.AddDeleteDependence<MassExpenseOperation>(x => x.MassExpenseDoc)
				.AddDeleteDependence<MassExpenseNomenclature>(x => x.DocumentMassExpense);

			DeleteConfig.AddHibernateDeleteInfo<MassExpenseEmployee>();

			DeleteConfig.AddHibernateDeleteInfo<MassExpenseNomenclature>();

			DeleteConfig.AddHibernateDeleteInfo<MassExpenseOperation>()
				.AddDeleteCascadeDependence(x => x.EmployeeIssueOperation)
				.AddDeleteCascadeDependence(x => x.WarehouseOperationExpense);

			DeleteConfig.AddHibernateDeleteInfo<Writeoff> ()
				.AddDeleteDependence<WriteoffItem>(x => x.Document)
				.AddClearDependence<Expense>(x => x.WriteOffDoc);

			DeleteConfig.AddHibernateDeleteInfo<WriteoffItem> ()
				.AddDeleteCascadeDependence(x => x.EmployeeWriteoffOperation)
				.AddDeleteCascadeDependence(x => x.SubdivisionWriteoffOperation)
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);

			DeleteConfig.AddHibernateDeleteInfo<Transfer>()
				.AddDeleteDependence<TransferItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<TransferItem>()
				.AddDeleteCascadeDependence(x => x.WarehouseOperation);

			#endregion

			#region Statements

			DeleteConfig.AddHibernateDeleteInfo<IssuanceSheet>()
				.AddDeleteDependence<IssuanceSheetItem>(x => x.IssuanceSheet);

			DeleteConfig.AddHibernateDeleteInfo<IssuanceSheetItem>();

			#endregion

			#region Операции

			DeleteConfig.AddHibernateDeleteInfo<EmployeeIssueOperation>()
				.RequiredCascadeDeletion()
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.IssuedOperation)
				.AddDeleteDependence<ExpenseItem>(x => x.EmployeeIssueOperation)
				.AddDeleteDependence<IncomeItem>(x => x.ReturnFromEmployeeOperation)
				.AddDeleteDependence<WriteoffItem>(x => x.EmployeeWriteoffOperation)
				.AddDeleteDependence<MassExpenseOperation>(x => x.EmployeeIssueOperation)
				.AddDeleteCascadeDependence(x => x.EmployeeOperationIssueOnWriteOff)
				.AddClearDependence<IssuanceSheetItem>(x => x.IssueOperation)
				.AddClearDependence<EmployeeIssueOperation>(x => x.EmployeeOperationIssueOnWriteOff);

			DeleteConfig.AddHibernateDeleteInfo<SubdivisionIssueOperation>()
				.RequiredCascadeDeletion()
				.AddDeleteDependence<SubdivisionIssueOperation>(x => x.IssuedOperation)
				.AddDeleteDependence<ExpenseItem>(x => x.SubdivisionIssueOperation)
				.AddDeleteDependence<IncomeItem>(x => x.ReturnFromSubdivisionOperation)
				.AddDeleteDependence<WriteoffItem>(x => x.SubdivisionWriteoffOperation);

			DeleteConfig.AddHibernateDeleteInfo<WarehouseOperation>()
				.RequiredCascadeDeletion()
				.AddDeleteDependence<ExpenseItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<IncomeItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<WriteoffItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<TransferItem>(x => x.WarehouseOperation)
				.AddDeleteDependence<MassExpenseOperation>(x => x.WarehouseOperationExpense)
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.WarehouseOperation)
				.AddDeleteDependence<SubdivisionIssueOperation>(x => x.WarehouseOperation);

			#endregion

			#region Пользователь

			DeleteConfig.AddHibernateDeleteInfo<UserBase> ()
				.AddDeleteDependence<UserSettings>(x => x.User)
				.AddClearDependence<EmployeeCard> (x => x.CreatedbyUser)
				.AddClearDependence<Writeoff> (x => x.CreatedbyUser)
				.AddClearDependence<Expense> (x => x.CreatedbyUser)
				.AddClearDependence<Income> (x => x.CreatedbyUser)
				.AddClearDependence<MassExpense>(x => x.CreatedbyUser)
				.AddClearDependence<Transfer>(x => x.CreatedbyUser);

			DeleteConfig.AddHibernateDeleteInfo<UserSettings>();

			#endregion

			logger.Info ("Ок");
		}
	}
}