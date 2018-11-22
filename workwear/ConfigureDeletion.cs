using QS.Deletion;
using QS.Project.Domain;
using QSBusinessCommon.Domain;
using workwear.Domain.Operations;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;
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

			DeleteConfig.AddHibernateDeleteInfo<Facility> ()
				.AddDeleteDependence<FacilityPlace>(x => x.Facility)
				.AddDeleteDependence<Expense> (x => x.Facility)
				.AddDeleteDependence<Income> (x => x.Facility)
				.AddClearDependence<EmployeeCard> (x => x.Facility);

			DeleteConfig.AddHibernateDeleteInfo<FacilityPlace>()
				.AddClearDependence<ExpenseItem>(x => x.FacilityPlace);

			DeleteConfig.AddHibernateDeleteInfo<Leader> ()
				.AddClearDependence<EmployeeCard> (x => x.Leader);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeCard> ()
				.AddDeleteDependence<EmployeeCardItem> (x => x.EmployeeCard)
				.AddDeleteDependence<Expense> (x => x.EmployeeCard)
				.AddDeleteDependence<Income> (x => x.EmployeeCard)
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.Employee);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeCardItem> ();

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
				.AddDeleteDependence<NormItem> (x => x.Item);

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
				.AddClearDependence<EmployeeCardItem> (x => x.MatchedNomenclature);

			DeleteConfig.AddHibernateDeleteInfo<Expense> ()
				.AddDeleteDependence<ExpenseItem> (x => x.ExpenseDoc);

			DeleteConfig.AddHibernateDeleteInfo<Income> ()
				.AddDeleteDependenceFromCollection (x => x.Items);

			DeleteConfig.AddHibernateDeleteInfo<Writeoff> ()
				.AddDeleteDependenceFromCollection (x => x.Items);

			DeleteConfig.AddHibernateDeleteInfo<IncomeItem> ()
				.AddDeleteDependence<ExpenseItem> (x => x.IncomeOn)
				.AddDeleteDependence<WriteoffItem> (x => x.IncomeOn)
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.IncomeOnStock)
				.AddDeleteCascadeDependence(x => x.EmployeeIssueOperation);

			DeleteConfig.AddHibernateDeleteInfo<ExpenseItem> ()
				.AddDeleteDependence<IncomeItem> (x => x.IssuedOn)
				.AddDeleteDependence<WriteoffItem> (x => x.IssuedOn)
				.AddDeleteCascadeDependence(x => x.EmployeeIssueOperation);

			DeleteConfig.AddHibernateDeleteInfo<WriteoffItem> ()
				.AddDeleteCascadeDependence(x => x.EmployeeIssueOperation);

			#endregion

			#region Операции

			DeleteConfig.AddHibernateDeleteInfo<EmployeeIssueOperation>()
				.RequiredCascadeDeletion()
				.AddDeleteDependence<EmployeeIssueOperation>(x => x.IssuedOperation)
				.AddDeleteDependence<ExpenseItem>(x => x.EmployeeIssueOperation)
				.AddDeleteDependence<IncomeItem>(x => x.EmployeeIssueOperation)
				.AddDeleteDependence<WriteoffItem>(x => x.EmployeeIssueOperation);

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