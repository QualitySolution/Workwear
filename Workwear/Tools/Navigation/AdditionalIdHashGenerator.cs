using System;
using System.Linq;
using QS.Navigation;

namespace workwear.Tools.Navigation
{
	/// <summary>
	/// Генератор позволяет создавать диалоги принимающие дополнительный параметр в виде Id.
	/// Если эта штука окажется полезной и будет широко использоваться в разных диалогах,
	/// ее следует перенести в библиотеки.
	/// </summary>
	public class AdditionalIdHashGenerator : IExtraPageHashGenerator
	{
		private readonly Type[] typesOfVmWithIntId;

		public AdditionalIdHashGenerator(Type[] typesOfVmWithIntId)
		{
			this.typesOfVmWithIntId = typesOfVmWithIntId;
		}

		public string GetHash(Type typeViewModel, object[] ctorValues)
		{
			if (!typesOfVmWithIntId.Contains(typeViewModel))
				return null;
			
			string hash = null;
			foreach(var ctorArg in ctorValues) {
				if(ctorArg is int id) {
					hash = $"{typeViewModel.FullName}#{id}";
				}
			}

			return hash;
		}
	}
}