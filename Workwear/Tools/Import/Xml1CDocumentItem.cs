using System.Xml.Linq;

namespace workwear.Tools.Import 
{
	public class Xml1CDocumentItem 
	{
		public Xml1CDocumentItem(XElement element, XNamespace xNamespace) {
			root = element;
			this.xNamespace = xNamespace;
		}

		#region private field

		private readonly XElement root;
		private readonly XNamespace xNamespace;

		#endregion

		#region public property

		public string DocumentType => "Empty";
		public uint DocumentNumber => 0;

		#endregion
	}
}
