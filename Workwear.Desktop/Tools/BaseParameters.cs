using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Reflection;
using QS.BaseParameters;

namespace Workwear.Tools
{
	public class  BaseParameters : ParametersService
	{
		public BaseParameters(Func<DbConnection> connectionFactory) : base(connectionFactory) { }
		/// <summary>
		/// Используется только для тестов!!!
		/// </summary>
		public BaseParameters() {
			var exe = Assembly.GetEntryAssembly();
			if(exe?.ManifestModule.Name == "workwear.exe")
				throw new InvalidProgramException(
					$"Использовать пустой конструктор для {nameof(BaseParameters)} можно только в тестах.");
		}
		#region Типизированный доступ и дефолтные значения
		//Ключевое слово virtual у свойств необходимо для возможности подмены в тестах.
		public virtual bool DefaultAutoWriteoff {
			get => Dynamic.DefaultAutoWriteoff(typeof(bool)) ?? true;
			set => Dynamic[nameof(DefaultAutoWriteoff)] = value;
		}
		
		/// <summary>
		/// Разрешать выдачу раньше срока (дней)
		/// </summary>
		public virtual int ColDayAheadOfShedule {
			get => Dynamic.ColDayAheadOfShedule(typeof(int)) ?? 0;
			set => Dynamic[nameof(ColDayAheadOfShedule)] = value;
		}

		/// <summary>
		/// Проверять остатки при расходе со склада.
		/// </summary>
		public virtual bool CheckBalances {
			get => Dynamic.CheckBalances(typeof(bool)) ?? true;
			set => Dynamic[nameof(CheckBalances)] = value;
		}
		/// <summary>
		/// Спрашивать о  перенесении начала эксплуатации:
		/// </summary>
		public virtual AnswerOptions ShiftExpluatacion {
			get => Dynamic.ShiftExpluatacion(typeof(AnswerOptions)) ?? AnswerOptions.Ask;
			set => Dynamic[nameof(ShiftExpluatacion)] = value;
		}
		/// <summary>
		/// Спрашивать об увеличении периода эксплуатации выданного пропорционально количеству
		/// </summary>
		public virtual AnswerOptions ExtendPeriod {
			get => Dynamic.ExtendPeriod(typeof(AnswerOptions)) ?? AnswerOptions.Ask;
			set => Dynamic[nameof(ExtendPeriod)] = value;
		}
		/// <summary>
		/// Префикс для штрихкодов, желательно чтобы был разный для каждой базы, чтобы штрихкоды не пересекались.
		/// По умолчанию заполняется последними цифрами кода клиента прочитанного из серийного номера. 
		/// </summary>
		public virtual int? BarcodePrefix {
			get => Dynamic.BarcodePrefix(typeof(int?));
			set => Dynamic[nameof(BarcodePrefix)] = value;
		}
		
		/// <summary>
		/// При заполнении документа персональной выдачи проставлять позиции коллективной выдачи.
		/// </summary>
		public virtual bool CollectiveIssueWithPersonal {
			get => Dynamic.CollectiveIssueWithPersonal(typeof(bool)) ?? false;
			set => Dynamic[nameof(CollectiveIssueWithPersonal)] = value;
		}
		
		/// <summary>
		/// При показе ведомости сворачивать дублирующиеся строки.
		/// </summary>
		public virtual bool CollapseDuplicateIssuanceSheet {
			get => Dynamic.CollapseDuplicateIssuanceSheet(typeof(bool)) ?? true;
			set => Dynamic[nameof(CollapseDuplicateIssuanceSheet)] = value;
		}
		
		/// <summary>
		/// Используемая валюта
		/// </summary>
		public virtual string UsedCurrency {
			get => Dynamic.UsedCurrency(typeof(string)) ?? "\u20bd";
			set => Dynamic[nameof(UsedCurrency)] = value;
		}

		/// <summary>
		/// На оборотной стороне карточки сотрудника вместо подписи, печатать номер документа выдачи.
		/// </summary>
		public virtual bool IsDocNumberInIssueSign {
			get => Dynamic.IsDocNumberInIssueSign(typeof(bool)) ?? true;
			set => Dynamic[nameof(IsDocNumberInIssueSign)] = value;
		}
		/// <summary>
		/// На оборотной стороне карточки сотрудника вместо подписи, печатать номер документа возврата\списания.
		/// </summary>
		public virtual bool IsDocNumberInReturnSign {
			get => Dynamic.IsDocNumberInReturnSign(typeof(bool)) ?? true;
			set => Dynamic[nameof(IsDocNumberInReturnSign)] = value;
		}
		
		/// <summary>
		/// Дата запрета редактирования документов.
		/// </summary>
		public virtual DateTime? EditLockDate {
			get => Dynamic.EditLockDate(typeof(DateTime?));
			set => Dynamic[nameof(EditLockDate)] = value;
		}
		#endregion
	}
	public enum AnswerOptions {
		[Display(Name = "Спрашивать")]
		Ask,
		[Display(Name ="Всегда да")]
		Yes,
		[Display(Name = "Всегда нет")]
		No
	}
	
}
