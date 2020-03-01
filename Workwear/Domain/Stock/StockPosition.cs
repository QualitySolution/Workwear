using System;
namespace workwear.Domain.Stock
{
	/// <summary>
	/// Класс используется для передачи внутри бизнес логики конкретной позиции складского учета.
	/// То есть всех характеристик по которым на складе идет учет и это считаются отдельные позиции для хранения.
	/// ВНИМАНИЕ Класс не сохраняется в базу данных.
	/// </summary>
	public class StockPosition
	{
		public Nomenclature Nomenclature { get; private set; }
		public string Size { get; private set; }
		public string Growth { get; private set; }
		public decimal WearPercent { get; private set; }

		public StockPosition(Nomenclature nomenclature, string size, string growth, decimal wearPercent)
		{
			Nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));
			Size = size;
			Growth = growth;
			WearPercent = wearPercent;
		}

		public override bool Equals(object obj)
		{
			var anotherPos = obj as StockPosition;

			return 
				anotherPos.Nomenclature.Id == Nomenclature.Id && 
				anotherPos.Size == Size &&
				anotherPos.Growth == Growth &&
				anotherPos.WearPercent == WearPercent
			;
		}

		public string Title => String.Join(" ", Nomenclature.Name, Size, Growth, WearPercent.ToString("P"));
	}
}
