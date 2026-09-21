using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Repository.Operations;

namespace Workwear.Tools.Barcodes 
{
	public class BarcodeService 
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private static Random random = new Random();
		private EmployeeIssueRepository employeeIssueRepository;

		public BarcodeService(BaseParameters baseParameters, EmployeeIssueRepository employeeIssueRepository) {
			BaseCode = baseParameters.BarcodePrefix ?? 2001;
			this.employeeIssueRepository = employeeIssueRepository;
		}

		/// <summary>
		/// Стартовый код для серийных номеров. Коды, начинающиеся с 2, зарезервированы под внутреннее использование на предприятии по стандарту EAN-13.
		/// Мы в первые 3 цифры после 2-ки зашиваем код клиента, вернее последние 3 цифры кода клиента, для того чтобы штрих коды из разных баз отличались.
		/// Это параметр можно поменять в настройке базы BarcodePrefix
		/// </summary>
		public int BaseCode { get; } //2001-2999

		#region Create

		/// <summary>
		/// Генерирует штрихкоды для выданного сотруднику
		/// </summary>
		public void CreateBarcodeEAN13(IUnitOfWork unitOfWork, IEnumerable<EmployeeIssueOperation> employeeIssueOperations) {
			if(employeeIssueOperations == null)
				throw new ArgumentNullException(nameof(employeeIssueOperations));

			if(!employeeIssueOperations.Any())
				return;

			var usedNumbers = employeeIssueRepository
				.GetBarcodeNumbersForEmployee(
					employeeIssueOperations.Select(x => x.Employee).ToList(),
					employeeIssueOperations.Max(x=> x.OperationTime),
					unitOfWork)
				.ToList();
			foreach(var operation in employeeIssueOperations) {
				int bcount = operation.BarcodeOperations.Count;

				if(operation.Issued > bcount) {
					var barcodes = Create(unitOfWork, operation.Issued - bcount, operation.Nomenclature, operation.WearSize, operation.Height);
					foreach(var barcode in barcodes) {
						var kitNumber = GetNextKitNumber(usedNumbers, operation);
						var barcodeOperation = new BarcodeOperation {
							Barcode = barcode,
							EmployeeIssueOperation = operation,
							WarehouseOperation = operation.WarehouseOperation,
							KitNumber = kitNumber
						};
						operation.BarcodeOperations.Add(barcodeOperation);
						unitOfWork.Save(barcodeOperation);
						usedNumbers.Add(new BarcodeNumberInfo {
							EmployeeId = operation.Employee.Id,
							ProtectionToolsId = GetProtectionToolsId(operation),
							KitNumber = kitNumber
						});
					}
				}
			}
		}

		public int GetNextKitNumber(IUnitOfWork unitOfWork, EmployeeIssueOperation operation) {
			var usedNumbers = employeeIssueRepository
				.GetBarcodeNumbersForEmployee(new[] { operation.Employee }, operation.OperationTime, unitOfWork)
				.ToList();
			usedNumbers.AddRange(operation.BarcodeOperations
				.Where(x => x.KitNumber > 0)
				.Select(x => new BarcodeNumberInfo {
					EmployeeId = operation.Employee.Id,
					ProtectionToolsId = GetProtectionToolsId(operation),
					KitNumber = x.KitNumber
				}));
			return GetNextKitNumber(usedNumbers, operation);
		}

		private static int GetNextKitNumber(
			IEnumerable<BarcodeNumberInfo> usedNumbers,
			EmployeeIssueOperation operation)
		{
			var protectionToolsId = GetProtectionToolsId(operation);
			return Enumerable.Range(1, int.MaxValue - 1)
				.First(n => !usedNumbers
					.Where(x => x.EmployeeId == operation.Employee.Id && x.ProtectionToolsId == protectionToolsId)
					.Select(x => x.KitNumber.Value)
					.Contains(n));
		}

		private static int GetProtectionToolsId(EmployeeIssueOperation operation) =>
			operation.ProtectionTools?.Id ?? operation.NormItem?.ProtectionTools?.Id ?? 0;

		/// <summary>
		/// Возвращает <paramref name="count"/> свободных номеров комплектов для маркировки на складе.
		/// </summary>
		public IList<int> GetNextKitNumbers(
			IUnitOfWork unitOfWork,
			Warehouse warehouse,
			Nomenclature nomenclature,
			int count,
			KitNumberingMode kitNumberingMode)
		{
			BarcodeOperation barcodeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;
			var query = unitOfWork.Session.QueryOver(() => barcodeOperationAlias)
				.JoinAlias(() => barcodeOperationAlias.WarehouseOperation, () => warehouseOperationAlias)
				.Where(() => warehouseOperationAlias.ReceiptWarehouse.Id == warehouse.Id)
				.Where(() => barcodeOperationAlias.KitNumber != null);
			if(kitNumberingMode == KitNumberingMode.PerNomenclature)
				query = query.Where(() => warehouseOperationAlias.Nomenclature.Id == nomenclature.Id);

			var usedNumbers = new HashSet<int>(query
				.SelectList(list => list.Select(() => barcodeOperationAlias.KitNumber))
				.List<int?>()
				.Select(x => x.Value));

			var result = new List<int>();
			for(var n = 1; result.Count < count; n++) {
				if(usedNumbers.Contains(n))
					continue;
				result.Add(n);
				usedNumbers.Add(n);
			}
			return result;
		}

