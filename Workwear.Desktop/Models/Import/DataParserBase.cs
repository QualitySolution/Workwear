using System;
using System.Collections.Generic;
using System.Linq;

namespace Workwear.Models.Import
{
	public class DataParserBase : IDataParser
	{
		#region DataTypes
		public List<DataType> SupportDataTypes { get; } = new List<DataType>{new DataType()}; //По умолчанию добавляем пункт "Пропустить"
		
		protected DataType AddColumnName(object data, params string[] names)
		{
			var dataType = new DataType(data);
			dataType.ColumnNameKeywords.AddRange(names.Select(x => x.ToLower()));
			SupportDataTypes.Add(dataType);
			return dataType;
		}

		public virtual DataType DetectDataType(string columnName)
		{
			foreach (var dataType in SupportDataTypes.OrderByDescending(x => x.ColumnNameDetectPriority)) {
				if (dataType.ColumnNameMatch(columnName))
					return dataType;
			}

			return null;
		}
		#endregion
	}
}
