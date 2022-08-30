using System;
using System.Xml.Linq;

namespace workwear.Tools.Import 
{
	public class Xml1CDocument 
	{
		public Xml1CDocument(XElement element, XNamespace xNamespace) {
			Root = element;
			this.xNamespace = xNamespace;
		}
		
		#region private field
		
		private readonly XNamespace xNamespace;
		
		#endregion

		#region public property
		
		public XElement Root { get; }
		public string DocumentType => Root.Name.LocalName.Replace("DocumentObject.", "");
		public uint DocumentNumber {
			get {
				var value = Root.Element(xNamespace + "Number")?.Value.Replace("-","").TrimStart('0');
				if(value != null) 
					return UInt32.Parse(value);
				throw new ArgumentException($"Не удалось получить значение номера документа");
			}
		}

		public DateTime? Date {
			get {
				var dateString = Root.Element(xNamespace + "Date")?.Value;
				if(DateTime.TryParse(dateString, out var result))
					return result;
				return null;
			}
		}

		#endregion
	}
}
