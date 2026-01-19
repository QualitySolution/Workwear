using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Company {
	public class EmployeeSelectedNomenclature: PropertyChangedBase, IDomainObject {
		
		public virtual int Id { get; }
		
		private EmployeeCard employee;
		[Display (Name = "Сотрудник")]
		public virtual EmployeeCard Employee {
			get => employee;
			set => SetField(ref employee, value);
		}

		private ProtectionTools protectionTools;
		[Display (Name = "Номенклатура нормы")]
		public virtual ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}

		private Nomenclature nomenclature;
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}
	}
}
