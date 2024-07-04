using System;
using System.Collections.Generic;
using System.Linq;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Tools.OverNorms
{
	public class OverNormParam 
	{
		public EmployeeCard Employee { get; }
		
		public Nomenclature Nomenclature { get; }
		
		public int Amount { get; }
		
		public Size Size { get; }
		
		public Size Height { get; }
		
		public EmployeeIssueOperation EmployeeIssueOperation { get; }
		
		public IList<Barcode> Barcodes { get; }

		private OverNormParam() 
		{
			Barcodes = new List<Barcode>();
		}

		public OverNormParam(EmployeeCard employee) : this() 
		{
			Employee = employee ?? throw new ArgumentNullException(nameof(employee));
		}
		
		/// <param name="employee">Сотрудник, для которого определяется операция сверх нормы</param>
		/// <param name="amount">Количетсво выдаваемой номенклатуры</param>
		/// <param name="employeeIssueOperation">Операция выдачи сотруднику для сопосталвения подменной вещи и заменяемой</param>
		/// <param name="barcodes">Список штрихкодов выдаваемых вещей</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException">Количество выдаваемой номенклатуры не соответствует количеству переданных штрихкодов, если коллекция не пустая</exception>
		/// <exception cref="InvalidOperationException"></exception>
		public OverNormParam(EmployeeCard employee, Nomenclature nomenclature, int amount, Size size = null, Size height = null, EmployeeIssueOperation employeeIssueOperation = null, IList<Barcode> barcodes = null) 
		{
			Employee = employee ?? throw new ArgumentNullException(nameof(employee));;
			Nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));
			Amount = amount;
			Size = size;
			Height = height;
			EmployeeIssueOperation = employeeIssueOperation;
			Barcodes = barcodes ?? new List<Barcode>();
			
			if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));
			if (employeeIssueOperation != null && employee.Id != employeeIssueOperation.Employee.Id) throw new InvalidOperationException("Сотрудник в операции выдачи отличается от переданного");
			if (Barcodes.Any()) 
			{
				if (amount != Barcodes.Count) throw new InvalidOperationException("Количество штрихкодов должно соответствовать указанному количеству");
				if (employeeIssueOperation != null && !ItemsTypeMatch(employeeIssueOperation, Barcodes)) throw new InvalidOperationException("Группа номенклатур подменных вещей не соответсвует заменяемой у сотрудника");
				if (!NomeclatureMatch(nomenclature, size, height, Barcodes)) throw new InvalidOperationException("Номенклатура штрихкодов должна соответствовать указанным");
			}
		}
		
		private bool ItemsTypeMatch(EmployeeIssueOperation employeeIssueOperation, IList<Barcode> substituteBarcodes) 
		{
			return substituteBarcodes.All(x => x.Nomenclature.Type.Id == employeeIssueOperation.Nomenclature.Type.Id);
		}

		private bool NomeclatureMatch(Nomenclature nomenclature, Size size, Size height, IList<Barcode> barcodes) 
		{
			return barcodes.All(x => x.Nomenclature.Id == nomenclature?.Id && x.Size.Id == size?.Id && x.Height.Id == height?.Id);
		}
	}
}
