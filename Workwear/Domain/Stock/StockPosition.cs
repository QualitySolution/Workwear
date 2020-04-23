using System;
using System.Collections.Generic;
using System.Linq;

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

		public string Title {
			get {
				var parameters = new List<string>();
				if(!String.IsNullOrWhiteSpace(Size))
					parameters.Add("Размер:" + Size);

				if(!String.IsNullOrWhiteSpace(Growth))
					parameters.Add("Рост:" + Growth);

				if(WearPercent > 0)
					parameters.Add("Износ:" + WearPercent.ToString("P"));

				var text = Nomenclature.Name;

				if(parameters.Any())
					text += String.Format(" ({0})", String.Join("; ", parameters));

				return text;
			}
		}

		#region Сравнение

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

		public override int GetHashCode()
		{
			return (Nomenclature.Id, Size, Growth, WearPercent).GetHashCode();
		}

		#endregion
	}
}
