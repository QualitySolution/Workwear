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

			DeleteConfig.AddHibernateDeleteInfo<Organization>()
				.AddClearDependence<IssuanceSheet>(x => x.Organization);

			DeleteConfig.AddHibernateDeleteInfo<Subdivision> ()
				.AddDeleteDependence<SubdivisionPlace>(x => x.Subdivision)
				.AddDeleteDependence<Expense> (x => x.Subdivision)
				.AddDeleteDependence<Income> (x => x.Subdivision)
				.AddClearDependence<EmployeeCard> (x => x.Subdivision)
				.AddClearDependence<IssuanceSheet>(x => x.Subdivision);

			DeleteConfig.AddHibernateDeleteInfo<SubdivisionPlace>()
				.AddClearDependence<ExpenseItem>(x => x.SubdivisionPlace);

			DeleteConfig.AddHibernateDeleteInfo<Leader> ()
				.AddClearDependence<EmployeeCard> (x => x.Leader)
				.AddClearDependence<IssuanceSheet>(x => x.HeadOfDivisionPerson)
				.AddClearDependence<IssuanceSheet>(x => x.ResponsiblePerson);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeCard> ()
				.AddDeleteDependence<EmployeeCardItem> (x => x.EmployeeCard)
				.AddDeleteDependence<EmployeeVacation> (x => x.Employee)
				.AddDeleteDependence<Expense> (x => x.Employee)
				.AddDeleteDependence<Income> (x => x.EmployeeCard)
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.Employee)
				.AddDeleteDependence<IssuanceSheetItem>(x => x.Employee);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeCardItem> ();

			DeleteConfig.AddHibernateDeleteInfo<EmployeeVacation>();

			DeleteConfig.AddHibernateDeleteInfo<VacationType>()
				.AddDeleteDependence<EmployeeVacation>(x => x.VacationType);

			#endregion
			#region Нормы выдачи

			DeleteConfig.AddHibernateDeleteInfo<RegulationDoc>()
			            .AddDeleteDependence<Norm>(x => x.Document)
						.AddDeleteDependence<RegulationDocAnnex>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<RegulationDocAnnex>()
			            .AddDeleteDependence<Norm>(x => x.Annex);

			DeleteConfig.AddHibernateDeleteInfo<ItemsType> ()
				.AddDeleteDependence<Nomenclature> (x => x.Type)
				.AddDeleteDependence<EmployeeCardItem> (x => x.Item)
				.AddDeleteDependence<NormItem> (x => x.Item)
				.AddDeleteDependence<IssuanceSheetItem>(x => x.ItemsType);

			DeleteConfig.AddHibernateDeleteInfo<Norm> ()
				.AddRemoveFromDependence<EmployeeCard> (x => x.UsedNorms, x => x.RemoveUsedNorm)
				.AddDeleteDependence<NormItem> (x => x.Norm);

			DeleteConfig.AddHibernateDeleteInfo<NormItem> ()
				.AddClearDependence<EmployeeIssueOperation>(x => x.NormItem)
				//Ну нужна так как должна удалятся через пересчет. .AddClearDependence<EmployeeCardItem> (x => x.ActiveNormItem) //FIXME После этого нужно пересчитать требования к выдаче, то новому списку норм.
				; 

			DeleteConfig.AddHibernateDeleteInfo<Post>()
				.AddRemoveFromDependence<Norm>(x => x.Professions)
				.AddClearDependence<EmployeeCard>(x => x.Post);

			#endregion
			#region Склад

			DeleteConfig.AddHibernateDeleteInfo<MeasurementUnits> ()
				.AddDeleteDependence<ItemsType> (x => x.Units);

			DeleteConfig.AddHibernateDeleteInfo<Nomenclature> ()
				.AddDeleteDependence<ExpenseItem> (x => x.Nomenclature)
				.AddDeleteDependence<IncomeItem> (x => x.Nomenclature)
				.AddDeleteDependence<WriteoffItem> (x => x.Nomenclature)
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.Nomenclature)
				.AddDeleteDependence<IssuanceSheetItem>(x => x.Nomenclature);

			DeleteConfig.AddHibernateDeleteInfo<Expense> ()
				.AddDeleteDependence<ExpenseItem> (x => x.ExpenseDoc)
				.AddDeleteDependence<IssuanceSheet>(x => x.Expense);

			DeleteConfig.AddHibernateDeleteInfo<ExpenseItem> ()
				.AddDeleteDependence<IssuanceSheetItem>(x => x.ExpenseItem)
				.AddDeleteDependence<IncomeItem> (x => x.IssuedOn)
				.AddDeleteDependence<WriteoffItem> (x => x.IssuedOn)
				.AddDeleteCascadeDependence(x => x.EmployeeIssueOperation);

			DeleteConfig.AddHibernateDeleteInfo<Income> ()
				.AddDeleteDependence<IncomeItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<IncomeItem> ()
				.AddDeleteDependence<ExpenseItem> (x => x.IncomeOn)
				.AddDeleteDependence<WriteoffItem> (x => x.IncomeOn)
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.IncomeOnStock)
				.AddDeleteCascadeDependence(x => x.EmployeeIssueOperation);

			DeleteConfig.AddHibernateDeleteInfo<Writeoff> ()
				.AddDeleteDependence<WriteoffItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<WriteoffItem> ()
				.AddDeleteCascadeDependence(x => x.EmployeeIssueOperation);

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
				.AddDeleteDependence<IncomeItem>(x => x.EmployeeIssueOperation)
				.AddDeleteDependence<WriteoffItem>(x => x.EmployeeIssueOperation)
				.AddClearDependence<IssuanceSheetItem>(x => x.IssueOperation);

			#endregion

			#region Пользователь

			DeleteConfig.AddHibernateDeleteInfo<UserBase> ()
				.AddDeleteDependence<UserSettings>(x => x.User)
				.AddClearDependence<EmployeeCard> (x => x.CreatedbyUser)
				.AddClearDependence<Writeoff> (x => x.CreatedbyUser)
				.AddClearDependence<Expense> (x => x.CreatedbyUser)
				.AddClearDependence<Income> (x => x.CreatedbyUser);

			DeleteConfig.AddHibernateDeleteInfo<UserSettings>();

			#endregion

			logger.Info ("Ок");
		}
	}
}