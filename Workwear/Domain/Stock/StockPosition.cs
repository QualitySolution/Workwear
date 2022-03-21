using System;
using System.Collections.Generic;
using System.Linq;
using workwear.Domain.Sizes;

namespace workwear.Domain.Stock
{
	/// <summary>
	/// Класс используется для передачи внутри бизнес логики конкретной позиции складского учета.
	/// То есть всех характеристик по которым на складе идет учет и это считаются отдельные позиции для хранения.
	/// ВНИМАНИЕ Класс не сохраняется в базу данных.
	/// </summary>
	public class StockPosition
	{
		public Nomenclature Nomenclature { get; }
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")]
		public string Size { get; private set; }
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")]
		public string Growth { get; private set; }
		public decimal WearPercent { get; private set; }
		
		public Size SizeType { get; set; }
		public Size Height { get; set; }

		public StockPosition(Nomenclature nomenclature, string size, string growth, decimal wearPercent, Size sizeType = null, Size height = null)
		{
			Nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));
			Size = size;
			Growth = growth;
			WearPercent = wearPercent;
			SizeType = sizeType;
			Height = height;
		}

		public string Title {
			get {
				var parameters = new List<string>();
				if(!string.IsNullOrWhiteSpace(Size))
					parameters.Add("Устаревший размер:" + Size);

				if(!string.IsNullOrWhiteSpace(Growth))
					parameters.Add("Устаревший рост:" + Growth);
				
				if(SizeType != null)
					parameters.Add("Размер:" + SizeType.Name);
				
				if(Height != null)
					parameters.Add("Рост:" + Height.Name);

				if(WearPercent > 0)
					parameters.Add("Износ:" + WearPercent.ToString("P"));

				var text = Nomenclature.Name;

				if(parameters.Any())
					text += $" ({string.Join("; ", parameters)})";

				return text;
			}
		}

		#region Сравнение

		public override bool Equals(object obj)
		{
			var anotherPos = obj as StockPosition;

			return
				anotherPos.Nomenclature.Id == Nomenclature.Id &&
				anotherPos.SizeType == SizeType &&
				anotherPos.Height == Height &&
				anotherPos.WearPercent == WearPercent
			;
		}

		public override int GetHashCode()
		{
			return (Nomenclature.Id, SizeType, Height, WearPercent).GetHashCode();
		}

		#endregion
	}
}
