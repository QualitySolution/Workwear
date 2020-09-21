using System;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Tools.Oracle.HRDTO;

namespace workwear.Tools.Oracle
{
	public class HRSystem
	{
		private readonly OracleConnection connection;

		public HRSystem(OracleConnection connection)
		{
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
		}

		public EmployeeHRInfo GetEmployeeInfo(string tn)
		{
			var sql = "select t.* from KIT.exp_hum_sklad t WHERE t.tn = :tnumber";

			return connection.QuerySingleOrDefault<EmployeeHRInfo>(sql, new { tnumber = tn });
		}

		public Department GetDepartment(int id)
		{
			var sql = "select t.ID_DEPT as Id, t.DEPT as Name FROM KIT.DEPTS t WHERE t.ID_DEPT = :dept_id";

			return connection.QuerySingleOrDefault<Department>(sql, new { dept_id = id });
		}

		public Post GetPost(int id)
		{
			var sql = "select t.ID_WP as Id, t.WP_NAME as Name, E_PROF as ProfessionId FROM KIT.WP t WHERE t.ID_WP = :wp_id";

			return connection.QuerySingleOrDefault<Post>(sql, new { wp_id = id });
		}

		public Profession GetProfession(int id)
		{
			var sql = "select t.ID_REF as Id, t.ID_REF as Code, t.ABBR as Name FROM KIT.EXP_PROF t WHERE t.ID_REF = :prof_id";

			return connection.QuerySingleOrDefault<Profession>(sql, new { prof_id = id });
		}
	}
}
