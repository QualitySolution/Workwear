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

		private List<StockBalance> stockBalances = new List<StockBalance>();

		public StockBalanceModel(UnitOfWorkProvider unitOfWorkProvider, StockRepository stockRepository) {
			this.unitOfWorkProvider = unitOfWorkProvider ?? throw new ArgumentNullException(nameof(unitOfWorkProvider));
			this.stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
		}
		
		/// <summary>
		/// Только для тестов
		///	</summary>
		public StockBalanceModel() { }
		
		#region Helpers
		private IUnitOfWork UoW => unitOfWorkProvider?.UoW;
		#endregion

		#region Параметры работы

		private DateTime? onDate;
		/// <summary>
		/// Устанавливаем дату на которую необходимо получать остатки.
		/// Если дата не указана остатки отображаются на текущую дату.
		/// При установке даты, если есть добавленные номенклатуры остатки обновляются.
		/// </summary>
		public DateTime? OnDate {
			get => onDate;
			set {
				if(onDate != value) {
					onDate = value;
					Refresh();
				}
			}
		}

		private Warehouse warehouse;

		/// <summary>
		/// Устанавливаем склад, для которого необходимо получать остатки.
		/// Если склад не указан остатки отображаются по всем складам.
		/// При установке склада, если есть добавленные номенклатуры остатки обновляются.
		/// </summary>
		public Warehouse Warehouse {
			get => warehouse;
			set {
				if(warehouse != value) {
					warehouse = value;
					Refresh();
				}
			}
		}

		public IEnumerable<WarehouseOperation> ExcludeOperations { get; set; }
		#endregion

		#region Номеклатура
		public HashSet<Nomenclature> Nomenclatures { get; } = new HashSet<Nomenclature>();
		
		public void AddNomenclatures(IEnumerable<Nomenclature> nomenclatures) {
			var newNomenclatures = nomenclatures.Except(Nomenclatures).ToList();
			if (newNomenclatures.Any()) {
				Nomenclatures.UnionWith(newNomenclatures);
				stockBalances.AddRange(stockRepository.StockBalances(UoW, Warehouse, newNomenclatures, OnDate?.Date ?? DateTime.Today, ExcludeOperations)
					.Select(sto => new StockBalance(sto.StockPosition, sto.Amount)));
			}
			
		}
		#endregion

		#region Работа с базой
		/// <summary>
		/// Обновляет информацию об остатках. 
		/// </summary>
		public void Refresh() {
			if(Nomenclatures.Any())
				stockBalances = stockRepository.StockBalances(UoW, Warehouse, Nomenclatures, OnDate?.Date ?? DateTime.Today, ExcludeOperations)
					.Select(sto => new StockBalance(sto.StockPosition, sto.Amount)).ToList();
		}
		#endregion
		
		#region Получение данных
		public virtual IEnumerable<StockBalance> Balances => stockBalances;
		public IEnumerable<StockBalance> ForNomenclature(params Nomenclature[] nomenclatures) => stockBalances.Where(x => nomenclatures.Contains(x.Position.Nomenclature));
		public int GetAmount(StockPosition stockPosition) => stockBalances.FirstOrDefault(x => x.Position.Equals(stockPosition))?.Amount ?? 0;
		public int GetInStock(StockPosition stockPosition) => stockBalances
			.Where(x => x.Position.Nomenclature.Id == stockPosition.Nomenclature.Id &&
			            x.Position.WearSize?.Id == stockPosition.WearSize?.Id && 
			            x.Position.Height?.Id == stockPosition.Height?.Id)
			.Sum(x => x.Amount);
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
