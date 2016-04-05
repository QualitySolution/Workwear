using QSOrmProject.Deletion;
using workwear.Domain;
using workwear.Domain.Stock;
using System.Collections.Generic;
using QSBusinessCommon.Domain;

namespace workwear
{
	partial class MainClass
	{
		public static void ConfigureDeletion ()
		{
			logger.Info ("Настройка параметров удаления...");

			DeleteConfig.AddHibernateDeleteInfo<Facility> ()
				.AddDeleteDependence (new DeleteDependenceInfo ("object_places", "WHERE object_id = @id "))
				.AddDeleteDependence<Expense> (x => x.Facility)
				.AddDeleteDependence<Income> (x => x.Facility)
				.AddClearDependence<EmployeeCard> (x => x.Facility);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectsName = "Размещения в объекте",
				TableName = "object_places",
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					new ClearDependenceInfo (typeof(ExpenseItem), "WHERE object_place_id = @id", "object_place_id")
				}
			});

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

			DeleteConfig.AddHibernateDeleteInfo<MeasurementUnits> ()
				.AddDeleteDependence<ItemsType> (x => x.Units);

			DeleteConfig.AddHibernateDeleteInfo<User> ()
				.AddClearDependence<EmployeeCard> (x => x.CreatedbyUser)
				.AddClearDependence<Writeoff> (x => x.CreatedbyUser)
				.AddClearDependence<Expense> (x => x.CreatedbyUser)
				.AddClearDependence<Income> (x => x.CreatedbyUser);

			DeleteConfig.AddHibernateDeleteInfo<Leader> ()
				.AddClearDependence<EmployeeCard> (x => x.Leader);

			DeleteConfig.AddHibernateDeleteInfo<Post> ()
				.AddRemoveFromDependence<Norm> (x => x.Professions)
				.AddClearDependence<EmployeeCard> (x => x.Post);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeCard> ()
				.AddDeleteDependence<EmployeeCardItem> (x => x.EmployeeCard)
				.AddDeleteDependence<Expense> (x => x.EmployeeCard)
				.AddDeleteDependence<Income> (x => x.EmployeeCard);

			DeleteConfig.AddHibernateDeleteInfo<EmployeeCardItem> ();

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
			
			//Для тетирования
			#if DEBUG
			DeleteConfig.DeletionCheck ();
			#endif

			logger.Info ("Ок");
		}
	}
}