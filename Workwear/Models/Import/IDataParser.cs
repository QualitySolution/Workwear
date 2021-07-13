using System;
namespace workwear.Models.Import
{
	public interface IDataParser<TDataTypeEnum>
	{
		TDataTypeEnum DetectDataType(string columnName);
	}
}
