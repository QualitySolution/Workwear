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
		
		public XElement Root { get; }
		private readonly XNamespace xNamespace;
		
		#endregion

		#region public property
		
		public string DocumentType => Root.Name.LocalName.Replace("DocumentObject.", "");
		public uint DocumentNumber {
			get {
				var value = Root.Element(xNamespace + "Number")?.Value.Replace("-","").TrimStart('0');
				if(value != null) 
					return UInt32.Parse(value);
				throw new ArgumentException($"Не удалось получить значение номера {value}");
			}
		}
		
		#endregion
	}
}
