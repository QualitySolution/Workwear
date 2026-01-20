using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Regulations {
	public class ProtectionToolsNomenclature : PropertyChangedBase, IDomainObject {
		public virtual int Id { get; set; }

		ProtectionTools protectionTools;
		[Display(Name = "Номенклатура нормы")]
		public virtual ProtectionTools ProtectionTools {
			get { return protectionTools; }
			set { SetField(ref protectionTools, value); }
		}

		Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value); }
		}
		
		private bool canChoose;
		[Display(Name = "Можно мыбрать в ЛК")]
		public virtual bool CanChoose {
			get => canChoose;
			set => SetField(ref canChoose, value);
		}
	}
}
