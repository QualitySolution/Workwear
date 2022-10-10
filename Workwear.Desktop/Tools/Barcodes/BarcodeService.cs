using System;
using System.Collections.Generic;
using System.Linq;
using QS.BaseParameters;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;

namespace Workwear.Tools.Barcodes 
{
	public class BarcodeService 
	{
		public BarcodeService(ParametersService baseParameters) {
			BaseCode = baseParameters.Dynamic.BarcodePrefix(typeof(int?)) ?? 2000;
		}

		/// <summary>
		/// Стартовый код для серийных номеров. Коды начинающиеся с 2 зарезервированы под внутреннее использование на предприятии по стандарту EAN-13
		/// Мы в первые 3 цифры после 2-ки зашиваем код клиента, вернее последние 3 цифры кода клиента, для того чтобы штрих коды из разных баз отличались.
		/// Это параметр можно поменять в настройка базы BarcodePrefix
		/// </summary>
		public int BaseCode { get; } = 2000; //2000-2999

		#region Create

		public IList<Barcode> Create(IUnitOfWork unitOfWork, int issued, EmployeeIssueOperation employeeIssueOperation) {
			var barcodes = Create(unitOfWork, issued);
			foreach(var barcode in barcodes) {
				barcode.EmployeeIssueOperation = employeeIssueOperation;
				unitOfWork.Save(barcode);
			}
			return barcodes;
		}
		
		public IList<Barcode> Create(IUnitOfWork unitOfWork, int amount) {
			var barCodeList = new List<Barcode>();
			for(var i = 1; i < amount + 1; i++) {
				var newBarCode = new Barcode();
				unitOfWork.Save(newBarCode);
				newBarCode.Title = $"{BaseCode}{newBarCode.Id:D8}{CheckSum($"{BaseCode}{newBarCode.Id:D8}")}";
				newBarCode.Fractional = $"{i}/{amount}";
				barCodeList.Add(newBarCode);
			}
			return barCodeList;
		}

		#endregion

		#region Get

		public IList<Barcode> GetByEmployeeIssueOperation(EmployeeIssueOperation employeeIssueOperation, IUnitOfWork uoW) => 
			uoW.GetAll<Barcode>().Where(b => b.EmployeeIssueOperation.Id == employeeIssueOperation.Id).ToList();

		#endregion

		#region Private Methods

		static int CheckSum(string upccode)
		{
			var sum = 0;
			var bOdd=false;
			foreach (var digit in upccode.Select(c => (int) Char.GetNumericValue(c)))
			{
				sum += bOdd ? digit * 3 : digit;
				bOdd = !bOdd;                       // switch every other character
			}
			var cs = 10 - sum % 10;
			return cs == 10? 0: cs;
		}
		
		#endregion
	}
}
