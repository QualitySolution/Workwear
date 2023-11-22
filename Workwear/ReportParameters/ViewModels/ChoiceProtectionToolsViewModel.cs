using System;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.ViewModels;
using Workwear.Domain.Regulations;

namespace Workwear.ReportParameters.ViewModels {
	public class ChoiceProtectionToolsViewModel : ViewModelBase {
		
		private readonly IUnitOfWork UoW;
		
		public ChoiceProtectionToolsViewModel(IUnitOfWork uow)
		{
			this.UoW = uow ?? throw new ArgumentNullException(nameof(uow));
		}
		
		private IObservableList<SelectedProtectionTools> protectionTools;
		public IObservableList<SelectedProtectionTools> ProtectionTools {
			get {
				if(protectionTools == null)
					FillProtectionTools();
				return protectionTools;
			}
		}

		void FillProtectionTools(){
			SelectedProtectionTools resultAlias = null;

			protectionTools = new ObservableList<SelectedProtectionTools>(UoW.Session.QueryOver<ProtectionTools>()
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(() => true).WithAlias(() => resultAlias.Select)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<SelectedProtectionTools>())
				.List<SelectedProtectionTools>());
		}

		public int[] SelectedProtectionToolsIds()
		{
			if(ProtectionTools.All(x => x.Select))
				return new int[] { -1 };
			if(ProtectionTools.All(x => !x.Select))
				return new int[] { -2 };
			return ProtectionTools.Where(x => x.Select).Select(x => x.Id).Distinct().ToArray();
		}

		public void SelectAll() {
			foreach (var pt in ProtectionTools)
				pt.Select = true;
		}
		 
		public void UnSelectAll() {
			foreach (var pt in ProtectionTools)
				pt.Select = false;
		}
	}

	public class SelectedProtectionTools : PropertyChangedBase
	{
		private bool select;
		public virtual bool Select {
			get => select;
			set => SetField(ref select, value);
		}

		public int Id { get; set; }
		public string Name { get; set; }
	}
}
