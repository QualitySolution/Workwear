using System;
using QS.Utilities.Text;
using workwear.Domain.Company;

namespace workwear.Tools.Oracle.HRDTO
{
	public class EmployeeHRInfo
	{
		#region Поля базы
		public string TN { get; set; }

		/// <summary>
		/// Фамилия
		/// </summary>
		public string SURNAME { get; set; }
		/// <summary>
		/// Имя
		/// </summary>
		public string NAME { get; set; }
		/// <summary>
		/// Отчество
		/// </summary>
		public string SECNAME { get; set; }

		public uint E_SEX { get; set; }

		/// <summary>
		/// Дата приема
		/// </summary>
		public DateTime? DHIRING { get; set; }

		/// <summary>
		/// Дата увольнения
		/// </summary>
		public DateTime? DUVOL { get; set; }

		/// <summary>
		/// Предполагаю что здесь должна быть дата смены должности, с текущей базе она все время <see langword="null"/>, но добавил на всякий случай.
		/// </summary>
		public DateTime? DCHANGE { get; set; }

		/// <summary>
		/// Код профессии
		/// </summary>
		public uint? E_PROF { get; set; }

		/// <summary>
		/// Код подразделения(дубль ID_DEPT?)
		/// </summary>
		//		public uint DEPT_CODE { get; set; }

		/// <summary>
		/// Место возникновения затрат
		/// </summary>
		public uint? COST_CENTER { get; set; }

		/// <summary>
		/// ???
		/// </summary>
		public uint? CODE_STAFF { get; set; }

		/// <summary>
		/// Отдел\подразделение
		/// </summary>
		public uint? ID_DEPT { get; set; }

		/// <summary>
		/// Код клетки штатного расписания
		/// </summary>
		public uint? ID_WP { get; set; }

		/// <summary>
		/// Код цеха
		/// </summary>
		public uint? PARENT_DEPT_CODE { get; set; }

		#endregion

		#region Рассчетные

		public string FIO {
			get {
				return String.Join(" ", SURNAME, NAME, SECNAME);
			}
		}

		public Sex Sex => E_SEX == 2 ? Sex.F : (E_SEX == 1 ? Sex.M : Sex.None);

		public bool Dismiss { get { return DUVOL.HasValue; } }

		public string Title => PersonHelper.PersonNameWithInitials(SURNAME, NAME, SECNAME);

		#endregion
	}
}
