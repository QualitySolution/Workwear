using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Gamma.Utilities;
using QS.DomainModel.Entity;

namespace workwear.Models.Import
{
    public class DataType
    {
        public DataType(object data = null, int? order = null)
        {
            Data = data;
            if(order.HasValue)
	            ValueSetOrder = order.Value;
            else if(data is Enum) {
	            ValueSetOrder = (int)data;
            }
        }

        public bool IsUnknown => Data == null;
        public string Title {
            get {
                if (IsUnknown)
                    return "Пропустить";
                if (Data is Enum enumType)
                    return enumType.GetEnumTitle();
                return Data.GetTitle();
            }
        }
        public object Data { get; set; }

        #region Имена колонок
        public int ColumnNameDetectPriority;
        public readonly List<string> ColumnNameKeywords = new List<string>();
        public string ColumnNameRegExp;

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

        public Func<string, string> ValueInterpreter;

        #endregion
    }
}
