using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Repository.Stock;

namespace Workwear.Models.Operations {
	public class StockBalanceModel {
		private readonly UnitOfWorkProvider unitOfWorkProvider;
		private readonly StockRepository stockRepository;

		private List<StockBalance> stockBalances;

		public StockBalanceModel(UnitOfWorkProvider unitOfWorkProvider, StockRepository stockRepository) {
			this.unitOfWorkProvider = unitOfWorkProvider ?? throw new ArgumentNullException(nameof(unitOfWorkProvider));
			this.stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
		}
		
		#region Helpers
		private IUnitOfWork UoW => unitOfWorkProvider?.UoW;
		#endregion

		#region Параметры работы
		public Warehouse Warehouse { get; set; }
		public IEnumerable<WarehouseOperation> ExcludeOperations { get; set; }
		public IList<Nomenclature> Nomenclatures { get; set; }
		#endregion

		#region Работа с базой
		public void Update(IList<Nomenclature> nomenclatures, DateTime? OnDate = null) {
			Nomenclatures = nomenclatures;
			Update(OnDate);
		}
		
		public void Update(DateTime? OnDate = null) {
			stockBalances = stockRepository.StockBalances(UoW, Warehouse, Nomenclatures, OnDate ?? DateTime.Today, ExcludeOperations)
				.Select(sto => new StockBalance(sto.StockPosition, sto.Amount)).ToList();
		}
		#endregion
		
		#region Получение данных
		public IEnumerable<StockBalance> Balances => stockBalances;
		#endregion
	}

	public class StockBalance {
		
		public StockBalance(StockPosition position, int amount) {
			Position = position ?? throw new ArgumentNullException(nameof(position));
			Amount = amount;
		}
		
		public StockPosition Position { get; }
		public int Amount { get; }
	}
}
