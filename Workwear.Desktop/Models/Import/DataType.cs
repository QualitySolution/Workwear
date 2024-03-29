using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Gamma.Utilities;
using QS.DomainModel.Entity;

namespace Workwear.Models.Import
{
    public class DataType
    {
        public DataType(object data = null, int? order = null)
        {
            Data = data;
            if(order.HasValue)
	            ValueSetOrder = order.Value;
        }

        public bool IsUnknown => Data == null;
        public virtual string Title {
            get {
                if (IsUnknown)
                    return "Пропустить";
                if (Data is Enum enumType)
                    return enumType.GetEnumTitle();
                return Data.GetTitle();
            }
        }

        public object Data {
	        get => data;
	        set {
		        data = value; 
		        if(data is Enum)
			        ValueSetOrder = (int)data;
	        }
        }

        #region Имена колонок
        public int ColumnNameDetectPriority;
        public readonly List<string> ColumnNameKeywords = new List<string>();
        public string ColumnNameRegExp;

	    /// <summary>
	    /// Внимание, здесь предполагается что строка придет уже преобразованная к нижнему регистру.
	    /// Это делается на более высоком уровне вложенности вызовов, для ускорения обработки.
	    /// </summary>
	    public bool ColumnNameMatch(string columnName)
        {
            if (!String.IsNullOrEmpty(ColumnNameRegExp) && Regex.IsMatch(columnName, ColumnNameRegExp))
                return true;
            foreach (var keyword in ColumnNameKeywords) {
                if (columnName.Contains(keyword))
                    return true;
            }

            return false;
        }
        #endregion

        #region Сохранение
        public int ValueSetOrder = 99;
        private object data;

        #endregion
    }
}
