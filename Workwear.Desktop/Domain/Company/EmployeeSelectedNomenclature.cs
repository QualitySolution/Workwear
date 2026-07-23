using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Company {

	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "выборов сотрудником номенклатур",
		Nominative = "выбор сотрудником номенклатуры",
		Genitive = "выбор сотрудником номенклатуры",
		GenitivePlural = "выбор сотрудником номенклатур"
	)]
	public class EmployeeSelectedNomenclature: PropertyChangedBase, IDomainObject {
		public virtual int Id { get; }
		
		public virtual string Title => $"{Employee.ShortName} в качестве \"{ProtectionTools.Name}\" предпоч{(Employee.Sex == Sex.F ? "ла" : "ёл")} \"{Nomenclature.Name}\"";
		
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
