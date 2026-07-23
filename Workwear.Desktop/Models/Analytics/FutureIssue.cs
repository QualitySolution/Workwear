using System;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Models.Analytics
{
	/// <summary>
	/// Базовый класс будущей выдачи
	/// </summary>
	public abstract class FutureIssue {
		public Nomenclature Nomenclature { get; set; }
		public DateTime? LastIssueDate { get; set; }
		public bool VirtualLastIssue { get; set; }
		public DateTime? OperationDate { get; set; }
		public DateTime? DelayIssueDate { get; set; }
		public int Amount { get; set; }

		public abstract ProtectionTools ProtectionTools { get; }
		public abstract Subdivision Subdivision { get; }
		/// <summary>
		/// Для выдач сотрудникам — карточка сотрудника.
		/// Для дежурных норм — ответственный сотрудник из дежурной нормы.
		/// </summary>
		public abstract EmployeeCard Employee { get; }
		public ItemsType ItemsType => ProtectionTools.Type;
		public virtual Size Size => null;
		public virtual Size Height => null;

		/// <summary>Пол получателя.</summary>
		public virtual Sex? EmployeeSex => Employee?.Sex;

		/// <summary>Подразделение получателя.</summary>
		public virtual Department Department => Employee?.Department;

		/// <summary>Профессия получателя.</summary>
		public virtual Post Post => Employee?.Post;

		/// <summary>Текстовое количество по норме.</summary>
		public abstract string NormAmountText { get; }

		/// <summary>Текстовый срок использования.</summary>
		public abstract string NormLifeText { get; }

		/// <summary>Название ограничения нормы.</summary>
		public abstract string NormConditionName { get; }

		/// <summary>Идентификатор нормы для экспорта.</summary>
		public abstract int? NormId { get; }

		/// <summary>
		/// Тип выдачи в виде строки для экспорта.
		/// Для дежурных норм — "Дежурное", для сотрудников — тип из ItemsType.
		/// </summary>
		public abstract string IssueTypeTitle { get; }
	}
}
