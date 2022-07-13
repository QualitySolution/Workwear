using System;
using Gamma.Utilities;
using QS.DomainModel.Entity;

namespace workwear.Models.Import
{
    public class EntityField
    {
        public string Title {
            get {
                if (Data is Enum enumType)
                    return enumType.GetEnumTitle();
                return Data.GetTitle();
            }
        }
        public object Data { get; set; }
    }
}