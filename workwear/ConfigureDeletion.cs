using QS.Deletion;
using QS.Project.Domain;
using QSBusinessCommon.Domain;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear
{
	partial class MainClass
	{
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
				.AddDeleteDependence<Income> (x => x.EmployeeCard);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeCardItem> ();

			#endregion
			#region Нормы выдачи

			DeleteConfig.AddHibernateDeleteInfo<RegulationDoc>()
			            .AddDeleteDependence<Norm>(x => x.Document)
						.AddDeleteDependenceFromBag(x => x.Annexess);

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
				.AddClearDependence<EmployeeCardItem> (x => x.MatchedNomenclature);

			DeleteConfig.AddHibernateDeleteInfo<Expense> ()
				.AddDeleteDependence<ExpenseItem> (x => x.ExpenseDoc);

			DeleteConfig.AddHibernateDeleteInfo<Income> ()
				.AddDeleteDependenceFromBag (x => x.Items);

			DeleteConfig.AddHibernateDeleteInfo<Writeoff> ()
				.AddDeleteDependenceFromBag (x => x.Items);

			DeleteConfig.AddHibernateDeleteInfo<IncomeItem> ()
				.AddDeleteDependence<ExpenseItem> (x => x.IncomeOn)
				.AddDeleteDependence<WriteoffItem> (x => x.IncomeOn);

			DeleteConfig.AddHibernateDeleteInfo<ExpenseItem> ()
				.AddDeleteDependence<IncomeItem> (x => x.IssuedOn)
				.AddDeleteDependence<WriteoffItem> (x => x.IssuedOn);

			DeleteConfig.AddHibernateDeleteInfo<WriteoffItem> ();

			#endregion

			DeleteConfig.AddHibernateDeleteInfo<UserBase> ()
				.AddClearDependence<EmployeeCard> (x => x.CreatedbyUser)
				.AddClearDependence<Writeoff> (x => x.CreatedbyUser)
				.AddClearDependence<Expense> (x => x.CreatedbyUser)
				.AddClearDependence<Income> (x => x.CreatedbyUser);

			//Для тестирования
			#if DEBUG
			DeleteConfig.DeletionCheck ();
			#endif

			logger.Info ("Ок");
		}
	}
}