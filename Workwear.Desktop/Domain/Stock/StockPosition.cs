using System;
using System.Collections.Generic;
using System.Linq;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock
{
	/// <summary>
	/// Класс используется для передачи внутри бизнес логики конкретной позиции складского учета.
	/// То есть всех характеристик по которым на складе идет учет и это считаются отдельные позиции для хранения.
	/// ВНИМАНИЕ Класс не сохраняется в базу данных.
	/// </summary>
	public class StockPosition
	{
		public Nomenclature Nomenclature { get; }
		public decimal WearPercent { get; }
		public Size WearSize { get; }
		public Size Height { get; }
		public Owner Owner { get; }
		public StockPosition(Nomenclature nomenclature, decimal wearPercent, Size wearSize, Size height, Owner owner) {
			Nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));
			WearPercent = wearPercent;
			WearSize = wearSize;
			Height = height;
			Owner = owner;
		}

		public string Title {
			get {
				var parameters = new List<string>();
				if(WearSize != null)
					parameters.Add("Размер:" + WearSize.Name);
				if(Height != null)
					parameters.Add("Рост:" + Height.Name);
				if(WearPercent > 0)
					parameters.Add("Износ:" + WearPercent.ToString("P"));
				if(Owner != null)
					parameters.Add("Владелец:" + Owner.Name);
				var text = Nomenclature.Name;
				if(parameters.Any())
					text += $" ({string.Join("; ", parameters)})";
				return text;
			}
		}
		#region Сравнение
		public override bool Equals(object obj) {
			var anotherPos = obj as StockPosition;
			return
				anotherPos?.Nomenclature.Id == Nomenclature.Id &&
				anotherPos.WearSize?.Id == WearSize?.Id &&
				anotherPos.Height?.Id == Height?.Id &&
				anotherPos.WearPercent == WearPercent &&
				anotherPos.Owner?.Id == Owner?.Id;
		}
		public override int GetHashCode() {
			return (Nomenclature.Id, WearSize?.Id, Height?.Id, WearPercent, Owner?.Id).GetHashCode();
		}
		#endregion
	}
}
