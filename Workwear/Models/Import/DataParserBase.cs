using System;
using System.Collections.Generic;
using System.Linq;

namespace workwear.Models.Import
{
	public class DataParserBase<TDataTypeEnum> : IDataParser<TDataTypeEnum>
	{
		public Dictionary<string, TDataTypeEnum> ColumnNames = new Dictionary<string, TDataTypeEnum>();

		public TDataTypeEnum DetectDataType(string columnName)
		{
			var foundKey = ColumnNames.Keys.FirstOrDefault(columnName.Contains);
			return (foundKey != null) ? ColumnNames[foundKey] : default;
		}
	}
}
