
using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Project.DB;
using workwear.Domain.Company;
using workwear.Domain.Regulations;

namespace DonwloadNMLK
{
	class MainClass
	{
		public static void Main(string[] args)
		{
				#region Connecnt to our DB
			var db = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.ConnectionString("server=office.qsolution.ru;port=3306;database=workwear_dev;user id=andrey;sslmode=None; password=123")
				.ShowSql()
				.FormatSql();

				Console.WriteLine("ORM");

				OrmConfig.ConfigureOrm(db, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(workwear.Domain.Users.UserSettings)),
				System.Reflection.Assembly.GetAssembly (typeof(MeasurementUnits)),
			});

			#endregion

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				Console.WriteLine("start");
				DataTable dtPROTECTION_TOOLS = getTableFromDataBase("SELECT * FROM PROTECTION_TOOL");
				DataTable dtPROTECTION_REPLACEMENT = getTableFromDataBase("SELECT * FROM PROTECTION_REPLACEMENT");

				var dicItemsTypes = new Dictionary<int, ItemsType>();

				foreach(DataRow row in dtPROTECTION_TOOLS.Rows) {
					var item = new ItemsType();
					item.Name = row["NAME"].ToString();
					item.Category = ItemTypeCategory.wear;
					dicItemsTypes[int.Parse(row["PROTECTION_ID"].ToString())] = item;
				}

				foreach(DataRow row in dtPROTECTION_REPLACEMENT.Rows) {
					dicItemsTypes[int.Parse(row["PROTECTION_ID"].ToString())].ItemsTypesAnalogs.Add(dicItemsTypes[int.Parse(row["PROTECTION_ID_ANALOG"].ToString())]);
					}

				foreach(var item in dicItemsTypes.Values) {
					uow.Save(item);
					}

				DataTable dtNORMA = getTableFromDataBase("SELECT * FROM SKLAD.NORMA");
				DataTable dtNORMA_ROW = getTableFromDataBase("SELECT * FROM SKLAD.NORMA_ROW");

				var dicNorms = new Dictionary<int, Norm>();
				var dicNorms_row = new Dictionary<int, NormItem>();

				foreach(DataRow rowNorma in dtNORMA.Rows) {
					Norm norm = new Norm();
					norm.DateFrom = DateTime.Parse(rowNorma["DATE_BEGIN"].ToString());
					norm.DateTo = DateTime.Parse(rowNorma["DATE_END"].ToString());

					dicNorms[int.Parse(rowNorma["NORMA_ID"].ToString())] = norm;

				}

				foreach(DataRow rowNorma_row in dtNORMA_ROW.Rows) {
					NormItem normItem = new NormItem();
					normItem.Amount = int.Parse(rowNorma_row["COUNT"].ToString());
					normItem.NormPeriod = NormPeriodType.Month;
					normItem.Item = dicItemsTypes[int.Parse(rowNorma_row["PROTECTION_ID"].ToString())];
					normItem.PeriodCount = int.Parse(rowNorma_row["WEARING_PERIOD"].ToString());
					normItem.Norm = dicNorms[int.Parse(rowNorma_row["NORMA_ID"].ToString())];

					dicNorms_row[int.Parse(rowNorma_row["NORMA_ROW_ID"].ToString())] = normItem;
					dicNorms[int.Parse(rowNorma_row["NORMA_ID"].ToString())].Items.Add(normItem);
				}

				foreach(var Norma in dicNorms.Values)
					uow.Save(Norma);

				DataTable dtPERSONAL_CARDS = getTableFromDataBase("SELECT * FROM SKLAD.PERSONAL_CARD");
				foreach (DataRow rowPers_cards in dtPERSONAL_CARDS.Rows) {
					EmployeeCard card = new EmployeeCard();
					card.PersonnelNumber = rowPers_cards["TN"].ToString();
					card.LastName = rowPers_cards["SURNAME"].ToString();
					card.FirstName = rowPers_cards["NAME"].ToString();
					card.Patronymic = rowPers_cards["SECNAME"].ToString();
					card.CardNumber = rowPers_cards["NUM_CARD"].ToString();
					card.Comment = rowPers_cards["COMM"].ToString();
					card.Sex = rowPers_cards["E_SEX"].ToString() == "0" ? Sex.M : Sex.F;
					uow.Save(card);
				}

				uow.Commit();
			}

			Console.ReadLine();
		}
		#region Connect to them DB
		public static DataTable getTableFromDataBase( string sql)
		{
			DataTable dt = new DataTable();

			using(OracleConnection oc = new OracleConnection()) {
				oc.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.10.30)(PORT=1521)) (CONNECT_DATA=(SERVICE_NAME=XE))); User Id=sklad;Password=Good#Admins;";
				oc.Open();

				OracleDataAdapter oda = new OracleDataAdapter(sql, oc);

				oda.Fill(dt);
				oc.Dispose();

				Console.WriteLine(dt.Rows.Count.ToString());
				Console.WriteLine($"{sql} end");

			}
			return dt;
		}

		#endregion
	}
}