		/// <summary>
		/// Сгенерировать набор штрихкодов
		/// </summary>
		/// <param name="unitOfWork">В котором будут созданы и сохранены, без комита</param>
		/// <param name="label">до 50 символов</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public IList<Barcode> Create(IUnitOfWork unitOfWork, int amount, Nomenclature nomenclature, Size size, Size height, string label = "") {
			if (label?.Length > Barcode.LabelMaxLength)
				throw new ArgumentOutOfRangeException(nameof(label));
			var barCodeList = new List<Barcode>();
			for(var i = 1; i < amount + 1; i++) {
				var newBarCode = new Barcode();
				newBarCode.Nomenclature = nomenclature;
				newBarCode.Size = size;
				newBarCode.Height = height;
				newBarCode.Title = "generated" + random.Next(); //т.к. в базе not null, а далее уже нужен id
				unitOfWork.Save(newBarCode);
				//Перезаписываем Title, так как он формируется на основании полученного Id
				newBarCode.Title = $"{BaseCode}{newBarCode.Id:D8}{CheckSum($"{BaseCode}{newBarCode.Id:D8}")}";
				unitOfWork.Save(newBarCode);
				barCodeList.Add(newBarCode);
			}
			return barCodeList;
		}

		public static void SetClothingServiceCode(IUnitOfWork uow, Service service) {
			uow.Save(service);
			service.Code = $"2000{service.Id:D8}{CheckSum($"2000{service.Id:D8}")}";
			uow.Save(service);
		}
		#endregion
        #region Barcodes Info
		public static bool CheckBarcode(string barcode, BarcodeTypes type) {
			switch(type) {
				case BarcodeTypes.EAN13:
					return barcode.Length == 13 && barcode.All(char.IsDigit) && CheckSum(barcode.Substring(0, 12)).ToString()[0] == barcode[12];
				case BarcodeTypes.EPC96:
					return barcode.Length == 24 && System.Text.RegularExpressions.Regex.IsMatch(barcode, @"^[0-9A-Fa-f]+$");
				default:
					throw new NotImplementedException(typeof(BarcodeTypes)+ " in " + type);
			}
		}

		public int CountBarcodesOnWarehouse(IUnitOfWork unitOfWork, StockPosition stockPosition, Warehouse warehouse)
		{
			if (unitOfWork == null) throw new ArgumentNullException(nameof(unitOfWork));
			if (stockPosition == null) throw new ArgumentNullException(nameof(stockPosition));
			if (warehouse == null) throw new ArgumentNullException(nameof(warehouse));

			Barcode bAlias = null;
			WarehouseOperation who = null;
			BarcodeOperation boAlias = null;
			BarcodeOperation lastOperationSubAlias = null;
			EmployeeIssueOperation lastEmpSubAlias = null;
			DutyNormIssueOperation lastDutyNormSubAlias = null;
			WarehouseOperation lastWhSubAlias = null;
			OverNormOperation lastOverNormSubAlias = null;
			var lastOperationIdSubQuery = QueryOver.Of(() => lastOperationSubAlias)
				.Left.JoinAlias(() => lastOperationSubAlias.WarehouseOperation, () => lastWhSubAlias)
				.Left.JoinAlias(() => lastOperationSubAlias.EmployeeIssueOperation, () => lastEmpSubAlias)
				.Left.JoinAlias(() => lastOperationSubAlias.DutyNormIssueOperation, () => lastDutyNormSubAlias)
				.Left.JoinAlias(() => lastOperationSubAlias.OverNormOperation, () => lastOverNormSubAlias)
				.Where(() => lastOperationSubAlias.Barcode.Id == bAlias.Id)
				.OrderBy(Projections.SqlFunction("coalesce", NHibernateUtil.Date,
					Projections.Property(() => lastEmpSubAlias.OperationTime),
					Projections.Property(() => lastDutyNormSubAlias.OperationTime),
					Projections.Property(() => lastOverNormSubAlias.OperationTime),
					Projections.Property(() => lastWhSubAlias.OperationTime)))
					.Desc
				.Select(x => x.Id)
				.Take(1);

			int barcodesInStock = unitOfWork.Session.QueryOver<BarcodeOperation>(() => boAlias)
				.JoinAlias(bo => bo.Barcode, () => bAlias)
				.JoinAlias(bo => bo.WarehouseOperation, () => who)
				.Where(b =>
					bAlias.Nomenclature == stockPosition.Nomenclature &&
					bAlias.Size == stockPosition.WearSize &&
					bAlias.Height == stockPosition.Height &&
					who.WearPercent == stockPosition.WearPercent &&
					who.Owner == stockPosition.Owner)
				.Where(() => who.ReceiptWarehouse.Id == warehouse.Id)
				.Where(Subqueries.WhereProperty(() => boAlias.Id).Eq(lastOperationIdSubQuery))
				.SelectList(list => list
					.SelectCountDistinct(() => bAlias.Id)
				)
				.SingleOrDefault<int>();

			return barcodesInStock;
		}
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
