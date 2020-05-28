using System;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using workwear.Domain.Company;
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

		public Subdivision GetSubdivision(int id)
		{
			var sql = "select t.KGRPOL as Id, t.NGRPOL as Name FROM SKLAD.SGRPOL t WHERE t.KGRPOL = :dept_code";

			return connection.QuerySingleOrDefault<Subdivision>(sql, new { dept_code = id });
		}

		public Department GetDepartment(int id)
		{
			var sql = "select t.ID_DEPT as Id, t.DEPT as Name FROM KIT.DEPTS t WHERE t.ID_DEPT = :dept_id";

			return connection.QuerySingleOrDefault<Department>(sql, new { dept_id = id });
		}

		public Post GetPost(int id)
		{
			var sql = "select t.ID_WP as Id, t.WP_NAME as Name FROM KIT.WP t WHERE t.ID_WP = :wp_id";

			return connection.QuerySingleOrDefault<Post>(sql, new { wp_id = id });
		}
	}
}
