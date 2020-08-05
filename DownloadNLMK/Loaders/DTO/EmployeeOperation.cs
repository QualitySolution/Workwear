using System;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace DownloadNLMK.Loaders.DTO
{
	public class EmployeeOperation
	{
		public EmployeeOperation()
		{
		}

		//public virtual int Id { get; set; }

		public DateTime operation_time { get; set; }

		public int employee_id => employee.Id;

		public int protection_tools_id => protectionTools.Id;

		public int nomenclature_id => nomenclature.Id;

		public int issued { get; set; }

		public int returned { get; set; }

		public bool auto_writeoff { get; set; } = true;

		public DateTime? StartOfUse { get; set; }

		public DateTime? ExpiryByNorm { get; set; }

		public DateTime? auto_writeoff_date { get; set; }

		//public int? issued_operation_id { get; set; }

		public int norm_item_id => normItem.Id;

		#region Установка ссылок

		private Nomenclature nomenclature;
		public Nomenclature Nomenclature { set => nomenclature = value; }

		private EmployeeCard employee;
		public EmployeeCard Employee { set => employee = value; }

		private ProtectionTools protectionTools;
		public ProtectionTools ProtectionTools { set => protectionTools = value; }

		private NormItem normItem;
		public NormItem NormItem { set => normItem = value; }

		#endregion

		public string Title => $"{protectionTools?.Name}({nomenclature?.Name}) - {issued}";
	}
}
