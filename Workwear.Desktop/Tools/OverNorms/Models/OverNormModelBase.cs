using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.Project.Domain;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Tools.OverNorms.Models 
{
	public abstract class OverNormModelBase : PropertyChangedBase, IOverNormModel 
	{
		public virtual bool Editable => true;
		
		/// <summary>
		/// Указвает может ли эта модель использоваться со штрихкодами
		/// </summary>
		public abstract bool CanUseWithBarcodes { get; }
		
		/// <summary>
		/// Указвает может ли эта модель использоваться без штрихкодов
		/// </summary>
		public abstract bool CanUseWithoutBarcodes { get; }

		/// <summary>
		/// Указывает можно ли изменять статус использования штрихкодов
		/// <see cref="UseBarcodes"/>
		/// </summary>
		public virtual bool CanChangeUseBarcodes => CanUseWithBarcodes && CanUseWithoutBarcodes;

		private bool useBarcodes;
		/// <summary>
		/// Свойство для обозначения статуса использования штрихкодов для этой модели (используются ли штрихкоды в данный момент)
		/// </summary>
		public virtual bool UseBarcodes
		{
			get => CanUseWithBarcodes && (!CanChangeUseBarcodes || useBarcodes);
			set => SetField(ref useBarcodes,  value);
		}

		/// <summary>
		/// Обязательно ли наличие операции выдачи сотруднику для этой модели
		/// </summary>
		public abstract bool RequiresEmployeeIssueOperation { get; }

		/// <summary>
		/// Создание нового документа
		/// </summary>
		/// <param name="params">Параметры операций сверх нормы</param>
		/// <param name="expenseWarehouse">Склад списания</param>
		/// <param name="createdByUser">Пользователь, создавший документ</param>
		/// <param name="docNumber">Пользовательский номер документа</param>
		/// <param name="comment">Комментарий</param>
		/// <returns>Новый документ сверзз нормы с заполненными строками</returns>
		public abstract OverNorm CreateDocument(IList<OverNormParam> @params, Warehouse expenseWarehouse, UserBase createdByUser = null, string docNumber = null, string comment = null);

		/// <summary>
		/// Создать операцию списания
		/// </summary>
		/// <param name="operation">Операция, которую необходимо списать</param>
		/// <param name="receiptWarehouse">Склад прихода</param>
		/// <param name="createdByUser">Пользователь, создавший документ</param>
		/// <param name="docNumber">Пользовательский номер документа</param>
		/// <param name="comment">Комментарий</param>
		public abstract void WriteOffOperation(OverNormOperation operation, Warehouse receiptWarehouse, UserBase createdByUser = null, string docNumber = null, string comment = null);

		/// <summary>
		/// Обновить/изменить операцию
		/// </summary>
		/// <param name="item">Строка документа, которую необходимо изменить</param>
		/// <param name="param">Параметры операции сверх нормы</param>
		public abstract void UpdateOperation(OverNormItem item, OverNormParam param);

		/// <summary>
		/// Добавить новую строку в документ
		/// </summary>
		/// <param name="document">Документ для добавления</param>
		/// <param name="param">Параметры операции сверх нормы</param>
		/// <param name="expenseWarehouse">Склад списания</param>
		public abstract void AddOperation(OverNorm document, OverNormParam param, Warehouse expenseWarehouse);
	}
}
