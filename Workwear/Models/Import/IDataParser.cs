using System;
using System.Collections.Generic;

namespace workwear.Models.Import
{
	public interface IDataParser
	{
		DataType DetectDataType(string columnName);
		
		List<DataType> SupportDataTypes { get; }
	}
}
