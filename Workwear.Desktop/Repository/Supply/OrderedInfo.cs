using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Repository.Supply
{
    /// <summary>
    /// Класс для хранения информации о заказанном, но не полученном товаре
    /// </summary>
    public class OrderedInfo
    {
        /// <summary>
        /// ID номенклатуры
        /// </summary>
        public int NomenclatureId { get; set; }
        
        /// <summary>
        /// Ссылка на номенклатуру
        /// </summary>
        public Nomenclature Nomenclature { get; set; }
        
        /// <summary>
        /// Количество заказанного, но не полученного товара
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// Размер одежды
        /// </summary>
        public Size WearSize { get; set; }
        
        /// <summary>
        /// Рост одежды
        /// </summary>
        public Size Height { get; set; }
        
        /// <summary>
        /// Общее заказанное количество
        /// </summary>
        public int OrderedCount { get; set; }

        public OrderedInfo() { }

        public OrderedInfo(int nomenclatureId, int amount, Size wearSize, Size height, Nomenclature nomenclature = null)
        {
            NomenclatureId = nomenclatureId;
            Amount = amount;
            WearSize = wearSize;
            Height = height;
            Nomenclature = nomenclature;
        }
    }
}
